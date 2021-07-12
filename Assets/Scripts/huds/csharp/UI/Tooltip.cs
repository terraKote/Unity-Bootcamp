using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
[RequireComponent(typeof(Image))]
public class Tooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField, Multiline] private string caption;
    [SerializeField] private Sprite regularImage;
    [SerializeField] private Sprite hoverImage;
    [SerializeField] private TooltipPopup tooltipPopup;

    private Image _image;

    private void Awake()
    {
        _image = GetComponent<Image>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _image.sprite = hoverImage;

        tooltipPopup.Caption = caption;
        tooltipPopup.Position = transform.position;
        tooltipPopup.Display(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _image.sprite = regularImage;
        tooltipPopup.Display(false);
    }
}