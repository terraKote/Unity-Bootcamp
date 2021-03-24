#pragma strict
#pragma implicit
#pragma downcast

class ForestCutscenePlayer extends MonoBehaviour
{
	public var forestCutscene : ForestCutscene;
	
	function OnTriggerEnter(other : Collider)
	{
		PlayScene(other);
	}
	
	function OnTriggerStay(other : Collider)
	{
		PlayScene(other);
	}
	
	function OnTriggerExit(other : Collider)
	{
		PlayScene(other);
	}
	
	function PlayScene(other : Collider)
	{
		if(other.name.ToLower() == "soldier")
		{
			forestCutscene.Play();
			Destroy(this);
		}
	}
}