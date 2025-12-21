using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

namespace DigitPark.Effects
{
    /// <summary>
    /// Sistema de texto flotante para mostrar puntos, combos, mensajes
    /// +100, COMBO x3!, PERFECT!, etc.
    /// </summary>
    public class FloatingText : MonoBehaviour
    {
        public static FloatingText Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private float defaultDuration = 1.5f;
        [SerializeField] private float floatSpeed = 100f;
        [SerializeField] private float fadeStartPercent = 0.6f;

        [Header("Colors")]
        [SerializeField] private Color pointsColor = new Color(0f, 0.9608f, 1f, 1f);    // Cyan
        [SerializeField] private Color comboColor = new Color(1f, 0.84f, 0f, 1f);        // Gold
        [SerializeField] private Color perfectColor = new Color(0.2f, 1f, 0.4f, 1f);     // Green
        [SerializeField] private Color errorColor = new Color(1f, 0.3f, 0.3f, 1f);       // Red
        [SerializeField] private Color bonusColor = new Color(0.6157f, 0.2941f, 1f, 1f); // Purple

        private Canvas canvas;
        private Queue<GameObject> textPool = new Queue<GameObject>();
        private Transform container;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            // Buscar o crear canvas
            canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                Debug.LogWarning("[FloatingText] No se encontro Canvas");
            }

