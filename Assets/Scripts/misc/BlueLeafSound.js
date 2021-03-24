#pragma strict
#pragma implicit
#pragma downcast

class BlueLeafSound extends MonoBehaviour
{
	public var delay : float;
	
	function Update()
	{
		if(delay > 0.0)
		{
			delay -= Time.deltaTime;
			
			if(delay < 0.0)
			{
				audio.Play();
			}
		}	
	}
}