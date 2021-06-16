using UnityEngine;

public class MainMenuHudService : MonoBehaviour, IPauseListener
{
    public void OnPause()
    {
        gameObject.SetActive(true);
    }

    public void OnUnPause()
    {
        gameObject.SetActive(false);
    }
}