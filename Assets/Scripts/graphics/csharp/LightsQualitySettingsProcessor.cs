using UnityEngine;

[System.Serializable]
public struct LightsQualitySettingsData
{
    public Light[] lights;
}

public class LightsQualitySettingsProcessor : IGameQualitySettingsProcessor
{
    private LightsQualitySettingsData lightQualitySettingsData;

    public LightsQualitySettingsProcessor(LightsQualitySettingsData lightQualitySettingsData)
    {
        this.lightQualitySettingsData = lightQualitySettingsData;
    }

    public void ApplySettings(GameQualityService gameQualityService)
    {
        var lights = lightQualitySettingsData.lights;

        if (lights == null || lights.Length == 0)
        {
            Debug.LogError("No lights were assigned!");
            return;
        }

        for (var l = 0; l < lights.Length; l++)
        {
            Light light = lights[l];

            if (light == null) continue;
            light.shadowStrength = (gameQualityService.CurrentRenderingPath == RenderingPath.DeferredLighting) ? 0.75f : 0.65f;
        }
    }
}