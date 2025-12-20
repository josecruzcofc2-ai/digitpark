using UnityEngine;
using TMPro;
using System.Collections;
using DigitPark.Localization;

namespace DigitPark.UI
{
    /// <summary>
    /// Script que fuerza la visibilidad de los labels de Settings
    /// Agregar al Canvas de la escena Settings
    /// </summary>
    public class ForceShowSettingsLabels : MonoBehaviour
    {
        [Header("Referencias (se buscan automaticamente si estan vacias)")]
        [SerializeField] private TextMeshProUGUI changeStyleText;
        [SerializeField] private TextMeshProUGUI changeLanguageText;

        private void Start()
        {
            StartCoroutine(ForceShowLabels());
        }

        private IEnumerator ForceShowLabels()
        {
            // Esperar varios frames para que todo se inicialice
            yield return new WaitForSeconds(0.3f);

            Debug.Log("[ForceShowLabels] Buscando y forzando visibilidad de labels...");

            // Buscar los textos si no estan asignados
            if (changeStyleText == null)
            {
                changeStyleText = FindTextByName("ChangeStyleText");
            }

            if (changeLanguageText == null)
            {
                changeLanguageText = FindTextByName("ChangeLanguageText");
            }

            // Forzar visibilidad de ChangeStyleText
            if (changeStyleText != null)
            {
                ForceTextVisible(changeStyleText, "Cambiar Estilo", "change_style");
            }
            else
            {
                Debug.LogError("[ForceShowLabels] NO SE ENCONTRO ChangeStyleText!");
            }

            // Forzar visibilidad de ChangeLanguageText
            if (changeLanguageText != null)
            {
                ForceTextVisible(changeLanguageText, "Cambiar Idioma", "change_language");
            }
            else
            {
                Debug.LogError("[ForceShowLabels] NO SE ENCONTRO ChangeLanguageText!");
            }

            Debug.Log("[ForceShowLabels] Proceso completado");
        }

        private TextMeshProUGUI FindTextByName(string name)
        {
            // Buscar en toda la escena, incluyendo objetos inactivos
            var allTMP = FindObjectsOfType<TextMeshProUGUI>(true);

            foreach (var tmp in allTMP)
            {
                if (tmp.gameObject.name == name)
                {
                    Debug.Log($"[ForceShowLabels] Encontrado {name}");
                    return tmp;
                }
            }

            Debug.LogWarning($"[ForceShowLabels] No se encontro ningún TextMeshProUGUI llamado '{name}'");
            return null;
        }

        private void ForceTextVisible(TextMeshProUGUI tmp, string fallbackText, string localizationKey)
        {
            Debug.Log($"[ForceShowLabels] Procesando {tmp.gameObject.name}...");

            // 1. Asegurar que el GameObject y todos sus padres esten activos
            tmp.gameObject.SetActive(true);
            Transform parent = tmp.transform.parent;
            while (parent != null)
            {
                parent.gameObject.SetActive(true);
                parent = parent.parent;
            }

            // 2. Asegurar que el componente TMP este habilitado
            tmp.enabled = true;

            // 3. Asegurar que el layer sea UI (5)
            tmp.gameObject.layer = 5;

            // 4. Si el texto esta vacio, poner el fallback
            if (string.IsNullOrEmpty(tmp.text))
            {
                // Intentar obtener texto localizado
                if (LocalizationManager.Instance != null)
                {
                    tmp.text = LocalizationManager.Instance.GetText(localizationKey);
                }

                // Si aun esta vacio, usar fallback
                if (string.IsNullOrEmpty(tmp.text))
                {
                    tmp.text = fallbackText;
                }
                Debug.Log($"[ForceShowLabels] Texto estaba vacio, ahora: '{tmp.text}'");
            }

            // 5. Asegurar color visible (cyan brillante)
            if (tmp.color.a < 0.1f || (tmp.color.r < 0.1f && tmp.color.g < 0.1f && tmp.color.b < 0.1f))
            {
                tmp.color = new Color(0f, 1f, 1f, 1f); // Cyan
                Debug.Log($"[ForceShowLabels] Color era invisible, cambiado a cyan");
            }

            // 6. Asegurar font size razonable
            if (tmp.fontSize < 10)
            {
                tmp.fontSize = 28;
                Debug.Log($"[ForceShowLabels] Font size era muy pequeno, cambiado a 28");
            }

            // 7. Asegurar que tenga fuente asignada
            if (tmp.font == null)
            {
                tmp.font = TMP_Settings.defaultFontAsset;
                Debug.Log($"[ForceShowLabels] Fuente era null, asignada fuente por defecto");
            }

            // 8. Verificar CanvasRenderer
            var canvasRenderer = tmp.GetComponent<CanvasRenderer>();
            if (canvasRenderer == null)
            {
                canvasRenderer = tmp.gameObject.AddComponent<CanvasRenderer>();
                Debug.Log($"[ForceShowLabels] CanvasRenderer no existia, creado");
            }
            canvasRenderer.cull = false;

            // 9. Forzar rebuild del texto
            tmp.ForceMeshUpdate();

            Debug.Log($"[ForceShowLabels] {tmp.gameObject.name} configurado:");
            Debug.Log($"  - Texto: '{tmp.text}'");
            Debug.Log($"  - Color: {tmp.color}");
            Debug.Log($"  - Activo: {tmp.gameObject.activeInHierarchy}");
            Debug.Log($"  - Enabled: {tmp.enabled}");
            Debug.Log($"  - Layer: {tmp.gameObject.layer}");
            Debug.Log($"  - Font: {(tmp.font != null ? tmp.font.name : "NULL")}");
        }
    }
}
