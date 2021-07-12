using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
[RequireComponent(typeof(CanvasGroup))]
public class TooltipPopup : MonoBehaviour
{
    [SerializeField] private Text label;
    [SerializeField] private Vector2 offset = new Vector2(45, 0);

    private CanvasGroup _canvasGroup;

    public string Caption
    {
        get { return label.text; }
        set { label.text = value; }
    }

    public Vector3 Position
    {
        get { return transform.position; }
        set { transform.position = value + (Vector3)offset; }
    }

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();

        Display(false);
    }

    public void Display(bool active)
    {
        _canvasGroup.alpha = active ? 1.0f : 0.0f;
    }
}