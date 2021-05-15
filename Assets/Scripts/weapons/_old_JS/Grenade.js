#pragma strict
#pragma implicit
#pragma downcast

class Grenade extends MonoBehaviour
{
	private var thisTransform : Transform;
	public var minY : float = -10.0;
	public var smoke : GameObject;
	public var explosionEmitter : GameObject;
	public var explosionTime : float;
	public var explosionRadius : float;
	public var power : float = 3200;
	private var timer : float;

	public var soldierCamera : SoldierCamera;
	
	public var nearSounds : AudioClip[];
	public var farSounds : AudioClip[];
	public var farSoundDistance : float = 25.0;
	
	private var exploded : boolean;
	private var hit : RaycastHit;
	
	function Start ()
	{
		exploded = false;
		
		timer = 0.0;
		
		thisTransform = transform;
	}
	
	function Detonate ()
	{
		if(exploded) return;
		
		exploded = true;
		
		if(renderer != null)
		{
			renderer.enabled = false;
			
			if(smoke != null)
			{
				Destroy(smoke);
			}
		}
		else
		{
			var renderers = GetComponentsInChildren(Renderer);
			
			for(var r : Renderer in renderers)
			{
				r.enabled = false;
			}
		}
		
		var _explosionPosition : Vector3 = thisTransform.position;
		var col : Collider[] = Physics.OverlapSphere(_explosionPosition, explosionRadius);
		
		var distance : float = Vector3.Distance(soldierCamera.transform.position, _explosionPosition);
		soldierCamera.StartShake(distance);
		
		var body : Rigidbody;
		if(col != null)
		{
			for(var c : int = 0; c < col.length; c++)
			{
				col[c].gameObject.SendMessage("Destruct", SendMessageOptions.DontRequireReceiver);
				
				body = null;
				body = col[c].gameObject.rigidbody;
				if(body != null)
				{
					body.isKinematic = false;
				}
				else if(col[c].gameObject.transform.parent != null)
				{
					body = col[c].gameObject.transform.parent.rigidbody;
					if(body != null)
					{
						body.isKinematic = false;
					}
				}
				
				if(body != null)
				{
					body.AddExplosionForce(power, _explosionPosition, explosionRadius, 3.0f);
				}
				
				if(col[c].collider.tag == "glass")
				{
					col[c].gameObject.SendMessage("BreakAll", SendMessageOptions.DontRequireReceiver);
				}
			}
		}
		
		gameObject.SendMessage("Explode", SendMessageOptions.DontRequireReceiver);
		
		PlaySound(distance);
		
		if(explosionEmitter != null)
		{
			GameObject.Instantiate(explosionEmitter, transform.position, Quaternion.identity);
		}
	}
	
	function PlaySound(distance : float)
	{
		var sIndex : int;
	
		if (distance < farSoundDistance)
		{
			sIndex = Random.Range(0, nearSounds.length);
			audio.PlayOneShot(nearSounds[sIndex]);
			timer = nearSounds[sIndex].length + 1.0;
		}
		else
		{
			sIndex = Random.Range(0, farSounds.length);
			audio.PlayOneShot(farSounds[sIndex]);
			timer = farSounds[sIndex].length + 1.0;
		}
	}	
	
	function Update ()
	{
		if(thisTransform.position.y < minY)
		{
			Destroy(gameObject);
		}
		
		if(exploded)
		{
			if(timer > 0.0)
			{
				timer -= Time.deltaTime;
				
				if(timer <= 0.0)
				{
					Destroy(gameObject);
				}	
			}
		}
	}
	
	function OnCollisionEnter(c : Collision)
	{
		if(exploded) return;
		
		Detonate();
	}
	
	function OnCollisionStay(c : Collision)
	{
		if(exploded) return;
		
		Detonate();
	}
	
	function OnCollisionExit(c : Collision)
	{
		if(exploded) return;
		
		Detonate();
	}
}