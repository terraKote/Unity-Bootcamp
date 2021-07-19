using UnityEngine;
using UnityEngine.UI;

public class OverralQualityLevelMenu : GraphicsSettingsMenuBase
{
    [SerializeField] private Slider qualitySlider;
    [SerializeField] private Text qualityLabel;

    private void Start()
    {
        qualitySlider.maxValue = QualitySettings.names.Length - 1;
        qualitySlider.wholeNumbers = true;

        int currentSettings = QualitySettings.GetQualityLevel();
        qualitySlider.value = currentSettings;

        qualitySlider.onValueChanged.AddListener(OnSliderMoved);
        SetLabelText(currentSettings);
    }

    private void OnSliderMoved(float sliderValue)
    {
        int currentSettingsIndex = Mathf.RoundToInt(sliderValue);
        SetLabelText(currentSettingsIndex);
        _gameQualityService.ApplyCustomQualityLevel(currentSettingsIndex);
    }

    private void SetLabelText(int currentSettingsIndex)
    {
        var currentSettingsName = QualitySettings.names[currentSettingsIndex];
        qualityLabel.text = string.Format("QUALITY: {0}", currentSettingsName).ToUpper();
    }
}
