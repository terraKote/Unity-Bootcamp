using UnityEngine;

[System.Serializable]
public struct GunKeyBinder
{
    public Gun gun;
    public KeyCode keyToActivate;
    public bool switchModesOnKey;
}