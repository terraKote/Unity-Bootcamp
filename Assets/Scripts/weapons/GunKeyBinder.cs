using UnityEngine;
using System.Collections;

namespace Bootcamp.Weapons
{
    [System.Serializable]
    public struct GunKeyBinder
    {
        public Gun gun;
        public KeyCode keyToActivate;
        public bool switchModesOnKey;
    }
}