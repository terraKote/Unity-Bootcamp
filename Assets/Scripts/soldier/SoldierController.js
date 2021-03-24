#pragma strict
#pragma implicit
#pragma downcast

class SoldierController extends MonoBehaviour
{
	// Public variables shown in inspector
	
	public var runSpeed : float = 4.6;
	public var runStrafeSpeed : float = 3.07;
	public var walkSpeed : float = 1.22;
	public var walkStrafeSpeed : float = 1.22;
	public var crouchRunSpeed : float = 5;
	public var crouchRunStrafeSpeed : float = 5;
	public var crouchWalkSpeed : float = 1.8;
	public var crouchWalkStrafeSpeed : float = 1.8;

    public var radarObject : GameObject;
	
	public var maxRotationSpeed : float = 540;
	
	public var weaponSystem : GunManager;
	public var minCarDistance : float;
	
	static public var dead : boolean;
	
	// Public variables hidden in inspector
	
	@HideInInspector
	public var walk : boolean;
	
	@HideInInspector
	public var crouch : boolean;
	
	@HideInInspector
	public var inAir : boolean;
	
	@HideInInspector
	public var fire : boolean;
	
	@HideInInspector
	public var aim : boolean;
	
	@HideInInspector
	public var reloading : boolean;
	
	@HideInInspector
	public var currentWeaponName : String;
	
	@HideInInspector
	public var currentWeapon : int;
	
	@HideInInspector
	public var grounded : boolean;
	
	@HideInInspector
	public var targetYRotation : float;
	
	// Private variables
	
	private var soldierTransform : Transform;
	private var controller : CharacterController;
	private var headLookController : HeadLookController;
	private var motor : CharacterMotor;
	
	private var firing : boolean;
	private var firingTimer : float;
	public var idleTimer : float;
	
	public var enemiesRef : Transform;
	public var enemiesShootRef : Transform;
	
	static public var enemiesReference : Transform;
	static public var enemiesShootReference : Transform;
	
	@HideInInspector
	public var moveDir : Vector3;
    
    private var _useIK : boolean;

	function Awake()
	{   
		if(enemiesRef != null) enemiesReference = enemiesRef;
		if(enemiesShootRef != null) enemiesShootReference = enemiesShootRef;
	}
	
	function Start()
	{
		idleTimer = 0.0;

		soldierTransform = transform;

		walk = true;
		aim = false;
		reloading = false;

		controller = gameObject.GetComponent("CharacterController");
		motor = gameObject.GetComponent("CharacterMotor");
	}
	
	function OnEnable()
	{
        if(radarObject != null)
        {
            radarObject.SetActiveRecursively(true);
        }

        moveDir = Vector3.zero;
		headLookController = gameObject.GetComponent("HeadLookController");
		headLookController.enabled = true;
        walk = true;
		aim = false;
		reloading = false;
	}
	
	function OnDisable()
	{
        if(radarObject != null)
        {
            radarObject.SetActiveRecursively(false);
        }

        moveDir = Vector3.zero;
		headLookController.enabled = false;
        walk = true;
		aim = false;
		reloading = false;
	}
	
	function Update()
	{
		if(GameManager.pause || GameManager.scores)
		{
			moveDir = Vector3.zero;
			motor.canControl = false;
		}
		else
		{
			GetUserInputs();
			
			if(!motor.canControl)
			{
				motor.canControl = true;
			}
			
			if(!dead)
			{
				moveDir = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
			}
			else
			{
				moveDir = Vector3.zero;
				motor.canControl = false;
			}
		}
		
		//Check the soldier move direction
		if (moveDir.sqrMagnitude > 1)
			moveDir = moveDir.normalized;
		
		motor.inputMoveDirection = transform.TransformDirection(moveDir);
		motor.inputJump = Input.GetButton("Jump") && !crouch;
		
		motor.movement.maxForwardSpeed = ((walk) ? ((crouch) ? crouchWalkSpeed : walkSpeed) : ((crouch) ? crouchRunSpeed : runSpeed));
		motor.movement.maxBackwardsSpeed = motor.movement.maxForwardSpeed;
		motor.movement.maxSidewaysSpeed = ((walk) ? ((crouch) ? crouchWalkStrafeSpeed : walkStrafeSpeed) : ((crouch) ? crouchRunStrafeSpeed : runStrafeSpeed));
		
		if(moveDir != Vector3.zero)
        {
			idleTimer = 0.0;
        }
		
		inAir = !motor.grounded;
		
		var currentAngle = soldierTransform.localRotation.eulerAngles.y;
		var delta = Mathf.Repeat ((targetYRotation - currentAngle), 360);
		if (delta > 180)
			delta -= 360;
		soldierTransform.localRotation.eulerAngles.y = Mathf.MoveTowards(currentAngle, currentAngle + delta, Time.deltaTime * maxRotationSpeed);
	}
	
	function GetUserInputs()
	{
		//Check if the user if firing the weapon
		fire = Input.GetButton("Fire1") && weaponSystem.currentGun.freeToShoot && !dead && !inAir;
		
		//Check if the user is aiming the weapon
		aim = Input.GetButton("Fire2") && !dead;
		
		idleTimer += Time.deltaTime;
		
		if(aim || fire)
		{
			firingTimer -= Time.deltaTime;
			idleTimer = 0.0;
		}
		else
		{
			firingTimer = 0.3;
		}
		
		firing = (firingTimer <= 0.0 && fire);
		
		if(weaponSystem.currentGun != null)
		{
			weaponSystem.currentGun.fire = firing;
			reloading = weaponSystem.currentGun.reloading;
			currentWeaponName = weaponSystem.currentGun.gunName;
			currentWeapon = weaponSystem.currentWeapon;
		}
		
		//Check if the user wants the soldier to crouch
		if(Input.GetKeyDown(KeyCode.LeftControl))
		{
			crouch = !crouch;
			idleTimer = 0.0;
		}
		
		crouch |= dead;
		
		//Check if the user wants the soldier to walk
		walk = (!Input.GetKey(KeyCode.LeftShift) && !dead) || moveDir == Vector3.zero || crouch;
	}
}