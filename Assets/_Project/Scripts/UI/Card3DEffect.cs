using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using TMPro;
using System;

namespace DigitPark.UI
{
    /// <summary>
    /// Componente 3D mejorado para cartas de MemoryPairs
    /// Con animaciones de volteo, match (verde) y error (rojo) muy visuales
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class Card3DEffect : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("3D Effect Settings")]
        [SerializeField] private float pressDepth = 10f;
        [SerializeField] private float pressDuration = 0.08f;
        [SerializeField] private float flipDuration = 0.3f;

        [Header("Visual References")]
        [SerializeField] private RectTransform cardFace;
        [SerializeField] private Image shadowImage;
        [SerializeField] private Image sideImage;
        [SerializeField] private Image faceImage;
        [SerializeField] private Image cardImage;
        [SerializeField] private TextMeshProUGUI symbolText;
        [SerializeField] private Image cardBackPattern;

        [Header("Colors - Face Down (Oculta)")]
        [SerializeField] private Color faceDownColor = new Color(0.06f, 0.04f, 0.1f, 1f);
        [SerializeField] private Color faceDownSideColor = new Color(0.04f, 0.02f, 0.06f, 1f);
        [SerializeField] private Color faceDownGlowColor = new Color(0.6f, 0.1f, 0.6f, 0.3f);
        [SerializeField] private Color faceDownPatternColor = new Color(0.8f, 0.2f, 0.8f, 0.15f);

        [Header("Colors - Face Up (Revelada)")]
        [SerializeField] private Color faceUpColor = new Color(0.12f, 0.06f, 0.18f, 1f);
        [SerializeField] private Color faceUpSideColor = new Color(0.08f, 0.04f, 0.12f, 1f);
        [SerializeField] private Color faceUpGlowColor = new Color(1f, 0.4f, 1f, 1f);
        [SerializeField] private Color symbolColor = Color.white;

        [Header("Colors - Match (Verde Éxito)")]
        [SerializeField] private Color matchedFaceColor = new Color(0.05f, 0.25f, 0.1f, 1f);
        [SerializeField] private Color matchedSideColor = new Color(0.03f, 0.15f, 0.05f, 1f);
        [SerializeField] private Color matchedGlowColor = new Color(0.2f, 1f, 0.4f, 1f);
        [SerializeField] private Color matchedSymbolColor = new Color(0.5f, 1f, 0.6f, 1f);
        [SerializeField] private Color matchFlashColor = new Color(0.4f, 1f, 0.6f, 1f);

        [Header("Colors - Combo (Dorado)")]
        [SerializeField] private Color comboGlowColor = new Color(1f, 0.85f, 0.2f, 1f);
        [SerializeField] private Color comboFlashColor = new Color(1f, 0.95f, 0.5f, 1f);

        [Header("Colors - Error (Rojo Fallo)")]
        [SerializeField] private Color errorFaceColor = new Color(0.35f, 0.05f, 0.05f, 1f);
        [SerializeField] private Color errorSideColor = new Color(0.25f, 0.03f, 0.03f, 1f);
        [SerializeField] private Color errorGlowColor = new Color(1f, 0.2f, 0.2f, 1f);
        [SerializeField] private Color errorSymbolColor = new Color(1f, 0.5f, 0.5f, 1f);

        [Header("Colors - Hover")]
        [SerializeField] private Color hoverGlowColor = new Color(1f, 0.6f, 1f, 0.8f);

        [Header("Shadow")]
        [SerializeField] private Color shadowColor = new Color(0f, 0f, 0f, 0.5f);
        [SerializeField] private Color shadowReducedColor = new Color(0f, 0f, 0f, 0.2f);

        [Header("Animation Settings")]
        [SerializeField] private float matchPulseScale = 1.2f;
        [SerializeField] private float errorShakeMagnitude = 12f;
        [SerializeField] private float errorShakeDuration = 0.35f;
        [SerializeField] private int errorShakeVibrations = 8;

        [Header("State")]
        [SerializeField] private bool isFaceUp = false;
        [SerializeField] private bool isMatched = false;
        [SerializeField] private bool isAnimating = false;

        // Card data
        private int cardId = -1;
        private string cardSymbol = "?";

        private Button button;
        private Vector2 originalPosition;
        private Vector3 originalScale;
        private Coroutine currentAnimation;
        private Coroutine glowPulseCoroutine;
        private Outline glowOutline;
        private Shadow innerShadow;
        private bool isHovering = false;

        // Events
        public event Action<Card3DEffect> OnCardFlipped;

        public int CardId => cardId;
        public bool IsFaceUp => isFaceUp;
        public bool IsMatched => isMatched;
        public bool IsAnimating => isAnimating;

        private void Awake()
        {
            button = GetComponent<Button>();

            if (cardFace != null)
            {
                originalPosition = cardFace.anchoredPosition;
                originalScale = cardFace.localScale;
                faceImage = cardFace.GetComponent<Image>();
                symbolText = cardFace.GetComponentInChildren<TextMeshProUGUI>();
            }

            glowOutline = cardFace?.GetComponent<Outline>();
            innerShadow = cardFace?.GetComponent<Shadow>();
        }

        private void Start()
        {
            SetFaceDown(false);
        }

        #region Public Methods

        public void SetupCard(int id, string symbol)
        {
            cardId = id;
            cardSymbol = symbol;

            if (symbolText != null)
            {
                symbolText.text = "?";
            }
        }

        public void SetCardSprite(Sprite sprite)
        {
            if (cardImage != null)
            {
                cardImage.sprite = sprite;
                cardImage.gameObject.SetActive(false);
            }
        }

        public Image CardImage => cardImage;

        public void ResetCard()
        {
            isFaceUp = false;
            isMatched = false;
            isAnimating = false;
            isHovering = false;

            StopAllCardCoroutines();

            if (cardFace != null)
            {
                cardFace.localScale = originalScale;
                cardFace.anchoredPosition = originalPosition;
                cardFace.localRotation = Quaternion.identity;
            }

            SetFaceDown(false);
            button.interactable = true;
        }

        public void FlipCard()
        {
            if (isFaceUp || isMatched || isAnimating) return;

            StopAllCardCoroutines();
            currentAnimation = StartCoroutine(AnimateFlipReveal());
        }

        public void FlipBack()
        {
            if (!isFaceUp || isMatched || isAnimating) return;

            StopAllCardCoroutines();
            currentAnimation = StartCoroutine(AnimateFlipHide());
        }

        private int currentComboLevel = 1;

        /// <summary>
        /// Marca la carta como matched con nivel de combo
        /// </summary>
        public void MarkAsMatched(int comboLevel = 1)
        {
            if (isMatched) return;

            isMatched = true;
            isFaceUp = true;
            button.interactable = false;
            currentComboLevel = Mathf.Clamp(comboLevel, 1, 5);

            StopAllCardCoroutines();
            currentAnimation = StartCoroutine(AnimateMatchSuccess());
        }

        /// <summary>
        /// Animación de celebración para victoria
        /// </summary>
        public void PlayVictoryCelebration(float delay)
        {
            StartCoroutine(AnimateVictoryCelebration(delay));
        }

        /// <summary>
        /// Obtiene la posición del centro de la carta en el canvas
        /// </summary>
        public Vector2 GetCardCenterPosition()
        {
            if (cardFace != null)
            {
                return cardFace.anchoredPosition;
            }
            return Vector2.zero;
        }

        public void ShowError()
        {
            StopAllCardCoroutines();
            currentAnimation = StartCoroutine(AnimateErrorFail());
        }

        public void AnimateGameStart()
        {
            StartCoroutine(AnimatePopUp());
        }

        public void SetWaitingState()
        {
            if (cardFace != null)
            {
                cardFace.anchoredPosition = originalPosition + new Vector2(0, -pressDepth);
                cardFace.localScale = originalScale * 0.9f;
            }

            if (sideImage != null)
            {
                RectTransform sideRT = sideImage.GetComponent<RectTransform>();
                sideRT.sizeDelta = new Vector2(sideRT.sizeDelta.x, 2f);
            }

            if (shadowImage != null)
                shadowImage.color = shadowReducedColor;

            if (faceImage != null)
                faceImage.color = new Color(0.02f, 0.01f, 0.04f, 1f);

            if (glowOutline != null)
                glowOutline.effectColor = new Color(0.3f, 0.05f, 0.3f, 0.15f);
        }

        #endregion

        #region Pointer Events

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!button.interactable || isMatched || isFaceUp || isAnimating) return;

            StopAllCardCoroutines();
            currentAnimation = StartCoroutine(AnimatePress());
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!button.interactable || isMatched || isAnimating) return;

            if (!isFaceUp)
            {
                FlipCard();
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!button.interactable || isMatched || isFaceUp || isAnimating) return;

            isHovering = true;

            // Hover glow effect
            if (glowOutline != null)
            {
                glowOutline.effectColor = hoverGlowColor;
            }

            // Slight lift
            if (cardFace != null && !isAnimating)
            {
                StopAllCardCoroutines();
                currentAnimation = StartCoroutine(AnimateHoverEnter());
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (isMatched || isFaceUp || isAnimating) return;

            isHovering = false;

            if (glowOutline != null)
            {
                glowOutline.effectColor = faceDownGlowColor;
            }

            if (cardFace != null && !isAnimating)
            {
                StopAllCardCoroutines();
                currentAnimation = StartCoroutine(AnimateHoverExit());
            }
        }

        #endregion

        #region State Methods

        private void SetFaceDown(bool animate)
        {
            if (symbolText != null)
            {
                symbolText.text = "?";
                symbolText.color = new Color(1f, 0f, 0.8f, 0.25f);
                symbolText.fontSize = 72;
            }

            if (cardImage != null)
                cardImage.gameObject.SetActive(false);

            if (cardBackPattern != null)
            {
                cardBackPattern.gameObject.SetActive(true);
                cardBackPattern.color = faceDownPatternColor;
            }

            if (faceImage != null)
                faceImage.color = faceDownColor;

            if (sideImage != null)
                sideImage.color = faceDownSideColor;

            if (glowOutline != null)
                glowOutline.effectColor = faceDownGlowColor;

            if (shadowImage != null)
                shadowImage.color = shadowColor;
        }

        private void SetFaceUp()
        {
            isFaceUp = true;

            if (symbolText != null)
            {
                symbolText.text = cardSymbol;
                symbolText.color = symbolColor;
                symbolText.fontSize = 90;
            }

            if (cardImage != null)
                cardImage.gameObject.SetActive(true);

            if (cardBackPattern != null)
                cardBackPattern.gameObject.SetActive(false);

            if (faceImage != null)
                faceImage.color = faceUpColor;

            if (sideImage != null)
                sideImage.color = faceUpSideColor;

            if (glowOutline != null)
                glowOutline.effectColor = faceUpGlowColor;
        }

        private void StopAllCardCoroutines()
        {
            if (currentAnimation != null)
            {
                StopCoroutine(currentAnimation);
                currentAnimation = null;
            }

            if (glowPulseCoroutine != null)
            {
                StopCoroutine(glowPulseCoroutine);
                glowPulseCoroutine = null;
            }
        }

        #endregion

        #region Animations

        private IEnumerator AnimateHoverEnter()
        {
            if (cardFace == null) yield break;

            float duration = 0.1f;
            float elapsed = 0f;

            Vector2 startPos = cardFace.anchoredPosition;
            Vector2 targetPos = originalPosition + new Vector2(0, 4f);
            Vector3 startScale = cardFace.localScale;
            Vector3 targetScale = originalScale * 1.03f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = EaseOutCubic(elapsed / duration);

                cardFace.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
                cardFace.localScale = Vector3.Lerp(startScale, targetScale, t);

                yield return null;
            }
        }

        private IEnumerator AnimateHoverExit()
        {
            if (cardFace == null) yield break;

            float duration = 0.15f;
            float elapsed = 0f;

            Vector2 startPos = cardFace.anchoredPosition;
            Vector3 startScale = cardFace.localScale;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = EaseOutCubic(elapsed / duration);

                cardFace.anchoredPosition = Vector2.Lerp(startPos, originalPosition, t);
                cardFace.localScale = Vector3.Lerp(startScale, originalScale, t);

                yield return null;
            }

            cardFace.anchoredPosition = originalPosition;
            cardFace.localScale = originalScale;
        }

        private IEnumerator AnimatePress()
        {
            if (cardFace == null) yield break;

            Vector2 startPos = cardFace.anchoredPosition;
            Vector2 targetPos = originalPosition + new Vector2(0, -pressDepth * 0.6f);
            Vector3 startScale = cardFace.localScale;
            Vector3 targetScale = originalScale * 0.97f;

            float elapsed = 0f;

            while (elapsed < pressDuration)
            {
                elapsed += Time.deltaTime;
                float t = EaseOutCubic(elapsed / pressDuration);

                cardFace.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
                cardFace.localScale = Vector3.Lerp(startScale, targetScale, t);

                if (sideImage != null)
                {
                    RectTransform sideRT = sideImage.GetComponent<RectTransform>();
                    float sideHeight = Mathf.Lerp(pressDepth, pressDepth * 0.4f, t);
                    sideRT.sizeDelta = new Vector2(sideRT.sizeDelta.x, sideHeight);
                }

                yield return null;
            }
        }

        private IEnumerator AnimateFlipReveal()
        {
            isAnimating = true;

            if (cardFace == null)
            {
                isAnimating = false;
                yield break;
            }

            float halfDuration = flipDuration * 0.5f;

            // ===== FASE 1: Voltear hacia afuera (escala X → 0) =====
            float elapsed = 0f;
            Vector3 startScale = cardFace.localScale;
            Vector2 startPos = cardFace.anchoredPosition;

            // Levantar carta mientras voltea
            Vector2 liftPos = originalPosition + new Vector2(0, 15f);

            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / halfDuration;
                float easeT = EaseInQuad(t);

                // Escala X hacia 0 (simula rotación)
                float scaleX = Mathf.Lerp(1f, 0.02f, easeT);
                cardFace.localScale = new Vector3(scaleX * originalScale.x, originalScale.y, 1f);

                // Levantar ligeramente
                cardFace.anchoredPosition = Vector2.Lerp(startPos, liftPos, EaseOutQuad(t));

                // Intensificar glow durante volteo
                if (glowOutline != null)
                {
                    Color glowLerp = Color.Lerp(faceDownGlowColor, faceUpGlowColor * 1.5f, t);
                    glowOutline.effectColor = glowLerp;
                }

                yield return null;
            }

            // ===== CAMBIAR A CARA ARRIBA =====
            SetFaceUp();

            // ===== FASE 2: Voltear hacia adentro (escala X → 1) con bounce =====
            elapsed = 0f;

            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / halfDuration;
                float easeT = EaseOutBack(t);

                // Escala X vuelve con overshoot
                float scaleX = Mathf.Lerp(0.02f, 1f, easeT);
                cardFace.localScale = new Vector3(scaleX * originalScale.x, originalScale.y * (1f + Mathf.Sin(t * Mathf.PI) * 0.05f), 1f);

                // Bajar a posición original
                cardFace.anchoredPosition = Vector2.Lerp(liftPos, originalPosition, EaseOutQuad(t));

                yield return null;
            }

            // Estado final
            cardFace.localScale = originalScale;
            cardFace.anchoredPosition = originalPosition;

            if (sideImage != null)
            {
                RectTransform sideRT = sideImage.GetComponent<RectTransform>();
                sideRT.sizeDelta = new Vector2(sideRT.sizeDelta.x, pressDepth);
            }

            isAnimating = false;
            OnCardFlipped?.Invoke(this);
        }

        private IEnumerator AnimateFlipHide()
        {
            isAnimating = true;

            if (cardFace == null)
            {
                isAnimating = false;
                yield break;
            }

            float halfDuration = flipDuration * 0.5f;

            // ===== FASE 1: Voltear hacia afuera =====
            float elapsed = 0f;
            Vector2 startPos = cardFace.anchoredPosition;
            Vector2 liftPos = originalPosition + new Vector2(0, 10f);

            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / halfDuration;
                float easeT = EaseInQuad(t);

                float scaleX = Mathf.Lerp(1f, 0.02f, easeT);
                cardFace.localScale = new Vector3(scaleX * originalScale.x, originalScale.y, 1f);
                cardFace.anchoredPosition = Vector2.Lerp(startPos, liftPos, EaseOutQuad(t * 0.5f));

                yield return null;
            }

            // ===== CAMBIAR A CARA ABAJO =====
            isFaceUp = false;
            SetFaceDown(false);

            // ===== FASE 2: Voltear hacia adentro =====
            elapsed = 0f;

            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / halfDuration;
                float easeT = EaseOutCubic(t);

                float scaleX = Mathf.Lerp(0.02f, 1f, easeT);
                cardFace.localScale = new Vector3(scaleX * originalScale.x, originalScale.y, 1f);
                cardFace.anchoredPosition = Vector2.Lerp(liftPos, originalPosition, easeT);

                yield return null;
            }

            cardFace.localScale = originalScale;
            cardFace.anchoredPosition = originalPosition;
            isAnimating = false;
        }

        /// <summary>
        /// Animación de MATCH - Verde brillante con pulso y glow
        /// Con intensidad basada en combo level
        /// </summary>
        private IEnumerator AnimateMatchSuccess()
        {
            isAnimating = true;

            if (cardFace == null)
            {
                isAnimating = false;
                yield break;
            }

            // Calcular intensidad basada en combo
            float comboMultiplier = 1f + (currentComboLevel - 1) * 0.15f;
            float currentPulseScale = matchPulseScale + (currentComboLevel - 1) * 0.08f;

            // Colores ajustados por combo (más dorado con combo alto)
            Color flashColor = Color.Lerp(matchFlashColor, comboFlashColor, (currentComboLevel - 1) * 0.25f);
            Color finalGlowColor = Color.Lerp(matchedGlowColor, comboGlowColor, (currentComboLevel - 1) * 0.3f);

            // ===== FLASH INICIAL BRILLANTE =====
            if (faceImage != null) faceImage.color = flashColor;
            if (glowOutline != null) glowOutline.effectColor = Color.white;
            if (symbolText != null) symbolText.color = Color.white;

            yield return new WaitForSeconds(0.05f);

            // ===== PULSO GRANDE (scale up) - más grande con combo =====
            float pulseDuration = 0.25f;
            float elapsed = 0f;

            Vector3 startScale = cardFace.localScale;
            Vector3 pulseScale = originalScale * currentPulseScale;
            Vector2 startPos = cardFace.anchoredPosition;
            Vector2 liftPos = originalPosition + new Vector2(0, 8f + currentComboLevel * 3f);

            while (elapsed < pulseDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / pulseDuration;
                float easeT = EaseOutBack(t);

                // Scale con bounce
                cardFace.localScale = Vector3.Lerp(startScale, pulseScale, easeT);
                cardFace.anchoredPosition = Vector2.Lerp(startPos, liftPos, EaseOutQuad(t));

                // Transición a verde/dorado
                float colorT = EaseOutQuad(t);
                if (faceImage != null)
                    faceImage.color = Color.Lerp(flashColor, matchedFaceColor, colorT);
                if (sideImage != null)
                    sideImage.color = Color.Lerp(faceUpSideColor, matchedSideColor, colorT);
                if (glowOutline != null)
                    glowOutline.effectColor = Color.Lerp(Color.white, finalGlowColor, colorT);
                if (symbolText != null)
                    symbolText.color = Color.Lerp(Color.white, matchedSymbolColor, colorT);

                yield return null;
            }

            // ===== COMBO EXTRA: Segundo pulso para combos altos =====
            if (currentComboLevel >= 3)
            {
                elapsed = 0f;
                float extraPulseDuration = 0.15f;
                Vector3 extraPulseScale = originalScale * (currentPulseScale * 0.95f);

                while (elapsed < extraPulseDuration)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / extraPulseDuration;

                    // Pulso adicional
                    float pulse = Mathf.Sin(t * Mathf.PI);
                    cardFace.localScale = Vector3.Lerp(pulseScale, extraPulseScale, pulse);

                    // Flash dorado extra
                    if (glowOutline != null)
                    {
                        Color glowPulse = Color.Lerp(finalGlowColor, comboGlowColor, pulse * 0.5f);
                        glowOutline.effectColor = glowPulse;
                    }

                    yield return null;
                }
            }

            // ===== BOUNCE BACK =====
            elapsed = 0f;
            float settleDuration = 0.2f;

            while (elapsed < settleDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / settleDuration;
                float easeT = EaseOutBounce(t);

                cardFace.localScale = Vector3.Lerp(pulseScale, originalScale, easeT);
                cardFace.anchoredPosition = Vector2.Lerp(liftPos, originalPosition, EaseOutQuad(t));

                yield return null;
            }

            // ===== ESTADO FINAL =====
            cardFace.localScale = originalScale;
            cardFace.anchoredPosition = originalPosition + new Vector2(0, -pressDepth * 0.25f);

            if (sideImage != null)
            {
                RectTransform sideRT = sideImage.GetComponent<RectTransform>();
                sideRT.sizeDelta = new Vector2(sideRT.sizeDelta.x, pressDepth * 0.75f);
            }

            // Colores finales
            if (faceImage != null) faceImage.color = matchedFaceColor;
            if (sideImage != null) sideImage.color = matchedSideColor;
            if (glowOutline != null) glowOutline.effectColor = finalGlowColor;
            if (symbolText != null) symbolText.color = matchedSymbolColor;

            isAnimating = false;

            // Iniciar pulso continuo con intensidad de combo
            glowPulseCoroutine = StartCoroutine(AnimateMatchedGlowPulse());
        }

        /// <summary>
        /// Pulso continuo sutil para cartas matched
        /// </summary>
        private IEnumerator AnimateMatchedGlowPulse()
        {
            while (isMatched && glowOutline != null)
            {
                float t = (Mathf.Sin(Time.time * 3f) + 1f) * 0.5f;
                Color pulseColor = Color.Lerp(matchedGlowColor * 0.7f, matchedGlowColor, t);
                glowOutline.effectColor = pulseColor;

                yield return null;
            }
        }

        /// <summary>
        /// Animación de ERROR - Rojo con shake intenso
        /// </summary>
        private IEnumerator AnimateErrorFail()
        {
            isAnimating = true;

            if (cardFace == null)
            {
                isAnimating = false;
                yield break;
            }

            // Guardar colores originales
            Color originalFace = faceImage != null ? faceImage.color : faceUpColor;
            Color originalSide = sideImage != null ? sideImage.color : faceUpSideColor;
            Color originalGlow = glowOutline != null ? glowOutline.effectColor : faceUpGlowColor;
            Color originalSymbol = symbolText != null ? symbolText.color : symbolColor;

            // ===== FLASH ROJO INTENSO =====
            if (faceImage != null) faceImage.color = errorFaceColor;
            if (sideImage != null) sideImage.color = errorSideColor;
            if (glowOutline != null) glowOutline.effectColor = errorGlowColor;
            if (symbolText != null) symbolText.color = errorSymbolColor;

            // ===== SHAKE INTENSO =====
            Vector2 startPos = cardFace.anchoredPosition;
            float elapsed = 0f;

            while (elapsed < errorShakeDuration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / errorShakeDuration;

                // Shake que disminuye
                float currentMagnitude = errorShakeMagnitude * (1f - progress);

                // Vibración rápida
                float shakeX = Mathf.Sin(elapsed * errorShakeVibrations * Mathf.PI * 2f) * currentMagnitude;
                float shakeY = Mathf.Cos(elapsed * errorShakeVibrations * 0.7f * Mathf.PI * 2f) * currentMagnitude * 0.3f;

                cardFace.anchoredPosition = startPos + new Vector2(shakeX, shakeY);

                // Pulso de color rojo durante shake
                float colorPulse = (Mathf.Sin(elapsed * 20f) + 1f) * 0.5f;
                if (faceImage != null)
                    faceImage.color = Color.Lerp(errorFaceColor, errorFaceColor * 1.3f, colorPulse);

                yield return null;
            }

            cardFace.anchoredPosition = startPos;

            // ===== PAUSA BREVE MOSTRANDO ERROR =====
            yield return new WaitForSeconds(0.25f);

            // ===== TRANSICIÓN GRADUAL A COLORES ORIGINALES =====
            float fadeDuration = 0.2f;
            elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = EaseOutQuad(elapsed / fadeDuration);

                if (faceImage != null)
                    faceImage.color = Color.Lerp(errorFaceColor, originalFace, t);
                if (sideImage != null)
                    sideImage.color = Color.Lerp(errorSideColor, originalSide, t);
                if (glowOutline != null)
                    glowOutline.effectColor = Color.Lerp(errorGlowColor, originalGlow, t);
                if (symbolText != null)
                    symbolText.color = Color.Lerp(errorSymbolColor, originalSymbol, t);

                yield return null;
            }

            isAnimating = false;

            // ===== VOLTEAR DE VUELTA =====
            yield return StartCoroutine(AnimateFlipHide());
        }

        private IEnumerator AnimatePopUp()
        {
            if (cardFace == null) yield break;

            isAnimating = true;

            // Estado inicial: presionado y pequeño
            Vector2 pressedPos = originalPosition + new Vector2(0, -pressDepth * 1.2f);
            cardFace.anchoredPosition = pressedPos;
            cardFace.localScale = originalScale * 0.7f;

            if (sideImage != null)
            {
                RectTransform sideRT = sideImage.GetComponent<RectTransform>();
                sideRT.sizeDelta = new Vector2(sideRT.sizeDelta.x, 1f);
            }

            if (shadowImage != null)
                shadowImage.color = new Color(0f, 0f, 0f, 0.1f);

            if (faceImage != null)
                faceImage.color = new Color(0.02f, 0.01f, 0.03f, 1f);

            // Delay aleatorio para efecto de ola
            yield return new WaitForSeconds(UnityEngine.Random.Range(0f, 0.2f));

            // ===== POP UP CON BOUNCE =====
            float duration = 0.4f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float bounceT = EaseOutBack(t);
                float posT = EaseOutQuad(t);

                // Posición
                cardFace.anchoredPosition = Vector2.Lerp(pressedPos, originalPosition, posT);

                // Escala con bounce
                cardFace.localScale = Vector3.Lerp(originalScale * 0.7f, originalScale, bounceT);

                // Side height
                if (sideImage != null)
                {
                    RectTransform sideRT = sideImage.GetComponent<RectTransform>();
                    float sideHeight = Mathf.Lerp(1f, pressDepth, posT);
                    sideRT.sizeDelta = new Vector2(sideRT.sizeDelta.x, sideHeight);
                }

                // Colores
                float colorT = EaseOutQuad(t);
                if (faceImage != null)
                    faceImage.color = Color.Lerp(new Color(0.02f, 0.01f, 0.03f, 1f), faceDownColor, colorT);
                if (shadowImage != null)
                    shadowImage.color = Color.Lerp(new Color(0f, 0f, 0f, 0.1f), shadowColor, colorT);
                if (glowOutline != null)
                    glowOutline.effectColor = Color.Lerp(new Color(0.2f, 0.05f, 0.2f, 0.1f), faceDownGlowColor, colorT);

                yield return null;
            }

            // Estado final
            cardFace.anchoredPosition = originalPosition;
            cardFace.localScale = originalScale;

            if (shadowImage != null)
                shadowImage.color = shadowColor;

            isAnimating = false;
        }

        #endregion

        /// <summary>
        /// Animación de celebración de victoria - "ola" de cartas
        /// </summary>
        private IEnumerator AnimateVictoryCelebration(float delay)
        {
            yield return new WaitForSeconds(delay);

            if (cardFace == null || !isMatched) yield break;

            // Guardar estado inicial
            Vector2 startPos = cardFace.anchoredPosition;
            Vector3 startScale = cardFace.localScale;
            Color startGlow = glowOutline != null ? glowOutline.effectColor : matchedGlowColor;

            // ===== SALTO DE CELEBRACIÓN =====
            float jumpDuration = 0.4f;
            float elapsed = 0f;
            float jumpHeight = 30f;

            while (elapsed < jumpDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / jumpDuration;

                // Salto parabólico
                float jumpT = Mathf.Sin(t * Mathf.PI);
                cardFace.anchoredPosition = startPos + new Vector2(0, jumpHeight * jumpT);

                // Escala con bounce
                float scaleT = 1f + jumpT * 0.15f;
                cardFace.localScale = startScale * scaleT;

                // Glow intenso durante salto
                if (glowOutline != null)
                {
                    Color celebrationGlow = Color.Lerp(startGlow, comboGlowColor, jumpT);
                    glowOutline.effectColor = celebrationGlow;
                }

                // Rotación leve
                cardFace.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(t * Mathf.PI * 2f) * 5f);

                yield return null;
            }

            // ===== VOLVER A POSICIÓN ORIGINAL =====
            cardFace.anchoredPosition = startPos;
            cardFace.localScale = startScale;
            cardFace.localRotation = Quaternion.identity;
            if (glowOutline != null) glowOutline.effectColor = startGlow;
        }

        #region Easing Functions

        private float EaseOutCubic(float t) => 1f - Mathf.Pow(1f - t, 3f);
        private float EaseOutQuad(float t) => 1f - (1f - t) * (1f - t);
        private float EaseInQuad(float t) => t * t;

        private float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        }

        private float EaseOutBounce(float t)
        {
            const float n1 = 7.5625f;
            const float d1 = 2.75f;

            if (t < 1f / d1)
                return n1 * t * t;
            else if (t < 2f / d1)
                return n1 * (t -= 1.5f / d1) * t + 0.75f;
            else if (t < 2.5f / d1)
                return n1 * (t -= 2.25f / d1) * t + 0.9375f;
            else
                return n1 * (t -= 2.625f / d1) * t + 0.984375f;
        }

        #endregion
    }
}
