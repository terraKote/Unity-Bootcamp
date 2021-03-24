class IndustryLoader extends MonoBehaviour
{
    public var sarge : SargeManager;

    public var endSceneTrigger : GameObject;
    private var playing : boolean;

    private var auxBundle : AssetBundle;
    private var con : WWW;
    static public var industryProgress : float;
    function Start()
    {
        if(endSceneTrigger != null) Destroy(endSceneTrigger);

        playing = false;

        con = new WWW(StreamingController.baseAddress + "industry.unity3d");
    }

    function OnTriggerEnter(other : Collider)
	{
		if(!playing)
		{
			if(other.name.ToLower() == "soldier")
			{
				playing = true;

                StartCoroutine("LoadIndustry");
			}
		}
	}

    function LoadIndustry()
    {
        //var progress : float = Application.GetStreamProgressForLevel("demo_industry");

        if(con != null && con.isDone)//progress >= 1.0)
        {
            auxBundle = con.assetBundle;
            industryProgress = 1.0;
            MainMenuScreen.goingToGame = true;

            con.Dispose();
            con = null;
        }
        else
        {
            MainMenuScreen.showProgress = true;
            
            sarge.ShowInstruction("preparing_bots");

            while(!con.isDone)//progress < 1.0)
            {
                industryProgress = con.progress;
                //progress = Application.GetStreamProgressForLevel("demo_industry");
                yield;
            }

            auxBundle = con.assetBundle;

            MainMenuScreen.goingToGame = true;
            
            con.Dispose();
            con = null;
        }
    }
}