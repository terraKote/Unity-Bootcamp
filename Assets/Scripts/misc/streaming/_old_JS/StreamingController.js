enum StreamingStep
{
    None,
    Helicopter,
    Pilot,
    Wingman,
    Coleague,
    Soldier,
    Terrain,
    Cutscene,
}

class StreamingController extends MonoBehaviour
{
    private var currentOp : AsyncOperation;
    private var streamingStep : StreamingStep;
    private var ready : boolean;
    private var helicopterGO : GameObject;
    private var crewContainerGO : GameObject;
    private var cutsceneController : StartCutscene;
    private var auxGO : GameObject;
    private var readyToPlayCutscene : boolean;

	public var heliParent : Transform;
	
	public var cutIterations : int = 1;
	public var fixedCamAnimationWeights : float[];
	public var fixedCamAngles : Transform[];
	public var cloudBed : GameObject;
		
	public var theScale : float = 2.0;

    public var lerpSpeed : float = 3.0;
    private var loadedSoldiers : boolean;

    private var currentProgress : float;

    public var heliStartPoint : Transform;
    public var heliEndPoint : Transform;
    public var heliFlyAwayPoint : Transform;
    public var heliHoverPoint : Transform;

    public var startFOV : float;
    public var endFOV : float;

    public var fakeClouds : GameObject;
    private var readyToLoadTerrain : boolean;

    public var textStyle : GUIStyle;
    public var loadingBackground : Texture2D;
    private var angle : float;

    static public var loadForest : boolean;
    public var blackTexture : Texture2D;
    private var alpha : float;
    private var started : boolean;

    public var mainMenu : MainMenuScreen;
    public var sarge : SargeManager;
    public var gameManager : GameManager;
    public var quality : GameQualitySettings;

    private var heliSound : AudioSource;

    private var con : WWW;
    static public var baseAddress : String;
    private var auxBundle : AssetBundle;

    private var helicopterProgress : float;
    private var pilotProgress : float;
    private var wingmanProgress : float;
    private var coleagueProgress : float;
    private var soldierProgress : float;
    private var terrainProgress : float;
    private var forestProgress : float;

    function Start()
    {
        started = false;
        currentProgress = 0.0;
        angle = 0.0;
        loadedSoldiers = false;
        readyToPlayCutscene = false;
        streamingStep = streamingStep.None;    
        readyToLoadTerrain = false;
        camera.fieldOfView = startFOV;
        loadForest = false;
        alpha = 0.0;

        Screen.lockCursor = true;

		if(Application.isEditor)
        {
            baseAddress = "file://" + Application.dataPath + "/../webplayer/";
        }
        else if(Application.platform == RuntimePlatform.OSXPlayer && Application.platform != RuntimePlatform.WindowsPlayer)
		{
			baseAddress = "file://" + Application.dataPath + "../../webplayer/";
        }
        else
        {
        	baseAddress = "";
        }

        con = new WWW(baseAddress + "helicopter.unity3d");
    }
    
    var _hOfs : float = 0.0;
    var _vOfs : float = 0.0;

