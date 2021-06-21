using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class MainMenuHudService : MonoBehaviour, IPauseListener
{
    private Stack<BaseWindow> _windows = new Stack<BaseWindow>();
    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    public void ShowWindow(BaseWindow window)
    {
        if (_windows.Contains(window)) return;

        window.Show();
        _windows.Push(window);

        _canvasGroup.alpha = 0.5f;
    }

    public void CloseWindow()
    {
        var window = _windows.Pop();
        window.Hide();

        if(_windows.Count == 0)
        {
            _canvasGroup.alpha = 1.0f;
        }
    }

    public void OnPause()
    {
        _canvasGroup.alpha = 1.0f;
        gameObject.SetActive(true);
    }

    public void OnUnPause()
    {
        while (_windows.Count > 0)
        {
            var window = _windows.Pop();
            window.Hide();
        }

        gameObject.SetActive(false);
    }
}