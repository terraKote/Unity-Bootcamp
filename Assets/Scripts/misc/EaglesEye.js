#pragma strict
#pragma implicit
#pragma downcast

class EaglesEye extends MonoBehaviour
{
	private var registered : boolean;
	
	function Start()
	{
		registered = false;
	}
	
	function Update()
	{
		if(!registered)
		{
			TrainingStatistics.totalEaglesEye++;
			registered = true;
		}	
	}
	
	function Hit(hit : RaycastHit)
	{
		TrainingStatistics.eaglesEye++;
		Destroy(this);
	}
}