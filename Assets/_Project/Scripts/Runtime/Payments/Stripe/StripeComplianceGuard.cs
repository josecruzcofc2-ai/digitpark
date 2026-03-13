using System.Collections.Generic;
using UnityEngine;

namespace DigitPark.Payments.Compliance
{
    /// <summary>
    /// Validador de cumplimiento para Stripe.
    /// Verifica que NINGÚN producto o metadata contiene términos prohibidos.
    /// Estos términos violarían los ToS de Stripe para cosméticos.
    /// </summary>
    public static class StripeComplianceGuard
    {
        private static readonly string[] ProhibitedTerms = {
            "tournament", "prize", "cash_game", "skill_game",
            "real_money", "entry_fee", "wager", "bet", "gambling", "triumph"
        };

        public static bool ValidateProduct(CosmeticProduct product)
        {
            if (product == null)
            {
                Debug.LogError("[StripeCompliance] Producto nulo");
                return false;
            }

            string id = product.ProductId?.ToLowerInvariant() ?? "";
            string name = product.DisplayName?.ToLowerInvariant() ?? "";

            foreach (var term in ProhibitedTerms)
            {
                if (id.Contains(term) || name.Contains(term))
                {
                    Debug.LogError(
                        $"[StripeCompliance] VIOLATION: Producto '{product.ProductId}' " +
                        $"contiene término prohibido '{term}'. No puede procesar con Stripe.");
                    LogComplianceAudit(product.ProductId, "stripe", false,
                        $"Término prohibido: {term}");
                    return false;
                }
            }

            if (product.Metadata != null)
            {
                foreach (var kvp in product.Metadata)
                {
                    string val = kvp.Value?.ToLowerInvariant() ?? "";
                    foreach (var term in ProhibitedTerms)
                    {
                        if (val.Contains(term))
                        {
                            Debug.LogError(
                                $"[StripeCompliance] VIOLATION: Metadata '{kvp.Key}={kvp.Value}' " +
                                $"contiene término prohibido '{term}'.");
                            LogComplianceAudit(product.ProductId, "stripe", false,
                                $"Metadata prohibida: {kvp.Key}={kvp.Value}");
                            return false;
                        }
                    }
                }
            }

            LogComplianceAudit(product.ProductId, "stripe", true, "OK");
            return true;
        }

        public static bool ValidateSessionMetadata(Dictionary<string, string> metadata)
        {
            if (metadata == null) return false;

            // Verificar que contiene campos obligatorios
            if (!metadata.ContainsKey("type") || metadata["type"] != "cosmetic")
            {
                Debug.LogError("[StripeCompliance] Session metadata falta 'type=cosmetic'");
                return false;
            }

            if (!metadata.ContainsKey("has_tournament_benefit") ||
                metadata["has_tournament_benefit"] != "false")
            {
                Debug.LogError("[StripeCompliance] Session metadata debe tener 'has_tournament_benefit=false'");
                return false;
            }

            // Verificar términos prohibidos en todos los valores
            foreach (var kvp in metadata)
            {
                string val = kvp.Value?.ToLowerInvariant() ?? "";
                foreach (var term in ProhibitedTerms)
                {
                    if (val.Contains(term))
                    {
                        Debug.LogError(
                            $"[StripeCompliance] Session metadata contiene término prohibido: {term}");
                        return false;
                    }
                }
            }

            return true;
        }

        public static void LogComplianceAudit(string productId, string provider, bool passed, string details)
        {
            const string KEY = "dp_compliance_audit";
            const int MAX_ENTRIES = 100;

            string existing = PlayerPrefs.GetString(KEY, "");
            string newEntry = $"{System.DateTime.UtcNow:O}|{productId}|{provider}|{passed}|{details}";

            var lines = existing.Split('\n');
            int start = lines.Length >= MAX_ENTRIES ? lines.Length - MAX_ENTRIES + 1 : 0;
            var kept = new System.Text.StringBuilder();
            for (int i = start; i < lines.Length; i++)
            {
                if (!string.IsNullOrEmpty(lines[i]))
                    kept.Append(lines[i]).Append('\n');
            }
            kept.Append(newEntry);

            PlayerPrefs.SetString(KEY, kept.ToString());
            PlayerPrefs.Save();
        }
    }
}
