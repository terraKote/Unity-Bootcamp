#pragma strict
#pragma implicit
#pragma downcast

class SoundObjectManager extends MonoBehaviour
{
	public var minSpeedToParticle : float = 3.0;
	public var genericParticle : GameObject;
	public var waterParticles : GameObject;
	public var waterLayer : LayerMask;

	public var minSpeedSound : float = 2.0;
	public var maxSpeedSound : float = 10.0;
	
	public var defaultSound : AudioClip;
	public var defaultMetalSound : AudioClip;
	public var defaultWoodSound : AudioClip;
	public var defaultConcreteSound : AudioClip;
	
	public var additionalSounds : AudioClip[];
	
	static public var instance : SoundObjectManager;
	
	function Awake()
	{
		instance = this;	
	}
}