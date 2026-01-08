using UnityEngine;
using UnityEditor;

namespace DigitPark.Editor
{
    /// <summary>
    /// Script de emergencia para limpiar elementos de FlashTap
    /// que se agregaron accidentalmente a GameSelector
    /// </summary>
    public class CleanFlashTapFromGameSelector : EditorWindow
    {
        [MenuItem("DigitPark/EMERGENCIA - Limpiar FlashTap de GameSelector")]
        public static void CleanUp()
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("No se encontró Canvas");
                return;
            }

            Transform canvasTransform = canvas.transform;
            int deleted = 0;

            // Lista de elementos de FlashTap a eliminar
            string[] flashTapElements = new string[]
            {
                "RoundBadge",
                "InstructionText",
                "TapButton",
                "StatsPanel",
                "WinPanel"
            };

            foreach (string elementName in flashTapElements)
            {
                Transform element = canvasTransform.Find(elementName);
                if (element != null)
                {
                    Debug.Log($"Eliminando: {elementName}");
                    DestroyImmediate(element.gameObject);
                    deleted++;
                }
            }

            // Verificar si el Header tiene TitleText con "FLASH TAP"
            Transform header = canvasTransform.Find("Header");
            if (header != null)
            {
                Transform titleText = header.Find("TitleText");
                if (titleText != null)
                {
                    var tmp = titleText.GetComponent<TMPro.TextMeshProUGUI>();
                    if (tmp != null && tmp.text == "FLASH TAP")
                    {
                        // Cambiar de vuelta al texto de GameSelector
                        tmp.text = "Selecciona un Juego";
                        Debug.Log("TitleText restaurado a 'Selecciona un Juego'");
                    }
                }
            }

            Debug.Log($"Limpieza completada! {deleted} elementos eliminados.");

            if (canvas != null)
                EditorUtility.SetDirty(canvas.gameObject);
        }
    }
}
