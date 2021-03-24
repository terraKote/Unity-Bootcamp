#pragma strict
#pragma implicit
#pragma downcast

class SceneSettings
{
	//SCENE SETTINGS
	public var sceneInitialized : boolean = false;
	
	public var detailObjectDistance : float;
	public var detailObjectDensity : float;
	public var treeDistance : float;
	public var nearTerrainPixelError : float;
	public var terrainTreesBillboardStart : float;
	public var maxMeshTrees : int;
	public var heightmapMaximumLOD : int;
}

	class AmbientParticleSettings {
		var minSize : float;
		var maxSize : float;
		var minEmission : float;
		var maxEmission : float;	
	};

class GameQualitySettings extends MonoBehaviour
{
	//HAVE WE TAKE CURRENT GAME SETTINGS
	static private var initializedGameSettings : boolean;
	static private var _dynamicObjectsFarClip : float;
	
	static public var scenes : SceneSettings[];
	
	//GAME SETTINGS
	static public var overallQuality : int;
	static public var shadowDistance : float;
	static public var masterTextureLimit : int;
	static public var anisotropicFiltering : boolean;
	
	static public var particleQualityMultiplier : float = 1.0;
	static private var _particleQualityMultiplier : float = 1.0;
	
    //FULLSCREEN EFFECTS
    static public var colorCorrection : boolean = true;
    static private var _colorCorrection : boolean = true;

    static public var bloomAndFlares : boolean = true;
    static private var _bloomAndFlares : boolean = true;

    static public var sunShafts : boolean = true;
    static private var _sunShafts : boolean = true;

    static public var depthOfField : boolean = true;
    static private var _depthOfField : boolean = true;
    
    static public var depthOfFieldAvailable : boolean = true;
    static private var _depthOfFieldAvailable : boolean = true;
    
    static public var ssao : boolean = false;
    static private var _ssao : boolean = false;

    static public var clouds : boolean = true;
    static private var _clouds : boolean = true;

    static public var underwater : boolean = true;
    static private var _underwater : boolean = true;
    //END FULLSCREEN EFFECTS
    
    // WATER
    static private var _water : int = 1;
    static public var water : int = 1;

	static public var ambientParticles : boolean = true;
	static private var _ambientParticles : boolean = true;
	
	//RESET PER SCENE (MULTIPLY BASE DISTANCE EVERY TIME SCENE START)
	static public var dynamicObjectsFarClip : float = 0.55;
	static public var _dynamicLayersRange : Vector2;
	static public var _staticLayersRange : Vector2;

    static public var currentRenderingPath : RenderingPath;
    static public var currentDepthTextureMode : DepthTextureMode;
	
	//LOCAL, PER SCENE, PROPERTIES
	public var cameras : Camera[];
	
	public var dynamicLayers : int[];
	public var dynamicLayersRange : Vector2;
	public var staticLayersRange : Vector2;
	
	public var nearTerrain : Terrain;
	
	public var ambientParticleObjects : GameObject[];
	private var _ambientParticleObjectSettings : Array;
	
    public var lights : Light[];
	
	public var mainMenu : boolean = false;
	
	function Start ()
	{
		_dynamicLayersRange = dynamicLayersRange;
		_staticLayersRange = staticLayersRange;
		_ambientParticles = ambientParticles;
		_ambientParticleObjectSettings = new Array();
		
		for(var go : GameObject in ambientParticleObjects) 
		{
			var setting : AmbientParticleSettings = new AmbientParticleSettings();
			if(go) {
			setting.minSize = go.particleEmitter.minSize;
			setting.maxSize = go.particleEmitter.maxSize;
			setting.minEmission = go.particleEmitter.minEmission;
			setting.maxEmission = go.particleEmitter.maxEmission;
			}
			_ambientParticleObjectSettings.Push(setting);
		}
		
		InitializeGameSettings();
		
		InitializeSceneSettings();

		InitializeQualitySettings(QualitySettings.currentLevel);
		
		InitializeCameraSettings();
		
		AutoChooseQualityLevel ();
	}
	
	function AutoChooseQualityLevel ()
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
		var fillneed : float = (resx*resy + 400*300) * (30.0 / 1000000.0);
		var levelmult : float[] = [5.0, 30.0, 80.0, 130.0, 200.0, 320.0];
		
