using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class MainMenuHudService : PausableBehaviour
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

        if (_windows.Count == 0)
        {
            _canvasGroup.alpha = 1.0f;
        }
    }

    public override void OnSwitchPauseState(bool paused)
    {
        base.OnSwitchPauseState(paused);

        if (paused)
        {
            _canvasGroup.alpha = 1.0f;
            gameObject.SetActive(true);
        }
        else
        {
            while (_windows.Count > 0)
            {
                var window = _windows.Pop();
                window.Hide();
            }

            gameObject.SetActive(false);
        }
    }
}