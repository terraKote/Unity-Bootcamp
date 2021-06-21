using System;
using UnityEngine;
using UnityEngine.UI;

public class GraphicsHudService : BaseWindow
{
    [Header("Quality Settings")]
    [SerializeField] private Slider qualitySlider;
    [SerializeField] private Text qualityLabel;

    private void Start()
    {
        InitializeSlider();
        SetLabelText(0);
    }

    private void InitializeSlider()
    {
        qualitySlider.maxValue = QualitySettings.names.Length - 1;
        qualitySlider.wholeNumbers = true;
        qualitySlider.onValueChanged.AddListener(OnSliderMoved);
    }

    private void OnSliderMoved(float sliderValue)
    {
        int currentSettingsIndex = Mathf.RoundToInt(sliderValue);
        SetLabelText(currentSettingsIndex);
    }

    private void SetLabelText(int currentSettingsIndex)
    {
        var currentSettingsName = QualitySettings.names[currentSettingsIndex];
        qualityLabel.text = string.Format("QUALITY: {0}", currentSettingsName).ToUpper();
    }
}