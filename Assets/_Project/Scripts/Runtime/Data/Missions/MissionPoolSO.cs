using UnityEngine;
using System;
using System.Collections.Generic;
using DigitPark.UI.Items;

namespace DigitPark.Data
{
    /// <summary>
    /// ScriptableObject que agrupa MissionDefinitionSO por periodo.
    /// Selecciona misiones aleatoriamente con seed determinístico basado en fecha.
    /// </summary>
    [CreateAssetMenu(menuName = "DigitPark/Missions/Mission Pool")]
    public class MissionPoolSO : ScriptableObject
    {
        [Header("Pool Configuration")]
        [Tooltip("Nombre del pool (ej: DailyPool, WeeklyPool)")]
        public string poolName;

        [Tooltip("Categoria de las misiones en este pool")]
        public MissionCategory category;

        [Tooltip("Cantidad de misiones a seleccionar (6 daily, 3 weekly)")]
        public int selectionCount = 6;

        [Header("Missions")]
        [Tooltip("Lista completa de definiciones disponibles")]
        public List<MissionDefinitionSO> missions = new List<MissionDefinitionSO>();

        /// <summary>
        /// Selecciona misiones usando seed deterministico basado en fecha.
        /// Misma fecha = misma seleccion.
        /// </summary>
        public List<MissionDefinitionSO> SelectMissions(DateTime date)
        {
            if (missions == null || missions.Count == 0)
                return new List<MissionDefinitionSO>();

            int count = Mathf.Min(selectionCount, missions.Count);

            // Seed determinístico: YYYYMMDD + hash del nombre del pool
            int dateSeed = date.Year * 10000 + date.Month * 100 + date.Day;
            int seed = dateSeed + (poolName != null ? poolName.GetHashCode() : 0);

            // Crear copia para shuffle
            var shuffled = new List<MissionDefinitionSO>(missions);

            // Fisher-Yates shuffle con System.Random seeded
            var rng = new System.Random(seed);
            for (int i = shuffled.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                var temp = shuffled[i];
                shuffled[i] = shuffled[j];
                shuffled[j] = temp;
            }

            // Retornar los primeros N
            return shuffled.GetRange(0, count);
        }
    }
}
