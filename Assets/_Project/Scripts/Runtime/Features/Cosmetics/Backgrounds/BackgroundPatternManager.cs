using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DigitPark.Cosmetics
{
    /// <summary>
    /// Singleton que gestiona el sistema de background patterns cosméticos.
    /// Independiente de ThemeManager — el patrón es una segunda dimensión cosmética.
    /// Persiste entre escenas (DontDestroyOnLoad). Los BackgroundPatternReceiver
    /// se registran/desregistran automáticamente.
    /// </summary>
    public class BackgroundPatternManager : MonoBehaviour
    {
        public static BackgroundPatternManager Instance { get; private set; }

        private const string PREF_EQUIPPED = "EquippedBackground";
        private const string PREF_OWNED    = "OwnedBackgrounds";
        public  const string DEFAULT_ID    = "bg_solid";

        // Opacidad fija por patrón (ajustada para que sea tenue pero visible)
        private static readonly Dictionary<string, float> PatternOpacity =
            new Dictionary<string, float>
        {
            { "bg_solid",         0.00f },
            { "bg_dots",          0.08f },
            { "bg_grid",          0.07f },
            { "bg_crosshatch",    0.06f },
            { "bg_circuit",       0.05f },
            { "bg_binary",        0.06f },
            { "bg_constellation", 0.07f },
            { "bg_triangles",     0.07f },
            { "bg_neural",        0.06f },
            { "bg_fingerprint",   0.05f },
            { "bg_dna",           0.06f },
            { "bg_waveform",      0.08f },
            { "bg_hexgrid",       0.06f },
            { "bg_digits",        0.12f },
        };

        private string              _equippedId;
        private Sprite              _equippedSprite;
        private HashSet<string>     _ownedIds;
        private readonly List<BackgroundPatternReceiver> _receivers =
            new List<BackgroundPatternReceiver>();

        // Events
        public event Action<string> OnPatternEquipped;
        public event Action<string> OnPatternUnlocked;

        public string EquippedId => _equippedId;

        // ==================== Lifecycle ====================

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Initialize();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Receivers se registran solos en OnEnable; aplicar a los ya registrados.
            ApplyToAllReceivers();
        }

        // ==================== Init ====================

        private void Initialize()
        {
            _ownedIds = new HashSet<string>();
            _ownedIds.Add(DEFAULT_ID); // bg_solid siempre desbloqueado

            // Cargar IDs que posee el jugador
            string ownedJson = PlayerPrefs.GetString(PREF_OWNED, "");
            if (!string.IsNullOrEmpty(ownedJson))
            {
                try
                {
                    var wrapper = JsonUtility.FromJson<StringListWrapper>(ownedJson);
                    if (wrapper?.items != null)
                        foreach (var id in wrapper.items)
                            _ownedIds.Add(id);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[BackgroundPatternManager] Failed to parse owned: {e.Message}");
                }
            }

            // Cargar patrón equipado
            _equippedId = PlayerPrefs.GetString(PREF_EQUIPPED, DEFAULT_ID);
            if (!_ownedIds.Contains(_equippedId))
                _equippedId = DEFAULT_ID;

            _equippedSprite = LoadSprite(_equippedId);
            Debug.Log($"[BackgroundPatternManager] Initialized. Equipped: {_equippedId}");
        }

        // ==================== Receiver Registration ====================

        /// <summary>Called by BackgroundPatternReceiver.OnEnable()</summary>
        public void Register(BackgroundPatternReceiver receiver)
        {
            if (!_receivers.Contains(receiver))
            {
                _receivers.Add(receiver);
                ApplyToReceiver(receiver); // Apply immediately on registration
            }
        }

        /// <summary>Called by BackgroundPatternReceiver.OnDisable()</summary>
        public void Unregister(BackgroundPatternReceiver receiver)
        {
            _receivers.Remove(receiver);
        }

        // ==================== Public API ====================

        public bool IsOwned(string patternId) =>
            patternId == DEFAULT_ID || (_ownedIds != null && _ownedIds.Contains(patternId));

        /// <summary>Unlocks a pattern (called by ShopItemData.GrantRewards).</summary>
        public void UnlockBackground(string patternId)
        {
            if (string.IsNullOrEmpty(patternId) || _ownedIds.Contains(patternId)) return;

            _ownedIds.Add(patternId);
            SaveOwnedBackgrounds();
            OnPatternUnlocked?.Invoke(patternId);
            Debug.Log($"[BackgroundPatternManager] Unlocked: {patternId}");
        }

        /// <summary>Equips a pattern. Validates ownership before applying.</summary>
        public void SetPattern(string patternId)
        {
            if (string.IsNullOrEmpty(patternId)) patternId = DEFAULT_ID;

            if (!IsOwned(patternId))
            {
                Debug.LogWarning($"[BackgroundPatternManager] Pattern not owned: {patternId}");
                return;
            }

            _equippedId     = patternId;
            _equippedSprite = LoadSprite(patternId);

            PlayerPrefs.SetString(PREF_EQUIPPED, patternId);
            PlayerPrefs.Save();

            ApplyToAllReceivers();
            SyncToFirebase();
            OnPatternEquipped?.Invoke(patternId);
        }

        /// <summary>
        /// B4-E: Fetch background data from Firebase and restore locally.
        /// Call from BootManager after login (reinstall scenario).
        /// </summary>
        public async System.Threading.Tasks.Task RestoreFromFirebaseAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return;
            try
            {
                var db   = DigitPark.Services.Firebase.DatabaseService.Instance;
                if (db == null) return;
                var dbRef = Firebase.Database.FirebaseDatabase.DefaultInstance?.RootReference;
                if (dbRef == null) return;

                var snapshot = await dbRef.Child("players").Child(userId).GetValueAsync();
                if (snapshot == null || !snapshot.Exists) return;

                string equippedId = snapshot.Child("equippedBackground").Value?.ToString() ?? "";
                var ownedList = new List<string>();
                var ownedSnap = snapshot.Child("ownedBackgrounds");
                if (ownedSnap.Exists)
                {
                    foreach (var child in ownedSnap.Children)
                        ownedList.Add(child.Value?.ToString() ?? "");
                }

                if (!string.IsNullOrEmpty(equippedId) || ownedList.Count > 0)
                    RestoreFromFirebase(equippedId, ownedList);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[BackgroundPatternManager] RestoreFromFirebaseAsync failed: {e.Message}");
            }
        }

        /// <summary>
        /// Restores equipped pattern from Firebase data (called by AuthenticationService
        /// after profile load).
        /// </summary>
        public void RestoreFromFirebase(string equippedId, List<string> ownedIds)
        {
            if (ownedIds != null)
                foreach (var id in ownedIds)
                    _ownedIds.Add(id);

            if (!string.IsNullOrEmpty(equippedId) && IsOwned(equippedId))
            {
                _equippedId     = equippedId;
                _equippedSprite = LoadSprite(equippedId);
                PlayerPrefs.SetString(PREF_EQUIPPED, _equippedId);
                PlayerPrefs.Save();
            }

            ApplyToAllReceivers();
        }

        /// <summary>Applies visually WITHOUT saving — for Shop preview.</summary>
        public void PreviewPattern(string patternId)
        {
            var sprite  = LoadSprite(patternId);
            float opacity = GetOpacity(patternId);
            foreach (var r in _receivers)
                if (r != null) r.Apply(sprite, opacity);
        }

        /// <summary>Cancels preview — restores last saved pattern.</summary>
        public void CancelPreview() => ApplyToAllReceivers();

        // ==================== Internal Apply ====================

        public void ApplyToAllReceivers()
        {
            float opacity = GetOpacity(_equippedId);
            for (int i = _receivers.Count - 1; i >= 0; i--)
            {
                if (_receivers[i] == null) { _receivers.RemoveAt(i); continue; }
                _receivers[i].Apply(_equippedSprite, opacity);
            }
        }

        private void ApplyToReceiver(BackgroundPatternReceiver receiver)
        {
            if (receiver == null) return;
            receiver.Apply(_equippedSprite, GetOpacity(_equippedId));
        }

        // ==================== Helpers ====================

        private static Sprite LoadSprite(string patternId) =>
            patternId == DEFAULT_ID || string.IsNullOrEmpty(patternId)
                ? null
                : Resources.Load<Sprite>($"Backgrounds/{patternId}");

        public static float GetOpacity(string patternId) =>
            PatternOpacity.TryGetValue(patternId, out float op) ? op : 0f;

        // ==================== Persistence ====================

        private void SaveOwnedBackgrounds()
        {
            var wrapper = new StringListWrapper { items = new List<string>(_ownedIds) };
            PlayerPrefs.SetString(PREF_OWNED, JsonUtility.ToJson(wrapper));
            PlayerPrefs.Save();
            SyncToFirebase();
        }

        private async void SyncToFirebase()
        {
            try
            {
                var db   = DigitPark.Services.Firebase.DatabaseService.Instance;
                var auth = DigitPark.Services.Firebase.AuthenticationService.Instance;
                if (db == null || auth == null) return;

                string uid = auth.GetCurrentUserId();
                if (string.IsNullOrEmpty(uid)) return;

                var fields = new Dictionary<string, object>
                {
                    { "equippedBackground", _equippedId },
                    { "ownedBackgrounds",   new List<string>(_ownedIds) }
                };
                await db.UpdatePlayerFields(uid, fields);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[BackgroundPatternManager] Firebase sync failed: {e.Message}");
            }
        }

        // ==================== Serialization helpers ====================

        [System.Serializable]
        private class StringListWrapper
        {
            public List<string> items;
        }
    }
}
