using UnityEngine;
using UnityEngine.UI;

public class BulletHudService : AmmoHudService
{
    [SerializeField] private RectTransform bulletImageContainer;
    [SerializeField] private Image bulletIconTemplate;

    private int totalBulletClip = 30;
    private int currentBulletClip = 30;

    public int CurrentBulletClip
    {
        get { return currentBulletClip; }
        set
        {
            currentBulletClip = value;
            currentAmmoCountText.text = currentBulletClip.ToString();

            for (int i = 0; i < totalBulletClip; i++)
            {
                var bulletIcon = bulletImageContainer.GetChild(i).GetComponent<Image>();
                Color bulletColor = Color.white;

                if (i > currentBulletClip)
                {
                    bulletColor.a = 0.7f;
                }

                bulletIcon.color = bulletColor;
            }
        }
    }

    public void InitializeClip(int bulletCount)
    {
        totalBulletClip = bulletCount;

        for (int i = 0; i < totalBulletClip; i++)
        {
            Instantiate(bulletIconTemplate, bulletImageContainer);
        }
    }
}
