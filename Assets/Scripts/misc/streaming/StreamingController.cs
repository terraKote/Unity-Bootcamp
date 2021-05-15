using UnityEngine;
using System.Collections;
using Bootcamp.Hud.Sarge;

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

public class StreamingController : MonoBehaviour
{
    private AsyncOperation currentOp;
    private StreamingStep streamingStep;
    private bool ready;
    private GameObject helicopterGO;
    private GameObject crewContainerGO;
    private StartCutscene cutsceneController;
    private GameObject auxGO;
    private bool readyToPlayCutscene;

    public Transform heliParent;

    public int cutIterations = 1;
    public float[] fixedCamAnimationWeights;
    public Transform[] fixedCamAngles;
    public GameObject cloudBed;

    public float theScale = 2.0f;

    public float lerpSpeed = 3.0f;
    private bool loadedSoldiers;

    private float currentProgress;

    public Transform heliStartPoint;
    public Transform heliEndPoint;
    public Transform heliFlyAwayPoint;
    public Transform heliHoverPoint;

    public float startFOV;
    public float endFOV;

    public GameObject fakeClouds;
    private bool readyToLoadTerrain;

    public GUIStyle textStyle;
    public Texture2D loadingBackground;
    private float angle;

    static public bool loadForest;
    public Texture2D blackTexture;
    private float alpha;
    private bool started;

    public MainMenuScreen mainMenu;
    public SargeManager sarge;
    public GameManager gameManager;
    public GameQualitySettings quality;

    private AudioSource heliSound;

    private WWW con;
    static public string baseAddress;
    private AssetBundle auxBundle;

    private float helicopterProgress;
    private float pilotProgress;
    private float wingmanProgress;
    private float coleagueProgress;
    private float soldierProgress;
    private float terrainProgress;
    private float forestProgress;

    void Start()
    {
        started = false;
        currentProgress = 0.0f;
        angle = 0.0f;
        loadedSoldiers = false;
        readyToPlayCutscene = false;
        streamingStep = StreamingStep.None;
        readyToLoadTerrain = false;
        camera.fieldOfView = startFOV;
        loadForest = false;
        alpha = 0.0f;

        Screen.lockCursor = true;

        if (Application.isEditor)
        {
            baseAddress = "file://" + Application.dataPath + "/../webplayer/";
        }
        else if (Application.platform == RuntimePlatform.OSXPlayer && Application.platform != RuntimePlatform.WindowsPlayer)
        {
            baseAddress = "file://" + Application.dataPath + "../../webplayer/";
        }
        else
        {
            baseAddress = "";
        }

        con = new WWW(baseAddress + "helicopter.unity3d");
    }

    float _hOfs = 0.0f;
    float _vOfs = 0.0f;

