// <copyright file="UnityUIManager.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PEAKUnlimited.Core.Services
{
    using PEAKUnlimited.Core.Interfaces;
    using PEAKUnlimited.Core.UI;
    using Photon.Pun;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Unity UI manager implementation.
    /// </summary>
    public class UnityUIManager : IUIManager
    {
        private GameObject configurationPanel;
        private bool isUIVisible = false;
        private ConfigurationManager.PluginConfig currentConfig;
        private ConfigurationManager.PluginConfig originalConfig;
        private System.Action onConfigurationSaved;
        private UIKeyboardHandler keyboardHandler;
        private GameStateManager gameStateManager;

        // UI Control references for updating values
        private Toggle extraMarshmallowsToggle;
        private Toggle extraBackpacksToggle;
        private Toggle lateJoinMarshmallowsToggle;
        private Slider maxPlayersSlider;
        private Slider cheatMarshmallowsSlider;
        private Slider cheatBackpacksSlider;
        private Text statusText;

        /// <summary>
        /// Gets a value indicating whether gets whether the UI is currently visible.
        /// </summary>
        public bool IsUIVisible => this.isUIVisible;

        /// <summary>
        /// Gets a value indicating whether the player is currently climbing.
        /// </summary>
        public bool IsCurrentlyClimbing => this.gameStateManager?.IsCurrentlyClimbing ?? false;

        /// <summary>
        /// Update method to handle keyboard shortcuts and other UI updates.
        /// </summary>
        public void Update()
        {
            this.keyboardHandler?.HandleKeyboardShortcuts();
            this.UpdateTitleIfNeeded();
        }

        /// <summary>
        /// Sets the current configuration for the UI to display and modify.
        /// </summary>
        public void SetConfiguration(ConfigurationManager.PluginConfig config, System.Action onSaved = null)
        {
            this.currentConfig = config;

            // Store default values for reset functionality
            this.originalConfig = ConfigurationManager.Default;

            this.onConfigurationSaved = onSaved;
            this.UpdateUIWithCurrentConfig();
        }

        /// <summary>
        /// Sets the game state manager reference.
        /// </summary>
        public void SetGameStateManager(GameStateManager gameStateManager)
        {
            this.gameStateManager = gameStateManager;
        }

        /// <summary>
        /// Gets the current configuration with any pending changes.
        /// </summary>
        /// <returns></returns>
        public ConfigurationManager.PluginConfig GetCurrentConfig()
        {
            return this.currentConfig;
        }

        /// <summary>
        /// Shows the configuration UI.
        /// </summary>
        public void ShowConfigurationUI()
        {
            // Only allow config UI in the main menu
            var mainMenu = UnityEngine.Object.FindFirstObjectByType<MainMenu>();
            if (mainMenu == null || !mainMenu.gameObject.activeInHierarchy)
            {
                return;
            }

            UIPlayerController.PausePlayerMovement();

            if (this.configurationPanel == null)
            {
                this.CreateConfigurationUI();
            }
            else
            {
                this.UpdateCanvasReference();
            }

            if (this.configurationPanel != null)
            {
                this.configurationPanel.SetActive(true);
                this.isUIVisible = true;

                var persistenceComponent = this.configurationPanel.GetComponent<UIPersistenceComponent>();
                if (persistenceComponent == null)
                {
                    persistenceComponent = this.configurationPanel.AddComponent<UIPersistenceComponent>();
                }
                persistenceComponent.SetShouldBeVisible(true);

                this.UpdateUIWithCurrentConfig();

                this.keyboardHandler = new UIKeyboardHandler(
                    () => { if (this.CanModifyConfig()) { this.SaveConfiguration(); } },
                    () => { if (this.CanModifyConfig()) { this.ResetConfiguration(); } },
                    () => this.HideConfigurationUI(),
                    this.isUIVisible);
            }
            else
            {
                Plugin.Logger.LogError("PEAK Unlimited: Configuration panel is null after creation");
            }
        }

        /// <summary>
        /// Hides the configuration UI.
        /// </summary>
        public void HideConfigurationUI()
        {
            if (this.configurationPanel != null)
            {
                // Tell the persistence component we want to hide
                var persistenceComponent = this.configurationPanel.GetComponent<UIPersistenceComponent>();
                if (persistenceComponent != null)
                {
                    persistenceComponent.SetShouldBeVisible(false);
                }

                // Save configuration before closing
                this.SaveConfiguration();

                this.configurationPanel.SetActive(false);
                this.isUIVisible = false;
                this.keyboardHandler = null;
            }

            // Resume player movement and lock mouse cursor
            UIPlayerController.ResumePlayerMovement();
        }

        /// <summary>
        /// Toggles the configuration UI visibility.
        /// </summary>
        public void ToggleConfigurationUI()
        {
            var mainMenu = UnityEngine.Object.FindFirstObjectByType<MainMenu>();
            if (mainMenu == null || !mainMenu.gameObject.activeInHierarchy)
            {
                return;
            }

            if (this.configurationPanel == null)
            {
                this.ShowConfigurationUI();
                return;
            }

            if (this.isUIVisible && !this.configurationPanel.activeInHierarchy)
            {
                this.configurationPanel.SetActive(true);
            }

            if (this.isUIVisible)
            {
                this.HideConfigurationUI();
            }
            else
            {
                this.ShowConfigurationUI();
            }
        }

        /// <summary>
        /// Creates the configuration UI elements.
        /// </summary>
        private void CreateConfigurationUI()
        {
            // Create a simple UI that doesn't rely on complex canvas parenting
            this.configurationPanel = new GameObject("PEAKUnlimitedConfigPanel");
            UnityEngine.Object.DontDestroyOnLoad(this.configurationPanel);

            // Add a Canvas component to make it self-contained
            Canvas canvas = this.configurationPanel.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000; // High priority to stay on top
            canvas.overrideSorting = true; // Ensure our sorting order is respected

            // Add CanvasScaler for proper scaling
            CanvasScaler scaler = this.configurationPanel.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            // Add GraphicRaycaster for interaction
            GraphicRaycaster raycaster = this.configurationPanel.AddComponent<GraphicRaycaster>();
            raycaster.blockingObjects = GraphicRaycaster.BlockingObjects.None; // Allow interaction even when moving
            raycaster.ignoreReversedGraphics = false; // Don't ignore reversed graphics

            // Create our own EventSystem to ensure UI works independently
            var existingEventSystem = UnityEngine.Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>();
            if (existingEventSystem == null)
            {
                var eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                UnityEngine.Object.DontDestroyOnLoad(eventSystem);
            }

            // Create the main panel
            GameObject panelObject = new GameObject("Panel");
            panelObject.transform.SetParent(this.configurationPanel.transform, false);

            // Add RectTransform and Image components
            RectTransform rectTransform = panelObject.AddComponent<RectTransform>();
            Image panelImage = panelObject.AddComponent<Image>();

            // Set panel properties
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            // Set panel appearance - semi-transparent overlay
            panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);

            // Create title text
            GameObject titleObject = new GameObject("Title");
            titleObject.transform.SetParent(panelObject.transform, false);

            RectTransform titleRect = titleObject.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 1);
            titleRect.anchorMax = new Vector2(1, 1);
            titleRect.sizeDelta = new Vector2(0, 60);
            titleRect.anchoredPosition = new Vector2(0, -30);

            Text titleText = titleObject.AddComponent<Text>();
            string title = "PEAK Unlimited Configuration";
            if (this.IsCurrentlyClimbing)
            {
                title += " [GAME ACTIVE - CHANGES DISABLED]";
            }

            titleText.text = title;
            titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            titleText.fontSize = 28;
            titleText.color = Color.white;
            titleText.alignment = TextAnchor.MiddleCenter;

            // Create status text
            GameObject statusObject = new GameObject("StatusText");
            statusObject.transform.SetParent(panelObject.transform, false);

            RectTransform statusRect = statusObject.AddComponent<RectTransform>();
            statusRect.anchorMin = new Vector2(0, 0.88f);
            statusRect.anchorMax = new Vector2(1, 0.91f);
            statusRect.offsetMin = new Vector2(10, 0);
            statusRect.offsetMax = new Vector2(-10, 0);

            this.statusText = statusObject.AddComponent<Text>();
            this.statusText.text = "Press F1 to toggle this menu • ESC to save and close";
            this.statusText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            this.statusText.fontSize = 12;
            this.statusText.color = new Color(0.8f, 0.8f, 0.8f, 1f);
            this.statusText.alignment = TextAnchor.MiddleCenter;

            // Create configuration controls
            this.CreateConfigurationControls();

            // Create button panel
            this.CreateButtonPanel();

            // Initially hide the panel
            this.configurationPanel.SetActive(false);
        }

        /// <summary>
        /// Creates the configuration controls (toggles, sliders, etc.).
        /// </summary>
        private void CreateConfigurationControls()
        {
            // Find the panel object
            GameObject panelObject = this.configurationPanel.transform.Find("Panel")?.gameObject;
            if (panelObject == null)
            {
                Plugin.Logger.LogError("PEAK Unlimited: Panel object not found");
                return;
            }

            // Calculate total height needed for all controls
            const float controlHeight = 0.12f; // Each control takes 12% of available height (reduced from 16%)
            const int totalControls = 6; // Number of controls we have
            float totalHeightNeeded = controlHeight * totalControls;
            float availableHeight = 0.75f; // Available space from 0.1f to 0.85f

            // Create a content area for controls
            GameObject scrollContent = new GameObject("ScrollContent");
            scrollContent.transform.SetParent(panelObject.transform, false);

            RectTransform scrollContentRect = scrollContent.AddComponent<RectTransform>();
            scrollContentRect.anchorMin = new Vector2(0, 0.1f);
            scrollContentRect.anchorMax = new Vector2(1, 0.85f);
            scrollContentRect.offsetMin = new Vector2(10, 10);
            scrollContentRect.offsetMax = new Vector2(-10, -10);

            // Only add ScrollRect if content exceeds available space
            if (totalHeightNeeded > availableHeight)
            {
                // Add ScrollRect for scrolling
                ScrollRect scrollRect = panelObject.AddComponent<ScrollRect>();
                scrollRect.content = scrollContentRect;
                scrollRect.horizontal = false;
                scrollRect.vertical = true;
            }

            // Create controls using the factory with adjusted height
            this.extraMarshmallowsToggle = UIControlFactory.CreateToggleControl(scrollContent, "Extra Marshmallows", 0,
                "Spawns additional marshmallows for extra players", () =>
                {
                    if (this.CanModifyConfig())
                    {
                        this.currentConfig.ExtraMarshmallows = this.extraMarshmallowsToggle.isOn;
                        this.UpdateStatusText();
                    }
                }, this.IsCurrentlyClimbing);

            this.extraBackpacksToggle = UIControlFactory.CreateToggleControl(scrollContent, "Extra Backpacks", 1,
                "Spawns additional backpacks for extra players", () =>
                {
                    if (this.CanModifyConfig())
                    {
                        this.currentConfig.ExtraBackpacks = this.extraBackpacksToggle.isOn;
                        this.UpdateStatusText();
                    }
                }, this.IsCurrentlyClimbing);

            this.lateJoinMarshmallowsToggle = UIControlFactory.CreateToggleControl(scrollContent, "Late Join Marshmallows", 2,
                "Spawns additional marshmallows for players who join late", () =>
                {
                    if (this.CanModifyConfig())
                    {
                        this.currentConfig.LateJoinMarshmallows = this.lateJoinMarshmallowsToggle.isOn;
                        this.UpdateStatusText();
                    }
                }, this.IsCurrentlyClimbing);

            this.maxPlayersSlider = UIControlFactory.CreateIntegerSliderControl(scrollContent, "Max Players", 3, 1, 20, this.currentConfig.MaxPlayers,
                "Maximum number of players (1-20)", (value) =>
                {
                    if (this.CanModifyConfig())
                    {
                        this.currentConfig.MaxPlayers = value;
                        this.UpdateStatusText();
                    }
                }, this.IsCurrentlyClimbing);

            this.cheatMarshmallowsSlider = UIControlFactory.CreateIntegerSliderControl(scrollContent, "Cheat Marshmallows", 4, 0, 30, this.currentConfig.CheatExtraMarshmallows,
                "Force spawn additional marshmallows (0-30)", (value) =>
                {
                    if (this.CanModifyConfig())
                    {
                        this.currentConfig.CheatExtraMarshmallows = value;
                        this.UpdateStatusText();
                    }
                }, this.IsCurrentlyClimbing);

            this.cheatBackpacksSlider = UIControlFactory.CreateIntegerSliderControl(scrollContent, "Cheat Backpacks", 5, 0, 30, this.currentConfig.CheatExtraBackpacks,
                "Force spawn additional backpacks (0-30)", (value) =>
                {
                    if (this.CanModifyConfig())
                    {
                        this.currentConfig.CheatExtraBackpacks = value;
                        this.UpdateStatusText();
                    }
                }, this.IsCurrentlyClimbing);
        }

        /// <summary>
        /// Creates the button panel with save, reset, and close buttons.
        /// </summary>
        private void CreateButtonPanel()
        {
            GameObject panelObject = this.configurationPanel.transform.Find("Panel")?.gameObject;
            if (panelObject == null)
            {
                return;
            }

            // Create button container
            GameObject buttonContainer = new GameObject("ButtonContainer");
            buttonContainer.transform.SetParent(panelObject.transform, false);

            RectTransform buttonContainerRect = buttonContainer.AddComponent<RectTransform>();
            buttonContainerRect.anchorMin = new Vector2(0, 0);
            buttonContainerRect.anchorMax = new Vector2(1, 0.08f);
            buttonContainerRect.offsetMin = new Vector2(10, 5);
            buttonContainerRect.offsetMax = new Vector2(-10, -5);

            // Create buttons using the factory
            UIControlFactory.CreateButton(buttonContainer, "Reset (R)", new Vector2(0.2f, 0.5f), new Vector2(0.4f, 0.8f), Color.yellow, () =>
            {
                if (this.CanModifyConfig())
                {
                    this.ResetConfiguration();
                }
            });

            UIControlFactory.CreateButton(buttonContainer, "Close (ESC)", new Vector2(0.5f, 0.5f), new Vector2(0.7f, 0.8f), Color.red, () =>
            {
                this.HideConfigurationUI();
            });
        }

        /// <summary>
        /// Updates the UI with current configuration values.
        /// </summary>
        private void UpdateUIWithCurrentConfig()
        {
            if (this.currentConfig == null)
            {
                return;
            }

            if (this.extraMarshmallowsToggle != null)
            {
                this.extraMarshmallowsToggle.isOn = this.currentConfig.ExtraMarshmallows;
            }

            if (this.extraBackpacksToggle != null)
            {
                this.extraBackpacksToggle.isOn = this.currentConfig.ExtraBackpacks;
            }

            if (this.lateJoinMarshmallowsToggle != null)
            {
                this.lateJoinMarshmallowsToggle.isOn = this.currentConfig.LateJoinMarshmallows;
            }

            if (this.maxPlayersSlider != null)
            {
                this.maxPlayersSlider.value = this.currentConfig.MaxPlayers;
            }

            if (this.cheatMarshmallowsSlider != null)
            {
                this.cheatMarshmallowsSlider.value = this.currentConfig.CheatExtraMarshmallows;
            }

            if (this.cheatBackpacksSlider != null)
            {
                this.cheatBackpacksSlider.value = this.currentConfig.CheatExtraBackpacks;
            }

            this.UpdateStatusText();
        }

        /// <summary>
        /// Updates the status text with current configuration info.
        /// </summary>
        private void UpdateStatusText()
        {
            if (this.statusText == null)
            {
                return;
            }

            string status = "Press F1 to toggle this menu • ESC to save and close";

            if (this.currentConfig != null)
            {
                status += $" • Max Players: {this.currentConfig.MaxPlayers}";
                if (this.currentConfig.ExtraMarshmallows)
                {
                    status += " • Extra Marshmallows: ON";
                }

                if (this.currentConfig.ExtraBackpacks)
                {
                    status += " • Extra Backpacks: ON";
                }

                if (this.currentConfig.LateJoinMarshmallows)
                {
                    status += " • Late Join: ON";
                }

                if (this.currentConfig.CheatExtraMarshmallows > 0)
                {
                    status += $" • Cheat Marshmallows: {this.currentConfig.CheatExtraMarshmallows}";
                }

                if (this.currentConfig.CheatExtraBackpacks > 0)
                {
                    status += $" • Cheat Backpacks: {this.currentConfig.CheatExtraBackpacks}";
                }
            }

            this.statusText.text = status;
        }

        /// <summary>
        /// Saves the current configuration.
        /// </summary>
        private void SaveConfiguration()
        {
            if (this.currentConfig == null)
            {
                return;
            }

            // Validate the configuration
            this.currentConfig = ConfigurationManager.ProcessConfiguration(this.currentConfig);

            // Sync UI config to main plugin config
            this.SyncToMainConfig();

            // Call the save callback if provided
            this.onConfigurationSaved?.Invoke();

            this.UpdateStatusText();
        }

        /// <summary>
        /// Syncs the UI configuration back to the main plugin configuration.
        /// </summary>
        private void SyncToMainConfig()
        {
            if (this.currentConfig == null)
            {
                return;
            }

            // Get the main plugin instance and update its config using proper public method
            var plugin = Plugin.CurrentInstance;
            if (plugin != null)
            {
                plugin.UpdatePluginConfiguration(this.currentConfig);
            }
        }

        /// <summary>
        /// Resets the configuration to default values.
        /// </summary>
        private void ResetConfiguration()
        {
            if (this.currentConfig == null || this.originalConfig == null)
            {
                return;
            }

            // Restore default configuration values
            this.currentConfig.MaxPlayers = this.originalConfig.MaxPlayers;
            this.currentConfig.ExtraMarshmallows = this.originalConfig.ExtraMarshmallows;
            this.currentConfig.ExtraBackpacks = this.originalConfig.ExtraBackpacks;
            this.currentConfig.LateJoinMarshmallows = this.originalConfig.LateJoinMarshmallows;
            this.currentConfig.CheatExtraMarshmallows = this.originalConfig.CheatExtraMarshmallows;
            this.currentConfig.CheatExtraBackpacks = this.originalConfig.CheatExtraBackpacks;

            // Update UI to reflect reset values
            this.UpdateUIWithCurrentConfig();
        }

        /// <summary>
        /// Checks if configuration can be modified (not climbing).
        /// </summary>
        private bool CanModifyConfig()
        {
            if (this.IsCurrentlyClimbing)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Updates the title if the climbing state has changed.
        /// </summary>
        private void UpdateTitleIfNeeded()
        {
            if (this.configurationPanel == null)
            {
                return;
            }

            var titleObject = this.configurationPanel.transform.Find("Panel/Title");
            if (titleObject == null)
            {
                return;
            }

            var titleText = titleObject.GetComponent<Text>();
            if (titleText == null)
            {
                return;
            }

            string title = "PEAK Unlimited Configuration";
            if (this.IsCurrentlyClimbing)
            {
                title += " [GAME ACTIVE - CHANGES DISABLED]";
            }

            if (titleText.text != title)
            {
                titleText.text = title;
            }
        }

        /// <summary>
        /// Updates the canvas reference for the configuration panel.
        /// </summary>
        private void UpdateCanvasReference()
        {
            if (this.configurationPanel == null)
            {
                return;
            }

            // Since we're using our own canvas, just ensure it's still active
            if (!this.configurationPanel.activeInHierarchy)
            {
                this.configurationPanel.SetActive(true);
            }
        }
    }
}
