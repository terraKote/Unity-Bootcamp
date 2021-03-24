#pragma strict
#pragma implicit
#pragma downcast

class MainMenuEffects extends MonoBehaviour
{
	public var emitter : ParticleEmitter;
	
	function Start()
	{
		if(emitter != null)
		{
			emitter.Simulate(10);
		}	
	}
}