using UnityEngine;

[System.Serializable]
public struct CameraQualitySettingsData
{
    public Camera[] cameras;
    public int[] dynamicLayers;
}

public class CameraQualitySettingsProcessor : IGameQualitySettingsProcessor
{
    private CameraQualitySettingsData cameraQualitySettingsData;

    public CameraQualitySettingsProcessor(CameraQualitySettingsData cameraQualitySettingsData)
    {
        this.cameraQualitySettingsData = cameraQualitySettingsData;
    }

    public void ApplySettings(GameQualityService gameQualityService)
    {
        var distances = new float[32];

        int[] dynamicLayers = cameraQualitySettingsData.dynamicLayers;

        if (dynamicLayers == null || dynamicLayers.Length == 0)
        {
            Debug.LogError("No dynamic layers have been assigned!");
            return;
        }

        var dynamicLayersRange = gameQualityService.DynamicLayersRange;
        var dynamicDistance = (gameQualityService.DynamicObjectsFarClip * (dynamicLayersRange.y - dynamicLayersRange.x)) + dynamicLayersRange.x;

        for (var d = 0; d < dynamicLayers.Length; d++)
        {
            if (dynamicLayers[d] >= 0 && dynamicLayers[d] < 32)
            {
                distances[dynamicLayers[d]] = dynamicDistance;
            }
        }

        var cameras = cameraQualitySettingsData.cameras;

        for (var c = 0; c < cameras.Length; c++)
        {
            var camera = cameras[c];

            if (camera == null) continue;

            camera.layerCullDistances = distances;
            camera.renderingPath = gameQualityService.CurrentRenderingPath;
            camera.depthTextureMode = gameQualityService.CurrentDepthTextureMode;
        }
    }
}
