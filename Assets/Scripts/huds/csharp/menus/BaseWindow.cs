using UnityEngine;

public abstract class BaseWindow : MonoBehaviour
{
    private void Start()
    {
        OnInit();
        gameObject.SetActive(false);
    }

    protected virtual void OnInit() { }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}