using UnityEngine;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public GameObject gamePlaySoldier;
    public ParticleEmitter soldierSmoke;
    public SargeManager sarge;

    static public bool receiveDamage;
    static public bool pause;
    static public bool scores;
    static public float time;
    static public bool running;

    public MainMenuScreen menu;

    public Camera[] PauseEffectCameras;
    private bool _paused;
    private IPauseListener[] _pauseListeners;

    void Start()
    {
        _pauseListeners = FindObjectsOfType<MonoBehaviour>().Where(x => x.GetComponent<IPauseListener>() != null).Select(x => x.GetComponent<IPauseListener>()).ToArray();
        UpdatePauseListenerState();

        TrainingStatistics.ResetStatistics();

        Screen.lockCursor = true;

        running = false;
        pause = false;
        scores = false;
        _paused = false;
        time = 0.0f;

        Transform auxT;
        bool hasCutscene = false;
        for (int i = 0; i < transform.childCount; i++)
        {
            auxT = transform.GetChild(i);

            if (auxT.name == "Cutscene")
            {
                if (auxT.gameObject.active)
                {
                    hasCutscene = true;
                    break;
                }
            }
        }

        if (!hasCutscene)
        {
            StartGame();
        }
    }

    void CutsceneStart()
    {
        gamePlaySoldier.SetActiveRecursively(false);
    }

    void Update()
    {
        if (!pause && running) time += Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.M) || Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
        {
            pause = !pause;

            menu.visible = pause;

            if (pause)
            {
                Time.timeScale = 0.00001f;
            }
            else
            {
                Time.timeScale = 1.0f;
            }

            UpdatePauseListenerState();
        }

        if (_paused != pause)
        {
            _paused = pause;
            CameraBlur(pause);


            for (int i = 0; i < PauseEffectCameras.Length; i++)
            {
                var cam = PauseEffectCameras[i];
                if (cam == null) continue;
                if (cam.name != "radar_camera") continue;

                cam.enabled = !pause;
            }
        }

        Screen.lockCursor = !pause && !scores;
    }

    private void UpdatePauseListenerState()
    {
        foreach (var pauseListener in _pauseListeners)
        {
            if (pause)
            {
                pauseListener.OnPause();
            }
            else
            {
                pauseListener.OnUnPause();
            }
        }
    }

    void StartGame()
    {
        running = true;

        if (gamePlaySoldier != null)
        {
            if (!gamePlaySoldier.active)
            {
                gamePlaySoldier.SetActiveRecursively(true);
            }
        }

        if (soldierSmoke != null)
        {
            if (GameQualitySettings.ambientParticles)
            {
                soldierSmoke.emit = true;
            }
        }

        if (sarge != null && Application.loadedLevelName == "demo_forest")
        {
            sarge.ShowInstruction("instructions");
            sarge.ShowInstruction("instructions2");
            sarge.ShowInstruction("instructions3");
            sarge.ShowInstruction("instructions4");
            sarge.ShowInstruction("instructions5");
            sarge.ShowInstruction("additional_instructions");
        }
    }

    void CameraBlur(bool state)
    {
        if (PauseEffectCameras == null) return;
        if (PauseEffectCameras.Length <= 0) return;

        BlurEffect blurEffect;

        for (int i = 0; i < PauseEffectCameras.Length; i++)
        {
            var cam = PauseEffectCameras[i];
            if (cam == null) continue;

            blurEffect = cam.GetComponent<BlurEffect>();

            if (blurEffect == null)
            {
                blurEffect = cam.gameObject.AddComponent<BlurEffect>();
                blurEffect.iterations = cam.gameObject.name.IndexOf("radar") != -1 ? 1 : 2;
                blurEffect.blurSpread = 0.4f;
            }

            blurEffect.enabled = state;
        }
    }
}