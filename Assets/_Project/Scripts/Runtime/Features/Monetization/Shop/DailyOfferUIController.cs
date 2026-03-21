using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using DigitPark.Services;
using DigitPark.Localization;

namespace DigitPark.Monetization
{
    /// <summary>
    /// Economy Rebalance V55 — Daily Offer UI Controller
    ///
    /// Connects DailyOfferService to the 3-slot Daily Offers UI in Shop.
    /// Finds slots by GO name: Daily_Free (slot 3), Daily_Gems (slot 2), Daily_Coins (slot 1).
    /// Updates text, prices, badges, and countdown timer every second.
    ///
    /// Economy role:
    /// - Slot 1 (theme 50% off) → F2P sees "Aurora Borealis 200 DG" → buys $1.99 pack → conversion
    /// - Slot 2 (cosmetic 35% off) → impulse buy for Dolphin with leftover DG
    /// - Slot 3 (free DC/XP) → daily login hook, +14% monthly F2P income
    /// </summary>
    public class DailyOfferUIController : MonoBehaviour
    {
        [Header("Slot UI References (auto-detected by name if null)")]
        [SerializeField] private GameObject slot1Root; // "Daily_Coins" or first daily item
        [SerializeField] private GameObject slot2Root; // "Daily_Gems" or second daily item
        [SerializeField] private GameObject slot3Root; // "Daily_Free" or third daily item
        [SerializeField] private TextMeshProUGUI timerText;

        // Cached per-slot UI elements
        private SlotUI[] _slotUIs = new SlotUI[3];
        private Coroutine _timerCoroutine;
        private Tween _timerFomoTween;
        private bool _entranceAnimated;

        private class SlotUI
        {
            public GameObject root;
            public TextMeshProUGUI nameText;
            public TextMeshProUGUI priceText;
            public TextMeshProUGUI originalPriceText;
            public TextMeshProUGUI badgeText;
            public Button button;
            public Image background;
        }

        private void OnEnable()
        {
            _entranceAnimated = false;
            AutoDetectSlots();
            RefreshUI();
            AnimateSlotEntrance();
            StartTimer();

            if (DailyOfferService.Instance != null)
            {
                DailyOfferService.Instance.OnOffersRefreshed += RefreshUI;
                DailyOfferService.Instance.OnOfferPurchased += OnOfferPurchased;
            }
        }

        private void OnDisable()
        {
            StopTimer();
            _timerFomoTween?.Kill();
            _timerFomoTween = null;

            if (DailyOfferService.Instance != null)
            {
                DailyOfferService.Instance.OnOffersRefreshed -= RefreshUI;
                DailyOfferService.Instance.OnOfferPurchased -= OnOfferPurchased;
            }
        }

        private void AutoDetectSlots()
        {
            if (slot1Root == null || slot2Root == null || slot3Root == null)
            {
                // Find by name in parent hierarchy (Shop DailyDealsSection)
                var parent = transform;
                // Try to find Items container
                Transform items = FindDeep(parent, "Items") ?? FindDeep(parent, "DailyDealsSection") ?? parent;

                slot3Root = slot3Root ?? FindChildGO(items, "Daily_Free");
                slot2Root = slot2Root ?? FindChildGO(items, "Daily_Gems");
                slot1Root = slot1Root ?? FindChildGO(items, "Daily_Coins");

                // Timer
                if (timerText == null)
                {
                    Transform timerT = FindDeep(parent, "Timer");
                    if (timerT != null)
                        timerText = timerT.GetComponent<TextMeshProUGUI>();
                }
            }

            _slotUIs[0] = CacheSlotUI(slot1Root);
            _slotUIs[1] = CacheSlotUI(slot2Root);
            _slotUIs[2] = CacheSlotUI(slot3Root);
        }

        private SlotUI CacheSlotUI(GameObject root)
        {
            if (root == null) return null;

            var slot = new SlotUI { root = root };
            slot.button = root.GetComponent<Button>();
            slot.background = root.GetComponent<Image>();

            // Find child TMP elements
            foreach (var tmp in root.GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                string goName = tmp.gameObject.name.ToLower();
                if (goName.Contains("name") || goName.Contains("item"))
                    slot.nameText = tmp;
                else if (goName.Contains("price") && !goName.Contains("orig"))
                    slot.priceText = tmp;
                else if (goName.Contains("orig"))
                    slot.originalPriceText = tmp;
                else if (goName.Contains("badge") || goName.Contains("discount"))
                    slot.badgeText = tmp;
            }

            // Fallback: use first/last TMPs
            var allTmps = root.GetComponentsInChildren<TextMeshProUGUI>(true);
            if (allTmps.Length >= 2)
            {
                if (slot.nameText == null) slot.nameText = allTmps[0];
                if (slot.priceText == null) slot.priceText = allTmps[allTmps.Length - 1];
            }
            else if (allTmps.Length == 1)
            {
                if (slot.nameText == null) slot.nameText = allTmps[0];
            }

            return slot;
        }

