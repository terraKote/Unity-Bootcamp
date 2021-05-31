#pragma strict
#pragma implicit
#pragma downcast

class SoldierCamera extends MonoBehaviour
{
	public var target : Transform;
	public var soldier : Transform;
	
	public var speed : Vector2 = new Vector2(135.0, 135.0);
	public var aimSpeed : Vector2 = new Vector2(70.0, 70.0);
	public var maxSpeed : Vector2 = new Vector2(100.0, 100.0);

	public var yMinLimit = -90;
	public var yMaxLimit = 90;
	
	public var normalFOV = 60;
	public var zoomFOV = 30;
	
	public var lerpSpeed = 8.0;
	
	private var distance = 10.0;

	private var x = 0.0;
	public var y = 0.0;
	
	private var camTransform : Transform;
	private var rotation : Quaternion;
	private var position : Vector3;
	private var deltaTime : float;
	private var originalSoldierRotation : Quaternion;
	
	private var soldierController : SoldierController;
	
	public var orbit : boolean;
	
	public var hitLayer : LayerMask;
	
	private var cPos : Vector3;
	
	public var normalDirection : Vector3;
	public var aimDirection : Vector3;
	public var crouchDirection : Vector3;
	public var aimCrouchDirection : Vector3;
	
	public var positionLerp : float;
	
	public var normalHeight : float;
	public var crouchHeight : float;
	public var normalAimHeight : float;
	public var crouchAimHeight : float;
	public var minHeight : float;
	public var maxHeight : float;
	
	public var normalDistance : float;
	public var crouchDistance : float;
	public var normalAimDistance : float;
	public var crouchAimDistance : float;
	public var minDistance : float;
	public var maxDistance : float;
	
	private var targetDistance : float;
	private var camDir : Vector3;
	private var targetHeight : float;
	
	
	public var minShakeSpeed : float;
	public var maxShakeSpeed : float;
	
	public var minShake : float;
	public var maxShake : float = 2.0;
	
	public var minShakeTimes : int;
	public var maxShakeTimes : int;
	
	public var maxShakeDistance : float;
	
	private var shake : boolean;
	private var shakeSpeed : float = 2.0;
	private var cShakePos : float;
	private var shakeTimes : int = 8;
	private var cShake : float;
	private var cShakeSpeed : float;
	private var cShakeTimes : int;
	
	public var radar : Transform;
	public var radarCamera : Transform;

    private var _depthOfFieldEffect : DepthOfField;
	
	function Start () 
	{	
		cShakeTimes = 0;
		cShake = 0.0;
		cShakeSpeed = shakeSpeed;
		
        _depthOfFieldEffect = gameObject.GetComponent("DepthOfField") as DepthOfField;

		if(target == null || soldier == null)
		{ 
			Destroy(this);
			return;
		}
		
		target.parent = null;
		
		camTransform = transform;
		
		var angles = camTransform.eulerAngles;
		x = angles.y;
	    y = angles.x;
	    
	    originalSoldierRotation = soldier.rotation;
	    
	    soldierController = soldier.GetComponent("SoldierController");
	    
	    targetDistance = normalDistance;
	    
	    cPos = soldier.position + new Vector3(0, normalHeight, 0);
	}
	
	function GoToOrbitMode(state : boolean)
	{
		orbit = state;
		
		soldierController.idleTimer = 0.0;
	}
	
	function Update()
	{
		if(GameManager.pause || GameManager.scores) return;
		//if(GameManager.scores) return;

		if(orbit && (Input.GetKeyDown(KeyCode.O) || Input.GetAxis("Horizontal") != 0.0 || Input.GetAxis("Vertical") != 0.0 || soldierController.aim || soldierController.fire))
		{
			GoToOrbitMode(false);
		}
		
		if(!orbit && soldierController.idleTimer > 0.1)
		{
			GoToOrbitMode(true);
		}
	}
	
	function LateUpdate () 
	{
		//if(GameManager.pause || GameManager.scores) return;
		if(GameManager.scores) return;

		deltaTime = Time.deltaTime;
		
		GetInput();
		
		RotateSoldier();

		CameraMovement();

        DepthOfFieldControl();
	}
	
