using UnityEngine;
using UnityEngine.UI;

public class DynamicObjectFarDistanceMenu : GraphicsSettingsMenuBase
{
    [SerializeField] private Slider qualitySlider;
    [SerializeField] private Text qualityLabel;

    private void Start()
    {
        qualitySlider.minValue = 10.0f;
        qualitySlider.maxValue = 100.0f ;

        qualitySlider.onValueChanged.AddListener(OnSliderMoved);
        SetLabelText(qualitySlider.value);
    }

    private void OnSliderMoved(float sliderValue)
    {
        SetLabelText(sliderValue);
        _gameQualityService.DynamicObjectsFarClip = sliderValue;
        _gameQualityService.ApplyAllSettings();
    }

    private void SetLabelText(float currentSettingsValue)
    {
        qualityLabel.text = string.Format("DYNAMIC: {0}", currentSettingsValue).ToUpper();
    }
}