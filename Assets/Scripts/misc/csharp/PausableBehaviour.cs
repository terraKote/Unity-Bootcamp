using UnityEngine;

public abstract class PausableBehaviour : MonoBehaviour, IPauseListener
{
    private bool _isPaused;
    public bool IsPaused { get { return _isPaused; } }

    public void OnPause()
    {
        _isPaused = true;
    }

    public void OnUnPause()
    {
        _isPaused = false;
    }
}
