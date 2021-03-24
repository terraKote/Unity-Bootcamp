#pragma strict
#pragma implicit
#pragma downcast

class Turret extends MonoBehaviour
{
	public var parts : Transform[];
	public var dead : boolean;
	public var target : Transform;
	
	public var vertical : Transform;
	public var horizontal : Transform;
	public var speed : Vector2;
	
	public var minAngle : float;
	public var maxAngle : float;
	
	public var hitLayer : LayerMask;
	
	private var timerToCreateDecal : float;
	
	public var shootDelay : float = 1.0;
	private var _shootDelay : float;
	
	public var shootBurst : int;
	private var _shootBurst : int;
	
	public var weapon : GameObject;
	
	public var woodParticle : GameObject;
	public var metalParticle : GameObject;
	public var sandParticle : GameObject;
	public var concreteParticle : GameObject;
	public var bulletMark : GameObject;
	
	public var shootRange : float = 40.0;
	public var activeDistance : float = 25.0;
	public var disableDistance : float = 30.0;
	
	public var pushPower : float = 5.0;
	
	public var shotingEmitter : GunParticles;
	public var shotLight : ShotLight;
	
	private var shooting : boolean;
	
	private var shootTime : float;
	
	private var running : boolean;
	
	public var fadeShader : Shader;
	
	function Start()
	{
		running = false;
		
		activeDistance *= activeDistance;
		
		dead = false;
		
		if(target == null) target = SoldierController.enemiesReference;
		
		GunEffects(false);
	}
	
	function Update()
	{
		if(GameManager.pause || GameManager.scores) return;
		
		if(dead) Destroy(this);
		
		if(target == null) target = SoldierController.enemiesReference;;
		
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
		
		timerToCreateDecal -= Time.deltaTime;
		
		if(!shooting)
		{
			_shootDelay -= Time.deltaTime;
			
			if(_shootDelay <= 0.0)
			{
				shooting = true;
			}
		}
		
		var hDir	 : Vector3;
		
		if(horizontal != null)
		{
			hDir = (new Vector3(target.position.x, horizontal.position.y, target.position.z) - horizontal.position).normalized;
			
			horizontal.rotation = Quaternion.Slerp(horizontal.rotation, Quaternion.FromToRotation(Vector3.right, hDir), speed.x * Time.deltaTime);
		}
		
		if(vertical != null)
		{
			hDir = (new Vector3(target.position.x, vertical.position.y, target.position.z) - vertical.position).normalized;
			
			var vDir : Vector3 = (target.position - vertical.position).normalized;
			
			var angle : float = (vDir.y < 0.0 ? -1.0 : 1.0) * Vector3.Angle(hDir, vDir);
			
			if(angle < minAngle) angle = minAngle;
			else if(angle > maxAngle) angle = maxAngle;
			
			vertical.localRotation = Quaternion.Slerp(vertical.localRotation, Quaternion.Euler(new Vector3(0, 0, angle)), speed.y * Time.deltaTime);
		}
		
		ShootTheTarget();
	}
	
	function ShootTheTarget()
	{
		//if(GameManager.pause || GameManager.scores) return;
		
		if(!shooting) return;
		
		if(weapon == null) return;
		
		if(shootTime < Time.time)
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
						hit.collider.gameObject.SendMessage("HitSoldier", "Turret", SendMessageOptions.DontRequireReceiver);
						GenerateGraphicStuff(hit);
					}
				}
				else
				{
					hit.collider.gameObject.SendMessage("HitSoldier", "Turret", SendMessageOptions.DontRequireReceiver);		
					GenerateGraphicStuff(hit);
				}
			}
			
			audio.Play();
			
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
	
	function Destruct()
	{
		if(dead) return;
		
		dead = true;
		
		TrainingStatistics.turrets++;
		
		GunEffects(false);
		
		if(parts != null)
		{
			var length : int = parts.length;
			var t : Transform;
			var i : int;
			for(i = 0; i < length; i++)
			{
				t = parts[i] as Transform;
				
				t.parent = null;
				
				var mat : Material = new Material(fadeShader);
		
				var r : Renderer = t.gameObject.renderer;
				
				mat.mainTexture = r.material.mainTexture;
				r.material = mat;
				
				if(t.gameObject.GetComponent("TrainingDummyPartDestructor") == null)
				{
					t.gameObject.AddComponent("TrainingDummyPartDestructor");
				}
				
				if(t.rigidbody != null)
				{
					t.rigidbody.isKinematic = false;
				}
			}
			
			var p : Array = new Array();
			for(i = 0; i < length; i++)
			{
				t = parts[i] as Transform;
				
				if(t.childCount > 0)
				{
					for(var k : int = 0; k < t.childCount; k++)
					{
						p.Add(t.GetChild(k));
					}
					
					for(var a : Transform in p)
					{
						Destroy(a.gameObject);
					}
				}
				
				p.Clear();
			}
		}
		
		Destroy(this);
	}
	
	function Hit()
	{
		if(!running) running = true;
	}
}