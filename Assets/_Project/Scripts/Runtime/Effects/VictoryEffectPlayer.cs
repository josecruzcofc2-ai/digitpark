using UnityEngine;
using DigitPark.Services;

namespace DigitPark.Effects
{
    /// <summary>
    /// Componente runtime que se adjunta a cada prefab de efecto de victoria.
    /// Aplica colores del VictoryEffectData y se autodestruye al completar.
    /// </summary>
    public class VictoryEffectPlayer : MonoBehaviour
    {
        private VictoryEffectData _effectData;
        private ParticleSystem[] _particleSystems;

        public event System.Action OnEffectComplete;

        public void Initialize(VictoryEffectData data)
        {
            _effectData = data;
            _particleSystems = GetComponentsInChildren<ParticleSystem>(true);

            ApplyColors();

            // Auto-destroy after longest particle system duration + lifetime
            float maxDuration = 0f;
            foreach (var ps in _particleSystems)
            {
                var main = ps.main;
                float total = main.duration + main.startLifetime.constantMax;
                if (total > maxDuration) maxDuration = total;
            }

            Destroy(gameObject, Mathf.Max(maxDuration + 0.5f, 3f));
        }

        private void ApplyColors()
        {
            if (_effectData == null || _particleSystems == null) return;

            foreach (var ps in _particleSystems)
            {
                var main = ps.main;
                main.startColor = new ParticleSystem.MinMaxGradient(
                    _effectData.primaryColor, _effectData.secondaryColor);
            }
        }

        private void OnDestroy()
        {
            OnEffectComplete?.Invoke();
        }
    }
}
