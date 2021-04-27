using UnityEngine;
using System.Collections;

namespace Bootcamp.Hud
{
    public class HudWeapons : MonoBehaviour
    {
        public Texture2D[] weapon;
        public Texture2D[] ammunition;
        public Texture2D[] ammunitionBackground;

        public int selectedWeapon;
        public int[] maxAmmo;
        public int[] ammoRemaining;
        public int[] maxIcons;
        public int[] clipsRemaining;

        private Vector2 startCorner;
        private Rect[] weaponRect;
        private Rect[] ammunitionRect;

        public GUIStyle totalAmmoStyle;
        public GUIStyle ammoRemainingStyle;

        private float alphaWeapon;
        private float alphaAmmo;
        private Color auxColor;
        private Color cColor;

        private int state;
        private int currentWeapon;
        private int currentAmmo;
        private float hideTime;
        public float fadeTime = 0.2f;
        public float showTime = 2.0f;

        void Start()
        {
            fadeTime = 1.0f / fadeTime;

            state = 0;
            alphaWeapon = 0.0f;
            alphaAmmo = 0.0f;

            currentWeapon = selectedWeapon;
            currentAmmo = ammoRemaining[selectedWeapon];

            weaponRect = new Rect[weapon.Length];
            for (int i = 0; i < weaponRect.Length; i++)
            {
                weaponRect[i] = new Rect(0, 0, weapon[i].width, weapon[i].height);
            }

            ammunitionRect = new Rect[ammunitionBackground.Length];
            for (int i = 0; i < ammunitionBackground.Length; i++)
            {
                ammunitionRect[i] = new Rect(0, 0, ammunitionBackground[i].width, ammunitionBackground[i].height);
            }
        }

        void DrawGUI(Event e)
        {
            if (alphaAmmo <= 0.0) return;

            auxColor = cColor = GUI.color;

            startCorner = new Vector2(Screen.width, Screen.height) - new Vector2(5, 5);

            selectedWeapon = Mathf.Clamp(selectedWeapon, 0, 1);

            ShowAmmunition();
            ShowSelectedWeapon();

            GUI.color = cColor;
        }

        void Update()
        {
            selectedWeapon = Mathf.Clamp(selectedWeapon, 0, 1);

            switch (state)
            {
                case 0:
                    if (alphaAmmo > 0.0) alphaAmmo -= Time.deltaTime * fadeTime;
                    if (alphaWeapon > 0.0) alphaWeapon -= Time.deltaTime * fadeTime;
                    break;
                case 1:
                    alphaAmmo = 0.0f;

                    if (alphaWeapon < 1.0) alphaWeapon += Time.deltaTime * fadeTime;
                    break;
                case 2:
                    alphaWeapon = 0.0f;

                    if (alphaAmmo < 1.0) alphaAmmo += Time.deltaTime * fadeTime;
                    break;
            }

            if (selectedWeapon != currentWeapon)
            {
                currentWeapon = selectedWeapon;
                currentAmmo = ammoRemaining[selectedWeapon];
                state = 1;
                hideTime = showTime + ((1.0f - alphaWeapon) * (1 / fadeTime));
            }
            else if (currentAmmo != ammoRemaining[selectedWeapon])
            {
                currentAmmo = ammoRemaining[selectedWeapon];
                state = 2;
                hideTime = showTime + ((1.0f - alphaAmmo) * (1 / fadeTime));
            }
            else if (hideTime > 0.0)
            {
                hideTime -= Time.deltaTime;

                if (hideTime <= 0.0)
                {
                    state = 0;
                }
            }
        }

        void ShowAmmunition()
        {
            auxColor.a = alphaAmmo;
            GUI.color = auxColor;

            ammunitionRect[selectedWeapon].x = startCorner.x - ammunitionRect[selectedWeapon].width;
            ammunitionRect[selectedWeapon].y = startCorner.y - ammunitionRect[selectedWeapon].height;

            GUI.DrawTexture(ammunitionRect[selectedWeapon], ammunitionBackground[selectedWeapon]);

            var delta = Mathf.Clamp(ammoRemaining[selectedWeapon], 0, maxAmmo[selectedWeapon]);
            delta /= maxAmmo[selectedWeapon];
            delta *= maxIcons[selectedWeapon];

            var length = delta;
            for (int i = 0; i < length; i++)
            {
                GUI.DrawTexture(new Rect(ammunitionRect[selectedWeapon].x + 40 + (i * (ammunition[selectedWeapon].width - 1)), ammunitionRect[selectedWeapon].y + 28, ammunition[selectedWeapon].width, ammunition[selectedWeapon].height), ammunition[selectedWeapon]);
            }

            var auxRect = new Rect(ammunitionRect[selectedWeapon].x + 40, ammunitionRect[selectedWeapon].y + 2, 20, 20);

            GUI.Label(auxRect, clipsRemaining[selectedWeapon].ToString(), totalAmmoStyle);
            auxRect.x += 17;
            auxRect.y -= 1;
            GUI.Label(auxRect, "|", totalAmmoStyle);
            auxRect.x += 6;
            auxRect.y += 4;
            GUI.Label(auxRect, ammoRemaining[selectedWeapon].ToString(), ammoRemainingStyle);
        }

        void ShowSelectedWeapon()
        {
            auxColor.a = alphaWeapon;
            GUI.color = auxColor;

            weaponRect[selectedWeapon].x = startCorner.x - weaponRect[selectedWeapon].width;
            weaponRect[selectedWeapon].y = startCorner.y - weaponRect[selectedWeapon].height;

            GUI.DrawTexture(weaponRect[selectedWeapon], weapon[selectedWeapon]);
        }
    }
}