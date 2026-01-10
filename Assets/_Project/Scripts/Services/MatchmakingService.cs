using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DigitPark.Games;
using Firebase;
using Firebase.Database;
using Firebase.Auth;

namespace DigitPark.Services
{
    /// <summary>
    /// Servicio de Matchmaking usando Firebase Realtime Database
    /// Maneja búsqueda de oponentes para partidas 1v1 online
    /// </summary>
    public class MatchmakingService : MonoBehaviour
    {
        public static MatchmakingService Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private float pollInterval = 1f; // Check for match every second
        [SerializeField] private float matchTimeout = 120f; // 2 minutes timeout

        // Firebase references
        private DatabaseReference databaseRef;
        private DatabaseReference matchmakingQueueRef;
        private DatabaseReference activeMatchesRef;

        // Current matchmaking state
        private string currentQueueEntryKey;
        private string currentUserId;
        private string currentUserName;
        private bool isSearching = false;
        private Coroutine searchCoroutine;

        // Callbacks
        private Action<string, string> onMatchFoundCallback;
        private Action<string> onMatchFailedCallback;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeFirebase();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeFirebase()
        {
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
            {
                if (task.Result == DependencyStatus.Available)
                {
                    databaseRef = FirebaseDatabase.DefaultInstance.RootReference;
                    matchmakingQueueRef = databaseRef.Child("matchmaking_queue");
                    activeMatchesRef = databaseRef.Child("active_matches");

                    // Get current user
                    if (FirebaseAuth.DefaultInstance.CurrentUser != null)
                    {
                        currentUserId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
                        currentUserName = FirebaseAuth.DefaultInstance.CurrentUser.DisplayName ?? "Player";
                    }
                    else
                    {
                        // Use anonymous ID for testing
                        currentUserId = SystemInfo.deviceUniqueIdentifier;
                        currentUserName = PlayerPrefs.GetString("PlayerName", "Player");
                    }

                    Debug.Log($"[MatchmakingService] Initialized - User: {currentUserId}");
                }
                else
                {
                    Debug.LogError($"[MatchmakingService] Firebase init failed: {task.Result}");
                }
            });
        }

        #region Public Methods

        /// <summary>
        /// Find a match for a single game type
        /// </summary>
        public void FindMatch(GameType gameType, bool isCashMatch, Action<string, string> onFound, Action<string> onFailed)
        {
            if (isSearching)
            {
                Debug.LogWarning("[MatchmakingService] Already searching for a match");
                return;
            }

            onMatchFoundCallback = onFound;
            onMatchFailedCallback = onFailed;

            searchCoroutine = StartCoroutine(SearchForMatch(gameType.ToString(), isCashMatch));
        }

        /// <summary>
        /// Find a match for Cognitive Sprint with selected games
        /// </summary>
        public void FindCognitiveSprintMatch(List<GameType> selectedGames, Action<string, string> onFound, Action<string> onFailed)
        {
            if (isSearching)
            {
                Debug.LogWarning("[MatchmakingService] Already searching for a match");
                return;
            }

            onMatchFoundCallback = onFound;
            onMatchFailedCallback = onFailed;

            // Create a unique key for this combination of games
            string gamesKey = "CognitiveSprint_" + string.Join("_", selectedGames);
            searchCoroutine = StartCoroutine(SearchForMatch(gamesKey, false));
        }

        /// <summary>
        /// Cancel current matchmaking search
        /// </summary>
        public void CancelMatchmaking()
        {
            if (!isSearching) return;

            isSearching = false;

            if (searchCoroutine != null)
            {
                StopCoroutine(searchCoroutine);
                searchCoroutine = null;
            }

            // Remove from queue
            if (!string.IsNullOrEmpty(currentQueueEntryKey))
            {
                RemoveFromQueue(currentQueueEntryKey);
            }

            Debug.Log("[MatchmakingService] Matchmaking cancelled");
        }

        #endregion

        #region Matchmaking Logic

