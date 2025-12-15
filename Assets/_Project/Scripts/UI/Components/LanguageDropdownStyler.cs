using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using DigitPark.UI.Theme;

namespace DigitPark.UI.Components
{
    /// <summary>
    /// Aplica estilos visuales mejorados al dropdown de idioma
    /// Incluye: icono de globo, color cyan, borde, fondo oscuro y efectos hover
    /// </summary>
    [RequireComponent(typeof(TMP_Dropdown))]
    public class LanguageDropdownStyler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Referencias")]
        [SerializeField] private TextMeshProUGUI changeLangLabel;
        [SerializeField] private Image dropdownBackground;
        [SerializeField] private Image dropdownBorder;

        [Header("Colores")]
        [SerializeField] private Color cyanColor = new Color(0f, 0.9608f, 1f, 1f); // #00F5FF
        [SerializeField] private Color bgColor = new Color(0.02f, 0.08f, 0.16f, 0.9f);
        [SerializeField] private Color borderColor = new Color(0f, 0.9608f, 1f, 0.6f);
        [SerializeField] private Color hoverBorderColor = new Color(0f, 0.9608f, 1f, 1f);

        [Header("Configuracion")]
        [SerializeField] private Image languageIconImage;

        private TMP_Dropdown dropdown;
        private Image dropdownImage;
        private Outline dropdownOutline;
        private Shadow dropdownGlow;
        private Color originalLabelColor;

        private void Awake()
        {
            dropdown = GetComponent<TMP_Dropdown>();
            dropdownImage = GetComponent<Image>();
        }

        private void Start()
        {
            ApplyStyles();
        }

        /// <summary>
        /// Aplica todos los estilos al dropdown y label
        /// </summary>
        public void ApplyStyles()
        {
            StyleChangeLangLabel();
            StyleDropdownBackground();
            StyleDropdownBorder();
            StyleDropdownArrow();
            StyleDropdownItems();
        }

