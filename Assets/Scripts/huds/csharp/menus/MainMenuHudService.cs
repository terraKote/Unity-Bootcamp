using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public struct ButtonWindowRelation
{
    public Button button;
    public BaseWindow window;
}

[RequireComponent(typeof(CanvasGroup))]
public class MainMenuHudService : MonoBehaviour, IPauseListener
{
    [SerializeField] private ButtonWindowRelation[] buttonWindowRelations;

    private Stack<BaseWindow> _windows = new Stack<BaseWindow>();
    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    private void Start()
    {
        foreach (var relation in buttonWindowRelations)
        {
            relation.button.onClick.AddListener(() => ShowWindow(relation.window));
        }

        var windows = FindObjectsOfType<BaseWindow>();

        foreach (var window in windows)
        {
            window.Hide();
        }
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

    public void OnSwitchPauseState(bool paused)
    {
        if (paused)
        {
            _canvasGroup.alpha = 1.0f;
        }
        else
        {
            while (_windows.Count > 0)
            {
                var window = _windows.Pop();
                window.Hide();
            }

            _canvasGroup.alpha = 0.0f;
        }

        _canvasGroup.interactable = paused;
    }
}