#pragma strict
#pragma implicit
#pragma downcast

class TrainingDummy extends MonoBehaviour
{
	public var weapon : GameObject;
	public var shootsToDestroyPart : int;
	public var fadeShader : Shader;
	
	@HideInInspector
	public var dummyParts : TrainingDummyPart[];
	
	private var enableFailEffect : boolean;
	private var failEffectTimer : float;
	public var explosionEffectPrefab : GameObject;

	public var explosionUpwardsModifier : float;
	public var explosionPower : float;
	public var explosionRadius : float;
	
	public var animTime : Vector2;
	public var animSpeed : Vector2 = new Vector2(1.0, 1.0);
	public var animTimeVariation : float;
	
	public var failEffect : GameObject;
	public var failEffectTime : float;
	public var failEffectTimeVariation : float;
	
	private var dead : boolean;
	private var canShoot : boolean;
	private var timer : float;
	private var state : int;
	
	public var shotingEmitter : GunParticles;
	public var shotLight : ShotLight;

	public var target : Transform;
	public var rotatingPart : Transform;
	
	public var riseSound : AudioClip;
	
	public var downSound : AudioClip;
	
	public var dieSound : AudioClip;
	
	public var shootSound : AudioClip;
	
	public var moveSpeed : float;
	
	public var shootBurst : int;
	private var _shootBurst : int;
	
	public var shootDelay : float;
	private var _shootDelay : float;
	
	private var shooting : boolean;
	private var shootTime : float;
	
	public var hitLayer : LayerMask;
	public var shootRange : float = 25.0;
	
	private var shootRef : Transform;
	
	public var metalParticle : GameObject;
	public var concreteParticle : GameObject;
	public var sandParticle : GameObject;
	public var woodParticle : GameObject;
	public var bulletMark : GameObject;
	private var timerToCreateDecal : float;
	public var pushPower : float = 3.0;
	
	public var activeDistance : float =  15.0;
	public var disableDistance : float = 30.0;
	private var running : boolean;
	public var mat : Material;
	
	function Start()
	{	
		animation["dum_crouch"].speed = animSpeed.x;
		animation["dum_stand"].speed = animSpeed.y;	
		animation["dum_death"].speed = 2.0;		
					
		running = false;
		
		shootTime = 0.0;
		activeDistance *= activeDistance;
		
		if(target == null) target = SoldierController.enemiesReference;
		
		shootsToDestroyPart = Mathf.Max(shootsToDestroyPart, 1);
		failEffect.SetActiveRecursively(true);
		enableFailEffect = false;
		dead = false;
		timer = 0.0;
		state = 2;
		canShoot = true;
		animTimeVariation *= 0.5;
		failEffectTimeVariation *= 0.5;

		failEffectTimer = Mathf.Max(Random.Range(failEffectTime - failEffectTimeVariation, failEffectTime + failEffectTimeVariation), 0.5);
		animation.Play("dum_crouch_idle");
		
		GunEffects(false);
	}
	
	function Update()
	{
		if(GameManager.pause || GameManager.scores) 
		{
			GunEffects(false);
			return;
		}
		
		if(dead) return;
		
		if(target == null) target = SoldierController.enemiesReference;
		
		if(target == null) return;
		
		if(!running)
		{
			if((transform.position - target.position).sqrMagnitude < activeDistance)
			{
				running = true;
			}
			else
			{
				return;
			}
		}
		
		if((transform.position - target.position).magnitude > disableDistance)
		{
			GunEffects(false);
			return;
		}
		
		HandleFailEffects();
	
		HandleAnimations();
		
		timerToCreateDecal -= Time.deltaTime;
		
		if(!shooting)
		{
			_shootDelay -= Time.deltaTime;
			
			if(_shootDelay <= 0.0)
			{
				shooting = true;
			}
		}
		
		if(target == null) target = SoldierController.enemiesReference;
		if(shootRef == null) shootRef = SoldierController.enemiesShootReference;
		
		if(target != null && rotatingPart != null && state == 0)
		{
			rotatingPart.rotation = Quaternion.Lerp(rotatingPart.rotation, Quaternion.LookRotation(target.position - rotatingPart.position), moveSpeed * Time.deltaTime);
		}
		
		ShootTheTarget();
	}
	
