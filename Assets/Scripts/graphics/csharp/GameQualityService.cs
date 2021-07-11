using UnityEngine;

public class GameQualityService : MonoBehaviour
{
    [SerializeField] private Vector2 dynamicLayersRange;
    [SerializeField] private CameraQualitySettingsData cameraQualitySettingsData;
    [SerializeField] private LightsQualitySettingsData lightsQualitySettingsData;
    [SerializeField] private ParticlesQualitySettingsData particlesQualitySettingsData;
    [SerializeField] private TerrainQualitySettingsData terrainQualitySettingsData;

    private float _dynamicObjectsFarClip = 1.0f;
    private RenderingPath _currentRenderingPath = RenderingPath.DeferredLighting;
    private DepthTextureMode _currentDepthTextureMode = DepthTextureMode.Depth;
    private SceneSettings[] _sceneSettings;

    private IGameQualitySettingsProcessor[] _qualitySettingsProcessors;

    public Vector2 DynamicLayersRange { get { return dynamicLayersRange; } }
    public float DynamicObjectsFarClip { get { return _dynamicObjectsFarClip; } }
    public RenderingPath CurrentRenderingPath { get { return _currentRenderingPath; } }
    public DepthTextureMode CurrentDepthTextureMode { get { return _currentDepthTextureMode; } }
    public SceneSettings CurrentSceneSettings { get { return _sceneSettings[0]; } }

    private void Start()
    {
        InitSceneSettings();

        _qualitySettingsProcessors = new IGameQualitySettingsProcessor[]
        {
            new CameraQualitySettingsProcessor(cameraQualitySettingsData),
            new LightsQualitySettingsProcessor(lightsQualitySettingsData),
            new ParticlesQualitySettingsProcessor(particlesQualitySettingsData),
            new TerrainQualitySettingsProcessor(terrainQualitySettingsData)
        };
    }

    private void Update()
    {
		// Test
        ApplyAllSettings();
    }

    private void InitSceneSettings()
    {
        if (_sceneSettings == null)
        {
            _sceneSettings = new SceneSettings[3];

            for (var i = 0; i < 3; i++)
            {
                _sceneSettings[i] = new SceneSettings();
            }
        }

        SceneSettings cur = _sceneSettings[0];
        Terrain nearTerrain = terrainQualitySettingsData.terrain;

        if (!cur.sceneInitialized)
        {
            cur.detailObjectDistance = Mathf.Clamp(nearTerrain.detailObjectDistance, 0.0f, 50.0f);
            cur.detailObjectDensity = Mathf.Clamp(nearTerrain.detailObjectDensity, 0.0f, 1.0f);
            cur.treeDistance = Mathf.Clamp(nearTerrain.treeDistance, 200.0f, 400.0f);
            cur.nearTerrainPixelError = Mathf.Clamp(nearTerrain.heightmapPixelError, 5.0f, 50.0f);
            cur.terrainTreesBillboardStart = Mathf.Clamp(nearTerrain.treeBillboardDistance, 10.0f, 70.0f);
            cur.maxMeshTrees = Mathf.Clamp(nearTerrain.treeMaximumFullLODCount, 5, 60);
            cur.heightmapMaximumLOD = nearTerrain.heightmapMaximumLOD;
        }
    }

    public void ApplyAllSettings()
    {
        foreach (var processor in _qualitySettingsProcessors)
        {
            processor.ApplySettings(this);
        }
    }

