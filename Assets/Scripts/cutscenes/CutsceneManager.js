#pragma strict
#pragma implicit
#pragma downcast

class CutsceneManager extends MonoBehaviour
{
	public var heliCutscene : HelicopterCutscene;
	public var forestCutscene : ForestCutscene;
	
	function Awake()
	{
		if(forestCutscene != null)
		{
			forestCutscene.gameObject.SetActiveRecursively(true);
		}
	}
	
	function PlayHeli(step : int)
	{
		if(heliCutscene == null) return;
		heliCutscene.Play(step);
	}
	
	function PlayForest()
	{
		if(forestCutscene == null) return;
		forestCutscene.Play();	
	}
	
	function HeliCutsceneEnd(step : int)
	{
		switch(step)
		{
			case 0:
				PlayHeli(1);
				break;
			case 1:
				break;
			case 2:
				break;
			case 3:
				break;
		}
	}
	
	function Disable(cutsceneName : String)
	{
		switch(cutsceneName)
		{
			case "Helicopter":
				heliCutscene.DestroyScene();
				break;
		}
	}
}