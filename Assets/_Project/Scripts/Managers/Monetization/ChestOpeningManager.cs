using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;
using DigitPark.Monetization;

namespace DigitPark.Managers
{
    /// <summary>
    /// Manager para la escena de apertura de cofres.
    /// Maneja la animación y revelación de recompensas de cofres.
    /// </summary>
    public class ChestOpeningManager : MonoBehaviour
    {
        [Header("UI - Header")]
        [SerializeField] private Button backButton;
        [SerializeField] private Button skipButton;
        [SerializeField] private TextMeshProUGUI chestTypeText;

        [Header("UI - Chest Display")]
        [SerializeField] private GameObject chestContainer;
        [SerializeField] private Image chestImage;
        [SerializeField] private Animator chestAnimator;
        [SerializeField] private ParticleSystem chestParticles;
        [SerializeField] private ParticleSystem openingParticles;

        [Header("UI - Rewards")]
        [SerializeField] private GameObject rewardsPanel;
        [SerializeField] private Transform rewardsContainer;
        [SerializeField] private GameObject rewardItemPrefab;
        [SerializeField] private TextMeshProUGUI totalValueText;
        [SerializeField] private Button collectAllButton;

        [Header("UI - Single Reward Reveal")]
        [SerializeField] private GameObject singleRewardPanel;
        [SerializeField] private Image singleRewardIcon;
        [SerializeField] private TextMeshProUGUI singleRewardNameText;
        [SerializeField] private TextMeshProUGUI singleRewardDescriptionText;
        [SerializeField] private TextMeshProUGUI singleRewardRarityText;
        [SerializeField] private ParticleSystem rarityParticles;

        [Header("UI - Tap Prompt")]
        [SerializeField] private GameObject tapPrompt;
        [SerializeField] private TextMeshProUGUI tapPromptText;

