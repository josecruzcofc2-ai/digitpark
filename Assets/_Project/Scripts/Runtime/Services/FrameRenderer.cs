using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace DigitPark.Services
{
    /// <summary>
    /// Componente runtime que aplica el frame equipado del jugador al Image del AvatarFrame.
    /// Se agrega a todos los GameObjects "AvatarFrame" via UIBuilders.
    /// En Edit Mode aplica color fallback (#808080) — en Play Mode aplica el frame real.
    /// </summary>
    public class FrameRenderer : MonoBehaviour
    {
        public enum RenderMode
        {
            Full,       // Todas las animaciones (perfil propio, sala de espera, resultados)
            Reduced,    // Solo breathing/color, sin partículas (leaderboard, lista de amigos)
            Static      // Solo color, sin DOTween (miniaturas <48px)
        }

        private RenderMode _renderMode = RenderMode.Reduced;
        private Image _image;

        private static readonly Color FALLBACK_COLOR = new Color(0.502f, 0.502f, 0.502f); // #808080

        private void Awake()
        {
            // Tag usado por CashThemeForcer para excluir este GO del recolor
            if (!gameObject.CompareTag("FrameLayer"))
            {
                try { gameObject.tag = "FrameLayer"; }
                catch { /* Tag no creado en TagManager — usar alternativa por nombre */ }
            }
        }

        private void OnEnable()
        {
            _image = GetComponent<Image>();
            if (_image == null) return;

            ApplyFrame();
        }

        private void OnDisable()
        {
            DOTween.Kill(gameObject);
        }

        /// <summary>
        /// Configura el modo de renderizado antes de que OnEnable sea llamado.
        /// Llamar desde UIBuilders inmediatamente tras AddComponent.
        /// </summary>
        public void SetRenderMode(RenderMode mode)
        {
            _renderMode = mode;
        }

        private void ApplyFrame()
        {
            var service = PlayerFrameService.Instance;
            if (service == null)
            {
                _image.color = FALLBACK_COLOR;
                return;
            }

            var frame = service.GetEquippedFrame();
            if (frame == null)
            {
                // Intentar con "basic" como fallback
                frame = service.GetFrameData("basic");
                if (frame == null)
                {
                    _image.color = FALLBACK_COLOR;
                    return;
                }
            }

            _image.color = frame.primaryColor;

            if (!frame.isAnimated || _renderMode == RenderMode.Static)
                return;

            // Excluir animaciones de intensidad alta en modo Reduced
            if (_renderMode == RenderMode.Reduced && frame.animationIntensity >= 3)
                return;

            PlayAnimation(frame);
        }

        private void PlayAnimation(FrameData frame)
        {
            Color primary = frame.primaryColor;
            Color secondary = frame.secondaryColor;
            Color accent = frame.accentColor;

            switch (frame.animationType)
            {
                case FrameAnimationType.Shimmer:
                    _image.DOColor(secondary, 1.5f)
                        .SetLink(gameObject)
                        .SetLoops(-1, LoopType.Yoyo)
                        .SetEase(Ease.InOutSine);
                    break;

                case FrameAnimationType.LightningFlash:
                    PlayLightningFlash(primary);
                    break;

                case FrameAnimationType.GodRays:
                    var godRaysBright = new Color(
                        Mathf.Min(primary.r * 1.3f, 1f),
                        Mathf.Min(primary.g * 1.3f, 1f),
                        Mathf.Min(primary.b * 1.3f, 1f));
                    _image.DOColor(godRaysBright, 2f)
                        .SetLink(gameObject)
                        .SetLoops(-1, LoopType.Yoyo)
                        .SetEase(Ease.InOutSine);
                    break;

                case FrameAnimationType.GlitchBurst:
                    PlayGlitchBurst(primary, secondary, accent);
                    break;

                case FrameAnimationType.StarParticles:
                    _image.DOColor(accent, 2f)
                        .SetLink(gameObject)
                        .SetLoops(-1, LoopType.Yoyo)
                        .SetEase(Ease.InOutQuad);
                    break;

                case FrameAnimationType.CrackGlow:
                    DOTween.Sequence()
                        .SetLink(gameObject)
                        .SetLoops(-1, LoopType.Restart)
                        .Append(_image.DOColor(accent, 0.4f).SetEase(Ease.InQuad))
                        .Append(_image.DOColor(primary, 0.6f).SetEase(Ease.OutQuad))
                        .AppendInterval(1.2f);
                    break;

                case FrameAnimationType.CrownPulse:
                    _image.DOColor(secondary, 0.8f)
                        .SetLink(gameObject)
                        .SetLoops(-1, LoopType.Yoyo)
                        .SetEase(Ease.InOutSine);
                    transform.DOScale(1.05f, 0.8f)
                        .SetLink(gameObject)
                        .SetLoops(-1, LoopType.Yoyo)
                        .SetEase(Ease.InOutSine);
                    break;

                case FrameAnimationType.HueRotation:
                case FrameAnimationType.SpectrumCycle:
                    PlaySpectrumCycle();
                    break;

                case FrameAnimationType.ElectricArcs:
                    PlayElectricArcs(primary, secondary, accent);
                    break;

                case FrameAnimationType.AuroraRibbons:
                    PlayAuroraRibbons(primary, secondary, accent);
                    break;

                case FrameAnimationType.LayeredFire:
                    PlayLayeredFire(primary, secondary, accent);
                    break;

                case FrameAnimationType.PlasmaFire:
                    PlayPlasmaFire(primary, secondary, accent);
                    break;

                default:
                    _image.color = primary;
                    break;
            }
        }

        private void PlayLightningFlash(Color baseColor)
        {
            Color white = Color.white;
            DOTween.Sequence()
                .SetLink(gameObject)
                .SetLoops(-1, LoopType.Restart)
                .AppendInterval(Random.Range(0.8f, 2f))
                .Append(_image.DOColor(white, 0.05f))
                .Append(_image.DOColor(baseColor, 0.15f))
                .AppendInterval(Random.Range(0.1f, 0.3f))
                .Append(_image.DOColor(white, 0.05f))
                .Append(_image.DOColor(baseColor, 0.2f));
        }

        private void PlayGlitchBurst(Color primary, Color secondary, Color accent)
        {
            DOTween.Sequence()
                .SetLink(gameObject)
                .SetLoops(-1, LoopType.Restart)
                .AppendInterval(Random.Range(1f, 2.5f))
                .Append(_image.DOColor(accent, 0.05f))
                .Append(_image.DOColor(secondary, 0.05f))
                .Append(_image.DOColor(accent, 0.05f))
                .Append(_image.DOColor(primary, 0.1f))
                .AppendInterval(0.08f)
                .Append(_image.DOColor(accent, 0.04f))
                .Append(_image.DOColor(primary, 0.12f));
        }

        /// <summary>
        /// ElectricArcs: arcos eléctricos — ráfagas de brillo con shake sutil.
        /// Usado por: Plasma Spark ($0.99)
        /// </summary>
        private void PlayElectricArcs(Color primary, Color secondary, Color accent)
        {
            Color spark = accent.a > 0.01f ? accent : Color.white;

            DOTween.Sequence()
                .SetLink(gameObject)
                .SetLoops(-1, LoopType.Restart)
                // Idle: brillo sutil entre primary y secondary
                .Append(_image.DOColor(secondary, 0.6f).SetEase(Ease.InOutSine))
                .Append(_image.DOColor(primary, 0.6f).SetEase(Ease.InOutSine))
                // Spark burst: flash rápido + shake
                .Append(_image.DOColor(spark, 0.03f))
                .Join(transform.DOShakePosition(0.15f, 2f, 20, 90f, false, false).SetRelative(true))
                .Append(_image.DOColor(primary, 0.08f))
                .AppendInterval(Random.Range(0.3f, 0.6f))
                // Second arc (double-tap feel)
                .Append(_image.DOColor(spark, 0.03f))
                .Join(transform.DOShakePosition(0.1f, 1.5f, 15, 90f, false, false).SetRelative(true))
                .Append(_image.DOColor(secondary, 0.12f).SetEase(Ease.OutQuad))
                .Append(_image.DOColor(primary, 0.3f).SetEase(Ease.InOutSine))
                .AppendInterval(Random.Range(0.5f, 1.2f));
        }

        /// <summary>
        /// AuroraRibbons: transiciones suaves entre colores aurora con escala ondulante.
        /// Usado por: Aurora Borealis ($3.99)
        /// </summary>
        private void PlayAuroraRibbons(Color primary, Color secondary, Color accent)
        {
            // Colores aurora: primary → teal → secondary(purple) → pink → primary
            Color teal = new Color(0f, 0.9f, 0.7f);
            Color pink = new Color(0.8f, 0.3f, 0.6f);

            // Color ribbon
            DOTween.Sequence()
                .SetLink(gameObject)
                .SetLoops(-1, LoopType.Restart)
                .Append(_image.DOColor(teal, 1.8f).SetEase(Ease.InOutSine))
                .Append(_image.DOColor(secondary, 2.0f).SetEase(Ease.InOutSine))
                .Append(_image.DOColor(pink, 1.5f).SetEase(Ease.InOutSine))
                .Append(_image.DOColor(primary, 1.8f).SetEase(Ease.InOutSine));

            // Subtle wave scale (sinusoidal)
            DOTween.Sequence()
                .SetLink(gameObject)
                .SetLoops(-1, LoopType.Yoyo)
                .Append(transform.DOScaleX(1.03f, 2.5f).SetEase(Ease.InOutSine))
                .Join(transform.DOScaleY(0.97f, 2.5f).SetEase(Ease.InOutSine));
        }

        /// <summary>
        /// LayeredFire: fuego multicapa con intensidad creciente y pulsos de calor.
        /// Usado por: Infernal God ($9.99)
        /// </summary>
        private void PlayLayeredFire(Color primary, Color secondary, Color accent)
        {
            // primary: #1A0000 (dark red), secondary: #FF2200 (fire), accent: bright fire
            Color midFlame = Color.Lerp(primary, secondary, 0.6f);
            Color hotCore = accent.a > 0.01f ? accent : new Color(1f, 0.6f, 0f); // orange fallback

            // Base fire flicker (continuous)
            DOTween.Sequence()
                .SetLink(gameObject)
                .SetLoops(-1, LoopType.Restart)
                .Append(_image.DOColor(secondary, 0.3f).SetEase(Ease.InQuad))
                .Append(_image.DOColor(midFlame, 0.2f).SetEase(Ease.OutQuad))
                .Append(_image.DOColor(hotCore, 0.15f).SetEase(Ease.InQuad))
                .Append(_image.DOColor(secondary, 0.25f).SetEase(Ease.OutQuad))
                .Append(_image.DOColor(primary, 0.4f).SetEase(Ease.InOutSine))
                // Heat pulse: flare up
                .Append(_image.DOColor(hotCore, 0.1f).SetEase(Ease.InQuad))
                .Append(_image.DOColor(secondary, 0.2f).SetEase(Ease.OutCubic))
                .Append(_image.DOColor(midFlame, 0.3f).SetEase(Ease.OutQuad))
                .AppendInterval(Random.Range(0.1f, 0.3f));

            // Scale flicker (fire lick effect)
            DOTween.Sequence()
                .SetLink(gameObject)
                .SetLoops(-1, LoopType.Yoyo)
                .Append(transform.DOScale(1.02f, 0.35f).SetEase(Ease.InOutSine))
                .Append(transform.DOScale(0.99f, 0.25f).SetEase(Ease.InOutSine))
                .Append(transform.DOScale(1.01f, 0.3f).SetEase(Ease.InOutSine));
        }

        /// <summary>
        /// PlasmaFire: fuego eléctrico cyan con flashes de energía y vibración.
        /// Usado por: Quantum Fire ($2.99)
        /// </summary>
        private void PlayPlasmaFire(Color primary, Color secondary, Color accent)
        {
            // primary: #0088FF (blue), secondary: #00FFFF (cyan), accent: #FFFFFF (white)
            Color plasma = Color.Lerp(primary, secondary, 0.5f);
            Color flash = accent.a > 0.01f ? accent : Color.white;

            // Plasma flicker (electric fire rhythm)
            DOTween.Sequence()
                .SetLink(gameObject)
                .SetLoops(-1, LoopType.Restart)
                .Append(_image.DOColor(secondary, 0.25f).SetEase(Ease.InQuad))
                .Append(_image.DOColor(primary, 0.35f).SetEase(Ease.OutQuad))
                .Append(_image.DOColor(plasma, 0.2f).SetEase(Ease.InOutSine))
                // Energy discharge flash
                .Append(_image.DOColor(flash, 0.04f))
                .Append(_image.DOColor(secondary, 0.1f).SetEase(Ease.OutQuad))
                .Append(_image.DOColor(primary, 0.3f).SetEase(Ease.InOutSine))
                .AppendInterval(Random.Range(0.2f, 0.5f))
                // Second discharge
                .Append(_image.DOColor(flash, 0.04f))
                .Join(transform.DOShakePosition(0.08f, 1.5f, 12, 90f, false, false).SetRelative(true))
                .Append(_image.DOColor(plasma, 0.15f).SetEase(Ease.OutCubic))
                .Append(_image.DOColor(primary, 0.25f).SetEase(Ease.InOutSine))
                .AppendInterval(Random.Range(0.1f, 0.4f));
        }

        private void PlaySpectrumCycle()
        {
            // 8-step spectrum loop: R→Y→G→C→B→M→R
            Color[] spectrum = new Color[]
            {
                new Color(1f, 0f, 0f),
                new Color(1f, 0.5f, 0f),
                new Color(1f, 1f, 0f),
                new Color(0f, 1f, 0f),
                new Color(0f, 1f, 1f),
                new Color(0f, 0f, 1f),
                new Color(0.5f, 0f, 1f),
                new Color(1f, 0f, 0.5f),
            };

            var seq = DOTween.Sequence()
                .SetLink(gameObject)
                .SetLoops(-1, LoopType.Restart);

            foreach (var col in spectrum)
                seq.Append(_image.DOColor(col, 0.5f).SetEase(Ease.Linear));
        }
    }
}
