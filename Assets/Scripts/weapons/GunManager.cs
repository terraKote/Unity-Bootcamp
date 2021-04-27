using UnityEngine;
using System.Collections;
using Bootcamp.Soldier;
using Bootcamp.Hud;

namespace Bootcamp.Weapons
{
    public class GunManager : MonoBehaviour
    {
        public GunKeyBinder[] guns;
        public SoldierController soldier;
        public HudWeapons hud;

        [HideInInspector] public Gun currentGun;
        [HideInInspector] public int currentWeapon;

        void Start()
        {
            for (int i = 0; i < guns.Length; i++)
            {
                guns[i].gun.enabled = false;
            }
            currentWeapon = 0;
            guns[0].gun.enabled = true;
            currentGun = guns[0].gun;
        }

        void Update()
        {
            for (int i = 0; i < guns.Length; i++)
            {
                if (Input.GetKeyDown(guns[i].keyToActivate))
                {
                    ChangeToGun(i);
                }
            }

            hud.selectedWeapon = currentWeapon;
            hud.ammoRemaining[currentWeapon] = guns[currentWeapon].gun.currentRounds;
        }

        void ChangeToGun(int gunIndex)
        {
            Gun cGun = guns[gunIndex].gun;

            if (cGun.enabled)
            {
                if (guns[gunIndex].switchModesOnKey)
                {
                    switch (cGun.fireMode)
                    {
                        case FireMode.SEMI_AUTO:
                            cGun.fireMode = FireMode.FULL_AUTO;
                            break;
                        case FireMode.FULL_AUTO:
                            cGun.fireMode = FireMode.BURST;
                            break;
                        case FireMode.BURST:
                            cGun.fireMode = FireMode.SEMI_AUTO;
                            break;
                    }
                }
            }
            else
            {
                for (int j = 0; j < guns.Length; j++)
                {
                    guns[j].gun.enabled = false;
                }

                cGun.enabled = true;
                currentGun = cGun;
                currentWeapon = gunIndex;
            }
        }
    }
}