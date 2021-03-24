#pragma strict

class SyncAudioListener extends MonoBehaviour
{
	private var listener : AudioListener;
	
	function Start()
	{
		listener = gameObject.GetComponent("AudioListener") as AudioListener;
		
		if(listener == null) this.enabled = false;
		
		listener.enabled = false;
	}
	
	function Update()
	{
		listener.enabled = camera.enabled;
	}
}