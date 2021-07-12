using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
[RequireComponent(typeof(CanvasGroup))]
[ExecuteInEditMode]
public class TooltipPopup : MonoBehaviour
{
    [SerializeField] private Text label;
    [SerializeField] private Vector2 offset = new Vector2(45, 0);
    [SerializeField] private int characterWrapLimit = 60;
    [SerializeField] private float fadeSpeed = 0.01f;

    private CanvasGroup _canvasGroup;
    private LayoutElement _layoutElement;

    public string Caption
    {
        get { return label.text; }
        set { label.text = value; UpdateLayout(); }
    }

    public Vector3 Position
    {
        get { return transform.position; }
        set { transform.position = value + (Vector3)offset; }
    }

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _layoutElement = GetComponent<LayoutElement>();

        if (Application.isPlaying)
        {
            Display(false);
        }
    }

    private void Update()
    {
        if (!Application.isPlaying) return;

        UpdateLayout();
    }

    private void UpdateLayout()
    {
        _layoutElement.enabled = label.text.Length > characterWrapLimit ? true : false;
    }

    public void Display(bool active)
    {
        StopAllCoroutines();
        if (active)
        {
            _canvasGroup.alpha = 0.0f;
            StartCoroutine(FadeIn());
        }
        else
        {
            StartCoroutine(FadeOut());
        }
    }

    private IEnumerator FadeIn()
    {
        while (_canvasGroup.alpha < 1.0f)
        {
            _canvasGroup.alpha += fadeSpeed;
            yield return null;
        }
    }

    private IEnumerator FadeOut()
    {
        while (_canvasGroup.alpha > 0.0f)
        {
            _canvasGroup.alpha -= fadeSpeed;
            yield return null;
        }
    }
}