	private void ApplyCustomQualityLevel(int qualityLevel)
	{
		var dObjectDistance = 50.0f;
		var dObjectDensity = 1.0f;
		var nPError = 5.0f;
		var tDistance = 400.0f;
		var lod = 2;
		var billboards = 70.0f;
		var mTrees = 60;
		var fPError = 5.0f;

		switch (qualityLevel)
		{
			case 0: //FASTEST
				//ambientParticles = false;
				//particleQualityMultiplier = 0.0f;
				//dynamicObjectsFarClip = 0.0f;
				dObjectDistance = 0.0f;
				dObjectDensity = 0.0f;
				nPError = 50.0f;
				tDistance = 200.0f;
				lod = 2;
				billboards = 10.0f;
				mTrees = 5;
				fPError = 50.0f;
				_currentRenderingPath = RenderingPath.VertexLit;
				_currentDepthTextureMode = DepthTextureMode.None;
				//colorCorrection = false;
				//bloomAndFlares = false;
				//sunShafts = false;
				//depthOfField = false;
				//ssao = false;
				//clouds = false;
				//underwater = false;
				//water = 0;
				break;
			case 1: //FAST
				//ambientParticles = false;
				//particleQualityMultiplier = 0.2f;
				//dynamicObjectsFarClip = 0.2f;
				dObjectDistance = 10.0f;
				dObjectDensity = 0.1f;
				nPError = 41.0f;
				tDistance = 240.0f;
				lod = 2;
				billboards = 22.0f;
				mTrees = 16;
				fPError = 41.0f;
				_currentRenderingPath = RenderingPath.Forward;
				_currentDepthTextureMode = DepthTextureMode.None;
				//colorCorrection = true;
				//bloomAndFlares = false;
				//sunShafts = false;
				//depthOfField = false;
				//ssao = false;
				//clouds = false;
				//underwater = false;
				//water = 0;
				break;
			case 2: //SIMPLE
				//ambientParticles = false;
				//particleQualityMultiplier = 0.3f;
				//dynamicObjectsFarClip = 0.4f;
				dObjectDistance = 20.0f;
				dObjectDensity = 0.3f;
				nPError = 32.0f;
				tDistance = 280.0f;
				lod = 1;
				billboards = 34.0f;
				mTrees = 27;
				fPError = 32.0f;
				_currentRenderingPath = RenderingPath.Forward;
				_currentDepthTextureMode = DepthTextureMode.None;
				//colorCorrection = true;
				//bloomAndFlares = false;
				//sunShafts = false;
				//depthOfField = false;
				//ssao = false;
				//clouds = false;
				//underwater = false;
				//water = 0;
				break;
			case 3: //GOOD
				//ambientParticles = false;
				//particleQualityMultiplier = 0.4f;
				//dynamicObjectsFarClip = 0.6f;
				dObjectDistance = 35.0f;
				dObjectDensity = 0.4f;
				nPError = 23.0f;
				tDistance = 320.0f;
				lod = 1;
				billboards = 46.0f;
				mTrees = 38;
				fPError = 23.0f;
				_currentRenderingPath = RenderingPath.Forward;
				_currentDepthTextureMode = DepthTextureMode.None;
				//colorCorrection = true;
				//bloomAndFlares = true;
				//sunShafts = true;
				//depthOfField = false;
				//ssao = false;
				//clouds = false;
				//underwater = false;
				//water = 0;
				break;
			case 4: //BEAUTIFUL
				//ambientParticles = true;
				//particleQualityMultiplier = 0.5f;
				//dynamicObjectsFarClip = 0.8f;
				dObjectDistance = 40.0f;
				dObjectDensity = 0.6f;
				nPError = 14.0f;
				tDistance = 360.0f;
				lod = 0;
				billboards = 58.0f;
				mTrees = 49;
				fPError = 14.0f;
				_currentRenderingPath = RenderingPath.DeferredLighting;
				_currentDepthTextureMode = DepthTextureMode.Depth;
				//colorCorrection = true;
				//bloomAndFlares = true;
				//sunShafts = true;
				//depthOfField = true;
				//ssao = false;
				//clouds = true;
				//underwater = true;
				//water = 1;
				break;
			case 5: //FANTASTIC
				//ambientParticles = true;
				//particleQualityMultiplier = 1.0f;
				//dynamicObjectsFarClip = 1.0f;
				dObjectDistance = 50.0f;
				dObjectDensity = 1.0f;
				nPError = 5.0f;
				tDistance = 400.0f;
				lod = 0;
				billboards = 70.0f;
				mTrees = 60;
				fPError = 5.0f;
				_currentRenderingPath = RenderingPath.DeferredLighting;
				_currentDepthTextureMode = DepthTextureMode.Depth;
				//colorCorrection = true;
				//bloomAndFlares = true;
				//sunShafts = true;
				//depthOfField = true;
				//ssao = false;
				//clouds = true;
				//underwater = true;
				//water = 1;
				break;
		}

		var scenes = _sceneSettings;

		for (int i = 0; i < scenes.Length; i++)
		{
			scenes[i].sceneInitialized = true;
			scenes[i].detailObjectDistance = dObjectDistance;
			scenes[i].detailObjectDensity = dObjectDensity;
			scenes[i].nearTerrainPixelError = nPError;
			scenes[i].treeDistance = tDistance;
			scenes[i].heightmapMaximumLOD = lod;
			scenes[i].terrainTreesBillboardStart = billboards;
			scenes[i].maxMeshTrees = mTrees;
		}
	}
}