        public void RefreshUI()
        {
            var service = DailyOfferService.Instance;
            if (service == null || !service.IsInitialized) return;

            for (int i = 0; i < 3; i++)
            {
                var offer = service.GetOffer(i);
                var slotUI = _slotUIs[i];
                if (offer == null || slotUI == null) continue;

                // Name
                if (slotUI.nameText != null)
                    slotUI.nameText.text = offer.displayName;

                // Price
                if (slotUI.priceText != null)
                {
                    if (offer.isFree)
                        slotUI.priceText.text = AutoLocalizer.Get("free");
                    else if (offer.purchased)
                        slotUI.priceText.text = AutoLocalizer.Get("purchased");
                    else
                    {
                        string currSymbol = offer.currency == OfferCurrency.DG ? "DG" : "DC";
                        slotUI.priceText.text = $"{offer.discountedPrice} {currSymbol}";
                    }
                }

                // Original price (strikethrough)
                if (slotUI.originalPriceText != null && !offer.isFree)
                {
                    string currSymbol = offer.currency == OfferCurrency.DG ? "DG" : "DC";
                    slotUI.originalPriceText.text = $"<s>{offer.originalPrice} {currSymbol}</s>";
                    slotUI.originalPriceText.gameObject.SetActive(!offer.purchased);
                }

                // Badge
                if (slotUI.badgeText != null)
                {
                    if (offer.isFree)
                        slotUI.badgeText.text = "FREE";
                    else if (offer.purchased)
                        slotUI.badgeText.text = "OWNED";
                    else
                        slotUI.badgeText.text = $"-{offer.discountPercent}%";
                }

                // Button interactability
                if (slotUI.button != null)
                    slotUI.button.interactable = !offer.purchased;

                // Wire button
                if (slotUI.button != null && !offer.purchased)
                {
                    int slotIndex = i;
                    slotUI.button.onClick.RemoveAllListeners();
                    slotUI.button.onClick.AddListener(() => OnSlotClicked(slotIndex));
                }
            }
        }

        private void OnSlotClicked(int slotIndex)
        {
            var service = DailyOfferService.Instance;
            if (service == null) return;

            bool success = service.PurchaseOffer(slotIndex);
            if (success)
            {
                Debug.Log($"[DailyOfferUI] Slot {slotIndex} purchased successfully");
                // UI refresh handled by OnOfferPurchased callback
            }
            else
            {
                Debug.Log($"[DailyOfferUI] Slot {slotIndex} purchase failed (insufficient funds or already purchased)");
                // TODO: Show "not enough" popup via ShopManager
            }
        }

        private void OnOfferPurchased(int slotIndex, DailyOffer offer)
        {
            RefreshUI();

            // Purchase feedback: punch the slot
            if (slotIndex >= 0 && slotIndex < _slotUIs.Length && _slotUIs[slotIndex]?.root != null)
            {
                var root = _slotUIs[slotIndex].root;
                root.transform.DOPunchScale(Vector3.one * 0.1f, 0.25f, 8)
                    .SetUpdate(true).SetLink(root);

                // Badge pop
                if (_slotUIs[slotIndex].badgeText != null)
                {
                    var badge = _slotUIs[slotIndex].badgeText.transform;
                    badge.localScale = Vector3.zero;
                    badge.DOScale(1f, 0.3f).SetEase(Ease.OutElastic)
                        .SetUpdate(true).SetLink(badge.gameObject);
                }
            }
        }

        // ==================== TIMER ====================

        private void StartTimer()
        {
            StopTimer();
            _timerCoroutine = StartCoroutine(TimerCoroutine());
        }

        private void StopTimer()
        {
            if (_timerCoroutine != null)
            {
                StopCoroutine(_timerCoroutine);
                _timerCoroutine = null;
            }
        }

        private IEnumerator TimerCoroutine()
        {
            while (true)
            {
                UpdateTimer();
                yield return new WaitForSeconds(1f);
            }
        }

        private void UpdateTimer()
        {
            if (timerText == null) return;

            var service = DailyOfferService.Instance;
            if (service == null)
            {
                timerText.text = "--:--:--";
                return;
            }

            var remaining = service.TimeUntilRefresh;
            timerText.text = $"{remaining.Hours:D2}:{remaining.Minutes:D2}:{remaining.Seconds:D2}";

            // FOMO: pulse red when < 1 hour remaining
            if (remaining.TotalHours < 1 && _timerFomoTween == null)
            {
                _timerFomoTween = timerText.DOColor(new Color(1f, 0.27f, 0.27f), 0.5f)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetUpdate(true)
                    .SetLink(timerText.gameObject);
            }
            else if (remaining.TotalHours >= 1 && _timerFomoTween != null)
            {
                _timerFomoTween.Kill();
                _timerFomoTween = null;
                timerText.color = Color.white;
            }
        }

        private void AnimateSlotEntrance()
        {
            if (_entranceAnimated) return;
            _entranceAnimated = true;

            GameObject[] slots = { slot1Root, slot2Root, slot3Root };
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i] == null) continue;

                var cg = slots[i].GetComponent<CanvasGroup>();
                if (cg == null) cg = slots[i].AddComponent<CanvasGroup>();

                cg.alpha = 0f;
                slots[i].transform.localScale = Vector3.one * 0.85f;

                float delay = i * 0.08f;
                cg.DOFade(1f, 0.25f).SetDelay(delay).SetUpdate(true).SetLink(slots[i]);
                slots[i].transform.DOScale(1f, 0.25f).SetDelay(delay)
                    .SetEase(Ease.OutBack).SetUpdate(true).SetLink(slots[i]);
            }
        }

        // ==================== HELPERS ====================

        private static Transform FindDeep(Transform parent, string name)
        {
            if (parent.name == name) return parent;
            foreach (Transform child in parent)
            {
                Transform found = FindDeep(child, name);
                if (found != null) return found;
            }
            return null;
        }

        private static GameObject FindChildGO(Transform parent, string name)
        {
            Transform t = FindDeep(parent, name);
            return t != null ? t.gameObject : null;
        }
    }
}