            // Crear contenedor
            container = new GameObject("FloatingTextContainer").transform;
            container.SetParent(transform);
        }

        #region Public Methods

        /// <summary>
        /// Muestra puntos flotantes (+100, +500, etc)
        /// </summary>
        public void ShowPoints(int points, Vector3 worldPosition)
        {
            string text = points >= 0 ? $"+{points}" : $"{points}";
            Color color = points >= 0 ? pointsColor : errorColor;
            SpawnFloatingText(text, worldPosition, color, FloatType.Points);
        }

        /// <summary>
        /// Muestra combo (COMBO x2!, COMBO x3!, etc)
        /// </summary>
        public void ShowCombo(int comboCount, Vector3 worldPosition)
        {
            string text = $"COMBO x{comboCount}!";
            float scale = 1f + (comboCount - 1) * 0.1f;
            SpawnFloatingText(text, worldPosition, comboColor, FloatType.Combo, scale);
        }

        /// <summary>
        /// Muestra mensaje de perfect
        /// </summary>
        public void ShowPerfect(Vector3 worldPosition)
        {
            SpawnFloatingText("PERFECT!", worldPosition, perfectColor, FloatType.Special, 1.3f);
        }

        /// <summary>
        /// Muestra mensaje de excelente
        /// </summary>
        public void ShowExcellent(Vector3 worldPosition)
        {
            SpawnFloatingText("EXCELLENT!", worldPosition, comboColor, FloatType.Special, 1.4f);
        }

        /// <summary>
        /// Muestra mensaje de nuevo record
        /// </summary>
        public void ShowNewRecord(Vector3 worldPosition)
        {
            SpawnFloatingText("NEW RECORD!", worldPosition, comboColor, FloatType.Record, 1.5f);
        }

        /// <summary>
        /// Muestra tiempo bonus
        /// </summary>
        public void ShowTimeBonus(float seconds, Vector3 worldPosition)
        {
            string text = $"+{seconds:F1}s";
            SpawnFloatingText(text, worldPosition, bonusColor, FloatType.Bonus);
        }

        /// <summary>
        /// Muestra mensaje de error
        /// </summary>
        public void ShowError(string message, Vector3 worldPosition)
        {
            SpawnFloatingText(message, worldPosition, errorColor, FloatType.Error);
        }

        /// <summary>
        /// Muestra texto personalizado
        /// </summary>
        public void ShowCustom(string text, Vector3 worldPosition, Color color, float scale = 1f)
        {
            SpawnFloatingText(text, worldPosition, color, FloatType.Custom, scale);
        }

        #endregion

        #region Private Methods

        private enum FloatType
        {
            Points,
            Combo,
            Special,
            Record,
            Bonus,
            Error,
            Custom
        }

        private void SpawnFloatingText(string text, Vector3 worldPosition, Color color, FloatType type, float scale = 1f)
        {
            if (canvas == null) return;

            // Obtener o crear objeto de texto
            GameObject textObj = GetTextObject();
            textObj.transform.SetParent(canvas.transform, false);

            // Configurar posicion
            RectTransform rt = textObj.GetComponent<RectTransform>();

            // Convertir posicion mundial a posicion de canvas
            Vector2 screenPos = Camera.main.WorldToScreenPoint(worldPosition);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                screenPos,
                canvas.worldCamera,
                out Vector2 localPos
            );

            rt.anchoredPosition = localPos;
            rt.localScale = Vector3.one * scale;

            // Configurar texto
            TextMeshProUGUI tmp = textObj.GetComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.color = color;
            tmp.fontSize = GetFontSize(type);
            tmp.fontStyle = FontStyles.Bold;

            // Agregar outline si no existe
            Outline outline = textObj.GetComponent<Outline>();
            if (outline == null)
            {
                outline = textObj.AddComponent<Outline>();
            }
            outline.effectColor = new Color(0f, 0f, 0f, 0.8f);
            outline.effectDistance = new Vector2(2, 2);

            textObj.SetActive(true);

            // Iniciar animacion
            StartCoroutine(AnimateFloatingText(textObj, rt, tmp, type));
        }

        private float GetFontSize(FloatType type)
        {
            return type switch
            {
                FloatType.Points => 36f,
                FloatType.Combo => 42f,
                FloatType.Special => 48f,
                FloatType.Record => 56f,
                FloatType.Bonus => 32f,
                FloatType.Error => 28f,
                FloatType.Custom => 36f,
                _ => 36f
            };
        }

        private IEnumerator AnimateFloatingText(GameObject obj, RectTransform rt, TextMeshProUGUI tmp, FloatType type)
        {
            float duration = GetDuration(type);
            float elapsed = 0f;

            Vector2 startPos = rt.anchoredPosition;
            Vector3 startScale = rt.localScale;
            Color startColor = tmp.color;

            // Animacion inicial - escala punch
            float punchDuration = 0.15f;
            float punchElapsed = 0f;

            while (punchElapsed < punchDuration)
            {
                punchElapsed += Time.deltaTime;
                float t = punchElapsed / punchDuration;

                // Punch in then out
                float punchScale;
                if (t < 0.5f)
                {
                    punchScale = Mathf.Lerp(0.5f, 1.3f, t * 2f);
                }
                else
                {
                    punchScale = Mathf.Lerp(1.3f, 1f, (t - 0.5f) * 2f);
                }

                rt.localScale = startScale * punchScale;
                yield return null;
            }

            rt.localScale = startScale;

            // Animacion principal - flotar y desaparecer
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                // Movimiento hacia arriba con desaceleracion
                float yOffset = floatSpeed * t * (1f - t * 0.5f);

                // Movimiento horizontal sutil (wobble)
                float xOffset = Mathf.Sin(t * Mathf.PI * 4f) * 10f * (1f - t);

                rt.anchoredPosition = startPos + new Vector2(xOffset, yOffset);

                // Fade out en la parte final
                if (t > fadeStartPercent)
                {
                    float fadeT = (t - fadeStartPercent) / (1f - fadeStartPercent);
                    Color color = startColor;
                    color.a = Mathf.Lerp(1f, 0f, fadeT);
                    tmp.color = color;
                }

                // Escala que reduce ligeramente al final
                if (t > 0.7f)
                {
                    float scaleT = (t - 0.7f) / 0.3f;
                    rt.localScale = startScale * Mathf.Lerp(1f, 0.8f, scaleT);
                }

                yield return null;
            }

            // Devolver al pool
            ReturnTextObject(obj);
        }

        private float GetDuration(FloatType type)
        {
            return type switch
            {
                FloatType.Record => 2.5f,
                FloatType.Special => 2f,
                FloatType.Combo => 1.8f,
                _ => defaultDuration
            };
        }

        #endregion

        #region Object Pool

        private GameObject GetTextObject()
        {
            if (textPool.Count > 0)
            {
                return textPool.Dequeue();
            }

            return CreateTextObject();
        }

        private GameObject CreateTextObject()
        {
            GameObject obj = new GameObject("FloatingText");

            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(300, 100);

            TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.enableWordWrapping = false;
            tmp.raycastTarget = false;

            // Shadow para mejor legibilidad
            Shadow shadow = obj.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.5f);
            shadow.effectDistance = new Vector2(3, -3);

            return obj;
        }

        private void ReturnTextObject(GameObject obj)
        {
            obj.SetActive(false);
            obj.transform.SetParent(container);
            textPool.Enqueue(obj);
        }

        #endregion
    }
}
