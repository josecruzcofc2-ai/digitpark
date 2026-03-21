using System;
using System.Collections.Generic;
using UnityEngine;

namespace DigitPark.Services
{
    /// <summary>
    /// Global Android back button handler (KeyCode.Escape).
    /// Scenes push a callback onto the stack; pressing back invokes the top callback.
    /// If the stack is empty, falls back to SceneNavigator.GoBack().
    /// Usage: BackButtonManager.Instance?.Push(() => DoSomething());
    ///        BackButtonManager.Instance?.Pop();  // call when scene/panel closes
    /// </summary>
    public class BackButtonManager : MonoBehaviour
    {
        public static BackButtonManager Instance { get; private set; }

        private readonly Stack<Action> _handlers = new Stack<Action>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            if (!Input.GetKeyDown(KeyCode.Escape)) return;

            if (_handlers.Count > 0)
            {
                _handlers.Pop()?.Invoke();
            }
            else
            {
                DigitPark.Navigation.SceneNavigator.Instance?.GoBack();
            }
        }

        /// <summary>Push a handler for the current context (scene or overlay panel).</summary>
        public void Push(Action handler) => _handlers.Push(handler);

        /// <summary>Remove the top handler when the context that pushed it is closed.</summary>
        public void Pop()
        {
            if (_handlers.Count > 0) _handlers.Pop();
        }

        /// <summary>Remove all handlers (e.g. on scene load).</summary>
        public void Clear() => _handlers.Clear();

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
