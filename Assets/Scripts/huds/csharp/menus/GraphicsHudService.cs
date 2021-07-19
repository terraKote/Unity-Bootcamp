using System;
using UnityEngine;
using UnityEngine.UI;

public class GraphicsHudService : BaseWindow
{
    [SerializeField] private GameQualityService gameQualityService;
    private GraphicsSettingsMenuBase[] _graphicsSettingsMenus;

    private void Awake()
    {
        _graphicsSettingsMenus = FindObjectsOfType<GraphicsSettingsMenuBase>();

        foreach (var menu in _graphicsSettingsMenus)
        {
            menu.Setup(gameQualityService);
        }
    }
}