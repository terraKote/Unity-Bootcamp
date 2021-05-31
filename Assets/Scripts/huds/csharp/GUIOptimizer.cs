using UnityEngine;
using System.Collections;

public class GUIOptimizer : MonoBehaviour
{
    public HudWeapons hudWeapons;
    public MainMenuScreen mainMenu;
    public SargeManager sarge;
    public AchievmentScreen achievements;

    void OnGUI()
    {
        var evt = Event.current;

        if (mainMenu != null) mainMenu.DrawGUI(evt);

        if (achievements != null) achievements.DrawGUI(evt);

        if (evt.type == EventType.Repaint)
        {
            if (hudWeapons != null) hudWeapons.DrawGUI(evt);
            if (sarge != null) sarge.DrawGUI(evt);
        }
    }
}
