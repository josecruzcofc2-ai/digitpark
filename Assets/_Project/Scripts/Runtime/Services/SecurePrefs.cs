using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace DigitPark.Services
{
    /// <summary>
    /// Encrypted PlayerPrefs wrapper for sensitive data.
    /// Key is derived from device unique identifier — not perfect but better than plaintext.
    /// Use for: auth tokens, user IDs, sensitive settings.
    /// </summary>
    public static class SecurePrefs
    {
        private static byte[] _key;
        private static byte[] _iv;

        private static void EnsureKey()
        {
            if (_key != null) return;
            string deviceId = SystemInfo.deviceUniqueIdentifier;
            if (string.IsNullOrEmpty(deviceId)) deviceId = "DP_FallbackKey_2026";
            using var sha = SHA256.Create();
            byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(deviceId + "DP_Salt_v1"));
            _key = new byte[16];
            _iv  = new byte[16];
            Array.Copy(hash, 0, _key, 0, 16);
            Array.Copy(hash, 16, _iv, 0, 16);
        }

        private static string Encrypt(string plain)
        {
            EnsureKey();
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV  = _iv;
            byte[] input = Encoding.UTF8.GetBytes(plain);
            byte[] encrypted = aes.CreateEncryptor().TransformFinalBlock(input, 0, input.Length);
            return Convert.ToBase64String(encrypted);
        }

        private static string Decrypt(string cipher)
        {
            EnsureKey();
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV  = _iv;
            byte[] input = Convert.FromBase64String(cipher);
            byte[] decrypted = aes.CreateDecryptor().TransformFinalBlock(input, 0, input.Length);
            return Encoding.UTF8.GetString(decrypted);
        }

        public static void SetString(string key, string value)
        {
            try { PlayerPrefs.SetString(key, Encrypt(value)); PlayerPrefs.Save(); }
            catch (Exception ex) { Debug.LogWarning($"[SecurePrefs] SetString failed: {ex.Message}"); }
        }

        public static string GetString(string key, string defaultValue = "")
        {
            string raw = PlayerPrefs.GetString(key, "");
            if (string.IsNullOrEmpty(raw)) return defaultValue;
            try { return Decrypt(raw); }
            catch { return defaultValue; }
        }

        public static void SetInt(string key, int value) => SetString(key, value.ToString());

        public static int GetInt(string key, int defaultValue = 0)
        {
            string s = GetString(key, null);
            return s != null && int.TryParse(s, out int v) ? v : defaultValue;
        }

        public static bool HasKey(string key) => PlayerPrefs.HasKey(key);

        public static void DeleteKey(string key) { PlayerPrefs.DeleteKey(key); PlayerPrefs.Save(); }
    }
}
