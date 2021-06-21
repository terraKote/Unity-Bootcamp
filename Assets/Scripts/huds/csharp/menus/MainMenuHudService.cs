using System.Collections.Generic;
using UnityEngine;

public class MainMenuHudService : MonoBehaviour, IPauseListener
{
    private Stack<BaseWindow> _windows = new Stack<BaseWindow>();

    public void OnPause()
    {
        gameObject.SetActive(true);
    }

    public void OnUnPause()
    {
        gameObject.SetActive(false);
    }
}