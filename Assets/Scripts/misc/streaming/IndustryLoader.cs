using UnityEngine;
using System.Collections;
using Bootcamp.Hud.Sarge;

public class IndustryLoader : MonoBehaviour {
    public SargeManager sarge;

    public GameObject endSceneTrigger ;
    private bool playing ;

    private AssetBundle auxBundle;
    private WWW con;
    static public float industryProgress ;
    
    void Start()
    {
        if (endSceneTrigger != null) Destroy(endSceneTrigger);

        playing = false;

        con = new WWW(StreamingController.baseAddress + "industry.unity3d");
    }

    void OnTriggerEnter(Collider other )
    {
        if (!playing)
        {
            if (other.name.ToLower() == "soldier")
            {
                playing = true;

                StartCoroutine("LoadIndustry");
            }
        }
    }

    IEnumerator LoadIndustry()
    {
        //var progress : float = Application.GetStreamProgressForLevel("demo_industry");

        if (con != null && con.isDone)//progress >= 1.0)
        {
            auxBundle = con.assetBundle;
            industryProgress = 1.0f;
            MainMenuScreen.goingToGame = true;

            con.Dispose();
            con = null;
        }
        else
        {
            MainMenuScreen.showProgress = true;

            sarge.ShowInstruction("preparing_bots");

            while (!con.isDone)//progress < 1.0)
            {
                industryProgress = con.progress;
                //progress = Application.GetStreamProgressForLevel("demo_industry");
                yield return null;
            }

            auxBundle = con.assetBundle;

            MainMenuScreen.goingToGame = true;

            con.Dispose();
            con = null;
        }
    }
}
