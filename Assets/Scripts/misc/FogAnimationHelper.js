#pragma strict
#pragma implicit
#pragma downcast

class FogAnimationHelper extends MonoBehaviour
{
	public var fogColor : Color;
	public var fogDensity : float;
	
	function Start()
	{
		fogDensity = RenderSettings.fogDensity;
		fogColor = RenderSettings.fogColor;
	}
	
	function Update () 
	{
		fogDensity = Mathf.Clamp(fogDensity, 0.0, 1.0);
		
		if(RenderSettings.fogDensity != fogDensity)
		{
            if(!RenderSettings.fog) RenderSettings.fog = true;

			RenderSettings.fogDensity = fogDensity;
		}
		
		if(RenderSettings.fogColor != fogColor)
		{
            if(!RenderSettings.fog) RenderSettings.fog = true;

			RenderSettings.fogColor = fogColor;
		}
	}
}