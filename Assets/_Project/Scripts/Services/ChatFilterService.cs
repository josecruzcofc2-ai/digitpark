using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

namespace DigitPark.Services
{
    public class ChatFilterService : MonoBehaviour
    {
        public static ChatFilterService Instance { get; private set; }

        public struct FilterResult
        {
            public string FilteredMessage;
            public bool ContainsProfanity;
        }

        private readonly HashSet<string> _bannedWords = new HashSet<string>();
        private readonly HashSet<string> _allowedWords = new HashSet<string>();

        // Leet speak substitutions
        private static readonly Dictionary<char, char> LeetMap = new Dictionary<char, char>
        {
            { '@', 'a' }, { '4', 'a' },
            { '3', 'e' },
            { '1', 'i' }, { '!', 'i' },
            { '0', 'o' },
            { '$', 's' }, { '5', 's' },
            { '7', 't' }, { '+', 't' },
            { '8', 'b' },
            { '9', 'g' }
        };

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            LoadWordLists();
        }

        private void LoadWordLists()
        {
            // Load banned words
            var bannedFile = Resources.Load<TextAsset>("BannedWords");
            if (bannedFile != null)
            {
                foreach (var line in bannedFile.text.Split('\n'))
                {
                    var trimmed = line.Trim();
                    if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("#")) continue;

                    // Format: LANG:word or just word
                    int colonIdx = trimmed.IndexOf(':');
                    string word = colonIdx >= 0 ? trimmed.Substring(colonIdx + 1).Trim() : trimmed;
                    if (!string.IsNullOrEmpty(word))
                        _bannedWords.Add(word.ToLowerInvariant());
                }
                Debug.Log($"[ChatFilter] Loaded {_bannedWords.Count} banned words");
            }
            else
            {
                Debug.LogWarning("[ChatFilter] BannedWords.txt not found in Resources!");
            }