	function ShootTheTarget()
	{
		if(!shooting) return;
		
		if(weapon == null) return;
		
		if(shootTime < Time.time && state == 0)
		{
			_shootBurst += 1;
			
			GunEffects(true);
			
			var hit : RaycastHit;
			if(Physics.Raycast(weapon.transform.position, weapon.transform.forward, hit, shootRange, hitLayer))
			{
				if(hit.collider.tag == "glass")
				{
					hit.collider.gameObject.SendMessage("Hit", hit, SendMessageOptions.DontRequireReceiver);
					
					if(Physics.Raycast(hit.point + weapon.transform.forward * 0.05, weapon.transform.forward, hit, shootRange - hit.distance, hitLayer))
					{
						hit.collider.gameObject.SendMessage("HitSoldier", "Dummy", SendMessageOptions.DontRequireReceiver);
						GenerateGraphicStuff(hit);
					}
				}
				else
				{
					hit.collider.gameObject.SendMessage("HitSoldier", "Dummy", SendMessageOptions.DontRequireReceiver);		
					GenerateGraphicStuff(hit);
				}
			}
			
			audio.PlayOneShot(shootSound);
			
			shootTime = Time.time + 0.1;
			
			if(_shootBurst >= shootBurst)
			{
				GunEffects(false);
				_shootBurst = 0;
				shooting = false;
				_shootDelay = shootDelay;
			}
		}
	}
	
	function HandleFailEffects()
	{
		if(enableFailEffect)
		{
			failEffectTimer -= Time.deltaTime;
			
			if(failEffectTimer <= 0.0)
			{
				failEffectTimer = Mathf.Max(Random.Range(failEffectTime - failEffectTimeVariation, failEffectTime + failEffectTimeVariation), 0.5);
				(failEffect.GetComponent("ParticleEmitter") as ParticleEmitter).Emit();
			}
		}
	}
	
	function HandleAnimations()
	{
		timer -= Time.deltaTime;

		if(timer <= 0.0)
		{
			if(animation == null) return;
			
			switch(state)
			{
				case 0:
					if(animation["dum_crouch"] == null) return;
					
					timer = animation["dum_crouch"].length / animSpeed.x;
					animation.CrossFade("dum_crouch");
					
					audio.PlayOneShot(downSound);
					
					if(canShoot) GunEffects(false);
					break;
				case 1:
					if(animation["dum_crouch_idle"] == null) return;
					
					timer = Mathf.Max(Random.Range(animTime.x - animTimeVariation, animTime.x + animTimeVariation), 0.5);
					animation["dum_crouch_idle"].time = 0.0;
					animation.Play("dum_crouch_idle");
					break;
				case 2:
					if(animation["dum_stand"] == null) return;
					
					timer = animation["dum_stand"].length / animSpeed.y;
					
					audio.PlayOneShot(riseSound);
					
					animation.CrossFade("dum_stand");
					break;
				case 3:
					if(animation["dum_stand_idle"] == null) return;
					
					timer = Mathf.Max(Random.Range(animTime.y - animTimeVariation, animTime.y + animTimeVariation), 0.5);
					animation["dum_stand_idle"].time = 0.0;
					animation.Play("dum_stand_idle");
					if(canShoot) GunEffects(true);
					break;
			}
			
			state++;
			if(state > 3)
			{
				state = 0;
			}
		}
	}
	
