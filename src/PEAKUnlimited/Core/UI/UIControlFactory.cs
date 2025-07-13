// <copyright file="UIControlFactory.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PEAKUnlimited.Core.UI
{
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Factory for creating UI controls.
    /// </summary>
    public static class UIControlFactory
    {
        /// <summary>
        /// Creates a toggle control with tooltip.
        /// </summary>
        /// <returns></returns>
        public static Toggle CreateToggleControl(GameObject parent, string label, int index, string tooltip, System.Action onToggle, bool isCurrentlyClimbing)
        {
            GameObject toggleObject = new GameObject($"Toggle_{label.Replace(" ", string.Empty)}");
            toggleObject.transform.SetParent(parent.transform, false);

            RectTransform toggleRect = toggleObject.AddComponent<RectTransform>();
            toggleRect.anchorMin = new Vector2(0, 1 - ((index + 1) * 0.16f));
            toggleRect.anchorMax = new Vector2(1, 1 - (index * 0.16f));
            toggleRect.offsetMin = new Vector2(10, 5);
            toggleRect.offsetMax = new Vector2(-10, -5);

            // Background
            Image toggleBg = toggleObject.AddComponent<Image>();
            toggleBg.color = new Color(0.15f, 0.15f, 0.15f, 0.9f);

            // Label
            GameObject labelObject = new GameObject("Label");
            labelObject.transform.SetParent(toggleObject.transform, false);

            RectTransform labelRect = labelObject.AddComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0, 0);
            labelRect.anchorMax = new Vector2(0.7f, 1);
            labelRect.offsetMin = new Vector2(10, 0);
            labelRect.offsetMax = new Vector2(-5, 0);

            Text labelText = labelObject.AddComponent<Text>();
            labelText.text = label;
            labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            labelText.fontSize = 16;
            labelText.color = Color.white;
            labelText.alignment = TextAnchor.MiddleLeft;

            // Tooltip
            GameObject tooltipObject = new GameObject("Tooltip");
            tooltipObject.transform.SetParent(toggleObject.transform, false);

            RectTransform tooltipRect = tooltipObject.AddComponent<RectTransform>();
            tooltipRect.anchorMin = new Vector2(0, 0);
            tooltipRect.anchorMax = new Vector2(0.7f, 0.4f);
            tooltipRect.offsetMin = new Vector2(10, 0);
            tooltipRect.offsetMax = new Vector2(-5, 0);

            Text tooltipText = tooltipObject.AddComponent<Text>();
            tooltipText.text = tooltip;
            tooltipText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            tooltipText.fontSize = 10;
            tooltipText.color = new Color(0.7f, 0.7f, 0.7f, 1f);
            tooltipText.alignment = TextAnchor.UpperLeft;

            // Toggle button
            GameObject toggleButton = new GameObject("ToggleButton");
            toggleButton.transform.SetParent(toggleObject.transform, false);

            RectTransform toggleButtonRect = toggleButton.AddComponent<RectTransform>();
            toggleButtonRect.anchorMin = new Vector2(0.8f, 0.2f);
            toggleButtonRect.anchorMax = new Vector2(0.95f, 0.8f);
            toggleButtonRect.offsetMin = Vector2.zero;
            toggleButtonRect.offsetMax = Vector2.zero;

            Image toggleButtonImage = toggleButton.AddComponent<Image>();
            toggleButtonImage.color = !isCurrentlyClimbing ? new Color(0.3f, 0.3f, 0.3f, 1f) : new Color(0.5f, 0.5f, 0.5f, 0.5f);

            Toggle toggleComponent = toggleButton.AddComponent<Toggle>();
            toggleComponent.onValueChanged.AddListener((value) => onToggle());
            toggleComponent.navigation = new Navigation { mode = Navigation.Mode.None };
            toggleComponent.interactable = !isCurrentlyClimbing;

            // Toggle state indicator
            GameObject toggleIndicator = new GameObject("ToggleIndicator");
            toggleIndicator.transform.SetParent(toggleButton.transform, false);

            RectTransform indicatorRect = toggleIndicator.AddComponent<RectTransform>();
            indicatorRect.anchorMin = new Vector2(0.1f, 0.1f);
            indicatorRect.anchorMax = new Vector2(0.9f, 0.9f);
            indicatorRect.offsetMin = Vector2.zero;
            indicatorRect.offsetMax = Vector2.zero;

            Image indicatorImage = toggleIndicator.AddComponent<Image>();
            indicatorImage.color = Color.green;

            toggleComponent.graphic = indicatorImage;

            return toggleComponent;
        }

        /// <summary>
        /// Creates a slider control with tooltip.
        /// </summary>
        /// <returns></returns>
        public static Slider CreateSliderControl(GameObject parent, string label, int index, float minValue, float maxValue, float defaultValue, string tooltip, System.Action<float> onValueChanged, bool isCurrentlyClimbing)
        {
            GameObject sliderObject = new GameObject($"Slider_{label.Replace(" ", string.Empty)}");
            sliderObject.transform.SetParent(parent.transform, false);

            RectTransform sliderRect = sliderObject.AddComponent<RectTransform>();
            sliderRect.anchorMin = new Vector2(0, 1 - ((index + 1) * 0.16f));
            sliderRect.anchorMax = new Vector2(1, 1 - (index * 0.16f));
            sliderRect.offsetMin = new Vector2(10, 5);
            sliderRect.offsetMax = new Vector2(-10, -5);

            // Background
            Image sliderBg = sliderObject.AddComponent<Image>();
            sliderBg.color = new Color(0.15f, 0.15f, 0.15f, 0.9f);

            // Label
            GameObject labelObject = new GameObject("Label");
            labelObject.transform.SetParent(sliderObject.transform, false);

            RectTransform labelRect = labelObject.AddComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0, 0.6f);
            labelRect.anchorMax = new Vector2(0.7f, 1);
            labelRect.offsetMin = new Vector2(10, 0);
            labelRect.offsetMax = new Vector2(-5, 0);

            Text labelText = labelObject.AddComponent<Text>();
            labelText.text = $"{label}: {defaultValue}";
            labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            labelText.fontSize = 16;
            labelText.color = Color.white;
            labelText.alignment = TextAnchor.MiddleLeft;

            // Tooltip
            GameObject tooltipObject = new GameObject("Tooltip");
            tooltipObject.transform.SetParent(sliderObject.transform, false);

            RectTransform tooltipRect = tooltipObject.AddComponent<RectTransform>();
            tooltipRect.anchorMin = new Vector2(0, 0);
            tooltipRect.anchorMax = new Vector2(0.7f, 0.6f);
            tooltipRect.offsetMin = new Vector2(10, 0);
            tooltipRect.offsetMax = new Vector2(-5, 0);

            Text tooltipText = tooltipObject.AddComponent<Text>();
            tooltipText.text = tooltip;
            tooltipText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            tooltipText.fontSize = 10;
            tooltipText.color = new Color(0.7f, 0.7f, 0.7f, 1f);
            tooltipText.alignment = TextAnchor.UpperLeft;

            // Slider
            GameObject sliderComponent = new GameObject("SliderComponent");
            sliderComponent.transform.SetParent(sliderObject.transform, false);

            RectTransform sliderComponentRect = sliderComponent.AddComponent<RectTransform>();
            sliderComponentRect.anchorMin = new Vector2(0.8f, 0.3f);
            sliderComponentRect.anchorMax = new Vector2(0.95f, 0.7f);
            sliderComponentRect.offsetMin = Vector2.zero;
            sliderComponentRect.offsetMax = Vector2.zero;

            Slider slider = sliderComponent.AddComponent<Slider>();
            slider.minValue = minValue;
            slider.maxValue = maxValue;
            slider.value = defaultValue;
            slider.interactable = !isCurrentlyClimbing;
            slider.onValueChanged.AddListener((value) =>
            {
                labelText.text = $"{label}: {value}";
                onValueChanged(value);
            });

            // Slider background
            Image sliderBgImage = sliderComponent.AddComponent<Image>();
            sliderBgImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);

            // Slider fill
            GameObject fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(sliderComponent.transform, false);

            RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
            fillAreaRect.anchorMin = Vector2.zero;
            fillAreaRect.anchorMax = Vector2.one;
            fillAreaRect.offsetMin = Vector2.zero;
            fillAreaRect.offsetMax = Vector2.zero;

            Image fillImage = fillArea.AddComponent<Image>();
            fillImage.color = new Color(0.4f, 0.4f, 0.4f, 1f);

            // Slider handle
            GameObject handle = new GameObject("Handle");
            handle.transform.SetParent(sliderComponent.transform, false);

            RectTransform handleRect = handle.AddComponent<RectTransform>();
            handleRect.anchorMin = new Vector2(0, 0);
            handleRect.anchorMax = new Vector2(0, 1);
            handleRect.sizeDelta = new Vector2(20, 0);

            Image handleImage = handle.AddComponent<Image>();
            handleImage.color = new Color(0.6f, 0.6f, 0.6f, 1f);

            // Set up slider references
            slider.fillRect = fillAreaRect;
            slider.handleRect = handleRect;

            return slider;
        }

        /// <summary>
        /// Creates an integer slider control with tooltip.
        /// </summary>
        /// <returns></returns>
        public static Slider CreateIntegerSliderControl(GameObject parent, string label, int index, int minValue, int maxValue, int defaultValue, string tooltip, System.Action<int> onValueChanged, bool isCurrentlyClimbing)
        {
            GameObject sliderObject = new GameObject($"Slider_{label.Replace(" ", string.Empty)}");
            sliderObject.transform.SetParent(parent.transform, false);

            RectTransform sliderRect = sliderObject.AddComponent<RectTransform>();
            sliderRect.anchorMin = new Vector2(0, 1 - ((index + 1) * 0.16f));
            sliderRect.anchorMax = new Vector2(1, 1 - (index * 0.16f));
            sliderRect.offsetMin = new Vector2(10, 5);
            sliderRect.offsetMax = new Vector2(-10, -5);

            // Background
            Image sliderBg = sliderObject.AddComponent<Image>();
            sliderBg.color = new Color(0.15f, 0.15f, 0.15f, 0.9f);

            // Label
            GameObject labelObject = new GameObject("Label");
            labelObject.transform.SetParent(sliderObject.transform, false);

            RectTransform labelRect = labelObject.AddComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0, 0.6f);
            labelRect.anchorMax = new Vector2(0.7f, 1);
            labelRect.offsetMin = new Vector2(10, 0);
            labelRect.offsetMax = new Vector2(-5, 0);

            Text labelText = labelObject.AddComponent<Text>();
            labelText.text = $"{label}: {defaultValue}";
            labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            labelText.fontSize = 16;
            labelText.color = Color.white;
            labelText.alignment = TextAnchor.MiddleLeft;

            // Tooltip
            GameObject tooltipObject = new GameObject("Tooltip");
            tooltipObject.transform.SetParent(sliderObject.transform, false);

            RectTransform tooltipRect = tooltipObject.AddComponent<RectTransform>();
            tooltipRect.anchorMin = new Vector2(0, 0);
            tooltipRect.anchorMax = new Vector2(0.7f, 0.6f);
            tooltipRect.offsetMin = new Vector2(10, 0);
            tooltipRect.offsetMax = new Vector2(-5, 0);

            Text tooltipText = tooltipObject.AddComponent<Text>();
            tooltipText.text = tooltip;
            tooltipText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            tooltipText.fontSize = 10;
            tooltipText.color = new Color(0.7f, 0.7f, 0.7f, 1f);
            tooltipText.alignment = TextAnchor.UpperLeft;

            // Slider
            GameObject sliderComponent = new GameObject("SliderComponent");
            sliderComponent.transform.SetParent(sliderObject.transform, false);

            RectTransform sliderComponentRect = sliderComponent.AddComponent<RectTransform>();
            sliderComponentRect.anchorMin = new Vector2(0.8f, 0.3f);
            sliderComponentRect.anchorMax = new Vector2(0.95f, 0.7f);
            sliderComponentRect.offsetMin = Vector2.zero;
            sliderComponentRect.offsetMax = Vector2.zero;

            Slider slider = sliderComponent.AddComponent<Slider>();
            slider.minValue = minValue;
            slider.maxValue = maxValue;
            slider.value = defaultValue;
            slider.interactable = !isCurrentlyClimbing;
            slider.wholeNumbers = true; // Force integer values
            slider.onValueChanged.AddListener((value) =>
            {
                int intValue = Mathf.RoundToInt(value);
                labelText.text = $"{label}: {intValue}";
                onValueChanged(intValue);
            });

            // Slider background
            Image sliderBgImage = sliderComponent.AddComponent<Image>();
            sliderBgImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);

            // Slider fill
            GameObject fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(sliderComponent.transform, false);

            RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
            fillAreaRect.anchorMin = Vector2.zero;
            fillAreaRect.anchorMax = Vector2.one;
            fillAreaRect.offsetMin = Vector2.zero;
            fillAreaRect.offsetMax = Vector2.zero;

            Image fillImage = fillArea.AddComponent<Image>();
            fillImage.color = new Color(0.4f, 0.4f, 0.4f, 1f);

            // Slider handle
            GameObject handle = new GameObject("Handle");
            handle.transform.SetParent(sliderComponent.transform, false);

            RectTransform handleRect = handle.AddComponent<RectTransform>();
            handleRect.anchorMin = new Vector2(0, 0);
            handleRect.anchorMax = new Vector2(0, 1);
            handleRect.sizeDelta = new Vector2(20, 0);

            Image handleImage = handle.AddComponent<Image>();
            handleImage.color = new Color(0.6f, 0.6f, 0.6f, 1f);

            // Set up slider references
            slider.fillRect = fillAreaRect;
            slider.handleRect = handleRect;

            return slider;
        }

        /// <summary>
        /// Creates a button in the button panel.
        /// </summary>
        public static void CreateButton(GameObject parent, string text, Vector2 anchorMin, Vector2 anchorMax, Color color, System.Action onClick)
        {
            GameObject buttonObject = new GameObject(text + "Button");
            buttonObject.transform.SetParent(parent.transform, false);

            RectTransform buttonRect = buttonObject.AddComponent<RectTransform>();
            buttonRect.anchorMin = anchorMin;
            buttonRect.anchorMax = anchorMax;
            buttonRect.offsetMin = new Vector2(5, 2);
            buttonRect.offsetMax = new Vector2(-5, -2);

            Image buttonImage = buttonObject.AddComponent<Image>();
            buttonImage.color = color;

            Button button = buttonObject.AddComponent<Button>();
            button.onClick.AddListener(() => onClick());
            button.navigation = new Navigation { mode = Navigation.Mode.None };

            // Button text
            GameObject textObject = new GameObject("Text");
            textObject.transform.SetParent(buttonObject.transform, false);

            RectTransform textRect = textObject.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            Text buttonText = textObject.AddComponent<Text>();
            buttonText.text = text;
            buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            buttonText.fontSize = 14;
            buttonText.color = Color.white;
            buttonText.alignment = TextAnchor.MiddleCenter;
        }
    }
}
