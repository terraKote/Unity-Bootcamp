using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public abstract class AmmoHudService : MonoBehaviour, IPauseListener
{
    [SerializeField] protected Text totalAmmoCountText;
    [SerializeField] protected Text currentAmmoCountText;
    [SerializeField] protected float fadeTime = 0.2f;
    [SerializeField] private float showTime = 2.0f;
    [SerializeField] bool drawSeparator = true;

    private int _previousAmmoCount;
    private int _currentAmmoCount;
    private int _totalAmmoCount;
    private float _alpha = 0.0f;
    private float _hideTime;
    private CanvasGroup _canvasGroup;

    public int CurrentAmmoCount
    {
        get { return _currentAmmoCount; }
        set
        {
            _previousAmmoCount = _currentAmmoCount;
            _currentAmmoCount = value;
        }
    }

    public int TotalAmmoCount
    {
        get { return _totalAmmoCount; }
        set { _totalAmmoCount = value; }
    }

    private void Start()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    private void LateUpdate()
    {
        UpdateCurrentAmmoText();
        UpdateTotalAmmoText();
        SetAlpha();
        OnLateUpdate();
    }

    private void UpdateTotalAmmoText()
    {
        if (totalAmmoCountText)
        {
            string separator = string.Empty;

            if (drawSeparator)
            {
                separator = "|";
            }

            string totalAmmoCountString = _totalAmmoCount.ToString();

            if (_totalAmmoCount < 0)
            {
                totalAmmoCountString = "\u221E";
            }

            totalAmmoCountText.text = string.Format("{0} {1}", totalAmmoCountString, separator);
        }
    }

    private void UpdateCurrentAmmoText()
    {
        if (currentAmmoCountText)
        {
            currentAmmoCountText.text = _currentAmmoCount.ToString();
        }

        if (_previousAmmoCount != _currentAmmoCount)
        {
            Show();
        }
    }

    private void SetAlpha()
    {
        _hideTime -= Time.deltaTime;

        if (_hideTime <= 0)
        {
            _alpha = Mathf.Clamp01(_alpha - Time.deltaTime * fadeTime);
        }
        _canvasGroup.alpha = _alpha;
    }

    protected virtual void OnLateUpdate() { }

    public void OnPause()
    {
        gameObject.SetActive(false);
    }

    public void OnUnPause()
    {
        gameObject.SetActive(true);
    }

    public void Show()
    {
        _alpha = 1.0f;
        _hideTime = showTime + ((1.0f - _alpha) * (1 / fadeTime));
    }
}