        private IEnumerator SearchForMatch(string gameKey, bool isCashMatch)
        {
            isSearching = true;
            float searchStartTime = Time.time;

            Debug.Log($"[MatchmakingService] Starting search for: {gameKey}");

            // First, check if there's someone already waiting
            string opponentId = null;
            string opponentName = null;
            string opponentQueueKey = null;

            // Query the queue for the same game type
            var queueQuery = matchmakingQueueRef
                .Child(gameKey)
                .OrderByChild("timestamp")
                .LimitToFirst(1);

            bool checkComplete = false;
            string existingMatchKey = null;

            queueQuery.GetValueAsync().ContinueWith(task =>
            {
                if (task.IsCompleted && !task.IsFaulted && task.Result.Exists)
                {
                    foreach (var child in task.Result.Children)
                    {
                        var data = child.Value as Dictionary<string, object>;
                        if (data != null)
                        {
                            string odId = data["userId"].ToString();
                            // Don't match with yourself
                            if (odId != currentUserId)
                            {
                                opponentId = odId;
                                opponentName = data.ContainsKey("userName") ? data["userName"].ToString() : "Opponent";
                                opponentQueueKey = child.Key;
                                existingMatchKey = child.Key;
                            }
                        }
                    }
                }
                checkComplete = true;
            });

            // Wait for the query to complete
            while (!checkComplete)
            {
                yield return null;
            }

            if (!string.IsNullOrEmpty(opponentId))
            {
                // Found an opponent! Create match
                Debug.Log($"[MatchmakingService] Found waiting opponent: {opponentName}");

                // Remove opponent from queue
                RemoveFromQueue(opponentQueueKey, gameKey);

                // Create match
                string matchId = CreateMatch(gameKey, currentUserId, currentUserName, opponentId, opponentName);

                isSearching = false;
                onMatchFoundCallback?.Invoke(matchId, opponentName);
                yield break;
            }

            // No opponent found, add ourselves to the queue
            Debug.Log("[MatchmakingService] No opponent found, joining queue...");
            currentQueueEntryKey = AddToQueue(gameKey, isCashMatch);

            // Poll for match
            while (isSearching)
            {
                // Check if we've been matched
                bool matched = false;
                string matchId = null;
                string matchedOpponentName = null;

                var matchCheck = activeMatchesRef
                    .OrderByChild("player1Id")
                    .EqualTo(currentUserId)
                    .LimitToFirst(1);

                bool matchCheckComplete = false;

                matchCheck.GetValueAsync().ContinueWith(task =>
                {
                    if (task.IsCompleted && !task.IsFaulted && task.Result.Exists)
                    {
                        foreach (var child in task.Result.Children)
                        {
                            var data = child.Value as Dictionary<string, object>;
                            if (data != null && data.ContainsKey("status") && data["status"].ToString() == "ready")
                            {
                                matched = true;
                                matchId = child.Key;
                                matchedOpponentName = data.ContainsKey("player2Name") ? data["player2Name"].ToString() : "Opponent";
                            }
                        }
                    }
                    matchCheckComplete = true;
                });

                while (!matchCheckComplete)
                {
                    yield return null;
                }

                // Also check if we're player2 in a match
                if (!matched)
                {
                    var matchCheck2 = activeMatchesRef
                        .OrderByChild("player2Id")
                        .EqualTo(currentUserId)
                        .LimitToFirst(1);

                    matchCheckComplete = false;

                    matchCheck2.GetValueAsync().ContinueWith(task =>
                    {
                        if (task.IsCompleted && !task.IsFaulted && task.Result.Exists)
                        {
                            foreach (var child in task.Result.Children)
                            {
                                var data = child.Value as Dictionary<string, object>;
                                if (data != null && data.ContainsKey("status") && data["status"].ToString() == "ready")
                                {
                                    matched = true;
                                    matchId = child.Key;
                                    matchedOpponentName = data.ContainsKey("player1Name") ? data["player1Name"].ToString() : "Opponent";
                                }
                            }
                        }
                        matchCheckComplete = true;
                    });

                    while (!matchCheckComplete)
                    {
                        yield return null;
                    }
                }

                if (matched)
                {
                    // Remove from queue
                    RemoveFromQueue(currentQueueEntryKey, gameKey);

                    isSearching = false;
                    onMatchFoundCallback?.Invoke(matchId, matchedOpponentName);
                    yield break;
                }

                // Check timeout
                if (Time.time - searchStartTime >= matchTimeout)
                {
                    RemoveFromQueue(currentQueueEntryKey, gameKey);
                    isSearching = false;
                    onMatchFailedCallback?.Invoke("Search timed out. No opponents found.");
                    yield break;
                }

                yield return new WaitForSeconds(pollInterval);
            }
        }

