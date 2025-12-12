using UnityEngine;
using System;
using System.Collections.Generic;

namespace DigitPark.Skillz
{
    /// <summary>
    /// Configuración de distribución de premios para torneos
    ///
    /// Distribución por defecto (después de que Skillz tome su 50%):
    /// - 80% para ganadores
    /// - 20% para desarrolladores
    ///
    /// Ejemplo con pozo de $100:
    /// - Skillz: $50 (50%)
    /// - Disponible: $50
    ///   - Ganadores: $40 (80% de $50)
    ///   - Desarrolladores: $10 (20% de $50)
    /// </summary>
    [CreateAssetMenu(fileName = "PrizeDistributionConfig", menuName = "DigitPark/Skillz/Prize Distribution Config")]
    public class PrizeDistributionConfig : ScriptableObject
    {
        [Header("Comisión de Skillz (fijo)")]
        [Tooltip("Skillz se lleva aproximadamente 50% del pozo total")]
        [Range(0.4f, 0.6f)]
        [SerializeField] private float skillzCommission = 0.50f;

        [Header("Distribución del 50% restante")]
        [Tooltip("Porcentaje para los ganadores (del monto después de Skillz)")]
        [Range(0.5f, 0.95f)]
        [SerializeField] private float winnersPercentage = 0.80f;

        [Tooltip("Porcentaje para desarrolladores (del monto después de Skillz)")]
        [Range(0.05f, 0.5f)]
        [SerializeField] private float developersPercentage = 0.20f;

        [Header("Distribución entre ganadores")]
        [Tooltip("Distribución de premios entre posiciones (debe sumar 100%)")]
        [SerializeField] private List<PlaceDistribution> placeDistributions = new List<PlaceDistribution>
        {
            new PlaceDistribution { place = 1, percentage = 0.50f, displayName = "1er Lugar" },
            new PlaceDistribution { place = 2, percentage = 0.30f, displayName = "2do Lugar" },
            new PlaceDistribution { place = 3, percentage = 0.20f, displayName = "3er Lugar" }
        };

        [Header("Configuración adicional")]
        [Tooltip("Mínimo de jugadores para activar premios en efectivo")]
        [SerializeField] private int minimumPlayersForCash = 2;

        [Tooltip("Entrada mínima para torneos con dinero real (USD)")]
        [SerializeField] private float minimumEntryFee = 0.60f;

        [Tooltip("Entrada máxima para torneos con dinero real (USD)")]
        [SerializeField] private float maximumEntryFee = 100f;

        // Propiedades públicas
        public float SkillzCommission => skillzCommission;
        public float WinnersPercentage => winnersPercentage;
        public float DevelopersPercentage => developersPercentage;
        public int MinimumPlayersForCash => minimumPlayersForCash;
        public float MinimumEntryFee => minimumEntryFee;
        public float MaximumEntryFee => maximumEntryFee;
        public List<PlaceDistribution> PlaceDistributions => placeDistributions;

        private void OnValidate()
        {
            // Asegurar que winners + developers = 100%
            if (Mathf.Abs(winnersPercentage + developersPercentage - 1f) > 0.01f)
            {
                Debug.LogWarning("[PrizeConfig] Winners + Developers debe sumar 100%");
                developersPercentage = 1f - winnersPercentage;
            }

            // Validar distribución de lugares
            ValidatePlaceDistributions();
        }

        private void ValidatePlaceDistributions()
        {
            float total = 0f;
            foreach (var place in placeDistributions)
            {
                total += place.percentage;
            }

            if (Mathf.Abs(total - 1f) > 0.01f)
            {
                Debug.LogWarning($"[PrizeConfig] Distribución de lugares debe sumar 100% (actual: {total * 100}%)");
            }
        }

        /// <summary>
        /// Calcula el desglose completo de premios
        /// </summary>
        /// <param name="totalPot">Pozo total del torneo</param>
        /// <param name="numberOfWinners">Número de ganadores a premiar</param>
        public PrizeBreakdown CalculateBreakdown(float totalPot, int numberOfWinners = 3)
        {
            var breakdown = new PrizeBreakdown
            {
                TotalPot = totalPot,
                SkillzTake = totalPot * skillzCommission,
                NumberOfWinners = Mathf.Min(numberOfWinners, placeDistributions.Count)
            };

            // Monto disponible después de Skillz
            breakdown.AvailableAfterSkillz = totalPot - breakdown.SkillzTake;

            // Distribución del monto disponible
            breakdown.TotalForWinners = breakdown.AvailableAfterSkillz * winnersPercentage;
            breakdown.DevelopersTake = breakdown.AvailableAfterSkillz * developersPercentage;

            // Calcular premio por posición
            breakdown.PrizesByPlace = new List<PlacePrize>();
            for (int i = 0; i < breakdown.NumberOfWinners && i < placeDistributions.Count; i++)
            {
                var placeConfig = placeDistributions[i];
                breakdown.PrizesByPlace.Add(new PlacePrize
                {
                    Place = placeConfig.place,
                    DisplayName = placeConfig.displayName,
                    Amount = breakdown.TotalForWinners * placeConfig.percentage,
                    Percentage = placeConfig.percentage
                });
            }

            return breakdown;
        }

        /// <summary>
        /// Calcula el pozo total basado en entrada y número de jugadores
        /// </summary>
        public float CalculateTotalPot(float entryFee, int numberOfPlayers)
        {
            return entryFee * numberOfPlayers;
        }

        /// <summary>
        /// Obtiene una descripción legible de la distribución
        /// </summary>
        public string GetDistributionDescription()
        {
            string desc = $"Distribución de Premios:\n";
            desc += $"• Skillz: {skillzCommission * 100}%\n";
            desc += $"• Ganadores: {winnersPercentage * 100}% del restante\n";
            desc += $"• Desarrolladores: {developersPercentage * 100}% del restante\n\n";
            desc += "Premios por posición:\n";

            foreach (var place in placeDistributions)
            {
                desc += $"• {place.displayName}: {place.percentage * 100}%\n";
            }

            return desc;
        }
    }

    /// <summary>
    /// Distribución por posición
    /// </summary>
    [Serializable]
    public class PlaceDistribution
    {
        public int place;
        [Range(0f, 1f)]
        public float percentage;
        public string displayName;
    }

    /// <summary>
    /// Desglose completo de premios calculado
    /// </summary>
    [Serializable]
    public class PrizeBreakdown
    {
        public float TotalPot;
        public float SkillzTake;
        public float AvailableAfterSkillz;
        public float TotalForWinners;
        public float DevelopersTake;
        public int NumberOfWinners;
        public List<PlacePrize> PrizesByPlace;

        public override string ToString()
        {
            string result = $"=== Desglose de Premios ===\n";
            result += $"Pozo Total: ${TotalPot:F2}\n";
            result += $"Skillz (50%): ${SkillzTake:F2}\n";
            result += $"Disponible: ${AvailableAfterSkillz:F2}\n";
            result += $"├─ Ganadores (80%): ${TotalForWinners:F2}\n";
            result += $"└─ Desarrolladores (20%): ${DevelopersTake:F2}\n\n";
            result += "Premios:\n";

            foreach (var prize in PrizesByPlace)
            {
                result += $"  {prize.DisplayName}: ${prize.Amount:F2}\n";
            }

            return result;
        }
    }

    /// <summary>
    /// Premio por posición
    /// </summary>
    [Serializable]
    public class PlacePrize
    {
        public int Place;
        public string DisplayName;
        public float Amount;
        public float Percentage;
    }
}
