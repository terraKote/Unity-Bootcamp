using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameQualitySettings : MonoBehaviour {
	//HAVE WE TAKE CURRENT GAME SETTINGS
	static private bool initializedGameSettings ;
	static private float _dynamicObjectsFarClip;
	
	static public SceneSettings[] scenes;
	
	//GAME SETTINGS
	static public int overallQuality ;
	static public float shadowDistance ;
	static public int masterTextureLimit ;
	static public bool anisotropicFiltering ;
	
	static public float particleQualityMultiplier  = 1.0f;
	static private float _particleQualityMultiplier  = 1.0f;
	
    //FULLSCREEN EFFECTS
    static public bool colorCorrection = true;
    static private bool _colorCorrection = true;

    static public bool bloomAndFlares  = true;
    static private bool _bloomAndFlares = true;

    static public bool sunShafts  = true;
    static private bool _sunShafts = true;

    static public bool depthOfField  = true;
    static private bool _depthOfField = true;
    
    static public bool depthOfFieldAvailable = true;
    static private bool _depthOfFieldAvailable = true;
    
    static public bool ssao  = false;
    static private bool _ssao = false;

    static public bool clouds = true;
    static private bool _clouds = true;

    static public bool underwater= true;
    static private bool _underwater = true;
    //END FULLSCREEN EFFECTS
    
    // WATER
    static private int _water = 1;
    static public int water = 1;

	static public bool ambientParticles = true;
	static private bool _ambientParticles = true;
	
	//RESET PER SCENE (MULTIPLY BASE DISTANCE EVERY TIME SCENE START)
	static public float dynamicObjectsFarClip= 0.55f;
	static public Vector2 _dynamicLayersRange ;
	static public Vector2 _staticLayersRange ;

    static public RenderingPath currentRenderingPath ;
    static public DepthTextureMode currentDepthTextureMode ;
	
	//LOCAL, PER SCENE, PROPERTIES
	public Camera[] cameras;
	
	public int[]  dynamicLayers ;
	public Vector2 dynamicLayersRange ;
	public Vector2 staticLayersRange ;
	
	public Terrain nearTerrain ;
	
	public GameObject[]  ambientParticleObjects ;
	private List<AmbientParticleSettings> _ambientParticleObjectSettings ;
	
    public Light[] lights;
	
	public bool mainMenu  = false;
	
	void Start()
	{
		_dynamicLayersRange = dynamicLayersRange;
		_staticLayersRange = staticLayersRange;
		_ambientParticles = ambientParticles;
		_ambientParticleObjectSettings = new  List<AmbientParticleSettings>();

		foreach (var go in ambientParticleObjects)
		{
			var setting  = new AmbientParticleSettings();
			if (go)
			{
				setting.minSize = go.GetComponent<ParticleEmitter>().minSize;
				setting.maxSize = go.GetComponent<ParticleEmitter>().maxSize;
				setting.minEmission = go.GetComponent<ParticleEmitter>().minEmission;
				setting.maxEmission = go.GetComponent<ParticleEmitter>().maxEmission;
			}
			_ambientParticleObjectSettings.Add(setting);
		}

		InitializeGameSettings();

		InitializeSceneSettings();

		InitializeQualitySettings((int)QualitySettings.currentLevel);

		InitializeCameraSettings();

		AutoChooseQualityLevel();
	}

	void AutoChooseQualityLevel()
	{
		var shaderLevel = SystemInfo.graphicsShaderLevel;
		var fillrate = SystemInfo.graphicsPixelFillrate;
		var vram = SystemInfo.graphicsMemorySize;
		var cpus = SystemInfo.processorCount;
		if (fillrate < 0)
		{
			if (shaderLevel < 10)
				fillrate = 1000;
			else if (shaderLevel < 20)
				fillrate = 1300;
			else if (shaderLevel < 30)
				fillrate = 2000;
			else
				fillrate = 3000;
			if (cpus >= 6)
				fillrate *= 3;
			else if (cpus >= 3)
				fillrate *= 2;
			if (vram >= 512)
				fillrate *= 2;
			else if (vram <= 128)
				fillrate /= 2;
		}

		var resx = Screen.width;
		var resy = Screen.height;
		var fillneed  = (resx * resy + 400f * 300f) * (30.0f / 1000000.0f);
		var levelmult  =new float[] { 5.0f, 30.0f, 80.0f, 130.0f, 200.0f, 320.0f };

		var level = 0;
		while (level < (int)QualityLevel.Fantastic && fillrate > fillneed * levelmult[level + 1])
			++level;

		//print (String.Format("{0}x{1} need {2} has {3} = {4} level", resx, resy, fillneed, fillrate, level));

		overallQuality = level;
		UpdateAllSettings();
	}

	void InitializeQualitySettings(int qualityLevel)
	{
		ApplyCustomQualityLevel(qualityLevel);

		_ambientParticles = ambientParticles;

		if (ambientParticleObjects != null)
		{
			for (int k = 0; k < ambientParticleObjects.Length; k++)
			{
				if (ambientParticleObjects[k] == null) continue;

				if (ambientParticleObjects[k].name == "dust" || ambientParticleObjects[k].name == "leaves") continue;

				ambientParticleObjects[k].SetActiveRecursively(ambientParticles);
			}
		}

		if (_particleQualityMultiplier != particleQualityMultiplier)
		{
			UpdateAmbientParticleQuality();
		}
	}

	void UpdateAmbientParticleQuality()
	{
		if (_particleQualityMultiplier != particleQualityMultiplier)
		{
			_particleQualityMultiplier = particleQualityMultiplier;

			for (int k = 0; k < ambientParticleObjects.Length; k++)
			{
				var setting = _ambientParticleObjectSettings[k];

				if (ambientParticleObjects[k] == null) continue;
				if (!ambientParticleObjects[k].active) continue;

				ambientParticleObjects[k].GetComponent<ParticleEmitter>().minSize = setting.minSize * _particleQualityMultiplier;
				ambientParticleObjects[k].GetComponent<ParticleEmitter>().maxSize = setting.maxSize * _particleQualityMultiplier;
				ambientParticleObjects[k].GetComponent<ParticleEmitter>().minEmission = setting.minEmission * _particleQualityMultiplier;
				ambientParticleObjects[k].GetComponent<ParticleEmitter>().maxEmission = setting.maxEmission * _particleQualityMultiplier;
			}
		}
	}

	void InitializeGameSettings()
	{
		if (initializedGameSettings) return;

		//If we are running the game first time, we need to take the current game quality settings
		initializedGameSettings = true;

		overallQuality = (int)QualitySettings.currentLevel;

		shadowDistance = QualitySettings.shadowDistance;

		masterTextureLimit = QualitySettings.masterTextureLimit;

		anisotropicFiltering = (QualitySettings.anisotropicFiltering == AnisotropicFiltering.ForceEnable);
	}

	void InitializeSceneSettings()
	{
		if (scenes == null)
		{
			scenes = new SceneSettings[3];

			for (var i  = 0; i < 3; i++)
			{
				scenes[i] = new SceneSettings();
			}
		}

		var currentScene = 0;// = Application.loadedLevel;

		if (Application.loadedLevelName == "demo_start_cutscene")
		{
			currentScene = 0;
		}
		else if (Application.loadedLevelName == "demo_forest")
		{
			currentScene = 1;
		}
		else if (Application.loadedLevelName == "demo_industry")
		{
			currentScene = 2;
		}

		var cur = scenes[currentScene];

		if (!cur.sceneInitialized)
		{
			//If this is the first time we entered this level, we need to take the current settings
			//into account...
			//cur.fogDensity = RenderSettings.fogDensity;

			if (nearTerrain != null)
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
		else
		{
			//If we have already entered the level, we overwrite current scene settings as 
			//the user may have changed some settings...
			//RenderSettings.fogDensity = cur.fogDensity;

			if (nearTerrain != null)
			{
				nearTerrain.detailObjectDistance = cur.detailObjectDistance;
				nearTerrain.detailObjectDensity = cur.detailObjectDensity;
				nearTerrain.treeDistance = cur.treeDistance;
				nearTerrain.heightmapPixelError = cur.nearTerrainPixelError;
				nearTerrain.treeBillboardDistance = cur.terrainTreesBillboardStart;
				nearTerrain.treeMaximumFullLODCount = cur.maxMeshTrees;
				nearTerrain.heightmapMaximumLOD = cur.heightmapMaximumLOD;
			}
		}
	}

	void InitializeCameraSettings()
	{
		_dynamicObjectsFarClip = dynamicObjectsFarClip;

		_colorCorrection = colorCorrection;
		_bloomAndFlares = bloomAndFlares;
		_sunShafts = sunShafts;
		_depthOfField = depthOfField;
		_ssao = ssao;
		_clouds = clouds;
		_underwater = underwater;
		_depthOfFieldAvailable = depthOfFieldAvailable;
		_water = water;

		if (_particleQualityMultiplier != particleQualityMultiplier)
		{
			UpdateAmbientParticleQuality();
		}

		if (cameras != null)
		{
			var distances  = new float[32];

			if (dynamicLayers != null)
			{
				var dynamicDistance  = (dynamicObjectsFarClip * (dynamicLayersRange.y - dynamicLayersRange.x)) + dynamicLayersRange.x;

				for (var d  = 0; d < dynamicLayers.Length; d++)
				{
					if (dynamicLayers[d] >= 0 && dynamicLayers[d] < 32)
					{
						distances[dynamicLayers[d]] = dynamicDistance;
					}
				}
			}

			ColorCorrectionCurves cCorrection;
			BloomAndFlares bloomFlares;
			SunShafts shafts;
			DepthOfField depth ;
			SSAOEffect screenSpaceAO;
			//var cloud : CloudEffects;

			//var water : ???

			if (cameras.Length > 0)
			{
				for (var c  = 0; c < cameras.Length; c++)
				{
					if (cameras[c] == null) continue;

					cameras[c].layerCullDistances = distances;

					cameras[c].renderingPath = currentRenderingPath;
					cameras[c].depthTextureMode = currentDepthTextureMode;

					cCorrection = cameras[c].GetComponent<ColorCorrectionCurves>();
					if (cCorrection != null) cCorrection.enabled = colorCorrection;

					bloomFlares = cameras[c].GetComponent<BloomAndFlares>();
					if (bloomFlares != null) bloomFlares.enabled = bloomAndFlares;

					shafts = cameras[c].GetComponent<SunShafts>();
					if (shafts != null) shafts.enabled = sunShafts;
					if (shafts && currentDepthTextureMode == DepthTextureMode.None)
					{
						(shafts as SunShafts).useDepthTexture = false;
					}
					else if (shafts)
						(shafts as SunShafts).useDepthTexture = true;

					depth = cameras[c].GetComponent<DepthOfField>();
					if (depth != null) depth.available = depthOfFieldAvailable;

					screenSpaceAO = cameras[c].GetComponent<SSAOEffect>();
					if (screenSpaceAO != null) screenSpaceAO.enabled = ssao;

					//cloud = cameras[c].GetComponent("CloudEffects");
					//if(cloud != null) cloud.enabled = clouds;
				}
			}

			if (lights != null)
			{
				for (var l  = 0; l < lights.Length; l++)
                {
					if (lights[l] == null) continue;

					lights[l].shadowStrength = (currentRenderingPath == RenderingPath.DeferredLighting) ? 0.75f : 0.65f;
				}
			}
		}
	}

	void Update()
	{
		if (GameManager.GetInstance().pause || mainMenu)
		{
			UpdateAllSettings();
		}
	}

	void UpdateAllSettings()
	{
		UpdateGameQuality();
		UpdateSceneQuality();
		UpdateCameraSettings();
	}

	void ApplyCustomQualityLevel(int qualityLevel )
	{
		var dObjectDistance  = 50.0f;
		var dObjectDensity = 1.0f;
		var nPError  = 5.0f;
		var tDistance = 400.0f;
		var lod  = 2;
		var billboards = 70.0f;
		var mTrees  = 60;
		var fPError = 5.0f;

		switch (qualityLevel)
		{
			case 0: //FASTEST
				ambientParticles = false;
				particleQualityMultiplier = 0.0f;
				dynamicObjectsFarClip = 0.0f;
				dObjectDistance = 0.0f;
				dObjectDensity = 0.0f;
				nPError = 50.0f;
				tDistance = 200.0f;
				lod = 2;
				billboards = 10.0f;
				mTrees = 5;
				fPError = 50.0f;
				currentRenderingPath = RenderingPath.VertexLit;
				currentDepthTextureMode = DepthTextureMode.None;
				colorCorrection = false;
				bloomAndFlares = false;
				sunShafts = false;
				depthOfField = false;
				ssao = false;
				clouds = false;
				underwater = false;
				water = 0;
				break;
			case 1: //FAST
				ambientParticles = false;
				particleQualityMultiplier = 0.2f;
				dynamicObjectsFarClip = 0.2f;
				dObjectDistance = 10.0f;
				dObjectDensity = 0.1f;
				nPError = 41.0f;
				tDistance = 240.0f;
				lod = 2;
				billboards = 22.0f;
				mTrees = 16;
				fPError = 41.0f;
				currentRenderingPath = RenderingPath.Forward;
				currentDepthTextureMode = DepthTextureMode.None;
				colorCorrection = true;
				bloomAndFlares = false;
				sunShafts = false;
				depthOfField = false;
				ssao = false;
				clouds = false;
				underwater = false;
				water = 0;
				break;
			case 2: //SIMPLE
				ambientParticles = false;
				particleQualityMultiplier = 0.3f;
				dynamicObjectsFarClip = 0.4f;
				dObjectDistance = 20.0f;
				dObjectDensity = 0.3f;
				nPError = 32.0f;
				tDistance = 280.0f;
				lod = 1;
				billboards = 34.0f;
				mTrees = 27;
				fPError = 32.0f;
				currentRenderingPath = RenderingPath.Forward;
				currentDepthTextureMode = DepthTextureMode.None;
				colorCorrection = true;
				bloomAndFlares = false;
				sunShafts = false;
				depthOfField = false;
				ssao = false;
				clouds = false;
				underwater = false;
				water = 0;
				break;
			case 3: //GOOD
				ambientParticles = false;
				particleQualityMultiplier = 0.4f;
				dynamicObjectsFarClip = 0.6f;
				dObjectDistance = 35.0f;
				dObjectDensity = 0.4f;
				nPError = 23.0f;
				tDistance = 320.0f;
				lod = 1;
				billboards = 46.0f;
				mTrees = 38;
				fPError = 23.0f;
				currentRenderingPath = RenderingPath.Forward;
				currentDepthTextureMode = DepthTextureMode.None;
				colorCorrection = true;
				bloomAndFlares = true;
				sunShafts = true;
				depthOfField = false;
				ssao = false;
				clouds = false;
				underwater = false;
				water = 0;
				break;
			case 4: //BEAUTIFUL
				ambientParticles = true;
				particleQualityMultiplier = 0.5f;
				dynamicObjectsFarClip = 0.8f;
				dObjectDistance = 40.0f;
				dObjectDensity = 0.6f;
				nPError = 14.0f;
				tDistance = 360.0f;
				lod = 0;
				billboards = 58.0f;
				mTrees = 49;
				fPError = 14.0f;
				currentRenderingPath = RenderingPath.DeferredLighting;
				currentDepthTextureMode = DepthTextureMode.Depth;
				colorCorrection = true;
				bloomAndFlares = true;
				sunShafts = true;
				depthOfField = true;
				ssao = false;
				clouds = true;
				underwater = true;
				water = 1;
				break;
			case 5: //FANTASTIC
				ambientParticles = true;
				particleQualityMultiplier = 1.0f;
				dynamicObjectsFarClip = 1.0f;
				dObjectDistance = 50.0f;
				dObjectDensity = 1.0f;
				nPError = 5.0f;
				tDistance = 400.0f;
				lod = 0;
				billboards = 70.0f;
				mTrees = 60;
				fPError = 5.0f;
				currentRenderingPath = RenderingPath.DeferredLighting;
				currentDepthTextureMode = DepthTextureMode.Depth;
				colorCorrection = true;
				bloomAndFlares = true;
				sunShafts = true;
				depthOfField = true;
				ssao = false;
				clouds = true;
				underwater = true;
				water = 1;
				break;
		}

		if (scenes != null)
		{
			for (int i  = 0; i < scenes.Length; i++)
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

	private void UpdateGameQuality()
	{
		if ((int)QualitySettings.currentLevel != overallQuality)
		{
			QualitySettings.currentLevel =(QualityLevel)overallQuality;

			initializedGameSettings = false;

			ApplyCustomQualityLevel(overallQuality);

			InitializeGameSettings();
		}
		else
		{
			if (QualitySettings.shadowDistance != shadowDistance)
			{
				QualitySettings.shadowDistance = shadowDistance;
			}

			if (QualitySettings.masterTextureLimit != masterTextureLimit)
			{
				QualitySettings.masterTextureLimit = masterTextureLimit;
			}

			if ((QualitySettings.anisotropicFiltering == AnisotropicFiltering.ForceEnable) != anisotropicFiltering)
			{
				if (anisotropicFiltering)
				{
					QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;
				}
				else
				{
					if (overallQuality < 2)
					{
						QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
					}
					else
					{
						QualitySettings.anisotropicFiltering = AnisotropicFiltering.Enable;
					}
				}
			}
		}
	}

	private void UpdateSceneQuality()
	{
		if (scenes == null) return;

		int currentScene = 0;// = Application.loadedLevel;

		if (Application.loadedLevelName == "demo_start_cutscene")
		{
			currentScene = 0;
		}
		else if (Application.loadedLevelName == "demo_forest")
		{
			currentScene = 1;
		}
		else if (Application.loadedLevelName == "demo_industry")
		{
			currentScene = 2;
		}

		if (currentScene < 0 || currentScene >= scenes.Length) return;

		var cur = scenes[currentScene];
		if (cur == null) return;

		if (nearTerrain != null)
		{
			if (nearTerrain.detailObjectDistance != cur.detailObjectDistance)
				nearTerrain.detailObjectDistance = cur.detailObjectDistance;
			if (nearTerrain.detailObjectDensity != cur.detailObjectDensity)
				nearTerrain.detailObjectDensity = cur.detailObjectDensity;
			if (nearTerrain.treeDistance != cur.treeDistance)
				nearTerrain.treeDistance = cur.treeDistance;
			if (nearTerrain.heightmapPixelError != cur.nearTerrainPixelError)
				nearTerrain.heightmapPixelError = cur.nearTerrainPixelError;
			if (nearTerrain.treeBillboardDistance != cur.terrainTreesBillboardStart)
				nearTerrain.treeBillboardDistance = cur.terrainTreesBillboardStart;
			if (nearTerrain.treeMaximumFullLODCount != cur.maxMeshTrees)
				nearTerrain.treeMaximumFullLODCount = cur.maxMeshTrees;
			if (nearTerrain.heightmapMaximumLOD != cur.heightmapMaximumLOD)
				nearTerrain.heightmapMaximumLOD = cur.heightmapMaximumLOD;
		}

		if (_ambientParticles != ambientParticles)
		{
			_ambientParticles = ambientParticles;

			if (ambientParticleObjects != null)
			{
				for (var k  = 0; k < ambientParticleObjects.Length; k++)
				{
					if (ambientParticleObjects[k] == null) continue;

					if (ambientParticleObjects[k].name == "dust" || ambientParticleObjects[k].name == "leaves") continue;

					ambientParticleObjects[k].SetActiveRecursively(ambientParticles);
				}
			}
		}
	}

	private void UpdateCameraSettings()
	{
		if ((_dynamicObjectsFarClip != dynamicObjectsFarClip) ||
			(_colorCorrection != colorCorrection) ||
			(_bloomAndFlares != bloomAndFlares) ||
			(_sunShafts != sunShafts) ||
			(_depthOfField != depthOfField) ||
			(_depthOfFieldAvailable != depthOfFieldAvailable) ||
			(_ssao != ssao) ||
			(_clouds != clouds) ||
			(_underwater != underwater) ||
			(_water != water) ||
			(_particleQualityMultiplier != particleQualityMultiplier))
		{
			InitializeCameraSettings();
		}
	}
}