    function Update()
    {
    	// a little bit of control
    	if(!GameManager.pause) {
    		var h : float =  Input.GetAxis ("Mouse X")*0.25;
    		var v : float =  Input.GetAxis ("Mouse Y")*0.25;
    	
    		_hOfs += h;
    		_vOfs += v;
    	    	
    		_hOfs = Mathf.Clamp(_hOfs, -7.5, 7.5);
    		_vOfs = Mathf.Clamp(_vOfs, -7.5, 7.5);

			if(_mouseControl) {
				camera.transform.localEulerAngles.y = _hOfs;
 				camera.transform.localEulerAngles.x = _vOfs;
			}
    	}
 		
 		
   	
        switch(streamingStep)
        {
            case StreamingStep.None:
                if(con != null) //Application.GetStreamProgressForLevel("demo_start_cutscene_helicopter") >= 1.0)
                {
                    if(con.isDone)
                    {
                        auxBundle = con.assetBundle;
                        helicopterProgress = 1.0;
                        currentOp = Application.LoadLevelAdditiveAsync("demo_start_cutscene_helicopter");
                    
                        streamingStep = StreamingStep.Helicopter;

                        con.Dispose();
                        con = null;
                    }
                    else
                    {
                        helicopterProgress = con.progress;
                    }
                }
                break;
            case StreamingStep.Helicopter:
                ready = false;
                currentProgress = 1.0;

                if(currentOp != null)
                {
                    if(currentOp.isDone)
                    {
                        ready = true;

                        crewContainerGO = GameObject.Find("Crew");

                        auxGO = GameObject.Find("Cutscene");

                        var cameras : Component[] = auxGO.GetComponentsInChildren(Camera) as Component[];

                        for(var i : int = 0; i < cameras.length; i++)
                        {
                            gameManager.PauseEffectCameras[i+1] = cameras[i] as Camera;
                            quality.cameras[i+1] = cameras[i] as Camera;
                        }

                        if(auxGO != null)
                        {
                            cutsceneController = auxGO.GetComponent("StartCutscene") as StartCutscene;
                            helicopterGO = cutsceneController.heliRef.gameObject;
                            if(cutsceneController.blurRefBack)
                            	cutsceneController.blurRefBack.gameObject.active = false;
                            if(cutsceneController.blurRef)
                            	cutsceneController.blurRef.gameObject.active = false;
                        }

                        if(!started) 
                        {
                            StartCoroutine("GoToHeli");
                        }

                        con = new WWW(baseAddress + "pilot.unity3d");

                        currentOp = null;
                    }
                }
                else
                {
                    ready = true;
                }

                if(ready)
                {
                    if(con != null)//Application.GetStreamProgressForLevel("demo_start_cutscene_pilot") >= 1.0)
                    {
                        if(con.isDone)
                        {
                            auxBundle = con.assetBundle;
                            pilotProgress = 1.0;
                            currentOp = Application.LoadLevelAdditiveAsync("demo_start_cutscene_pilot");
                        
                            streamingStep = StreamingStep.Pilot;

                            con.Dispose();
                            con = null;
                        }
                        else
                        {
                            pilotProgress = con.progress;
                        }
                    }
                }
                break;
            case StreamingStep.Pilot:
                if(LoadChar("Pilot", "wingman"))
                {
                    wingmanProgress = 1.0;
                    streamingStep = StreamingStep.Wingman;
                }
                else
                {
                    if(con != null)
                    {
                        wingmanProgress = con.progress;
                    }
                }
                break;
            case StreamingStep.Wingman:
                if(LoadChar("Wingman", "coleague"))
                {
                    coleagueProgress = 1.0;
                    streamingStep = StreamingStep.Coleague;
                }
                else
                {
                    if(con != null)
                    {
                        coleagueProgress = con.progress;
                    }
                }
                break;
            case StreamingStep.Coleague:
                if(LoadChar("Coleague", "mainsoldier"))
                {
                    streamingStep = StreamingStep.Soldier;
                    soldierProgress = 1.0;
                    auxGO = GameObject.Find("Coleague");

                    cutsceneController.coleague = auxGO.GetComponentInChildren(Animation);
                }
                else
                {
                    if(con != null)
                    {
                        soldierProgress = con.progress;
                    }
                }
                break;
            case StreamingStep.Soldier:
                if(!readyToLoadTerrain)
                {
                    if(LoadChar("MainSoldier", null))
                    {
                        if(!loadedSoldiers)
                        {
                            loadedSoldiers = true;

                            auxGO = GameObject.Find("MainSoldier");

                            con = new WWW(baseAddress + "terrain.unity3d");

                            cutsceneController.soldierT = auxGO.transform;
                            cutsceneController.soldierRenderer = auxGO.GetComponentInChildren(SkinnedMeshRenderer);
                        }

                        if(con != null)//Application.GetStreamProgressForLevel("demo_start_cutscene_terrain") >= 1.0)
                        {
                            if(con.isDone)
                            {
                                auxBundle = con.assetBundle;
                                readyToLoadTerrain = true;
                                terrainProgress = 1.0;
                                con.Dispose();
                                con = null;
                            }
                            else
                            {
                                terrainProgress = con.progress;
                            }
                        }
                    }
                }
                break;
            case StreamingStep.Terrain:
                ready = false;
                if(currentOp != null)
                {
                    if(currentOp.isDone)
                    {
                        ready = true;
                    }
                }
                else
                {
                    ready = true;
                }

                if(ready) 
                {
                    streamingStep = StreamingStep.Cutscene;
                    readyToPlayCutscene = true;
                }
                break;
        }

        if(loadForest)
        {
            if(alpha < 1.0)
            {
                alpha += Time.deltaTime;

                if(alpha >= 1.0)
                {
                    alpha = 1.2;
                    Application.LoadLevelAsync("demo_forest");
                }
            }
        }

        HandleProgress();

        if(heliSound != null)
        {
            if(heliSound.volume < 0.45)
            {
                heliSound.volume += Time.deltaTime;
            }
            else
            {
                heliSound = null;
            }
        }
    }