        /// <summary>
        /// Estiliza el label "Change Language" con color cyan
        /// </summary>
        private void StyleChangeLangLabel()
        {
            if (changeLangLabel == null)
            {
                // Intentar encontrar el label automáticamente
                Transform parent = transform.parent;
                if (parent != null)
                {
                    // Primero buscar en LanguageIconContainer si existe
                    Transform container = parent.Find("LanguageIconContainer");
                    if (container != null)
                    {
                        foreach (Transform child in container)
                        {
                            var tmp = child.GetComponent<TextMeshProUGUI>();
                            if (tmp != null)
                            {
                                changeLangLabel = tmp;
                                break;
                            }
                        }

                        // Buscar el icono tambien
                        if (languageIconImage == null)
                        {
                            Transform iconTransform = container.Find("LanguageIcon");
                            if (iconTransform != null)
                            {
                                languageIconImage = iconTransform.GetComponent<Image>();
                            }
                        }
                    }

                    // Si no encontramos en el container, buscar directamente
                    if (changeLangLabel == null)
                    {
                        foreach (Transform child in parent)
                        {
                            var tmp = child.GetComponent<TextMeshProUGUI>();
                            if (tmp != null && child.gameObject != gameObject)
                            {
                                string text = tmp.text.ToLower();
                                if (text.Contains("language") || text.Contains("idioma") || text.Contains("change"))
                                {
                                    changeLangLabel = tmp;
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            if (changeLangLabel != null)
            {
                originalLabelColor = changeLangLabel.color;

                // Aplicar color cyan
                changeLangLabel.color = cyanColor;

                // Hacer bold
                changeLangLabel.fontStyle = FontStyles.Bold;

                Debug.Log($"[LanguageStyler] Label estilizado: {changeLangLabel.text}");
            }

            // Estilizar el icono si existe
            if (languageIconImage != null)
            {
                languageIconImage.color = cyanColor;
            }
        }

        /// <summary>
        /// Aplica fondo oscuro semi-transparente al dropdown
        /// </summary>
        private void StyleDropdownBackground()
        {
            if (dropdownImage != null)
            {
                dropdownImage.color = bgColor;
            }

            // Si hay una imagen de fondo separada
            if (dropdownBackground != null)
            {
                dropdownBackground.color = bgColor;
            }
        }

        /// <summary>
        /// Agrega borde cyan al dropdown
        /// </summary>
        private void StyleDropdownBorder()
        {
            // Agregar Outline component para simular borde
            dropdownOutline = GetComponent<Outline>();
            if (dropdownOutline == null)
            {
                dropdownOutline = gameObject.AddComponent<Outline>();
            }

            dropdownOutline.effectColor = borderColor;
            dropdownOutline.effectDistance = new Vector2(2f, 2f);
            dropdownOutline.useGraphicAlpha = false;

            // Agregar Shadow para efecto glow sutil
            dropdownGlow = GetComponent<Shadow>();
            if (dropdownGlow == null && GetComponents<Shadow>().Length < 2)
            {
                dropdownGlow = gameObject.AddComponent<Shadow>();
            }

            if (dropdownGlow != null)
            {
                dropdownGlow.effectColor = new Color(cyanColor.r, cyanColor.g, cyanColor.b, 0.3f);
                dropdownGlow.effectDistance = new Vector2(4f, -4f);
            }
        }

        /// <summary>
        /// Estiliza la flecha del dropdown
        /// </summary>
        private void StyleDropdownArrow()
        {
            // Buscar el Arrow dentro del dropdown
            Transform arrow = transform.Find("Arrow");
            if (arrow == null)
            {
                // Buscar en Template
                Transform template = transform.Find("Template");
                if (template != null)
                {
                    arrow = template.Find("Arrow");
                }
            }

            if (arrow != null)
            {
                Image arrowImage = arrow.GetComponent<Image>();
                if (arrowImage != null)
                {
                    arrowImage.color = cyanColor;
                }
            }

            // Tambien estilizar el texto seleccionado
            Transform label = transform.Find("Label");
            if (label != null)
            {
                TextMeshProUGUI labelTMP = label.GetComponent<TextMeshProUGUI>();
                if (labelTMP != null)
                {
                    labelTMP.color = cyanColor;
                }
            }
        }

        /// <summary>
        /// Estiliza los items del dropdown cuando se abre
        /// </summary>
        private void StyleDropdownItems()
        {
            if (dropdown == null) return;

            // Configurar colores del dropdown
            ColorBlock colors = dropdown.colors;
            colors.normalColor = bgColor;
            colors.highlightedColor = new Color(cyanColor.r * 0.3f, cyanColor.g * 0.3f, cyanColor.b * 0.3f, 0.8f);
            colors.pressedColor = new Color(cyanColor.r * 0.5f, cyanColor.g * 0.5f, cyanColor.b * 0.5f, 0.9f);
            colors.selectedColor = colors.highlightedColor;
            dropdown.colors = colors;

            // Estilizar el template si existe
            Transform template = transform.Find("Template");
            if (template != null)
            {
                Image templateBg = template.GetComponent<Image>();
                if (templateBg != null)
                {
                    templateBg.color = new Color(0.03f, 0.06f, 0.12f, 0.95f);
                }

                // Agregar borde al template
                Outline templateOutline = template.GetComponent<Outline>();
                if (templateOutline == null)
                {
                    templateOutline = template.gameObject.AddComponent<Outline>();
                }
                templateOutline.effectColor = borderColor;
                templateOutline.effectDistance = new Vector2(1.5f, 1.5f);

                // Estilizar items
                Transform viewport = template.Find("Viewport");
                if (viewport != null)
                {
                    Transform content = viewport.Find("Content");
                    if (content != null)
                    {
                        Transform item = content.Find("Item");
                        if (item != null)
                        {
                            // Item Background
                            Image itemBg = item.GetComponent<Image>();
                            if (itemBg != null)
                            {
                                itemBg.color = Color.clear;
                            }

                            // Item Label
                            Transform itemLabel = item.Find("Item Label");
                            if (itemLabel != null)
                            {
                                TextMeshProUGUI itemTMP = itemLabel.GetComponent<TextMeshProUGUI>();
                                if (itemTMP != null)
                                {
                                    itemTMP.color = new Color(0.9f, 0.95f, 1f, 1f);
                                }
                            }

                            // Item Checkmark
                            Transform checkmark = item.Find("Item Checkmark");
                            if (checkmark != null)
                            {
                                Image checkImage = checkmark.GetComponent<Image>();
                                if (checkImage != null)
                                {
                                    checkImage.color = cyanColor;
                                }
                            }
                        }
                    }
                }
            }
        }

        #region Hover Effects

        public void OnPointerEnter(PointerEventData eventData)
        {
            ApplyHoverEffect(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            ApplyHoverEffect(false);
        }

        private void ApplyHoverEffect(bool hover)
        {
            if (dropdownOutline != null)
            {
                dropdownOutline.effectColor = hover ? hoverBorderColor : borderColor;
                dropdownOutline.effectDistance = hover ? new Vector2(3f, 3f) : new Vector2(2f, 2f);
            }

            if (dropdownGlow != null)
            {
                float alpha = hover ? 0.5f : 0.3f;
                dropdownGlow.effectColor = new Color(cyanColor.r, cyanColor.g, cyanColor.b, alpha);
                dropdownGlow.effectDistance = hover ? new Vector2(6f, -6f) : new Vector2(4f, -4f);
            }

            // Escalar ligeramente el dropdown
            transform.localScale = hover ? Vector3.one * 1.02f : Vector3.one;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Establece el color del tema manualmente
        /// </summary>
        public void SetThemeColor(Color newCyanColor)
        {
            cyanColor = newCyanColor;
            borderColor = new Color(newCyanColor.r, newCyanColor.g, newCyanColor.b, 0.6f);
            hoverBorderColor = newCyanColor;
            ApplyStyles();
        }

        /// <summary>
        /// Establece el sprite del icono de idioma
        /// </summary>
        public void SetLanguageIcon(Sprite iconSprite)
        {
            if (languageIconImage != null && iconSprite != null)
            {
                languageIconImage.sprite = iconSprite;
                languageIconImage.color = cyanColor;
            }
        }

        #endregion

#if UNITY_EDITOR
        [ContextMenu("Apply Styles Now")]
        private void ApplyStylesEditor()
        {
            dropdown = GetComponent<TMP_Dropdown>();
            dropdownImage = GetComponent<Image>();
            ApplyStyles();
        }
#endif
    }
}
