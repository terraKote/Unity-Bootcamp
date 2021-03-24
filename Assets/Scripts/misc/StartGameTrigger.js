#pragma strict
#pragma implicit
#pragma downcast

class StartGameTrigger extends MonoBehaviour
{
	function OnTriggerEnter(other : Collider)
	{
		if(other.name.ToLower() == "soldier")
		{
			GameManager.running = true;
			Destroy(this);
		}
	}
}