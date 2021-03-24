#pragma strict
#pragma implicit
#pragma downcast

class EndGameTrigger extends MonoBehaviour
{
	public var achievments : AchievmentScreen;
	public var sarge : SargeManager;

	function OnTriggerEnter(other : Collider)
	{
		if(other.name.ToLower() == "soldier")
		{
			achievments.visible = true;
			GameManager.scores = true;
			GameManager.running = false;
            sarge.ShowInstruction("good_work");
			Destroy(this);		
		}
	}
}