        private string AddToQueue(string gameKey, bool isCashMatch)
        {
            var queueEntry = new Dictionary<string, object>
            {
                { "userId", currentUserId },
                { "userName", currentUserName },
                { "timestamp", ServerValue.Timestamp },
                { "isCashMatch", isCashMatch },
                { "status", "searching" }
            };

            var newEntryRef = matchmakingQueueRef.Child(gameKey).Push();
            newEntryRef.SetValueAsync(queueEntry);

            Debug.Log($"[MatchmakingService] Added to queue: {newEntryRef.Key}");
            return newEntryRef.Key;
        }

        private void RemoveFromQueue(string entryKey, string gameKey = null)
        {
            if (string.IsNullOrEmpty(entryKey)) return;

            if (!string.IsNullOrEmpty(gameKey))
            {
                matchmakingQueueRef.Child(gameKey).Child(entryKey).RemoveValueAsync();
            }

            Debug.Log($"[MatchmakingService] Removed from queue: {entryKey}");
        }

        private string CreateMatch(string gameKey, string player1Id, string player1Name, string player2Id, string player2Name)
        {
            var matchData = new Dictionary<string, object>
            {
                { "gameKey", gameKey },
                { "player1Id", player1Id },
                { "player1Name", player1Name },
                { "player2Id", player2Id },
                { "player2Name", player2Name },
                { "status", "ready" },
                { "createdAt", ServerValue.Timestamp },
                { "player1Score", 0 },
                { "player2Score", 0 },
                { "player1Finished", false },
                { "player2Finished", false }
            };

            var newMatchRef = activeMatchesRef.Push();
            newMatchRef.SetValueAsync(matchData);

            Debug.Log($"[MatchmakingService] Match created: {newMatchRef.Key}");
            return newMatchRef.Key;
        }

        #endregion

        #region Match Results

        /// <summary>
        /// Submit match result when game ends
        /// </summary>
        public void SubmitMatchResult(string matchId, float score, float time, int errors)
        {
            if (string.IsNullOrEmpty(matchId)) return;

            // Determine if we're player1 or player2
            activeMatchesRef.Child(matchId).GetValueAsync().ContinueWith(task =>
            {
                if (task.IsCompleted && !task.IsFaulted && task.Result.Exists)
                {
                    var data = task.Result.Value as Dictionary<string, object>;
                    if (data != null)
                    {
                        bool isPlayer1 = data["player1Id"].ToString() == currentUserId;
                        string scoreKey = isPlayer1 ? "player1Score" : "player2Score";
                        string timeKey = isPlayer1 ? "player1Time" : "player2Time";
                        string finishedKey = isPlayer1 ? "player1Finished" : "player2Finished";

                        var updates = new Dictionary<string, object>
                        {
                            { scoreKey, score },
                            { timeKey, time },
                            { finishedKey, true }
                        };

                        activeMatchesRef.Child(matchId).UpdateChildrenAsync(updates);
                        Debug.Log($"[MatchmakingService] Result submitted for match: {matchId}");
                    }
                }
            });
        }

        /// <summary>
        /// Listen for opponent's result
        /// </summary>
        public void ListenForOpponentResult(string matchId, Action<float, float> onOpponentFinished)
        {
            activeMatchesRef.Child(matchId).ValueChanged += (sender, args) =>
            {
                if (args.Snapshot.Exists)
                {
                    var data = args.Snapshot.Value as Dictionary<string, object>;
                    if (data != null)
                    {
                        bool isPlayer1 = data["player1Id"].ToString() == currentUserId;
                        string opponentFinishedKey = isPlayer1 ? "player2Finished" : "player1Finished";
                        string opponentScoreKey = isPlayer1 ? "player2Score" : "player1Score";
                        string opponentTimeKey = isPlayer1 ? "player2Time" : "player1Time";

                        if (data.ContainsKey(opponentFinishedKey) && (bool)data[opponentFinishedKey])
                        {
                            float opponentScore = Convert.ToSingle(data[opponentScoreKey]);
                            float opponentTime = data.ContainsKey(opponentTimeKey) ? Convert.ToSingle(data[opponentTimeKey]) : 0f;
                            onOpponentFinished?.Invoke(opponentScore, opponentTime);
                        }
                    }
                }
            };
        }

        #endregion

        private void OnDestroy()
        {
            CancelMatchmaking();
        }

        private void OnApplicationQuit()
        {
            CancelMatchmaking();
        }
    }
}