    function HandleProgress()
    {
        currentProgress = 1.0;

        angle -= Time.deltaTime * 360;

        if(angle < -360.0)
        {
            angle += 360.0;
        }

        if(streamingStep == StreamingStep.None)
        {
            currentProgress = helicopterProgress;//Application.GetStreamProgressForLevel("demo_start_cutscene_helicopter");
        }
        if(streamingStep == StreamingStep.Helicopter || streamingStep == StreamingStep.Pilot || streamingStep == StreamingStep.Wingman || streamingStep == StreamingStep.Coleague)
        {
            currentProgress = pilotProgress;//Application.GetStreamProgressForLevel("demo_start_cutscene_pilot");
            currentProgress += wingmanProgress;//Application.GetStreamProgressForLevel("demo_start_cutscene_wingman");
            currentProgress += coleagueProgress;//Application.GetStreamProgressForLevel("demo_start_cutscene_coleague");
            currentProgress += soldierProgress;//Application.GetStreamProgressForLevel("demo_start_cutscene_mainsoldier");
            currentProgress /= 4.0;
        }
        if(streamingStep == StreamingStep.Soldier)
        {
            currentProgress = terrainProgress;//Application.GetStreamProgressForLevel("demo_start_cutscene_terrain");
        }
        else if(streamingStep == StreamingStep.Cutscene)
        {
            currentProgress = StartCutscene.forestProgress;//Application.GetStreamProgressForLevel("demo_forest");
        }

        currentProgress *= 100.0;
        var aux : int = currentProgress;
        currentProgress = aux;
    }