            // Load allowed words (whitelist)
            var allowedFile = Resources.Load<TextAsset>("AllowedWords");
            if (allowedFile != null)
            {
                foreach (var line in allowedFile.text.Split('\n'))
                {
                    var trimmed = line.Trim();
                    if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("#")) continue;
                    _allowedWords.Add(trimmed.ToLowerInvariant());
                }
                Debug.Log($"[ChatFilter] Loaded {_allowedWords.Count} allowed words");
            }
        }

        // ── Public API ──

        public FilterResult Filter(string message)
        {
            if (string.IsNullOrEmpty(message))
                return new FilterResult { FilteredMessage = message, ContainsProfanity = false };

            bool foundProfanity = false;
            string[] words = message.Split(' ', '\t');
            var result = new StringBuilder();

            // Phase A: Word-by-word check
            for (int i = 0; i < words.Length; i++)
            {
                if (i > 0) result.Append(' ');

                string word = words[i];
                string normalized = NormalizeWord(word);

                if (normalized.Length > 0 && _bannedWords.Contains(normalized))
                {
                    result.Append(new string('*', word.Length));
                    foundProfanity = true;
                }
                else
                {
                    result.Append(word);
                }
            }

            // Phase B: Evasion detection (collapsed string substring check)
            string collapsedMessage = CollapseMessage(message);
            foreach (var banned in _bannedWords)
            {
                if (banned.Length < 3) continue; // Skip very short words to avoid false positives
                if (collapsedMessage.Contains(banned))
                {
                    // Check if this is a false positive (Scunthorpe protection)
                    if (!IsAllowedWord(message, banned))
                    {
                        // Re-censor: find and replace in the result
                        if (!foundProfanity)
                        {
                            // Need to censor the original message more aggressively
                            foundProfanity = true;
                        }
                        result = CensorEvasion(result, message, banned);
                    }
                }
            }

            return new FilterResult
            {
                FilteredMessage = result.ToString(),
                ContainsProfanity = foundProfanity
            };
        }

        public string Censor(string message)
        {
            return Filter(message).FilteredMessage;
        }

        public bool ContainsProfanity(string message)
        {
            return Filter(message).ContainsProfanity;
        }

        // ── Normalization Pipeline ──

        private string NormalizeWord(string word)
        {
            if (string.IsNullOrEmpty(word)) return string.Empty;

            var sb = new StringBuilder(word.Length);
            // Step 1: Lowercase
            string lower = word.ToLowerInvariant();
            // Step 2: Strip accents
            string noAccents = StripAccents(lower);
            // Step 3: Desubstitute leet + Step 4: Collapse repeats
            char lastChar = '\0';
            int repeatCount = 0;

            for (int i = 0; i < noAccents.Length; i++)
            {
                char c = noAccents[i];

                // Leet substitution
                if (LeetMap.TryGetValue(c, out char replacement))
                    c = replacement;

                // Skip non-alpha
                if (!char.IsLetter(c)) continue;

                // Collapse repeats (3+ → 1)
                if (c == lastChar)
                {
                    repeatCount++;
                    if (repeatCount >= 2) continue; // Skip 3rd+ repeat
                }
                else
                {
                    repeatCount = 0;
                }

                lastChar = c;
                sb.Append(c);
            }

            return sb.ToString();
        }

        private string CollapseMessage(string message)
        {
            if (string.IsNullOrEmpty(message)) return string.Empty;

            string lower = message.ToLowerInvariant();
            string noAccents = StripAccents(lower);
            var sb = new StringBuilder(noAccents.Length);
            char lastChar = '\0';
            int repeatCount = 0;

            for (int i = 0; i < noAccents.Length; i++)
            {
                char c = noAccents[i];

                // Leet substitution
                if (LeetMap.TryGetValue(c, out char replacement))
                    c = replacement;

                // Strip non-alpha entirely
                if (!char.IsLetter(c)) continue;

                // Collapse repeats
                if (c == lastChar)
                {
                    repeatCount++;
                    if (repeatCount >= 2) continue;
                }
                else
                {
                    repeatCount = 0;
                }

                lastChar = c;
                sb.Append(c);
            }

            return sb.ToString();
        }

        private static string StripAccents(string text)
        {
            var normalized = text.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(normalized.Length);

            foreach (char c in normalized)
            {
                var category = CharUnicodeInfo.GetUnicodeCategory(c);
                if (category != UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }

            // Handle special cases
            return sb.ToString().Replace("ß", "ss");
        }

        // ── Scunthorpe Protection ──

        private bool IsAllowedWord(string originalMessage, string bannedWord)
        {
            string[] words = originalMessage.Split(' ', '\t');
            foreach (var word in words)
            {
                string normalized = NormalizeWord(word);
                if (normalized.Contains(bannedWord))
                {
                    // Check if the full normalized word is in the allowed list
                    if (_allowedWords.Contains(normalized))
                        return true;
                    // Also check the original word lowercase
                    if (_allowedWords.Contains(word.ToLowerInvariant()))
                        return true;
                }
            }
            return false;
        }

        private StringBuilder CensorEvasion(StringBuilder current, string original, string bannedWord)
        {
            // Find which original words contribute to the banned substring
            string[] words = original.Split(' ', '\t');
            var result = new StringBuilder();

            for (int i = 0; i < words.Length; i++)
            {
                if (i > 0) result.Append(' ');

                string word = words[i];
                string normalized = NormalizeWord(word);

                if (normalized.Contains(bannedWord) && !_allowedWords.Contains(normalized)
                    && !_allowedWords.Contains(word.ToLowerInvariant()))
                {
                    result.Append(new string('*', word.Length));
                }
                else
                {
                    // Preserve what we already have from Phase A
                    result.Append(current.ToString().Split(' ')[i < words.Length ? i : words.Length - 1]);
                }
            }

            return result;
        }
    }
}
