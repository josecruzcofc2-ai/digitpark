#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using DigitPark.Themes;

namespace DigitPark.Editor
{
    /// <summary>
    /// Utilidad estática para que los UIBuilders añadan ThemeApplier a GameObjects
    /// de forma concisa y correcta.
    ///
    /// Uso típico en un UIBuilder:
    ///   ThemeApplierHelper.Apply(bgGO, ThemeApplier.ElementType.PrimaryBackground);
    ///   ThemeApplierHelper.ApplyText(titleGO, ThemeApplier.ElementType.TextTitle);
    ///   ThemeApplierHelper.ApplyOutline(cardGO, ThemeApplier.ElementType.Glow);
    ///   ThemeApplierHelper.ApplyDual(cardGO, ThemeApplier.ElementType.CardBackground,
    ///                                ThemeApplier.ElementType.Glow);
    /// </summary>
    public static class ThemeApplierHelper
    {
        // ─── Image (applyToImage=true, applyToText=false, applyToOutline=false) ───

        /// <summary>
        /// Añade ThemeApplier al Image del GameObject.
        /// Útil para backgrounds, cards, botones, iconos tintables.
        /// Idempotente: si ya existe un ThemeApplier con el mismo elementType, no añade otro.
        /// </summary>
        public static ThemeApplier Apply(GameObject go, ThemeApplier.ElementType elementType)
        {
            if (go == null) return null;

            // Avoid duplicates with the same elementType
            foreach (var existing in go.GetComponents<ThemeApplier>())
            {
                var existingSo = new SerializedObject(existing);
                if (existingSo.FindProperty("elementType").enumValueIndex == (int)elementType)
                    return existing;
            }

            var applier = go.AddComponent<ThemeApplier>();
            var so = new SerializedObject(applier);
            so.FindProperty("elementType").enumValueIndex = (int)elementType;
            so.FindProperty("applyToImage").boolValue = true;
            so.FindProperty("applyToText").boolValue = false;
            so.FindProperty("applyToOutline").boolValue = false;
            so.FindProperty("applyToShadow").boolValue = false;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(go);
            return applier;
        }

        // ─── Text (applyToText=true, applyToImage=false, applyToOutline=false) ───

        /// <summary>
        /// Añade ThemeApplier al TextMeshProUGUI del GameObject.
        /// Útil para títulos, labels, placeholders.
        /// </summary>
        public static ThemeApplier ApplyText(GameObject go, ThemeApplier.ElementType elementType)
        {
            if (go == null) return null;

            foreach (var existing in go.GetComponents<ThemeApplier>())
            {
                var existingSo = new SerializedObject(existing);
                if (existingSo.FindProperty("elementType").enumValueIndex == (int)elementType)
                    return existing;
            }

            var applier = go.AddComponent<ThemeApplier>();
            var so = new SerializedObject(applier);
            so.FindProperty("elementType").enumValueIndex = (int)elementType;
            so.FindProperty("applyToImage").boolValue = false;
            so.FindProperty("applyToText").boolValue = true;
            so.FindProperty("applyToOutline").boolValue = false;
            so.FindProperty("applyToShadow").boolValue = false;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(go);
            return applier;
        }

        // ─── Outline (applyToOutline=true, applyToImage=false, applyToText=false) ───

        /// <summary>
        /// Añade ThemeApplier al Outline del GameObject.
        /// Útil para glows, bordes de cards, bordes de botones.
        /// </summary>
        public static ThemeApplier ApplyOutline(GameObject go, ThemeApplier.ElementType elementType)
        {
            if (go == null) return null;

            foreach (var existing in go.GetComponents<ThemeApplier>())
            {
                var existingSo = new SerializedObject(existing);
                if (existingSo.FindProperty("elementType").enumValueIndex == (int)elementType)
                    return existing;
            }

            var applier = go.AddComponent<ThemeApplier>();
            var so = new SerializedObject(applier);
            so.FindProperty("elementType").enumValueIndex = (int)elementType;
            so.FindProperty("applyToImage").boolValue = false;
            so.FindProperty("applyToText").boolValue = false;
            so.FindProperty("applyToOutline").boolValue = true;
            so.FindProperty("applyToShadow").boolValue = false;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(go);
            return applier;
        }

        // ─── Dual: Image + Outline on same GameObject ───

        /// <summary>
        /// Añade DOS ThemeApplier al mismo GameObject:
        ///   #1 → Image (imageType)
        ///   #2 → Outline (outlineType)
        /// Usado en cards con borde, botones con glow, inputs con borde.
        /// </summary>
        public static void ApplyDual(
            GameObject go,
            ThemeApplier.ElementType imageType,
            ThemeApplier.ElementType outlineType)
        {
            Apply(go, imageType);
            ApplyOutline(go, outlineType);
        }

        // ─── Text + Outline on same GameObject ───

        /// <summary>
        /// Añade DOS ThemeApplier al mismo GameObject:
        ///   #1 → Text (textType)
        ///   #2 → Outline (outlineType)
        /// Usado en títulos TMP con Outline glow.
        /// </summary>
        public static void ApplyTextWithOutline(
            GameObject go,
            ThemeApplier.ElementType textType,
            ThemeApplier.ElementType outlineType)
        {
            ApplyText(go, textType);
            ApplyOutline(go, outlineType);
        }
    }
}
#endif
