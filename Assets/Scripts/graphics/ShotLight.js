#pragma strict
#pragma implicit
#pragma downcast

class ShotLight extends MonoBehaviour
{
	public var time : float = 0.02;
	private var timer : float;
	
	function OnEnable()
	{
		if(GetComponent.<Light>() == null)
		{
			Destroy(this);
		}
		else
		{
			timer = time;
			GetComponent.<Light>().enabled = false;
		}
	}
	
	function OnDisable()
	{
		if(GetComponent.<Light>() == null)
		{
			Destroy(this);
		}
		else
		{
			timer = time;
			GetComponent.<Light>().enabled = false;
		}
	}
	
	function LateUpdate()
	{
		timer -= Time.deltaTime;
		
		if(timer <= 0.0)
		{
			timer = time;
			GetComponent.<Light>().enabled = !GetComponent.<Light>().enabled;
		}
	}
}