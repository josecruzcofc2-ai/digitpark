using UnityEngine;
using TMPro;
using System.Collections;

namespace DigitPark.Debugging
{
    /// <summary>
    /// Debug en runtime para encontrar por que los textos no aparecen
    /// Agrega este script a cualquier objeto en la escena Settings
    /// </summary>
    public class SettingsTextRuntimeDebug : MonoBehaviour
    {
        private void Start()
        {
            StartCoroutine(DebugAfterDelay());
        }

        private IEnumerator DebugAfterDelay()
        {
            // Esperar a que todo se inicialice
            yield return new WaitForSeconds(0.5f);

            UnityEngine.Debug.Log("========== DEBUG RUNTIME TEXTOS SETTINGS ==========");

            // Buscar ChangeStyleText
            DebugFindText("ChangeStyleText");
            DebugFindText("ChangeLanguageText");

            // Buscar todos los TextMeshProUGUI en la escena
            UnityEngine.Debug.Log("\n--- TODOS LOS TEXTMESHPRO EN ESCENA ---");
            var allTMP = FindObjectsOfType<TextMeshProUGUI>(true);
            UnityEngine.Debug.Log($"Total TextMeshProUGUI encontrados: {allTMP.Length}");

            foreach (var tmp in allTMP)
            {
                string parentName = tmp.transform.parent != null ? tmp.transform.parent.name : "null";
                if (tmp.gameObject.name.Contains("Change") || tmp.gameObject.name.Contains("Style") || tmp.gameObject.name.Contains("Language"))
                {
                    UnityEngine.Debug.Log($"  RELEVANTE -> '{tmp.gameObject.name}' texto='{tmp.text}' color={tmp.color} activo={tmp.gameObject.activeInHierarchy} padre={parentName}");
                }
            }

            UnityEngine.Debug.Log("========== FIN DEBUG ==========");
        }

        private void DebugFindText(string objectName)
        {
            UnityEngine.Debug.Log($"\n--- Buscando '{objectName}' ---");

            GameObject[] allObjects = FindObjectsOfType<GameObject>(true);
            int found = 0;

            foreach (var obj in allObjects)
            {
                if (obj.name == objectName)
                {
                    found++;
                    UnityEngine.Debug.Log($"[{found}] Encontrado: {obj.name}");
                    UnityEngine.Debug.Log($"    Activo en jerarquia: {obj.activeInHierarchy}");
                    UnityEngine.Debug.Log($"    Activo self: {obj.activeSelf}");
                    UnityEngine.Debug.Log($"    Layer: {obj.layer}");

                    // Verificar padre
                    if (obj.transform.parent != null)
                    {
                        UnityEngine.Debug.Log($"    Padre: {obj.transform.parent.name} (activo: {obj.transform.parent.gameObject.activeInHierarchy})");

                        // Verificar abuelo
                        if (obj.transform.parent.parent != null)
                        {
                            UnityEngine.Debug.Log($"    Abuelo: {obj.transform.parent.parent.name} (activo: {obj.transform.parent.parent.gameObject.activeInHierarchy})");
                        }
                    }

                    // RectTransform
                    var rt = obj.GetComponent<RectTransform>();
                    if (rt != null)
                    {
                        UnityEngine.Debug.Log($"    Posicion: {rt.anchoredPosition}");
                        UnityEngine.Debug.Log($"    Tamano: {rt.sizeDelta}");
                        UnityEngine.Debug.Log($"    Escala: {rt.localScale}");
                    }

                    // TextMeshProUGUI
                    var tmp = obj.GetComponent<TextMeshProUGUI>();
                    if (tmp != null)
                    {
                        UnityEngine.Debug.Log($"    TMP Enabled: {tmp.enabled}");
                        UnityEngine.Debug.Log($"    TMP Texto: '{tmp.text}'");
                        UnityEngine.Debug.Log($"    TMP Color: {tmp.color} (alpha: {tmp.color.a})");
                        UnityEngine.Debug.Log($"    TMP Font: {(tmp.font != null ? tmp.font.name : "NULL!")}");
                        UnityEngine.Debug.Log($"    TMP FontSize: {tmp.fontSize}");
                        UnityEngine.Debug.Log($"    TMP AutoSize: {tmp.enableAutoSizing}");
                        if (tmp.enableAutoSizing)
                        {
                            UnityEngine.Debug.Log($"    TMP FontSize Min/Max: {tmp.fontSizeMin}/{tmp.fontSizeMax}");
                        }
                    }
                    else
                    {
                        UnityEngine.Debug.Log($"    NO tiene TextMeshProUGUI!");

                        // Verificar si tiene Image
                        var img = obj.GetComponent<UnityEngine.UI.Image>();
                        if (img != null)
                        {
                            UnityEngine.Debug.Log($"    Tiene IMAGE en lugar de texto");
                        }
                    }

                    // CanvasRenderer
                    var cr = obj.GetComponent<CanvasRenderer>();
                    if (cr != null)
                    {
                        UnityEngine.Debug.Log($"    CanvasRenderer: SI, cull={cr.cull}");
                    }
                    else
                    {
                        UnityEngine.Debug.Log($"    CanvasRenderer: NO!");
                    }
                }
            }

            if (found == 0)
            {
                UnityEngine.Debug.LogError($"    NO SE ENCONTRO NINGUN OBJETO llamado '{objectName}'!");
            }
        }
    }
}
