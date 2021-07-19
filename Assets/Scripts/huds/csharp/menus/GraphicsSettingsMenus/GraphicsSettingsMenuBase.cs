using UnityEngine;

public abstract class GraphicsSettingsMenuBase : MonoBehaviour
{
    protected GameQualityService _gameQualityService;

    public virtual void Setup(GameQualityService gameQualityService)
    {
        _gameQualityService = gameQualityService;
    }
}