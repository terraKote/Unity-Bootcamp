#pragma strict
#pragma implicit
#pragma downcast

class FadeLight extends MonoBehaviour
{
	public var delay : float;
	public var fadeTime : float;
	
	private var fadeSpeed : float;
	private var intensity : float;
	private var color : Color;
	
	function Start()
	{
		if(GetComponent.<Light>() == null)
		{
			Destroy(this);
			return;
		}
		
		intensity = GetComponent.<Light>().intensity;
		
		
		fadeTime = Mathf.Abs(fadeTime);
		
		if(fadeTime > 0.0)
		{
			fadeSpeed = intensity / fadeTime;
		}
		else
		{
			fadeSpeed = intensity;
		}
		//alpha = 1.0;
	}
	
	function Update()
	{
		if(delay > 0.0)
		{
			delay -= Time.deltaTime;
		}
		else if(intensity > 0.0)
		{
			intensity -= fadeSpeed * Time.deltaTime;
			GetComponent.<Light>().intensity = intensity;
		}
	}
}