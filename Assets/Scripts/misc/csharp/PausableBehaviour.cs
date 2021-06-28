using UnityEngine;

public abstract class PausableBehaviour : MonoBehaviour, IPauseListener
{
    private bool _isPaused;
    public bool IsPaused { get { return _isPaused; } }

    public void OnSwitchPauseState(bool paused)
    {
        _isPaused = paused;
    }
}