	function GenerateGraphicStuff(hit : RaycastHit)
	{
		if(timerToCreateDecal < 0.0)
		{
			timerToCreateDecal = 0.1;
			var hitType : HitType;
			
			var body : Rigidbody = hit.collider.rigidbody;
			if(body == null)
			{
				if(hit.collider.transform.parent != null)
				{
					body = hit.collider.transform.parent.rigidbody;
				}
			}
			
			if(body != null)
			{
				if(body.gameObject.layer != 10 && !body.gameObject.name.ToLower().Contains("door"))
				{
					body.isKinematic = false;
				}
				
				if(!body.isKinematic)
				{
	    				var direction : Vector3 = hit.collider.transform.position - weapon.transform.position;
					body.AddForceAtPosition(direction.normalized * pushPower, hit.point, ForceMode.Impulse);
				}
			}
			
			var go : GameObject;
			
			switch(hit.collider.tag)
			{
				case "wood":
					hitType = HitType.WOOD;
					go = GameObject.Instantiate(woodParticle, hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal)) as GameObject;
					break;
				case "metal":
					hitType = HitType.METAL;
					go = GameObject.Instantiate(metalParticle, hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal)) as GameObject;
					break;
				case "car":
					hitType = HitType.METAL;
					go = GameObject.Instantiate(metalParticle, hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal)) as GameObject;
					break;
				case "concrete":
					hitType = HitType.CONCRETE;
					go = GameObject.Instantiate(concreteParticle, hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal)) as GameObject;
					break;
				case "dirt":
					hitType = HitType.CONCRETE;
					go = GameObject.Instantiate(sandParticle, hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal)) as GameObject;
					break;
				case "sand":
					hitType = HitType.CONCRETE;
					go = GameObject.Instantiate(sandParticle, hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal)) as GameObject;
					break;
				default:
					return;
			}
			
			go.layer = hit.collider.gameObject.layer;
			
			if(hit.collider.renderer == null) return;
			
		
			go = GameObject.Instantiate(bulletMark, hit.point, Quaternion.FromToRotation(Vector3.forward, -hit.normal));
			var bm : BulletMarks = go.GetComponent("BulletMarks");
			bm.GenerateDecal(hitType, hit.collider.gameObject);
			
		}
	}
	
	function Hit(hit : RaycastHit, target : int)
	{
		Hit(hit, target, 1);
	}
	
	function Destruct(target : int)
	{
		var dp : TrainingDummyPart = dummyParts[target];
		
		if(dp.gameObject == null) return;
		
		dp.shootsTaken = shootsToDestroyPart;
		
		var go : GameObject;
		
		if(dp.gameObject.transform.parent != null)
		{
			if(dp.gameObject.name.Contains("head") || dp.gameObject.name.Contains("neck"))
			{
				if(!dead)
				{
					dead = true;
					TrainingStatistics.dummies++;
					GunEffects(false);
					
					audio.PlayOneShot(dieSound);
					
					Destruct(4);
					
					animation.CrossFade("dum_death");
				}
			}
			
			if(dp.gameObject != weapon)
			{
				dp.gameObject.transform.parent = null;
				dp.gameObject.active = false;
				
				for(var i : int = 0; i < dp.brokeParts.length; i++)
				{
					go = dp.brokeParts[i];
					
					if(go.transform.parent == null) continue;
					
					InitializeMeshCollider(go);

					DestroyPart(go, false);
				}
				
				for(var j : int = 0; j < dp.siblings.length; j++)
				{
					go = dp.siblings[j];
					
					if(go == null) continue;
					if(go.transform.parent == null) continue;
					
					if(go.name.Contains("head"))
					{
						dead = true;
						TrainingStatistics.dummies++;
						audio.PlayOneShot(dieSound);
						GunEffects(false);
					}
					if(go == weapon) 
					{
						canShoot = false;
						GunEffects(false);
					}
					
					DestroyPart(go, false);
				}
				
				Destroy(dp.gameObject);
			}
			else
			{
				canShoot = false;
				GunEffects(false);
				
				if(weapon.transform.parent != null)
				{
					DestroyPart(weapon, false);
				}
			}
		}
		else
		{
			if(dp.gameObject != weapon)
			{
				dp.gameObject.active = false;
					
				for(i = 0; i < dp.brokeParts.length; i++)
				{
					go = dp.brokeParts[i];

					InitializeMeshCollider(go);
					
					DestroyPart(go, false);
				}
				
				Destroy(dp.gameObject);
			}
		}
	}
	
	function Hit(hit : RaycastHit, target : int, damage : int)
	{
		if(!running) 
		{
			running = true;
		}
		
		var dp : TrainingDummyPart = dummyParts[target];
		
		if(dp.gameObject == null) return;
		
		dp.shootsTaken += damage;
		
		var go : GameObject;
		
		if(dp.gameObject.transform.parent != null)
		{
			if(!dead) 
			{
				if(dp.dummyPart == DummyPart.CHEST)
				{
					var dir : Vector3 = hit.point - transform.position;

					if(Vector3.Dot(dir, transform.right) > 0)
					{
						TrainingStatistics.Register(dp.dummyPart);
					}
					else
					{
						TrainingStatistics.Register(DummyPart.HEART);
					}
				}
				else
				{
					TrainingStatistics.Register(dp.dummyPart);
				}
			}
			
			if(dp.shootsTaken == shootsToDestroyPart - 1)
			{
				if(dp.gameObject != weapon)
				{
					enableFailEffect = true;
				}
			}
			else if(dp.shootsTaken >= shootsToDestroyPart)
			{
				if(dp.gameObject.name.Contains("head") || dp.gameObject.name.Contains("neck"))
				{
					if(!dead)
					{
						dead = true;
						TrainingStatistics.headShoot++;
						TrainingStatistics.dummies++;
						
						audio.PlayOneShot(dieSound);
						GunEffects(false);
						
						if(state != 0)
						{
							Hit(hit, 4, shootsToDestroyPart);
						}
						
						animation.CrossFade("dum_death");
					}
				}
				
				if(dp.gameObject != weapon)
				{
					dp.gameObject.transform.parent = null;
					dp.gameObject.active = false;
					
					for(var i : int = 0; i < dp.brokeParts.length; i++)
					{
						go = dp.brokeParts[i];
						
						if(go.transform.parent == null) continue;
						
						InitializeMeshCollider(go);

						DestroyPart(go, true);
					}
					
					for(var j : int = 0; j < dp.siblings.length; j++)
					{
						go = dp.siblings[j];
						
						if(go == null) continue;
						if(go.transform.parent == null) continue;
						
						if(go.name.Contains("head"))
						{
							dead = true;
							TrainingStatistics.dummies++;
							audio.PlayOneShot(dieSound);
							GunEffects(false);
						}
						if(go == weapon) 
						{
							canShoot = false;
							GunEffects(false);
						}
						
						DestroyPart(go, true);
					}
					
					GameObject.Instantiate(explosionEffectPrefab, hit.point, Quaternion.identity);
					
					Destroy(dp.gameObject);
				}
				else
				{
					canShoot = false;
					GunEffects(false);
					
					if(weapon.transform.parent != null)
					{
						DestroyPart(weapon, true);
					}
				}
			}
		}
		else
		{
			if(dp.gameObject != weapon)
			{
				dp.gameObject.active = false;
					
				for(i = 0; i < dp.brokeParts.length; i++)
				{
					go = dp.brokeParts[i];

					InitializeMeshCollider(go);
					
					DestroyPart(go, true);
				}
				
				Destroy(dp.gameObject);
			}
		}
	}
	
	function GunEffects(b : boolean)
	{
		if(shotingEmitter != null)
		{
			shotingEmitter.ChangeState(b);
			
		}
		
		if(shotLight != null)
		{
			shotLight.enabled = b;
		}
	}
	
	function DestroyPart(go : GameObject, applyForce : boolean)
	{
		go.transform.parent = null;
		
		mat = new Material(fadeShader);
		
		var r : Renderer = go.renderer;
		
		mat.mainTexture = r.material.mainTexture;
		r.material = mat;
		
		if(go.GetComponent("TrainingDummyPartDestructor") == null)
		{
			go.AddComponent("TrainingDummyPartDestructor");
		}
			
		
		var rb : Rigidbody = InitializeRigidbody(go);
		if(applyForce)
		{			
			rb.AddForceAtPosition((go.transform.position - transform.position).normalized * explosionPower, transform.position);
		}
	}
	
	function InitializeRigidbody(go : GameObject) : Rigidbody
	{
		var rb : Rigidbody = go.rigidbody;
		rb.isKinematic = false;
		
		return rb;
	}
	
	function InitializeMeshCollider(go : GameObject)
	{
		go.active = true;
	}
}