	function CameraMovement()
	{
		if(soldierController.aim)
		{
			(camera.GetComponent(DepthOfField) as DepthOfField).enabled = true;
			camera.fieldOfView = Mathf.Lerp(camera.fieldOfView, zoomFOV, deltaTime * lerpSpeed);
			
			if(soldierController.crouch)
			{
				camDir = (aimCrouchDirection.x * target.forward) + (aimCrouchDirection.z * target.right);
				targetHeight = crouchAimHeight;
				targetDistance = crouchAimDistance;
			}
			else
			{
				camDir = (aimDirection.x * target.forward) + (aimDirection.z * target.right);
				targetHeight = normalAimHeight;
				targetDistance = normalAimDistance;
			}
		}
		else
		{
			(camera.GetComponent(DepthOfField) as DepthOfField).enabled = false;
			camera.fieldOfView = Mathf.Lerp(camera.fieldOfView, normalFOV, deltaTime * lerpSpeed);
			
			if(soldierController.crouch)
			{
				camDir = (crouchDirection.x * target.forward) + (crouchDirection.z * target.right);
				targetHeight = crouchHeight;
				targetDistance = crouchDistance;
			}
			else
			{
				camDir = (normalDirection.x * target.forward) + (normalDirection.z * target.right);
				targetHeight = normalHeight;
				targetDistance = normalDistance;
			}
		}
		
		camDir = camDir.normalized;
		
		HandleCameraShake();
		
		cPos = soldier.position + new Vector3(0, targetHeight, 0);
		
		var hit : RaycastHit;
		if(Physics.Raycast(cPos, camDir, hit, targetDistance + 0.2, hitLayer))
		{
			var t : float = hit.distance - 0.1;
			t -= minDistance;
			t /= (targetDistance - minDistance);

			targetHeight = Mathf.Lerp(maxHeight, targetHeight, Mathf.Clamp(t, 0.0, 1.0));
			cPos = soldier.position + new Vector3(0, targetHeight, 0); 
		}
		
		if(Physics.Raycast(cPos, camDir, hit, targetDistance + 0.2, hitLayer))
		{
			targetDistance = hit.distance - 0.1;
		}
		if(radar != null)
		{
			radar.position = cPos;
			radarCamera.rotation = Quaternion.Euler(90, x, 0);
		}
		
		var lookPoint : Vector3 = cPos;
		lookPoint += (target.right * Vector3.Dot(camDir * targetDistance, target.right));

		camTransform.position = cPos + (camDir * targetDistance);
		camTransform.LookAt(lookPoint);
		
		target.position = cPos;
		target.rotation = Quaternion.Euler(y, x, 0);
	}
	
	function HandleCameraShake()
	{
		if(shake)
		{
			cShake += cShakeSpeed * deltaTime;
			
			if(Mathf.Abs(cShake) > cShakePos)
			{
				cShakeSpeed *= -1.0;
				cShakeTimes++;
				
				if(cShakeTimes >= shakeTimes)
				{
					shake = false;
				}
				
				if(cShake > 0.0)
				{
					cShake = maxShake;
				}
				else
				{
					cShake = -maxShake;
				}
			}
			
			targetHeight += cShake;
		}
	}
	
	function StartShake(distance : float)
	{
		var proximity : float = distance / maxShakeDistance;
		
		if(proximity > 1.0) return;
		
		proximity = Mathf.Clamp(proximity, 0.0, 1.0);
		
		proximity = 1.0 - proximity;
		
		cShakeSpeed = Mathf.Lerp(minShakeSpeed, maxShakeSpeed, proximity);
		shakeTimes = Mathf.Lerp(minShakeTimes, maxShakeTimes, proximity);
		cShakeTimes = 0;
		cShakePos = Mathf.Lerp(minShake, maxShake, proximity);
		
		shake = true;
	}

	function GetInput()
	{
		var a : Vector2 = soldierController.aim ? aimSpeed : speed;
		x += Mathf.Clamp(Input.GetAxis("Mouse X") * a.x, -maxSpeed.x, maxSpeed.x) * deltaTime;
		y -= Mathf.Clamp(Input.GetAxis("Mouse Y") * a.y, -maxSpeed.y, maxSpeed.y) * deltaTime;
		y = ClampAngle(y, yMinLimit, yMaxLimit);
	}

    function DepthOfFieldControl()
    {
        if(_depthOfFieldEffect == null) return;
        if(soldierController == null) return;
        
        if(soldierController.aim && GameQualitySettings.depthOfField)
        {
            if(!_depthOfFieldEffect.enabled)
            {
                _depthOfFieldEffect.enabled = true;
            }
        }
        else
        {
            if(_depthOfFieldEffect.enabled)
            {
                _depthOfFieldEffect.enabled = false;
            }
        }
    }
	
	function RotateSoldier()
	{
		if(!orbit)
			soldierController.targetYRotation = x;
	}
	
	static function ClampAngle(angle : float, min : float, max : float) : float
	{
		if (angle < -360)
		{
			angle += 360;
		}
		
		if (angle > 360)
		{
			angle -= 360;
		}
		
		return Mathf.Clamp (angle, min, max);
	}
}