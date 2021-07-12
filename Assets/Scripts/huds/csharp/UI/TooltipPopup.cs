using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class TooltipPopup : MonoBehaviour
{
    [SerializeField] private Text label;
    [SerializeField] private Vector2 offset = new Vector2(45, 0);

    private Image _image;

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
        _image = GetComponent<Image>();

        Display(false);
    }

    public void Display(bool active)
    {
        _image.enabled = active;
        label.enabled = active;
    }
}