    void Update()
    {
        // a little bit of control
        if (!GameManager.pause)
        {
            var h = Input.GetAxis("Mouse X") * 0.25f;
            var v = Input.GetAxis("Mouse Y") * 0.25f;

            _hOfs += h;
            _vOfs += v;

            _hOfs = Mathf.Clamp(_hOfs, -7.5f, 7.5f);
            _vOfs = Mathf.Clamp(_vOfs, -7.5f, 7.5f);

            if (_mouseControl)
            {
                var angles = camera.transform.localEulerAngles;

                angles.y = _hOfs;
                angles.x = _vOfs;

                camera.transform.localEulerAngles = angles;
            }
        }



        switch (streamingStep)
        {
            case StreamingStep.None:
                if (con != null) //Application.GetStreamProgressForLevel("demo_start_cutscene_helicopter") >= 1.0)
                {
                    if (con.isDone)
                    {
                        auxBundle = con.assetBundle;
                        helicopterProgress = 1.0f;
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
                currentProgress = 1.0f;

                if (currentOp != null)
                {
                    if (currentOp.isDone)
                    {
                        ready = true;

                        crewContainerGO = GameObject.Find("Crew");

                        auxGO = GameObject.Find("Cutscene");

                        var cameras = auxGO.GetComponentsInChildren<Camera>();

                        for (var i = 0; i < cameras.Length; i++)
                        {
                            gameManager.PauseEffectCameras[i + 1] = cameras[i];
                            quality.cameras[i + 1] = cameras[i];
                        }

                        if (auxGO != null)
                        {
                            cutsceneController = auxGO.GetComponent<StartCutscene>();
                            helicopterGO = cutsceneController.heliRef.gameObject;
                            if (cutsceneController.blurRefBack)
                                cutsceneController.blurRefBack.gameObject.active = false;
                            if (cutsceneController.blurRef)
                                cutsceneController.blurRef.gameObject.active = false;
                        }

                        if (!started)
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

                if (ready)
                {
                    if (con != null)//Application.GetStreamProgressForLevel("demo_start_cutscene_pilot") >= 1.0)
                    {
                        if (con.isDone)
                        {
                            auxBundle = con.assetBundle;
                            pilotProgress = 1.0f;
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
                if (LoadChar("Pilot", "wingman"))
                {
                    wingmanProgress = 1.0f;
                    streamingStep = StreamingStep.Wingman;
                }
                else
                {
                    if (con != null)
                    {
                        wingmanProgress = con.progress;
                    }
                }
                break;
            case StreamingStep.Wingman:
                if (LoadChar("Wingman", "coleague"))
                {
                    coleagueProgress = 1.0f;
                    streamingStep = StreamingStep.Coleague;
                }
                else
                {
                    if (con != null)
                    {
                        coleagueProgress = con.progress;
                    }
                }
                break;
            case StreamingStep.Coleague:
                if (LoadChar("Coleague", "mainsoldier"))
                {
                    streamingStep = StreamingStep.Soldier;
                    soldierProgress = 1.0f;
                    auxGO = GameObject.Find("Coleague");

                    cutsceneController.coleague = auxGO.GetComponentInChildren<Animation>();
                }
                else
                {
                    if (con != null)
                    {
                        soldierProgress = con.progress;
                    }
                }
                break;
            case StreamingStep.Soldier:
                if (!readyToLoadTerrain)
                {
                    if (LoadChar("MainSoldier", null))
                    {
                        if (!loadedSoldiers)
                        {
                            loadedSoldiers = true;

                            auxGO = GameObject.Find("MainSoldier");

                            con = new WWW(baseAddress + "terrain.unity3d");

                            cutsceneController.soldierT = auxGO.transform;
                            cutsceneController.soldierRenderer = auxGO.GetComponentInChildren<SkinnedMeshRenderer>();
                        }

                        if (con != null)//Application.GetStreamProgressForLevel("demo_start_cutscene_terrain") >= 1.0)
                        {
                            if (con.isDone)
                            {
                                auxBundle = con.assetBundle;
                                readyToLoadTerrain = true;
                                terrainProgress = 1.0f;
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
                if (currentOp != null)
                {
                    if (currentOp.isDone)
                    {
                        ready = true;
                    }
                }
                else
                {
                    ready = true;
                }

                if (ready)
                {
                    streamingStep = StreamingStep.Cutscene;
                    readyToPlayCutscene = true;
                }
                break;
        }

        if (loadForest)
        {
            if (alpha < 1.0)
            {
                alpha += Time.deltaTime;

                if (alpha >= 1.0)
                {
                    alpha = 1.2f;
                    Application.LoadLevelAsync("demo_forest");
                }
            }
        }

        HandleProgress();

        if (heliSound != null)
        {
            if (heliSound.volume < 0.45)
            {
                heliSound.volume += Time.deltaTime;
            }
            else
            {
                heliSound = null;
            }
        }
    }

    void HandleProgress()
    {
        currentProgress = 1.0f;

        angle -= Time.deltaTime * 360;

        if (angle < -360.0)
        {
            angle += 360.0f;
        }

        if (streamingStep == StreamingStep.None)
        {
            currentProgress = helicopterProgress;//Application.GetStreamProgressForLevel("demo_start_cutscene_helicopter");
        }
        if (streamingStep == StreamingStep.Helicopter || streamingStep == StreamingStep.Pilot || streamingStep == StreamingStep.Wingman || streamingStep == StreamingStep.Coleague)
        {
            currentProgress = pilotProgress;//Application.GetStreamProgressForLevel("demo_start_cutscene_pilot");
            currentProgress += wingmanProgress;//Application.GetStreamProgressForLevel("demo_start_cutscene_wingman");
            currentProgress += coleagueProgress;//Application.GetStreamProgressForLevel("demo_start_cutscene_coleague");
            currentProgress += soldierProgress;//Application.GetStreamProgressForLevel("demo_start_cutscene_mainsoldier");
            currentProgress /= 4.0f;
        }
        if (streamingStep == StreamingStep.Soldier)
        {
            currentProgress = terrainProgress;//Application.GetStreamProgressForLevel("demo_start_cutscene_terrain");
        }
        else if (streamingStep == StreamingStep.Cutscene)
        {
            currentProgress = StartCutscene.forestProgress;//Application.GetStreamProgressForLevel("demo_forest");
        }

        currentProgress *= 100.0f;
        var aux = currentProgress;
        currentProgress = aux;
    }

    void OnGUI()
    {
        var evt = Event.current;

        if (sarge != null) sarge.DrawGUI(evt);

        if (mainMenu != null) mainMenu.DrawGUI(evt);

        if (evt.type != EventType.Repaint) return;

        if (loadForest)
        {
            Color c;
            Color g;

            c = g = GUI.color;

            c.a = alpha;
            GUI.color = c;
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), blackTexture);
            GUI.Label(new Rect(Screen.width - 120, Screen.height - 40, 90, 20), "Loading...", textStyle);
            GUI.color = g;
            return;
        }

        if (currentProgress >= 100.0) return;

        GUIUtility.RotateAroundPivot(angle, new Vector2(Screen.width - 28, Screen.height - 28));
        GUI.DrawTexture(new Rect(Screen.width - 56, Screen.height - 56, 56, 56), loadingBackground, ScaleMode.ScaleToFit, true, 0);

        GUI.matrix = Matrix4x4.identity;
        GUI.Label(new Rect(Screen.width - 52, Screen.height - 36, 50, 20), currentProgress.ToString(), textStyle);
    }

    private bool _mouseControl = true;
    void EnableMouseControl(bool enable)
    {
        _mouseControl = enable;
    }

    GameObject _startWater = null;
    GameObject _startTerrain = null;

    void CheckTerrainHide()
    {
        if (readyToPlayCutscene)
        {
            _startTerrain = GameObject.Find("start_terrain");
            if (_startTerrain)
                _startTerrain.GetComponent<Terrain>().enabled = false;
            _startWater = GameObject.Find("water");
            if (_startWater)
                _startWater.GetComponent<MeshRenderer>().enabled = false;
        }
    }

    IEnumerator GoToHeli()
    {
        if (started)
            yield return null;

        started = true;

        // we need to remember the cutscene to make animations happen after this segment
        var cutsceneAni = helicopterGO.transform.parent.parent;

        helicopterGO.transform.parent.parent = heliParent;
        helicopterGO.transform.parent.transform.localPosition = Vector3.zero;
        helicopterGO.transform.parent.transform.localRotation = Quaternion.identity;
        helicopterGO.transform.parent.localScale *= theScale;

        var ani = heliParent.GetComponent<Animation>();

        // trigger the fly-in animation
        ani.enabled = true;
        ani["helicopterintro_start"].wrapMode = WrapMode.Once;

        var restWait = 2.75f;

        yield return new WaitForSeconds(0.25f);
        restWait += 0.25f;

        var time2Shake = 0.75f;
        restWait += time2Shake;

        while (time2Shake > 0.0)
        {
            var angles = transform.localEulerAngles;
            angles.x += Random.Range(-1.0f, 1.0f) * Mathf.Clamp01(time2Shake);
            transform.localEulerAngles = angles;

            yield return null;
            time2Shake -= Time.deltaTime;
        }

        yield return new WaitForSeconds(ani["helicopterintro_Mid"].clip.length - restWait);

        ani["helicopterintro_Mid"].wrapMode = WrapMode.Loop;
        ani["helicopterintro_Mid"].speed = 1.0f;
        ani.CrossFade("helicopterintro_Mid", 0.45f);

        // transform.localEulerAngles.x = 0.0;

        yield return new WaitForSeconds(2.5f);

        // sarge.ShowInstruction("good_morning");

        yield return new WaitForSeconds(2.5f);

        // 3 iterations for now
        var camAnglesToShow = 0;

        // we are doing this shit as long as needed
        while (!readyToPlayCutscene)
        {
            if (readyToLoadTerrain && !GameManager.pause)
            {
                streamingStep = StreamingStep.Terrain;
                currentOp = Application.LoadLevelAdditiveAsync("demo_start_cutscene_terrain");
            }

            var oldPos = transform.position;
            var oldRot = transform.rotation;

            if (transform.parent && transform.parent.animation)
                transform.parent.animation.Stop();

            var time2Play = 3.0f + Random.value * 2.0f;

            EnableMouseControl(false);
            cloudBed.SendMessage("SetCut", 1.0f);

            var aniWeight = 1.0f;
            var indexForSpeed = (camAnglesToShow) % fixedCamAngles.Length;
            if (indexForSpeed >= fixedCamAnimationWeights.Length)
                indexForSpeed = fixedCamAnimationWeights.Length - 1;
            if (indexForSpeed >= 0)
                aniWeight = fixedCamAnimationWeights[indexForSpeed];
            ani["helicopterintro_Mid"].speed = aniWeight;

            while (time2Play > 0.0)
            {
                transform.position = fixedCamAngles[(camAnglesToShow) % fixedCamAngles.Length].position;
                transform.rotation = fixedCamAngles[(camAnglesToShow) % fixedCamAngles.Length].rotation;

                CheckTerrainHide();
                yield return null;
                time2Play -= Time.deltaTime;
            }

            transform.position = oldPos;
            transform.rotation = oldRot;

            if (transform.parent && transform.parent.animation)
                transform.parent.animation.Play();

            EnableMouseControl(true);
            cloudBed.SendMessage("SetCut", 0.0);
            ani["helicopterintro_Mid"].speed = 1.0f;

            time2Play = (Random.Range(3.0f, 4.0f));
            while (time2Play > 0.0)
            {

                CheckTerrainHide();
                yield return null;
                time2Play -= Time.deltaTime;
            }

            camAnglesToShow++;
        }

        /*
        var mouseOrbit : MouseOrbit = gameObject.GetComponent("MouseOrbit") as MouseOrbit;

        var heliT  = helicopterGO.transform.parent;
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
            yield return null;
        ani.CrossFade("helicopterintro_End", 0.1f);
        yield return new WaitForSeconds(ani["helicopterintro_End"].clip.length);

        // set correct helicopter position
        ani.enabled = false;
        helicopterGO.transform.parent.parent = cutsceneAni;

        GameObject.Find("AssignSkybox").SendMessage("DoJoachimsSkyboxThing");
        (_startTerrain.GetComponent<Terrain>() as Terrain).enabled = true;
        (_startWater.GetComponent<MeshRenderer>() as MeshRenderer).enabled = true;

        //var heliT  = helicopterGO.transform.parent;
        //heliT.position = heliFlyAwayPoint.position;
        //heliT.rotation = heliFlyAwayPoint.rotation;        

        cutsceneController.enabled = true;

        // disable all the camera cloud effects shit
        (camera.GetComponent("CloudEffects") as MonoBehaviour).enabled = false;
        camera.enabled = false;

        // destroy clouds
        if (fakeClouds)
            Destroy(fakeClouds);

        var listener = gameObject.GetComponent("AudioListener") as AudioListener;

        if (listener != null)
        {
            listener.enabled = false;
        }
    }

    bool LoadChar(string current, string next)
    {
        ready = false;

        if (currentOp != null)
        {
            if (currentOp.isDone)
            {
                ready = true;

                auxGO = GameObject.Find(current);

                if (auxGO != null)
                {
                    auxGO.transform.parent = crewContainerGO.transform;
                    auxGO.transform.localPosition = Vector3.zero;
                    auxGO.transform.localScale = Vector3.one;
                    auxGO.transform.localRotation = Quaternion.identity;
                }

                if (next != null)
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

            if (auxGO != null)
            {
                auxGO.transform.parent = crewContainerGO.transform;
            }
        }

        if (ready)
        {
            if (next != null)
            {
                if (con != null && con.isDone)//Application.GetStreamProgressForLevel("demo_start_cutscene_" + next) >= 1.0)
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