    function OnGUI()
    {
        var evt : Event = Event.current;

        if(sarge != null) sarge.DrawGUI(evt);

        if(mainMenu != null) mainMenu.DrawGUI(evt);

        if(evt.type != EventType.Repaint) return;

        if(loadForest)
        {
            var c : Color;
            var g : Color;

            c = g = GUI.color;

            c.a = alpha;
            GUI.color = c;
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), blackTexture);
            GUI.Label(new Rect(Screen.width - 120, Screen.height - 40, 90, 20), "Loading...", textStyle);
            GUI.color = g;
            return;
        }

        if(currentProgress >= 100.0) return;

        GUIUtility.RotateAroundPivot(angle, new Vector2(Screen.width - 28, Screen.height - 28));
        GUI.DrawTexture(Rect(Screen.width - 56, Screen.height - 56, 56, 56), loadingBackground, ScaleMode.ScaleToFit, true, 0);

        GUI.matrix = Matrix4x4.identity;
        GUI.Label(new Rect(Screen.width - 52, Screen.height - 36, 50, 20), currentProgress.ToString(), textStyle);
    }
    
    private var _mouseControl : boolean = true;
    function EnableMouseControl(enable : boolean) {
    	_mouseControl = enable;
    }
    
    var _startWater : GameObject = null;
    var _startTerrain : GameObject = null;
    function CheckTerrainHide() {
    	if(readyToPlayCutscene) {
    		_startTerrain = GameObject.Find("start_terrain");	
    		if(_startTerrain)
    			(_startTerrain.GetComponent(Terrain) as Terrain).enabled = false;
    		_startWater =  GameObject.Find("water");	
    		if(_startWater)
    			(_startWater.GetComponent(MeshRenderer) as MeshRenderer).enabled = false;
    	}
    }

    function GoToHeli()
    { 	
        if(started) 
        	return;

        started = true;
        
        // we need to remember the cutscene to make animations happen after this segment
        var cutsceneAni : Transform = helicopterGO.transform.parent.parent;
        
        helicopterGO.transform.parent.parent = heliParent;
        helicopterGO.transform.parent.transform.localPosition = Vector3.zero;
        helicopterGO.transform.parent.transform.localRotation = Quaternion.identity;
        helicopterGO.transform.parent.localScale *= theScale;

        var ani = (heliParent.GetComponent(Animation) as Animation);
        
        // trigger the fly-in animation
        ani.enabled = true;
        ani["helicopterintro_start"].wrapMode = WrapMode.Once;
        
        var restWait : float = 2.75;
        
        yield WaitForSeconds(0.25);
        restWait += 0.25;
        
        var time2Shake : float = 0.75;
        restWait += time2Shake;
        
        while(time2Shake>0.0) {
        	transform.localEulerAngles.x += Random.Range(-1.0,1.0) * Mathf.Clamp01(time2Shake);
        	
        	yield;
        	time2Shake -= Time.deltaTime;	
        }
        
        yield WaitForSeconds(ani["helicopterintro_Mid"].clip.length - restWait);
        
        ani["helicopterintro_Mid"].wrapMode = WrapMode.Loop;
        ani["helicopterintro_Mid"].speed = 1.0;
        ani.CrossFade("helicopterintro_Mid", 0.45);
        
        // transform.localEulerAngles.x = 0.0;
        
        yield WaitForSeconds(2.5);
        
        // sarge.ShowInstruction("good_morning");
        
        yield WaitForSeconds(2.5);
        
        // 3 iterations for now
        var camAnglesToShow : int = 0;
        
        // we are doing this shit as long as needed
        while(!readyToPlayCutscene) 
        {
        	if(readyToLoadTerrain && !GameManager.pause) {
        		streamingStep = StreamingStep.Terrain;
        		currentOp = Application.LoadLevelAdditiveAsync("demo_start_cutscene_terrain");        		
        	}
        	
	        var oldPos : Vector3 = transform.position;
	        var oldRot : Quaternion = transform.rotation;
	        
	        if(transform.parent && transform.parent.animation)
	        	transform.parent.animation.Stop();
	        	
	        var time2Play : float = 3.0 + Random.value * 2.0;
	        
	        EnableMouseControl(false);
	        cloudBed.SendMessage("SetCut", 1.0);
	        
	        var aniWeight : float = 1.0;
	        var indexForSpeed = (camAnglesToShow)%fixedCamAngles.length;
	        if(indexForSpeed >= fixedCamAnimationWeights.length) 
	        	indexForSpeed = fixedCamAnimationWeights.length-1;
	        if(indexForSpeed >= 0)
	        	aniWeight = fixedCamAnimationWeights[indexForSpeed];
	        ani["helicopterintro_Mid"].speed = aniWeight;
	        
	        while(time2Play > 0.0) {
	        	transform.position = fixedCamAngles[(camAnglesToShow)%fixedCamAngles.length].position;
	        	transform.rotation = fixedCamAngles[(camAnglesToShow)%fixedCamAngles.length].rotation;
	        	
	        	CheckTerrainHide();
	        	yield;
	        	time2Play -= Time.deltaTime;
	        }
	        
	        transform.position = oldPos;
	        transform.rotation = oldRot;
	        
	        if(transform.parent && transform.parent.animation)	
	        	transform.parent.animation.Play();
	        	
	        EnableMouseControl(true);
	        cloudBed.SendMessage("SetCut", 0.0);
	        ani["helicopterintro_Mid"].speed = 1.0;
	        
	        time2Play = (Random.Range(3.0,4.0));
	        while(time2Play > 0.0) {
	        		
	        	CheckTerrainHide();
				yield;
	        	time2Play -= Time.deltaTime;
	        }
        
        	camAnglesToShow++;
        }
                
		/*
        var mouseOrbit : MouseOrbit = gameObject.GetComponent("MouseOrbit") as MouseOrbit;

        var heliT : Transform = helicopterGO.transform.parent;
        heliT.position = heliStartPoint.position;
        heliT.rotation = heliStartPoint.rotation;

        yield WaitForSeconds(0.2);
		*/
		
        // sarge.ShowInstruction("mouse_look");
        // sarge.ShowInstruction("menu");

        heliSound = cutsceneController.heliSound;


		/*
        while(!readyToLoadTerrain || GameManager.pause)
        {
            yield;
        }

        streamingStep = StreamingStep.Terrain;
        currentOp = Application.LoadLevelAdditiveAsync("demo_start_cutscene_terrain");

        while(!readyToPlayCutscene)
        {
            yield;
        }
        */
        
        // trigger fly away
        
        
        // trigger fly away
        ani["helicopterintro_Mid"].normalizedTime = Mathf.Repeat(ani["helicopterintro_Mid"].normalizedTime, 1);
        while (ani["helicopterintro_Mid"].normalizedTime < 1)
        	yield;
        ani.CrossFade("helicopterintro_End", 0.1);
        yield WaitForSeconds(ani["helicopterintro_End"].clip.length);
        
        // set correct helicopter position
        ani.enabled = false;
        helicopterGO.transform.parent.parent = cutsceneAni;
        
        GameObject.Find("AssignSkybox").SendMessage("DoJoachimsSkyboxThing");
		(_startTerrain.GetComponent(Terrain) as Terrain).enabled = true;
		(_startWater.GetComponent(MeshRenderer) as MeshRenderer).enabled = true;
        
        //var heliT : Transform = helicopterGO.transform.parent;
        //heliT.position = heliFlyAwayPoint.position;
        //heliT.rotation = heliFlyAwayPoint.rotation;        

        cutsceneController.enabled = true;
        
		// disable all the camera cloud effects shit
		(camera.GetComponent("CloudEffects") as MonoBehaviour).enabled = false;
        camera.enabled = false;
        
        // destroy clouds
        if(fakeClouds)
        	Destroy(fakeClouds);
        
        var listener : AudioListener = gameObject.GetComponent("AudioListener") as AudioListener;

        if(listener != null)
        {
            listener.enabled = false;
        }
    }

    function LoadChar(current : String, next : String) : boolean
    {
        ready = false;

        if(currentOp != null)
        {
            if(currentOp.isDone)
            {
                ready = true;

                auxGO = GameObject.Find(current);
                        
                if(auxGO != null)
                {
                    auxGO.transform.parent = crewContainerGO.transform;
                    auxGO.transform.localPosition = Vector3.zero;
                    auxGO.transform.localScale = Vector3.one;
                    auxGO.transform.localRotation = Quaternion.identity;
                }

                if(next != null)
                {
                    con = new WWW(baseAddress + next + ".unity3d");
                }

                currentOp = null;
            }
        }
        else
        {
            ready = true;

            auxGO = GameObject.Find(current);
                        
            if(auxGO != null)
            {
                auxGO.transform.parent = crewContainerGO.transform;
            }
        }

        if(ready)
        {
            if(next != null)
            {
                if(con != null && con.isDone)//Application.GetStreamProgressForLevel("demo_start_cutscene_" + next) >= 1.0)
                {
                    auxBundle = con.assetBundle;

                    currentOp = Application.LoadLevelAdditiveAsync("demo_start_cutscene_" + next);

                    con.Dispose();
                    con = null;

                    return true;
                }
            }
            else
            {
                return true;
            }
        }

        return false;
    }
}