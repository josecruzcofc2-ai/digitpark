using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using DigitPark.Games;
using DigitPark.UI;

namespace DigitPark.Managers
{
    /// <summary>
    /// Manager singleton para mostrar paneles de resultado post-minijuego
    /// </summary>
    public class ResultPanelManager : MonoBehaviour
    {
        private static ResultPanelManager _instance;
        public static ResultPanelManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("ResultPanelManager");
                    _instance = go.AddComponent<ResultPanelManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        // Panel activo actual
        private GameObject currentPanel;

        private const string PREFAB_BASE = "Prefabs/WinPanels/";

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (_instance == this)
                _instance = null;
        }

        // ====================================================================
        // HELPERS
        // ====================================================================

        private GameObject InstantiatePanel(GameObject prefab, string panelType)
        {
            CleanupCurrentPanel();

            if (prefab == null)
            {
                Debug.LogError($"[ResultPanelManager] {panelType} prefab not assigned!");
                return null;
            }

            Canvas canvas = UICanvasHelper.FindMainCanvas();
            if (canvas == null)
            {
                Debug.LogError("[ResultPanelManager] No Canvas found in scene!");
                return null;
            }

            currentPanel = Instantiate(prefab, canvas.transform);
            currentPanel.SetActive(true);
            return currentPanel;
        }

        private void CleanupCurrentPanel()
        {
            if (currentPanel != null)
            {
                Destroy(currentPanel);
                currentPanel = null;
            }
        }

        private void NavigateTo(string sceneName)
        {
            CleanupCurrentPanel();

            // Limpiar sesión si existe
            if (GameSessionManager.Instance != null)
            {
                GameSessionManager.Instance.EndSession();
            }

            SceneManager.LoadScene(sceneName);
        }

    }
}
