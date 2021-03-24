#pragma strict
#pragma implicit
#pragma downcast

class StartCutscene extends MonoBehaviour
{
	public var cutsceneCamera1 : GameObject;
	public var cutsceneCamera2 : GameObject;
	public var thirdPersonCamera : GameObject;
	public var soldierRenderer : SkinnedMeshRenderer;
	public var cutsceneMaterials : Material[];
	public var thirdPersonMaterials : Material[];
	public var soldierT : Transform;
	
	private var loopFinished : boolean;
	private var loading : boolean;
	private var startRotation : Quaternion;
	private var targetRotation : Quaternion;
	private var currentState : float;
	private var cameraController : MouseLook;
	private var playedPoint : boolean;
	public var coleague : Animation;

    public var heliRef : Transform;
	public var heliSound : AudioSource;
	
	public var blurRef : Transform;
	public var blurRefBack : Transform;

    private var auxBundle : AssetBundle;
    private var con : WWW;

    static public var forestProgress : float;
    
    public var sarge : SargeManager;
    private var playedClearing : boolean;
    private var timeToPlayRandom : float;
    private var waitToPlayRandom : float = 20.0;

	function OnEnable()
	{
        playedClearing = false;
        timeToPlayRandom = waitToPlayRandom;

        var go : GameObject = GameObject.Find("start_terrain");
        var terrain : Terrain = go.GetComponent("Terrain") as Terrain;
        terrain.treeMaximumFullLODCount = 15;

        var auxCam : GameObject = GameObject.Find("StartCamera");

        if(auxCam != null)
        {
            sarge = auxCam.GetComponent("SargeManager") as SargeManager;
        }

		animation.Play("intro_cutscene_1");
		thirdPersonCamera.active = false;
		cutsceneCamera1.active = true;
        cutsceneCamera1.camera.enabled = true;
		cutsceneCamera2.active = true;
		loopFinished = false;
		loading = false;
		playedPoint = false;
		
		currentState = 0.0;
		targetRotation = Quaternion.Euler(3.931946, -86.54218, 0.0);
		
		cameraController = thirdPersonCamera.GetComponent("MouseLook") as MouseLook;
		
		if(soldierRenderer != null)
		{
			soldierRenderer.materials = cutsceneMaterials;
		}
		
		if(soldierT != null)
		{
			soldierT.localScale = Vector3.one;
			soldierT.localPosition = Vector3.zero;
		}

        con = new WWW(StreamingController.baseAddress + "forest.unity3d");
        
        sarge.ShowInstruction("good_morning");
        sarge.ShowInstruction("menu");
	}

	function ChangeToThirdPersonCamera()
	{
		animation["intro_cutscene_2"].wrapMode = WrapMode.Loop;
		animation.Play("intro_cutscene_2");

		thirdPersonCamera.active = true;
        thirdPersonCamera.camera.enabled = true;
		cutsceneCamera1.active = false;
		cutsceneCamera2.active = false;

        var go : GameObject = GameObject.Find("start_terrain");
        var terrain : Terrain = go.GetComponent("Terrain") as Terrain;
        terrain.treeMaximumFullLODCount = 0;
		
		if(soldierRenderer != null)
		{
			soldierRenderer.materials = thirdPersonMaterials;
		}
		
		if(soldierT != null)
		{
			soldierT.localScale = new Vector3(0.9, 0.9, 0.9);
			soldierT.localPosition = new Vector3(0.1788807, -0.1774399, 0.3102949);
		}

        if(sarge != null) 
        {
            sarge.ShowInstruction("mouse_look");
            sarge.ShowInstruction("lz_woods");
        }
	}
	
	function LoadingSceneLoop()
	{
		loopFinished = true;
	}
	
	function Update()
	{
        if(StreamingController.loadForest)
        {
            thirdPersonCamera.transform.localRotation = targetRotation;
            return;
        }

        if(!loading)
        {
            if(GameManager.pause)
            {
                if(cameraController.enabled)
                {
                    cameraController.enabled = false;
                }
            }
            else
            {
                if(!cameraController.enabled)
                {
                    cameraController.enabled = true;
                }
            }
        }

        if(StreamingController.loadForest) 
        {
            if(heliSound.volume > 0.0)
            {
                heliSound.volume -= Time.deltaTime;
            }
            return;
        }

        if(con != null && !con.isDone)
        {
            forestProgress = con.progress;
        }

        if(!loading && thirdPersonCamera.active)
        {
            if(sarge != null)
                {
                    timeToPlayRandom -= Time.deltaTime;

                    if(timeToPlayRandom <= 0.0)
                    {
                        timeToPlayRandom = waitToPlayRandom;

                        var aux : int = Random.Range(0, 2);

                        if(aux == 0)
                        {
                            sarge.ShowInstruction("wait1");
                        }
                        else
                        {
                            sarge.ShowInstruction("wait2");
                        }
                    }
                }
        }

		if(loopFinished && !loading)
		{
			if(con != null)
			{
                if(con.isDone)
                {
                    auxBundle = con.assetBundle;

                    forestProgress = 1.0;

				    loading = true;
				    playedPoint = false;
				    cameraController.enabled = false;
				    currentState = 0.0;
				    startRotation = thirdPersonCamera.transform.localRotation;

                    con.Dispose();
                    con = null;
                }
			}
		}
		else if(loading)
		{
			if(!playedPoint)
			{
				currentState += Time.deltaTime;
				thirdPersonCamera.transform.localRotation = Quaternion.Slerp(startRotation, targetRotation, currentState);
                thirdPersonCamera.camera.fieldOfView = Mathf.Lerp(60, 45, currentState);
				cameraController.enabled = false;
				if(currentState >= 1.0)
				{
					playedPoint = true;
					coleague.Play("CS_Pointing");
				}
			}
			else
			{
                if(!playedClearing && coleague["CS_Pointing"].normalizedTime > 0.4)
                {
                    playedClearing = true;

                    if(sarge != null)
                    {
                        sarge.ShowInstruction("see_clearing");
                    }
                }
				
                if(coleague["CS_Pointing"].normalizedTime > 0.99)
				{
					loopFinished = false;
					loading = false;
                    StreamingController.loadForest = true;
				}
			}
		}
	}
}