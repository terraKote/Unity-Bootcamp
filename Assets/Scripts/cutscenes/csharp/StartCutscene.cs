using UnityEngine;
using System.Collections;

public class StartCutscene : MonoBehaviour
{
    public GameObject cutsceneCamera1;
	public GameObject cutsceneCamera2;
	public GameObject thirdPersonCamera;
	public SkinnedMeshRenderer soldierRenderer;
	public Material[] cutsceneMaterials;
	public Material[] thirdPersonMaterials;
	public Transform soldierT;
	
	private bool loopFinished;
	private bool loading;
	private Quaternion startRotation;
	private Quaternion targetRotation;
	private float currentState;
	private MouseLook cameraController;
	private bool playedPoint;
	public Animation coleague;

    public Transform heliRef;
	public AudioSource heliSound;
	
	public Transform blurRef;
	public Transform blurRefBack;

    private AssetBundle auxBundle;
    private WWW con;

    static public float forestProgress;
    
    public SargeManager sarge;
    private bool playedClearing;
    private float timeToPlayRandom;
    private float waitToPlayRandom = 20.0f;

	void OnEnable()
    {
        playedClearing = false;
        timeToPlayRandom = waitToPlayRandom;

        var go = GameObject.Find("start_terrain");
        var terrain = go.GetComponent("Terrain") as Terrain;
        terrain.treeMaximumFullLODCount = 15;

        var auxCam = GameObject.Find("StartCamera");

        if (auxCam != null)
        {
            sarge = auxCam.GetComponent("SargeManager") as SargeManager;
        }

        GetComponent<Animation>().Play("intro_cutscene_1");
        thirdPersonCamera.active = false;
        cutsceneCamera1.active = true;
        cutsceneCamera1.GetComponent<Camera>().enabled = true;
        cutsceneCamera2.active = true;
        loopFinished = false;
        loading = false;
        playedPoint = false;

        currentState = 0.0f;
        targetRotation = Quaternion.Euler(3.931946f, -86.54218f, 0.0f);

        cameraController = thirdPersonCamera.GetComponent("MouseLook") as MouseLook;

        if (soldierRenderer != null)
        {
            soldierRenderer.materials = cutsceneMaterials;
        }

        if (soldierT != null)
        {
            soldierT.localScale = Vector3.one;
            soldierT.localPosition = Vector3.zero;
        }

        con = new WWW(StreamingController.baseAddress + "forest.unity3d");

        sarge.ShowInstruction("good_morning");
        sarge.ShowInstruction("menu");
    }

    void ChangeToThirdPersonCamera()
    {
        GetComponent<Animation>()["intro_cutscene_2"].wrapMode = WrapMode.Loop;
        GetComponent<Animation>().Play("intro_cutscene_2");

        thirdPersonCamera.active = true;
        thirdPersonCamera.GetComponent<Camera>().enabled = true;
        cutsceneCamera1.active = false;
        cutsceneCamera2.active = false;

        var go = GameObject.Find("start_terrain");
        var terrain = go.GetComponent("Terrain") as Terrain;
        terrain.treeMaximumFullLODCount = 0;

        if (soldierRenderer != null)
        {
            soldierRenderer.materials = thirdPersonMaterials;
        }

        if (soldierT != null)
        {
            soldierT.localScale = new Vector3(0.9f, 0.9f, 0.9f);
            soldierT.localPosition = new Vector3(0.1788807f, -0.1774399f, 0.3102949f);
        }

        if (sarge != null)
        {
            sarge.ShowInstruction("mouse_look");
            sarge.ShowInstruction("lz_woods");
        }
    }

    void LoadingSceneLoop()
    {
        loopFinished = true;
    }

    void Update()
    {
        if (StreamingController.loadForest)
        {
            thirdPersonCamera.transform.localRotation = targetRotation;
            return;
        }

        if (!loading)
        {
            if (GameManager.GetInstance().pause)
            {
                if (cameraController.enabled)
                {
                    cameraController.enabled = false;
                }
            }
            else
            {
                if (!cameraController.enabled)
                {
                    cameraController.enabled = true;
                }
            }
        }

        if (StreamingController.loadForest)
        {
            if (heliSound.volume > 0.0)
            {
                heliSound.volume -= Time.deltaTime;
            }
            return;
        }

        if (con != null && !con.isDone)
        {
            forestProgress = con.progress;
        }

        if (!loading && thirdPersonCamera.active)
        {
            if (sarge != null)
            {
                timeToPlayRandom -= Time.deltaTime;

                if (timeToPlayRandom <= 0.0)
                {
                    timeToPlayRandom = waitToPlayRandom;

                    var aux = Random.Range(0, 2);

                    if (aux == 0)
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

        if (loopFinished && !loading)
        {
            if (con != null)
            {
                if (con.isDone)
                {
                    auxBundle = con.assetBundle;

                    forestProgress = 1.0f;

                    loading = true;
                    playedPoint = false;
                    cameraController.enabled = false;
                    currentState = 0.0f;
                    startRotation = thirdPersonCamera.transform.localRotation;

                    con.Dispose();
                    con = null;
                }
            }
        }
        else if (loading)
        {
            if (!playedPoint)
            {
                currentState += Time.deltaTime;
                thirdPersonCamera.transform.localRotation = Quaternion.Slerp(startRotation, targetRotation, currentState);
                thirdPersonCamera.GetComponent<Camera>().fieldOfView = Mathf.Lerp(60, 45, currentState);
                cameraController.enabled = false;
                if (currentState >= 1.0)
                {
                    playedPoint = true;
                    coleague.Play("CS_Pointing");
                }
            }
            else
            {
                if (!playedClearing && coleague["CS_Pointing"].normalizedTime > 0.4)
                {
                    playedClearing = true;

                    if (sarge != null)
                    {
                        sarge.ShowInstruction("see_clearing");
                    }
                }

                if (coleague["CS_Pointing"].normalizedTime > 0.99)
                {
                    loopFinished = false;
                    loading = false;
                    StreamingController.loadForest = true;
                }
            }
        }
    }
}
