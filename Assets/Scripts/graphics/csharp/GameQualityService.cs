using UnityEngine;

public class GameQualityService : MonoBehaviour
{
    [SerializeField] private Vector2 dynamicLayersRange;
    [SerializeField] private CameraQualitySettingsData cameraQualitySettingsData;
    [SerializeField] private LightsQualitySettingsData lightsQualitySettingsData;
    [SerializeField] private ParticlesQualitySettingsData particlesQualitySettingsData;

    private float _dynamicObjectsFarClip;
    private RenderingPath _currentRenderingPath;
    private DepthTextureMode _currentDepthTextureMode;
    private IGameQualitySettingsProcessor[] _qualitySettingsProcessors;

    public Vector2 DynamicLayersRange { get { return dynamicLayersRange; } }
    public float DynamicObjectsFarClip { get { return _dynamicObjectsFarClip; } }
    public RenderingPath CurrentRenderingPath { get { return _currentRenderingPath; } }
    public DepthTextureMode CurrentDepthTextureMode { get { return _currentDepthTextureMode; } }

    private void Start()
    {
        _qualitySettingsProcessors = new IGameQualitySettingsProcessor[]
        {
            new CameraQualitySettingsProcessor(cameraQualitySettingsData),
            new LightsQualitySettingsProcessor(lightsQualitySettingsData),
            new ParticlesQualitySettingsProcessor(particlesQualitySettingsData)
        };
    }

    public void ApplyAllSettings()
    {
        foreach (var processor in _qualitySettingsProcessors)
        {
            processor.ApplySettings(this);
        }
    }
}