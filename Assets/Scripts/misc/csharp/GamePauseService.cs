using System.Linq;
using UnityEngine;

public class GamePauseService : MonoBehaviour
{
    [SerializeField] private PlayerInputService playerInputService;

    public Camera[] PauseEffectCameras;
    private bool _paused;
    private IPauseListener[] _pauseListeners;

    private void Start()
    {
        _pauseListeners = FindObjectsOfType<MonoBehaviour>().Where(x => x.GetComponent<IPauseListener>() != null).Select(x => x.GetComponent<IPauseListener>()).ToArray();
        UpdatePauseListenerState();

        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        _paused = playerInputService.IsPausing;

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

        if (!_paused /*&& !scores*/)
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

    private void CameraBlur(bool state)
    {
        if (PauseEffectCameras == null || PauseEffectCameras.Length <= 0) return;

        for (int i = 0; i < PauseEffectCameras.Length; i++)
        {
            var cam = PauseEffectCameras[i];
            if (cam == null) continue;

            BlurEffect blurEffect = cam.GetComponent<BlurEffect>();

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