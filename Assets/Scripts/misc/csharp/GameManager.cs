using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;

public class GameManager : MonoSingleton<GameManager>
{
    public GameObject gamePlaySoldier;
    public ParticleEmitter soldierSmoke;
    public SargeManager sarge;

    public bool receiveDamage;
    public bool scores;
    public float time;
    public bool running;

    public Camera[] PauseEffectCameras;
    private bool _paused;
    private IPauseListener[] _pauseListeners;

    void Start()
    {
        _pauseListeners = FindObjectsOfType<MonoBehaviour>().Where(x => x.GetComponent<IPauseListener>() != null).Select(x => x.GetComponent<IPauseListener>()).ToArray();
        UpdatePauseListenerState();

        TrainingStatistics.ResetStatistics();

        Cursor.lockState = CursorLockMode.Locked;

        running = false;
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
                if (auxT.gameObject.activeSelf)
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
        gamePlaySoldier.SetActive(false);
    }

    void Update()
    {
        if (!_paused && running) time += Time.deltaTime;

        _paused = PlayerInputService.GetInstance().IsPausing;

        if (_paused)
        {
            Time.timeScale = 0.00001f;
        }
        else
        {
            Time.timeScale = 1.0f;
        }

        UpdatePauseListenerState();

        CameraBlur(_paused);

        for (int i = 0; i < PauseEffectCameras.Length; i++)
        {
            var cam = PauseEffectCameras[i];
            if (cam == null) continue;
            if (cam.name != "radar_camera") continue;

            cam.enabled = !_paused;
        }

        if (!_paused && !scores)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
        }
    }

    private void UpdatePauseListenerState()
    {
        foreach (var pauseListener in _pauseListeners)
        {
            pauseListener.OnSwitchPauseState(_paused);
        }
    }

    void StartGame()
    {
        running = true;

        if (gamePlaySoldier != null)
        {
            if (!gamePlaySoldier.activeSelf)
            {
                gamePlaySoldier.SetActive(true);
            }
        }

        if (soldierSmoke != null)
        {
            if (GameQualitySettings.ambientParticles)
            {
                soldierSmoke.emit = true;
            }
        }

        if (sarge != null && SceneManager.GetActiveScene().name == "demo_forest")
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