		var level = 0;
		while (level < QualityLevel.Fantastic && fillrate > fillneed * levelmult[level+1])
			++level;
		
		//print (String.Format("{0}x{1} need {2} has {3} = {4} level", resx, resy, fillneed, fillrate, level));

		overallQuality = level;
		UpdateAllSettings ();
	}
	
	function InitializeQualitySettings(qualityLevel : int)
	{
		ApplyCustomQualityLevel (qualityLevel);
		
		_ambientParticles = ambientParticles;
		
		if(ambientParticleObjects != null)
		{
			for(var k : int = 0; k < ambientParticleObjects.length; k++)
			{
				if(ambientParticleObjects[k] == null) continue;
				
                if(ambientParticleObjects[k].name == "dust" || ambientParticleObjects[k].name == "leaves") continue;

				ambientParticleObjects[k].SetActiveRecursively(ambientParticles);
			}
		}
		
		if(_particleQualityMultiplier != particleQualityMultiplier) 
		{	
			UpdateAmbientParticleQuality();
		}
	}
	
	function UpdateAmbientParticleQuality() 
	{
		if(_particleQualityMultiplier != particleQualityMultiplier) 
		{	
			_particleQualityMultiplier = particleQualityMultiplier;	
			
			for(var k : int = 0; k < ambientParticleObjects.length; k++)
			{
				var setting : AmbientParticleSettings = _ambientParticleObjectSettings[k] as AmbientParticleSettings;
					
				if(ambientParticleObjects[k] == null) continue;
				if(!ambientParticleObjects[k].active) continue;			
			
				ambientParticleObjects[k].particleEmitter.minSize = setting.minSize*_particleQualityMultiplier;
				ambientParticleObjects[k].particleEmitter.maxSize = setting.maxSize*_particleQualityMultiplier;
				ambientParticleObjects[k].particleEmitter.minEmission = setting.minEmission*_particleQualityMultiplier;
				ambientParticleObjects[k].particleEmitter.maxEmission = setting.maxEmission*_particleQualityMultiplier;
			}
		}	
	}
	
	function InitializeGameSettings()
	{
		if(initializedGameSettings) return;
		
		//If we are running the game first time, we need to take the current game quality settings
		initializedGameSettings = true;
		
		overallQuality = QualitySettings.currentLevel; 
	
		shadowDistance = QualitySettings.shadowDistance;
	
		masterTextureLimit = QualitySettings.masterTextureLimit;

		anisotropicFiltering = (QualitySettings.anisotropicFiltering == AnisotropicFiltering.ForceEnable);
	}
	
	function InitializeSceneSettings()
	{
		if(scenes == null)
		{
			scenes = new SceneSettings[3];
			
			for(var i : int = 0; i < 3; i++)
			{
				scenes[i] = new SceneSettings();
			}
		}
		
		var currentScene : int;// = Application.loadedLevel;
		
        if(Application.loadedLevelName == "demo_start_cutscene")
        {
            currentScene = 0;
        }
        else if(Application.loadedLevelName == "demo_forest")
        {
            currentScene = 1;
        }
        else if(Application.loadedLevelName == "demo_industry")
        {
            currentScene = 2;
        }
		
		var cur = scenes[currentScene];

		if(!cur.sceneInitialized)
		{
			//If this is the first time we entered this level, we need to take the current settings
			//into account...
			//cur.fogDensity = RenderSettings.fogDensity;
			
			if(nearTerrain != null)
			{
				cur.detailObjectDistance = Mathf.Clamp(nearTerrain.detailObjectDistance, 0.0, 50.0);
				cur.detailObjectDensity = Mathf.Clamp(nearTerrain.detailObjectDensity, 0.0, 1.0);
				cur.treeDistance = Mathf.Clamp(nearTerrain.treeDistance, 200.0, 400.0);
				cur.nearTerrainPixelError = Mathf.Clamp(nearTerrain.heightmapPixelError, 5.0, 50.0);
				cur.terrainTreesBillboardStart = Mathf.Clamp(nearTerrain.treeBillboardDistance, 10.0, 70.0);
				cur.maxMeshTrees = Mathf.Clamp(nearTerrain.treeMaximumFullLODCount, 5, 60);
				cur.heightmapMaximumLOD = nearTerrain.heightmapMaximumLOD;
			}
		}
		else
		{
			//If we have already entered the level, we overwrite current scene settings as 
			//the user may have changed some settings...
			//RenderSettings.fogDensity = cur.fogDensity;
			
			if(nearTerrain != null)
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
	
	function InitializeCameraSettings()
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
		
		if(_particleQualityMultiplier != particleQualityMultiplier) 
		{	
			UpdateAmbientParticleQuality();
		}
		
		if(cameras != null)
		{
			var distances : float[] = new float[32];
			
			if(dynamicLayers != null)
			{
				var dynamicDistance : float = (dynamicObjectsFarClip * (dynamicLayersRange.y - dynamicLayersRange.x)) + dynamicLayersRange.x;
				
				for(var d : int = 0; d < dynamicLayers.Length; d++)
				{
					if(dynamicLayers[d] >= 0 && dynamicLayers[d] < 32)
					{
						distances[dynamicLayers[d]] = dynamicDistance;
					}
				}
			}
			
            var cCorrection : ColorCorrectionCurves;
            var bloomFlares : BloomAndFlares;
            var shafts : SunShafts;
            var depth : DepthOfField;
            var screenSpaceAO : SSAOEffect;
            //var cloud : CloudEffects;

            //var water : ???

			if(cameras.length > 0)
			{
				for(var c : int = 0; c < cameras.length; c++)
				{
					if(cameras[c] == null) continue;
					
					cameras[c].layerCullDistances = distances;

                    cameras[c].renderingPath = currentRenderingPath;
                    cameras[c].depthTextureMode = currentDepthTextureMode;

                    cCorrection = cameras[c].GetComponent("ColorCorrectionCurves");
                    if(cCorrection != null) cCorrection.enabled = colorCorrection;

                    bloomFlares = cameras[c].GetComponent("BloomAndFlares");
                    if(bloomFlares != null) bloomFlares.enabled = bloomAndFlares;

                    shafts = cameras[c].GetComponent("SunShafts");
                    if(shafts != null) shafts.enabled = sunShafts;
                    if(shafts && currentDepthTextureMode == DepthTextureMode.None) {
                    	(shafts as SunShafts).useDepthTexture = false;
                    } else if(shafts)
                    	(shafts as SunShafts).useDepthTexture = true;

                    depth = cameras[c].GetComponent("DepthOfField");
                    if(depth != null ) depth.available = depthOfFieldAvailable;

                    screenSpaceAO = cameras[c].GetComponent("SSAOEffect");
                    if(screenSpaceAO != null) screenSpaceAO.enabled = ssao;

                    //cloud = cameras[c].GetComponent("CloudEffects");
                    //if(cloud != null) cloud.enabled = clouds;
				}
			}

            if(lights != null)
            {
                for(var l : int = 0; l < lights.length; l++)
                {
                    if(lights[l] == null) continue;

                    lights[l].shadowStrength = (currentRenderingPath == RenderingPath.DeferredLighting) ? 0.75 : 0.65;
                }
            }
		}
	}
	
	function Update()
	{
		if(GameManager.pause || mainMenu)
		{
			UpdateAllSettings ();
		}
	}
	
	function UpdateAllSettings ()
	{
		UpdateGameQuality();			
		UpdateSceneQuality();
		UpdateCameraSettings();
	}
	
	function ApplyCustomQualityLevel(qualityLevel : int)
	{
		var dObjectDistance : float = 50.0;
		var dObjectDensity : float = 1.0;
		var nPError : float = 5.0;
		var tDistance : float = 400.0;
		var lod : int = 2;
		var billboards : float = 70.0;
		var mTrees : int = 60;
		var fPError : float = 5.0;
		
		switch(qualityLevel)
		{
			case 0: //FASTEST
				ambientParticles = false;
				particleQualityMultiplier = 0.0;
				dynamicObjectsFarClip = 0.0;
				dObjectDistance = 0.0;
				dObjectDensity = 0.0;
				nPError = 50.0;
				tDistance = 200.0;
				lod = 2;
				billboards = 10.0;
				mTrees = 5;
				fPError = 50.0;
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
				particleQualityMultiplier = 0.2;
				dynamicObjectsFarClip = 0.2;
				dObjectDistance = 10.0;
				dObjectDensity = 0.1;
				nPError = 41.0;
				tDistance = 240.0;
				lod = 2;
				billboards = 22.0;
				mTrees = 16.0;
				fPError = 41.0;
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
				particleQualityMultiplier = 0.3;
				dynamicObjectsFarClip = 0.4;
				dObjectDistance = 20.0;
				dObjectDensity = 0.3;
				nPError = 32.0;
				tDistance = 280.0;
				lod = 1;
				billboards = 34.0;
				mTrees = 27.0;
				fPError = 32.0;
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
				particleQualityMultiplier = 0.4;
				dynamicObjectsFarClip = 0.6;
				dObjectDistance = 35.0;
				dObjectDensity = 0.4;
				nPError = 23.0;
				tDistance = 320.0;
				lod = 1;
				billboards = 46.0;
				mTrees = 38.0;
				fPError = 23.0;
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
				particleQualityMultiplier = 0.5;
				dynamicObjectsFarClip = 0.8;
				dObjectDistance = 40.0;
				dObjectDensity = 0.6;
				nPError = 14.0;
				tDistance = 360.0;
				lod = 0;
				billboards = 58.0;
				mTrees = 49.0;
				fPError = 14.0;
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
				particleQualityMultiplier = 1.0;
				dynamicObjectsFarClip = 1.0;
				dObjectDistance = 50.0;
				dObjectDensity = 1.0;
				nPError = 5.0;
				tDistance = 400.0;
				lod = 0;
				billboards = 70.0;
				mTrees = 60;
				fPError = 5.0;
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

		if(scenes != null)
		{
			for(var i : int = 0; i < scenes.length; i++)
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
	
	private function UpdateGameQuality()
	{
		if(QualitySettings.currentLevel != overallQuality)
		{
			QualitySettings.currentLevel = overallQuality;
			
			initializedGameSettings = false;
			
			ApplyCustomQualityLevel(overallQuality);
			
			InitializeGameSettings();
		}
		else
		{
			if(QualitySettings.shadowDistance != shadowDistance)
			{
				QualitySettings.shadowDistance = shadowDistance;
			}
			
			if(QualitySettings.masterTextureLimit != masterTextureLimit)
			{
				QualitySettings.masterTextureLimit = masterTextureLimit;
			}
			
			if((QualitySettings.anisotropicFiltering == AnisotropicFiltering.ForceEnable) != anisotropicFiltering)
			{
				if(anisotropicFiltering)
				{
					QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;
				}
				else
				{
					if(overallQuality < 2)
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
	
	private function UpdateSceneQuality()
	{
		if(scenes == null) return;

		var currentScene : int;// = Application.loadedLevel;
		
        if(Application.loadedLevelName == "demo_start_cutscene")
        {
            currentScene = 0;
        }
        else if(Application.loadedLevelName == "demo_forest")
        {
            currentScene = 1;
        }
        else if(Application.loadedLevelName == "demo_industry")
        {
            currentScene = 2;
        }

		if(currentScene < 0 || currentScene >= scenes.Length) return;
		
		var cur = scenes[currentScene];
		if(cur == null) return;
		
		if(nearTerrain != null)
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
		
		if(_ambientParticles != ambientParticles)
		{
			_ambientParticles = ambientParticles;
			
			if(ambientParticleObjects != null)
			{
				for(var k : int = 0; k < ambientParticleObjects.length; k++)
				{
					if(ambientParticleObjects[k] == null) continue;
					
                    if(ambientParticleObjects[k].name == "dust" || ambientParticleObjects[k].name == "leaves") continue;

					ambientParticleObjects[k].SetActiveRecursively(ambientParticles);
				}
			}
		}
	}
	
	private function UpdateCameraSettings()
	{
		if((_dynamicObjectsFarClip != dynamicObjectsFarClip) || 
            (_colorCorrection != colorCorrection) ||
            (_bloomAndFlares != bloomAndFlares) ||
            (_sunShafts != sunShafts) ||
            (_depthOfField != depthOfField) ||
            (_depthOfFieldAvailable != depthOfFieldAvailable) ||
            (_ssao != ssao) ||
            (_clouds != clouds) ||
            (_underwater != underwater) ||
            (_water != water)  ||
            (_particleQualityMultiplier != particleQualityMultiplier) )
		{
			InitializeCameraSettings();
		}
	}
}