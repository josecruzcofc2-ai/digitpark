using UnityEngine;
using DigitPark.Games;
using DigitPark.UI.Items;

namespace DigitPark.Data
{
    /// <summary>
    /// ScriptableObject que define una mision individual.
    /// Datos fijos y reutilizables para el sistema de misiones.
    /// </summary>
    [CreateAssetMenu(menuName = "DigitPark/Missions/Mission Definition")]
    public class MissionDefinitionSO : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("ID unico para persistencia")]
        public string missionId;

        [Header("Localization")]
        [Tooltip("Clave de localizacion para el titulo")]
        public string titleLocKey;
        [Tooltip("Clave de localizacion para la descripcion")]
        public string descriptionLocKey;

        [Header("Visual")]
        [Tooltip("Icono de la mision")]
        public Sprite icon;

        [Header("Action")]
        [Tooltip("Tipo de accion requerida")]
        public MissionActionType actionType;
        [Tooltip("Para misiones de juego especifico")]
        public GameType targetGame;
        [Tooltip("Si la mision es para un juego especifico")]
        public bool isGameSpecific;
        [Tooltip("Meta numerica")]
        public int targetAmount = 1;

        [Header("Classification")]
        [Tooltip("Dificultad de la mision")]
        public MissionDifficulty difficulty;
        [Tooltip("Categoria (Daily/Weekly/Special)")]
        public MissionCategory category;
        [Tooltip("Orden de visualizacion")]
        public int sortOrder;

        [Header("Reward")]
        [Tooltip("Tipo de recompensa")]
        public MissionRewardType rewardType;
        [Tooltip("Cantidad de recompensa")]
        public int rewardAmount;
    }

    /// <summary>
    /// Tipos de accion que una mision puede requerir.
    /// </summary>
    public enum MissionActionType
    {
        PlayGames,
        WinGames,
        PlaySpecificGame,
        WinSpecificGame,
        ReachScore,
        AccumulateScore,
        PrecisionGame,
        PlayAllGameTypes,
        WinStreak,
        CompleteCognitiveSprint,
        SpendCoins,
        EarnCoins,
        PlayTournament,
        InviteFriend
    }

    /// <summary>
    /// Tipo de recompensa de la mision.
    /// </summary>
    public enum MissionRewardType
    {
        Coins,
        Gems
    }
}
