using UnityEngine;
using UnityEditor;
using TMPro;

namespace DigitPark.Editor
{
    /// <summary>
    /// Script de diagnostico para encontrar problemas con los textos en Settings
    /// </summary>
    public class SettingsTextDebug : EditorWindow
    {
        [MenuItem("DigitPark/Debug/Diagnosticar Textos Settings")]
        public static void DiagnoseSettingsTexts()
        {
            Debug.Log("========== DIAGNOSTICO DE TEXTOS SETTINGS ==========");

            // Buscar todos los objetos con nombres relevantes
            string[] targetNames = { "ChangeStyleText", "ChangeLanguageText", "ThemesIconContainer", "LanguageIconContainer" };

            foreach (string targetName in targetNames)
            {
                Debug.Log($"\n--- Buscando objetos llamados '{targetName}' ---");

                GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
                int found = 0;

                foreach (var obj in allObjects)
                {
                    if (obj.name == targetName && obj.scene.IsValid())
                    {
                        found++;
                        Debug.Log($"[{found}] GameObject: {obj.name}");
                        Debug.Log($"    - Activo: {obj.activeSelf}");
                        Debug.Log($"    - Layer: {obj.layer} ({LayerMask.LayerToName(obj.layer)})");
                        Debug.Log($"    - Padre: {(obj.transform.parent != null ? obj.transform.parent.name : "null")}");

                        // Verificar RectTransform
                        var rt = obj.GetComponent<RectTransform>();
                        if (rt != null)
                        {
                            Debug.Log($"    - Posicion: {rt.anchoredPosition}");
                            Debug.Log($"    - Tamano: {rt.sizeDelta}");
                        }

                        // Verificar si tiene TextMeshProUGUI
                        var tmp = obj.GetComponent<TextMeshProUGUI>();
                        if (tmp != null)
                        {
                            Debug.Log($"    - TMP Texto: '{tmp.text}'");
                            Debug.Log($"    - TMP Color: {tmp.color}");
                            Debug.Log($"    - TMP Enabled: {tmp.enabled}");
                            Debug.Log($"    - TMP Font Size: {tmp.fontSize}");
                        }
                        else
                        {
                            Debug.Log($"    - NO tiene TextMeshProUGUI");
                        }

                        // Verificar si tiene Image
                        var img = obj.GetComponent<UnityEngine.UI.Image>();
                        if (img != null)
                        {
                            Debug.Log($"    - Image Sprite: {(img.sprite != null ? img.sprite.name : "null")}");
                            Debug.Log($"    - Image Color: {img.color}");
                        }

                        // Verificar CanvasRenderer
                        var cr = obj.GetComponent<CanvasRenderer>();
                        if (cr != null)
                        {
                            Debug.Log($"    - CanvasRenderer: SI");
                        }
                        else
                        {
                            Debug.Log($"    - CanvasRenderer: NO (PROBLEMA!)");
                        }
                    }
                }

                if (found == 0)
                {
                    Debug.LogWarning($"    No se encontro ningun objeto llamado '{targetName}'");
                }
                else if (found > 1)
                {
                    Debug.LogWarning($"    ADVERTENCIA: Se encontraron {found} objetos con el mismo nombre!");
                }
            }

            Debug.Log("\n========== FIN DIAGNOSTICO ==========");
            Debug.Log("Revisa los resultados arriba para identificar problemas.");
        }

        [MenuItem("DigitPark/Debug/Renombrar Iconos Duplicados")]
        public static void RenameIconDuplicates()
        {
            int renamed = 0;

            // Buscar todos los ChangeStyleText que sean Image (no TMP)
            GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();

            foreach (var obj in allObjects)
            {
                if (!obj.scene.IsValid()) continue;

                if (obj.name == "ChangeStyleText")
                {
                    var tmp = obj.GetComponent<TextMeshProUGUI>();
                    var img = obj.GetComponent<UnityEngine.UI.Image>();

                    // Si tiene Image pero NO TextMeshProUGUI, es un icono mal nombrado
                    if (img != null && tmp == null)
                    {
                        obj.name = "ChangeStyleIcon";
                        EditorUtility.SetDirty(obj);
                        renamed++;
                        Debug.Log($"Renombrado: ChangeStyleText -> ChangeStyleIcon (era un Image, no texto)");
                    }
                }

                if (obj.name == "ChangeLanguageText")
                {
                    var tmp = obj.GetComponent<TextMeshProUGUI>();
                    var img = obj.GetComponent<UnityEngine.UI.Image>();

                    if (img != null && tmp == null)
                    {
                        obj.name = "ChangeLanguageIcon";
                        EditorUtility.SetDirty(obj);
                        renamed++;
                        Debug.Log($"Renombrado: ChangeLanguageText -> ChangeLanguageIcon (era un Image, no texto)");
                    }
                }
            }

            if (renamed > 0)
            {
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                    UnityEngine.SceneManagement.SceneManager.GetActiveScene());
                Debug.Log($"Se renombraron {renamed} objetos. Guarda la escena (Ctrl+S).");
            }
            else
            {
                Debug.Log("No se encontraron iconos con nombres incorrectos.");
            }
        }
    }
}
