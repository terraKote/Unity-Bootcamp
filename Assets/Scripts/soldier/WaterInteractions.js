class WaterInteractions extends MonoBehaviour
{
	public var soldier : Transform;
	private var controller : SoldierController;
	
	private var emitMovement : boolean;
	public var movementContainer : Transform;
	public var movementEmitters : ParticleEmitter[];
	
	private var emitStand : boolean;
	public var standingContainer : Transform;
	public var standingEmitters : ParticleEmitter[];
	
	public var jumpHitDistance : float = 1.4;
	public var jumpParticle : GameObject;
	//public var jumpEmitters : ParticleEmitter[];
	
	private var thisT : Transform;
	
	public var affectedLayers : LayerMask;
	private var hitInfo : RaycastHit;
	private var jumped : boolean;
	private var emittedHit : boolean;
	private var jumpTimer : float;
	
	private var runSpeed : float;
	private var runStrafeSpeed : float;
	private var walkSpeed : float;
	private var walkStrafeSpeed : float;
	private var crouchRunSpeed : float;
	private var crouchRunStrafeSpeed : float;
	private var crouchWalkSpeed : float;
	private var crouchWalkStrafeSpeed : float;
	private var currentAmount : float;
	
	public var depthToReduceSpeed : float = 0.9;
	public var speedUnderWater : float = 0.8;
	
	public var waterImpactSound : AudioClip;
	public var waterJumpingSound : AudioClip;
	
	public var fadeSpeed : float = 0.6;
	
	private var lastPositon : Vector3;
	private var currentPosition : Vector3;
	
	function Start()
	{
		controller = soldier.GetComponent("SoldierController");
		
		currentAmount = 1.0;
		
		runSpeed = controller.runSpeed;
		runStrafeSpeed = controller.runStrafeSpeed;
		walkSpeed = controller.walkSpeed;
		walkStrafeSpeed = controller.walkStrafeSpeed;
		crouchRunSpeed = controller.crouchRunSpeed;
		crouchRunStrafeSpeed = controller.crouchRunStrafeSpeed;
		crouchWalkSpeed = controller.crouchWalkSpeed;
		crouchWalkStrafeSpeed = controller.crouchWalkStrafeSpeed;
		
		jumpTimer = 0.0;	 
		emitMovement = false;
		jumped = false;
		var i : int;

		movementContainer.parent = null;
		movementContainer.audio.volume = 0.0;
		for(i = 0; i < movementEmitters.Length; i++)
		{
			movementEmitters[i].emit = false;
		}
		
		emitStand = false;
		
		standingContainer.parent = null;
		for(i = 0; i < standingEmitters.Length; i++)
		{
			standingEmitters[i].emit = false;
		}
		
		thisT = transform;
	}
	
	function Update()
	{
		if(!soldier.gameObject.active) return;
		
		lastPositon = currentPosition;
		currentPosition = new Vector3(soldier.position.x, 0.0, soldier.position.z);
		
		var dir = (currentPosition - lastPositon).normalized;
		
		thisT.position = soldier.position + new Vector3(0, 1.8, 0);
		
		if(!GameManager.pause)
		{
			jumped = Input.GetButtonDown("Jump");
		}
		
		if(!controller.inAir)
		{
			jumpTimer = 0.0;
			emittedHit = false;
		}
		else
		{
			jumpTimer += Time.deltaTime;
		}
		
		if(Physics.Raycast(thisT.position, -Vector3.up, hitInfo, Mathf.Infinity, affectedLayers))
		{
			if(hitInfo.collider.tag == "water")
			{
				if(hitInfo.distance < depthToReduceSpeed)
				{
					ChangeSpeed(speedUnderWater);
				}
				else 
				{
					ChangeSpeed(1.0);
				}
				
				if(controller.inAir)
				{
					if(hitInfo.distance < jumpHitDistance && !emittedHit && jumpTimer > 0.5)
					{
						emittedHit = true;
						EmitJumpParticles(true, hitInfo);
						ChangeMovementState(false);
						ChangeStandingState(false);
					}
				}
				else
				{
					if(jumped)
					{
						EmitJumpParticles(false, hitInfo);
						ChangeMovementState(false);
						ChangeStandingState(false);
					}
					else if(!controller.inAir)
					{
						if(dir.magnitude > 0.2)
						{
							movementContainer.position = hitInfo.point;
							ChangeMovementState(true);
							ChangeStandingState(false);
						}
						else
						{
							standingContainer.position = hitInfo.point;
							ChangeMovementState(false);
							ChangeStandingState(true);
						}
					}
				}
			}
			else 
			{
				ChangeSpeed(1.0);
				ChangeMovementState(false);
				ChangeStandingState(false);
			}
		}
		else
		{
			ChangeSpeed(1.0);
			ChangeMovementState(false);
			ChangeStandingState(false);
		}
		
		if(emitMovement)
		{
			if(movementContainer.audio.volume < 0.65)
			{
				if(!movementContainer.audio.isPlaying) movementContainer.audio.Play();
				
				movementContainer.audio.volume += Time.deltaTime * fadeSpeed;
			}
			else
			{
				movementContainer.audio.volume = 0.65;
			}
		}
		else
		{
			if(movementContainer.audio.isPlaying)
			{
				if(movementContainer.audio.volume > 0.0)
				{
					movementContainer.audio.volume -= Time.deltaTime * fadeSpeed * 2.0;
				}
				else
				{
					movementContainer.audio.Pause();
				}
			}
		}
	}
	
	function ChangeSpeed(amount : float)
	{
		if(currentAmount == amount) return;
		
		currentAmount = amount;
		
		controller.runSpeed = runSpeed * amount;
		controller.runStrafeSpeed = runStrafeSpeed * amount;
		controller.walkSpeed = walkSpeed * amount;
		controller.walkStrafeSpeed = walkStrafeSpeed * amount;
		controller.crouchRunSpeed = crouchRunSpeed * amount;
		controller.crouchRunStrafeSpeed = crouchRunStrafeSpeed * amount;
		controller.crouchWalkSpeed = crouchWalkSpeed * amount;
		controller.crouchWalkStrafeSpeed = crouchWalkStrafeSpeed * amount;
	}
	
	function EmitJumpParticles(b : boolean, hitInfo : RaycastHit)
	{
		var go : GameObject = GameObject.Instantiate(jumpParticle, hitInfo.point, Quaternion.identity) as GameObject;
						
		if(go.audio != null)
		{
			if(b)
			{
				go.audio.PlayOneShot(waterImpactSound, 0.5);
			}
			else
			{
				go.audio.PlayOneShot(waterJumpingSound, 1);
			}
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
	}
	
	function ChangeMovementState(b : boolean)
	{
		if(b == emitMovement) return;
		
		emitMovement = b;
		
		for(var i : int = 0; i < movementEmitters.Length; i++)
		{
			movementEmitters[i].emit = b;
		}
	}
	
	function ChangeStandingState(b : boolean)
	{
		if(b == emitStand) return;
		
		emitStand = b;
		
		for(var i : int = 0; i < standingEmitters.Length; i++)
		{
			standingEmitters[i].emit = b;
		}
	}
}