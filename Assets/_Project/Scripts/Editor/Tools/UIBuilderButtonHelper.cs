using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using DigitPark.Effects;

namespace DigitPark.Editor
{
    /// <summary>
    /// Helper para que los UIBuilders añadan ButtonEffects automáticamente al crear botones.
    /// Detecta el tipo de botón según su nombre y configura el efecto apropiado.
    /// Uso: UIBuilderButtonHelper.AddEffects(buttonGameObject);
    /// </summary>
    public static class UIBuilderButtonHelper
    {
        /// <summary>
        /// Añade ButtonEffects al botón con auto-detección de tipo por nombre.
        /// Si ya tiene ButtonEffects, no hace nada.
        /// </summary>
        public static ButtonEffects AddEffects(GameObject btnObj)
        {
            if (btnObj == null) return null;
            if (btnObj.GetComponent<Button>() == null) return null;

            var existing = btnObj.GetComponent<ButtonEffects>();
            if (existing != null) return existing;

            var effects = btnObj.AddComponent<ButtonEffects>();
            var type = DetectType(btnObj.name);
            SetEffectType(effects, type);
            return effects;
        }

        /// <summary>
        /// Añade ButtonEffects con tipo explícito.
        /// </summary>
        public static ButtonEffects AddEffects(GameObject btnObj, ButtonEffects.ButtonEffectType type)
        {
            if (btnObj == null) return null;
            if (btnObj.GetComponent<Button>() == null) return null;

            var existing = btnObj.GetComponent<ButtonEffects>();
            if (existing != null)
            {
                SetEffectType(existing, type);
                return existing;
            }

            var effects = btnObj.AddComponent<ButtonEffects>();
            SetEffectType(effects, type);
            return effects;
        }

        /// <summary>
        /// Detecta el tipo de ButtonEffects según el nombre del GameObject.
        /// </summary>
        public static ButtonEffects.ButtonEffectType DetectType(string name)
        {
            string lower = name.ToLower();

            if (lower.Contains("play") || lower.Contains("start") || lower.Contains("buy") ||
                lower.Contains("join") || lower.Contains("create") || lower.Contains("claim"))
                return ButtonEffects.ButtonEffectType.Important;

            if (lower.Contains("confirm") || lower.Contains("ok") || lower.Contains("accept") ||
                lower.Contains("verify") || lower.Contains("send") || lower.Contains("save"))
                return ButtonEffects.ButtonEffectType.Success;

            if (lower.Contains("delete") || lower.Contains("cancel") || lower.Contains("close") ||
                lower.Contains("dismiss") || lower.Contains("decline") || lower.Contains("leave") ||
                lower.Contains("back"))
                return ButtonEffects.ButtonEffectType.Danger;

            if (lower.Contains("premium") || lower.Contains("pro") || lower.Contains("upgrade") ||
                lower.Contains("cashbattle") || lower.Contains("deposit") || lower.Contains("withdraw"))
                return ButtonEffects.ButtonEffectType.Premium;

            return ButtonEffects.ButtonEffectType.Normal;
        }

        private static void SetEffectType(ButtonEffects effects, ButtonEffects.ButtonEffectType type)
        {
            var so = new SerializedObject(effects);
            var typeProp = so.FindProperty("effectType");
            if (typeProp != null)
            {
                typeProp.enumValueIndex = (int)type;
                so.ApplyModifiedProperties();
            }
        }
    }
}