        [Header("UI - Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip chestShakeClip;
        [SerializeField] private AudioClip chestOpenClip;
        [SerializeField] private AudioClip rewardRevealClip;
        [SerializeField] private AudioClip rareRewardClip;
        [SerializeField] private AudioClip epicRewardClip;
        [SerializeField] private AudioClip legendaryRewardClip;

        [Header("Chest Sprites")]
        [SerializeField] private Sprite commonChestSprite;
        [SerializeField] private Sprite rareChestSprite;
        [SerializeField] private Sprite epicChestSprite;
        [SerializeField] private Sprite legendaryChestSprite;

        [Header("Rarity Colors")]
        [SerializeField] private Color commonColor = new Color(0.7f, 0.7f, 0.7f);
        [SerializeField] private Color rareColor = new Color(0.2f, 0.6f, 1f);
        [SerializeField] private Color epicColor = new Color(0.7f, 0.3f, 1f);
        [SerializeField] private Color legendaryColor = new Color(1f, 0.84f, 0f);

        [Header("Configuration")]
        [SerializeField] private float shakeAnimationDuration = 2f;
        [SerializeField] private float openAnimationDuration = 1.5f;
        [SerializeField] private float rewardRevealDelay = 0.5f;
        [SerializeField] private float autoSkipDelay = 30f;

        // State
        private ChestData currentChest;
        private List<ChestReward> pendingRewards = new List<ChestReward>();
        private List<ChestReward> revealedRewards = new List<ChestReward>();
        private List<GameObject> spawnedRewardItems = new List<GameObject>();
        private ChestOpeningState currentState = ChestOpeningState.Idle;
        private int currentRewardIndex = 0;
        private bool canSkip = false;

        public enum ChestOpeningState
        {
            Idle,
            Shaking,
            Opening,
            RevealingRewards,
            ShowingAllRewards,
            Completed
        }

        private void Start()
        {
            SetupUI();
            SetupListeners();
            LoadChestFromParams();
        }

        private void SetupUI()
        {
            if (rewardsPanel) rewardsPanel.SetActive(false);
            if (singleRewardPanel) singleRewardPanel.SetActive(false);
            if (tapPrompt) tapPrompt.SetActive(false);
            if (skipButton) skipButton.gameObject.SetActive(false);
        }

        private void SetupListeners()
        {
            if (backButton) backButton.onClick.AddListener(OnBackClicked);
            if (skipButton) skipButton.onClick.AddListener(SkipAnimation);
            if (collectAllButton) collectAllButton.onClick.AddListener(CollectAllRewards);

            // Tap anywhere to advance
            var tapDetector = gameObject.AddComponent<TapDetector>();
            tapDetector.OnTap += OnScreenTapped;
        }

        private void LoadChestFromParams()
        {
            var navParams = SceneNavigator.Instance?.PendingParams;

            if (navParams?.CustomData is ChestData chest)
            {
                currentChest = chest;
            }
            else
            {
                // Create mock chest for testing
                currentChest = CreateMockChest(ChestRarity.Rare);
            }

            SceneNavigator.Instance?.ClearPendingParams();
            SetupChestDisplay();
            StartChestOpenSequence();
        }

        private ChestData CreateMockChest(ChestRarity rarity)
        {
            var chest = new ChestData
            {
                id = Guid.NewGuid().ToString(),
                name = $"Cofre {rarity}",
                rarity = rarity,
                rewards = GenerateMockRewards(rarity)
            };
            return chest;
        }

        private List<ChestReward> GenerateMockRewards(ChestRarity chestRarity)
        {
            var rewards = new List<ChestReward>();
            int rewardCount = chestRarity switch
            {
                ChestRarity.Common => 3,
                ChestRarity.Rare => 4,
                ChestRarity.Epic => 5,
                ChestRarity.Legendary => 6,
                _ => 3
            };

            string[] rewardTypes = { "coins", "gems", "xp", "avatar", "frame", "effect" };
            string[] rewardNames = { "Monedas", "Gemas", "Experiencia", "Avatar", "Marco", "Efecto" };

            for (int i = 0; i < rewardCount; i++)
            {
                int typeIndex = UnityEngine.Random.Range(0, rewardTypes.Length);
                ChestRewardRarity rewardRarity = GetRandomRarity(chestRarity);

                rewards.Add(new ChestReward
                {
                    id = Guid.NewGuid().ToString(),
                    type = rewardTypes[typeIndex],
                    name = rewardNames[typeIndex],
                    description = $"Recompensa de cofre {chestRarity}",
                    amount = GetRewardAmount(rewardTypes[typeIndex], rewardRarity),
                    rarity = rewardRarity,
                    iconUrl = ""
                });
            }

            // Sort by rarity (reveal common first, legendary last)
            rewards.Sort((a, b) => a.rarity.CompareTo(b.rarity));

            return rewards;
        }

        private ChestRewardRarity GetRandomRarity(ChestRarity chestRarity)
        {
            float roll = UnityEngine.Random.value;

            return chestRarity switch
            {
                ChestRarity.Common => roll < 0.7f ? ChestRewardRarity.Common :
                                     roll < 0.95f ? ChestRewardRarity.Rare : ChestRewardRarity.Epic,
                ChestRarity.Rare => roll < 0.5f ? ChestRewardRarity.Common :
                                   roll < 0.85f ? ChestRewardRarity.Rare :
                                   roll < 0.98f ? ChestRewardRarity.Epic : ChestRewardRarity.Legendary,
                ChestRarity.Epic => roll < 0.3f ? ChestRewardRarity.Rare :
                                   roll < 0.75f ? ChestRewardRarity.Epic :
                                   ChestRewardRarity.Legendary,
                ChestRarity.Legendary => roll < 0.2f ? ChestRewardRarity.Epic :
                                        ChestRewardRarity.Legendary,
                _ => ChestRewardRarity.Common
            };
        }

        private int GetRewardAmount(string type, ChestRewardRarity rarity)
        {
            int baseAmount = type switch
            {
                "coins" => 100,
                "gems" => 5,
                "xp" => 50,
                _ => 1
            };

            float multiplier = rarity switch
            {
                ChestRewardRarity.Common => 1f,
                ChestRewardRarity.Rare => 2f,
                ChestRewardRarity.Epic => 5f,
                ChestRewardRarity.Legendary => 10f,
                _ => 1f
            };

            return Mathf.RoundToInt(baseAmount * multiplier);
        }

        private void SetupChestDisplay()
        {
            if (currentChest == null) return;

            // Set chest name
            if (chestTypeText)
            {
                chestTypeText.text = currentChest.name;
                chestTypeText.color = GetRarityColor(currentChest.rarity);
            }

            // Set chest sprite
            if (chestImage)
            {
                chestImage.sprite = currentChest.rarity switch
                {
                    ChestRarity.Common => commonChestSprite,
                    ChestRarity.Rare => rareChestSprite,
                    ChestRarity.Epic => epicChestSprite,
                    ChestRarity.Legendary => legendaryChestSprite,
                    _ => commonChestSprite
                };
            }

            // Prepare rewards
            pendingRewards = new List<ChestReward>(currentChest.rewards);
        }

        private Color GetRarityColor(ChestRarity rarity)
        {
            return rarity switch
            {
                ChestRarity.Common => commonColor,
                ChestRarity.Rare => rareColor,
                ChestRarity.Epic => epicColor,
                ChestRarity.Legendary => legendaryColor,
                _ => commonColor
            };
        }

        private Color GetRewardRarityColor(ChestRewardRarity rarity)
        {
            return rarity switch
            {
                ChestRewardRarity.Common => commonColor,
                ChestRewardRarity.Rare => rareColor,
                ChestRewardRarity.Epic => epicColor,
                ChestRewardRarity.Legendary => legendaryColor,
                _ => commonColor
            };
        }

        private void StartChestOpenSequence()
        {
            StartCoroutine(ChestOpenSequence());
        }

        private IEnumerator ChestOpenSequence()
        {
            // Phase 1: Shaking
            currentState = ChestOpeningState.Shaking;
            canSkip = false;

            if (tapPrompt)
            {
                tapPrompt.SetActive(true);
                if (tapPromptText) tapPromptText.text = "Toca para abrir";
            }

            PlaySound(chestShakeClip);

            if (chestAnimator)
            {
                chestAnimator.SetTrigger("Shake");
            }
            else
            {
                // Fallback shake animation
                StartCoroutine(ShakeChest());
            }

            yield return new WaitForSeconds(shakeAnimationDuration);

            // Wait for tap
            while (currentState == ChestOpeningState.Shaking)
            {
                yield return null;
            }

            // Phase 2: Opening
            currentState = ChestOpeningState.Opening;
            if (tapPrompt) tapPrompt.SetActive(false);

            PlaySound(chestOpenClip);

            if (chestAnimator)
            {
                chestAnimator.SetTrigger("Open");
            }

            if (openingParticles) openingParticles.Play();
            if (chestParticles) chestParticles.Play();

            yield return new WaitForSeconds(openAnimationDuration);

            // Phase 3: Reveal rewards one by one
            currentState = ChestOpeningState.RevealingRewards;
            if (skipButton) skipButton.gameObject.SetActive(true);
            canSkip = true;

            currentRewardIndex = 0;
            while (currentRewardIndex < pendingRewards.Count && currentState == ChestOpeningState.RevealingRewards)
            {
                yield return StartCoroutine(RevealSingleReward(pendingRewards[currentRewardIndex]));
                currentRewardIndex++;
            }

            // Phase 4: Show all rewards summary
            ShowAllRewardsSummary();
        }

        private IEnumerator ShakeChest()
        {
            if (chestContainer == null) yield break;

            Vector3 originalPos = chestContainer.transform.localPosition;
            float elapsed = 0f;
            float intensity = 10f;

            while (elapsed < shakeAnimationDuration && currentState == ChestOpeningState.Shaking)
            {
                float x = UnityEngine.Random.Range(-1f, 1f) * intensity;
                float y = UnityEngine.Random.Range(-1f, 1f) * intensity * 0.5f;
                chestContainer.transform.localPosition = originalPos + new Vector3(x, y, 0);

                intensity = Mathf.Lerp(10f, 20f, elapsed / shakeAnimationDuration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            chestContainer.transform.localPosition = originalPos;
        }

        private IEnumerator RevealSingleReward(ChestReward reward)
        {
            revealedRewards.Add(reward);

            if (singleRewardPanel)
            {
                singleRewardPanel.SetActive(true);

                // Setup reward display
                if (singleRewardNameText) singleRewardNameText.text = reward.name;
                if (singleRewardDescriptionText)
                {
                    singleRewardDescriptionText.text = reward.type == "coins" || reward.type == "gems" || reward.type == "xp"
                        ? $"+{reward.amount}"
                        : reward.description;
                }

                Color rarityColor = GetRewardRarityColor(reward.rarity);

                if (singleRewardRarityText)
                {
                    singleRewardRarityText.text = GetRarityName(reward.rarity);
                    singleRewardRarityText.color = rarityColor;
                }

                // Play sound based on rarity
                PlayRewardSound(reward.rarity);

                // Play particles for rare+
                if (rarityParticles && reward.rarity >= ChestRewardRarity.Rare)
                {
                    var main = rarityParticles.main;
                    main.startColor = rarityColor;
                    rarityParticles.Play();
                }

                if (tapPrompt)
                {
                    tapPrompt.SetActive(true);
                    if (tapPromptText) tapPromptText.text = "Toca para continuar";
                }
            }

            // Wait for tap or timeout
            float waitTime = 0f;
            bool tapped = false;

            while (!tapped && waitTime < autoSkipDelay)
            {
                waitTime += Time.deltaTime;
                yield return null;

                // Check for tap (handled by OnScreenTapped)
                if (currentRewardIndex < pendingRewards.Count - 1 || currentState != ChestOpeningState.RevealingRewards)
                {
                    tapped = true;
                }
            }

            if (singleRewardPanel) singleRewardPanel.SetActive(false);
            if (tapPrompt) tapPrompt.SetActive(false);

            yield return new WaitForSeconds(rewardRevealDelay);
        }

        private string GetRarityName(ChestRewardRarity rarity)
        {
            return rarity switch
            {
                ChestRewardRarity.Common => "COMUN",
                ChestRewardRarity.Rare => "RARO",
                ChestRewardRarity.Epic => "EPICO",
                ChestRewardRarity.Legendary => "LEGENDARIO",
                _ => "COMUN"
            };
        }

        private void PlayRewardSound(ChestRewardRarity rarity)
        {
            AudioClip clip = rarity switch
            {
                ChestRewardRarity.Legendary => legendaryRewardClip,
                ChestRewardRarity.Epic => epicRewardClip,
                ChestRewardRarity.Rare => rareRewardClip,
                _ => rewardRevealClip
            };

            PlaySound(clip ?? rewardRevealClip);
        }

        private void PlaySound(AudioClip clip)
        {
            if (audioSource && clip)
            {
                audioSource.PlayOneShot(clip);
            }
        }

        private void ShowAllRewardsSummary()
        {
            currentState = ChestOpeningState.ShowingAllRewards;

            if (chestContainer) chestContainer.SetActive(false);
            if (singleRewardPanel) singleRewardPanel.SetActive(false);
            if (tapPrompt) tapPrompt.SetActive(false);
            if (skipButton) skipButton.gameObject.SetActive(false);

            if (rewardsPanel)
            {
                rewardsPanel.SetActive(true);
                PopulateRewardsSummary();
            }
        }

        private void PopulateRewardsSummary()
        {
            // Clear existing
            foreach (var item in spawnedRewardItems)
            {
                if (item) Destroy(item);
            }
            spawnedRewardItems.Clear();

            // Create reward items
            foreach (var reward in revealedRewards)
            {
                CreateRewardSummaryItem(reward);
            }

            // Calculate total value (for display purposes)
            int totalCoins = 0;
            int totalGems = 0;

            foreach (var reward in revealedRewards)
            {
                if (reward.type == "coins") totalCoins += reward.amount;
                else if (reward.type == "gems") totalGems += reward.amount;
            }

            if (totalValueText)
            {
                string valueText = "";
                if (totalCoins > 0) valueText += $"+{totalCoins} Monedas";
                if (totalGems > 0)
                {
                    if (!string.IsNullOrEmpty(valueText)) valueText += " | ";
                    valueText += $"+{totalGems} Gemas";
                }
                totalValueText.text = valueText;
            }
        }

        private void CreateRewardSummaryItem(ChestReward reward)
        {
            GameObject item;

            if (rewardItemPrefab != null)
            {
                item = Instantiate(rewardItemPrefab, rewardsContainer);
            }
            else
            {
                item = CreateRewardItemFallback(reward);
            }

            spawnedRewardItems.Add(item);
        }

        private GameObject CreateRewardItemFallback(ChestReward reward)
        {
            var item = new GameObject($"Reward_{reward.type}");
            item.transform.SetParent(rewardsContainer, false);

            var rt = item.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(150, 150);

            var image = item.AddComponent<Image>();
            image.color = new Color(0.15f, 0.15f, 0.2f, 0.95f);

            // Rarity border
            var border = new GameObject("Border");
            border.transform.SetParent(item.transform, false);
            var borderRT = border.AddComponent<RectTransform>();
            borderRT.anchorMin = Vector2.zero;
            borderRT.anchorMax = Vector2.one;
            borderRT.sizeDelta = Vector2.zero;
            var borderImage = border.AddComponent<Image>();
            borderImage.color = GetRewardRarityColor(reward.rarity);
            borderImage.raycastTarget = false;

            // Name text
            var nameObj = new GameObject("Name");
            nameObj.transform.SetParent(item.transform, false);
            var nameRT = nameObj.AddComponent<RectTransform>();
            nameRT.anchorMin = new Vector2(0, 0);
            nameRT.anchorMax = new Vector2(1, 0.3f);
            nameRT.offsetMin = new Vector2(5, 5);
            nameRT.offsetMax = new Vector2(-5, 0);

            var nameText = nameObj.AddComponent<TextMeshProUGUI>();
            nameText.text = reward.type == "coins" || reward.type == "gems" || reward.type == "xp"
                ? $"{reward.name}\n+{reward.amount}"
                : reward.name;
            nameText.fontSize = 14;
            nameText.alignment = TextAlignmentOptions.Center;
            nameText.color = Color.white;

            return item;
        }

        private void OnScreenTapped()
        {
            switch (currentState)
            {
                case ChestOpeningState.Shaking:
                    // Advance to opening
                    currentState = ChestOpeningState.Opening;
                    break;

                case ChestOpeningState.RevealingRewards:
                    // Advance to next reward
                    if (singleRewardPanel && singleRewardPanel.activeSelf)
                    {
                        // Will be handled by the coroutine
                    }
                    break;

                case ChestOpeningState.ShowingAllRewards:
                    // Could tap to collect
                    break;
            }
        }

        private void SkipAnimation()
        {
            if (!canSkip) return;

            // Add all remaining rewards to revealed
            for (int i = currentRewardIndex; i < pendingRewards.Count; i++)
            {
                if (!revealedRewards.Contains(pendingRewards[i]))
                {
                    revealedRewards.Add(pendingRewards[i]);
                }
            }

            StopAllCoroutines();
            ShowAllRewardsSummary();
        }

        private void CollectAllRewards()
        {
            currentState = ChestOpeningState.Completed;

            // Apply rewards
            foreach (var reward in revealedRewards)
            {
                ApplyReward(reward);
            }

            // Mark chest as opened
            if (currentChest != null)
            {
                PlayerPrefs.SetInt($"Chest_{currentChest.id}_opened", 1);
                PlayerPrefs.Save();
            }

            Debug.Log($"[ChestOpening] Collected {revealedRewards.Count} rewards");

            // Navigate back
            SceneNavigator.Instance?.GoBack();
        }

        private void ApplyReward(ChestReward reward)
        {
            switch (reward.type)
            {
                case "coins":
                    int currentCoins = PlayerPrefs.GetInt("PlayerCoins", 0);
                    PlayerPrefs.SetInt("PlayerCoins", currentCoins + reward.amount);
                    break;

                case "gems":
                    int currentGems = PlayerPrefs.GetInt("PlayerGems", 0);
                    PlayerPrefs.SetInt("PlayerGems", currentGems + reward.amount);
                    break;

                case "xp":
                    int currentXP = PlayerPrefs.GetInt("PlayerXP", 0);
                    PlayerPrefs.SetInt("PlayerXP", currentXP + reward.amount);
                    break;

                default:
                    // Unlock item
                    PlayerPrefs.SetInt($"Unlocked_{reward.type}_{reward.id}", 1);
                    break;
            }

            PlayerPrefs.Save();
            Debug.Log($"[ChestOpening] Applied reward: {reward.type} x{reward.amount}");
        }

        private void OnBackClicked()
        {
            // Confirm if rewards not collected
            if (currentState != ChestOpeningState.Completed && revealedRewards.Count > 0)
            {
                // Could show confirmation dialog
                // For now, just collect and go back
                CollectAllRewards();
            }
            else
            {
                SceneNavigator.Instance?.GoBack();
            }
        }
    }

    // Helper class for tap detection
    public class TapDetector : MonoBehaviour, UnityEngine.EventSystems.IPointerClickHandler
    {
        public event Action OnTap;

        public void OnPointerClick(UnityEngine.EventSystems.PointerEventData eventData)
        {
            OnTap?.Invoke();
        }
    }

    [Serializable]
    public class ChestData
    {
        public string id;
        public string name;
        public ChestRarity rarity;
        public List<ChestReward> rewards;
    }

    [Serializable]
    public class ChestReward
    {
        public string id;
        public string type;
        public string name;
        public string description;
        public int amount;
        public ChestRewardRarity rarity;
        public string iconUrl;
    }

    public enum ChestRarity
    {
        Common,
        Rare,
        Epic,
        Legendary
    }

    public enum ChestRewardRarity
    {
        Common,
        Rare,
        Epic,
        Legendary
    }
}
