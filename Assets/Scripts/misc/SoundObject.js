#pragma strict
#pragma implicit
#pragma downcast

class SoundObject extends MonoBehaviour
{
	@HideInInspector
	public var overrideSound : int = -1;
	
	private var playSound : boolean;
	private var generateParticles : boolean;
	
	public var minSpeedToParticle : float;
	public var genericParticle : GameObject;
	public var waterParticles : GameObject;
	public var waterLayer : LayerMask;
	public var checkWater : boolean;
	private var waterTimer : float;
	
	public var minSpeedSound : float;
	public var maxSpeedSound : float;
	
	private var timeToGenerateParticles : float;
	private var timeToSound : float;
	private var delta : float;
	
	private var audioPlayer : AudioSource;
	private var ac : AudioClip;
	
	function Start()
	{
		playSound = false;
		generateParticles = false;
		checkWater = false;
		
		if(SoundObjectManager.instance == null)
		{
			Destroy(this);
			return;
		}
		
		var so : SoundObjectManager = SoundObjectManager.instance;
		minSpeedToParticle = so.minSpeedToParticle;
		minSpeedToParticle *= minSpeedToParticle;
		
		minSpeedSound = so.minSpeedSound;
		maxSpeedSound = so.maxSpeedSound;
		delta = 1.0 / (maxSpeedSound - minSpeedSound);
		genericParticle = so.genericParticle;
		
		generateParticles = genericParticle != null;
		
		waterParticles = so.waterParticles;
		waterLayer = so.waterLayer;
		checkWater = waterParticles != null;
		
		if(overrideSound == -1)
		{
			switch(gameObject.tag)
			{
				case "wood":
					ac = so.defaultWoodSound;
					break;
				case "metal":
					ac = so.defaultMetalSound;
					break;
				case "concrete":
					ac = so.defaultConcreteSound;
					break;
				default:
					ac = so.defaultSound;
					break;
			}
		}
		
		playSound = ac != null;
		
		if(!playSound && !generateParticles) 
		{
			Destroy(this);
		}
		
		if(rigidbody == null)
		{
			if(transform.parent != null)
			{
				if(transform.parent.rigidbody != null)
				{
					var aux : SoundObjectAux = transform.parent.gameObject.AddComponent("SoundObjectAux") as SoundObjectAux;
					aux.soundGenerator = this;
				}
				else
				{
					Destroy(this);
				}
			}
			else
			{
				Destroy(this);
			}
		}
	}
	
	function Update()
	{
		if(waterTimer > 0.0)
		{
			waterTimer -= Time.deltaTime;
		}
		
		timeToGenerateParticles -= Time.deltaTime;
		timeToSound -= Time.deltaTime;
	}
	
	function OnCollisionEnter(collisionData : Collision)
	{
		var speed : float = collisionData.relativeVelocity.sqrMagnitude;
		
		if(checkWater && waterTimer <= 0.0)
		{
			if(collisionData.collider.gameObject.GetComponent("Terrain") != null)
			{
				var hitInfo : RaycastHit;
				
				if(Physics.Raycast(transform.position + new Vector3(0, 4, 0), -Vector3.up, hitInfo, 4.0, waterLayer))
				{
					if(hitInfo.collider.tag == "water")
					{
						waterTimer = 1.0;
						
						var go : GameObject = GameObject.Instantiate(waterParticles, hitInfo.point, Quaternion.identity) as GameObject;
						
						if(go.audio != null)
						{
							go.audio.Play();
						}
						
						var emitter : ParticleEmitter;
						for(var i : int = 0; i < go.transform.childCount; i++)
						{
							emitter = go.transform.GetChild(i).GetComponent("ParticleEmitter") as ParticleEmitter;
							
							if(emitter == null) continue;
							
							emitter.emit = false;
							emitter.Emit();
						}
						
						var aux : AutoDestroy = go.AddComponent("AutoDestroy") as AutoDestroy;
						aux.time = 2;
						return;
					}
				}
			}
		}
		
		if(generateParticles && timeToGenerateParticles <= 0.0)
		{
			if(minSpeedToParticle < speed)
			{
				timeToGenerateParticles = 0.5;
				GameObject.Instantiate(genericParticle, collisionData.contacts[0].point, Quaternion.identity);
			}
		}
		
		if(playSound && timeToSound <= 0.0)
		{
			if(audioPlayer == null)
			{
				audioPlayer = gameObject.AddComponent("AudioSource") as AudioSource;
				audioPlayer.playOnAwake = false;
				audioPlayer.loop = false;
				audioPlayer.clip = ac;
			}
			
			if(minSpeedSound * minSpeedSound < speed)
			{
				speed = Mathf.Sqrt(speed);
				var v : float = Mathf.Clamp((speed - minSpeedSound) * delta, 0.05, 1.0);
				audioPlayer.volume = v;
				timeToSound = 0.2;
				audioPlayer.Play();
			}
		}
	}
}