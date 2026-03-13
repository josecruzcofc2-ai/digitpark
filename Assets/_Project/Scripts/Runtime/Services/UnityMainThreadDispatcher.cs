using System;
using System.Collections.Generic;
using UnityEngine;

namespace DigitPark.Services
{
    /// <summary>
    /// Dispatcher singleton que ejecuta acciones en el hilo principal de Unity.
    /// Usado para marshalizar callbacks de Firebase/FCM (que llegan en background threads)
    /// hacia el main thread donde las APIs de Unity son accesibles.
    /// </summary>
    public class UnityMainThreadDispatcher : MonoBehaviour
    {
        private static UnityMainThreadDispatcher _instance;
        private readonly Queue<Action> _queue = new Queue<Action>();
        private readonly object _lock = new object();

        public static UnityMainThreadDispatcher Instance()
        {
            if (_instance == null)
            {
                var go = new GameObject("UnityMainThreadDispatcher");
                _instance = go.AddComponent<UnityMainThreadDispatcher>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void Update()
        {
            lock (_lock)
            {
                while (_queue.Count > 0)
                {
                    try
                    {
                        _queue.Dequeue()?.Invoke();
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
            }
        }

        public void Enqueue(Action action)
        {
            if (action == null) return;
            lock (_lock)
            {
                _queue.Enqueue(action);
            }
        }

        private void OnDestroy()
        {
            if (_instance == this)
                _instance = null;
        }
    }
}
