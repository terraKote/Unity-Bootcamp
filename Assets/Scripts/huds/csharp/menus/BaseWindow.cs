using UnityEngine;

public abstract class BaseWindow : MonoBehaviour
{
    private void Start()
    {
        OnInit();
        gameObject.SetActive(false);
    }

    protected virtual void OnInit() { }
}