using System;
using UnityEngine;
using UnityEngine.UI;

public class GraphicsHudService : BaseWindow
{
    [SerializeField] GameQualityService gameQualityService;

    [Header("Quality Settings")]
    [SerializeField] private Slider qualitySlider;
    [SerializeField] private Text qualityLabel;

    protected override void OnInit()
    {
        InitializeSlider();
    }

    private void InitializeSlider()
    {
        qualitySlider.maxValue = QualitySettings.names.Length - 1;
        qualitySlider.wholeNumbers = true;
        qualitySlider.onValueChanged.AddListener(OnSliderMoved);

        int currentSettings = QualitySettings.GetQualityLevel();
        qualitySlider.value = currentSettings;

        SetLabelText(currentSettings);
    }

    private void OnSliderMoved(float sliderValue)
    {
        int currentSettingsIndex = Mathf.RoundToInt(sliderValue);
        SetLabelText(currentSettingsIndex);
        gameQualityService.ApplyCustomQualityLevel(currentSettingsIndex);
    }

    private void SetLabelText(int currentSettingsIndex)
    {
        var currentSettingsName = QualitySettings.names[currentSettingsIndex];
        qualityLabel.text = string.Format("QUALITY: {0}", currentSettingsName).ToUpper();
    }
}