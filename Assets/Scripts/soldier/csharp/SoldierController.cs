using UnityEngine;

public class SoldierController : MonoBehaviour
{
    public float runSpeed = 4.6f;
    public float runStrafeSpeed = 3.07f;
    public float walkSpeed = 1.22f;
    public float walkStrafeSpeed = 1.22f;
    public float crouchRunSpeed = 5.0f;
    public float crouchRunStrafeSpeed = 5.0f;
    public float crouchWalkSpeed = 1.8f;
    public float crouchWalkStrafeSpeed = 1.8f;

    public float maxRotationSpeed = 540.0f;
    public float minCarDistance;

    static public bool dead;

    [HideInInspector] public bool walk;
    [HideInInspector] public bool crouch;
    [HideInInspector] public bool inAir;
    [HideInInspector] public bool fire;
    [HideInInspector] public bool aim;
    [HideInInspector] public bool reloading;
    [HideInInspector] public string currentWeaponName;
    [HideInInspector] public int currentWeapon;
    [HideInInspector] public bool grounded;
    [HideInInspector] public float targetYRotation;

    // Private variables

    private Transform soldierTransform;
    private CharacterController controller;
    private HeadLookController headLookController;
    private CharacterMotor motor;

    private bool firing;
    private float firingTimer;
    public float idleTimer;

    public Transform enemiesRef;
    public Transform enemiesShootRef;

    static public Transform enemiesReference;
    static public Transform enemiesShootReference;

    [HideInInspector] public Vector3 moveDir;

    private bool _useIK;

    void Awake()
    {
        if (enemiesRef != null) enemiesReference = enemiesRef;
        if (enemiesShootRef != null) enemiesShootReference = enemiesShootRef;
    }

    void Start()
    {
        idleTimer = 0.0f;

        soldierTransform = transform;

        walk = true;
        aim = false;
        reloading = false;

        controller = GetComponent<CharacterController>();
        motor = GetComponent<CharacterMotor>();
    }

    void OnEnable()
    {
        moveDir = Vector3.zero;
        headLookController = GetComponent<HeadLookController>();
        headLookController.enabled = true;
        walk = true;
        aim = false;
        reloading = false;
    }

    void OnDisable()
    {
        moveDir = Vector3.zero;
        headLookController.enabled = false;
        walk = true;
        aim = false;
        reloading = false;
    }

    void Update()
    {
        if (GameManager.pause || GameManager.scores)
        {
            moveDir = Vector3.zero;
            motor.canControl = false;
        }
        else
        {
            GetUserInputs();

            if (!motor.canControl)
            {
                motor.canControl = true;
            }

            if (!dead)
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

        if (moveDir != Vector3.zero)
        {
            idleTimer = 0.0f;
        }

        inAir = !motor.grounded;

        var currentAngle = soldierTransform.localRotation.eulerAngles.y;
        var delta = Mathf.Repeat((targetYRotation - currentAngle), 360);
        if (delta > 180)
        {
            delta -= 360;
        }
        Vector3 localRotation = soldierTransform.localRotation.eulerAngles;
        localRotation.y = Mathf.MoveTowards(currentAngle, currentAngle + delta, Time.deltaTime * maxRotationSpeed);
        soldierTransform.localRotation = Quaternion.Euler(localRotation);
    }

    void GetUserInputs()
    {
        var weaponSystem = GunManager.GetInstance();

        if (!weaponSystem)
        {
            Debug.LogError("No GunManager found!");
            return;
        }

        //Check if the user if firing the weapon
        fire = Input.GetButton("Fire1") && weaponSystem.currentGun.freeToShoot && !dead && !inAir;

        //Check if the user is aiming the weapon
        aim = Input.GetButton("Fire2") && !dead;

        idleTimer += Time.deltaTime;

        if (aim || fire)
        {
            firingTimer -= Time.deltaTime;
            idleTimer = 0.0f;
        }
        else
        {
            firingTimer = 0.3f;
        }

        firing = (firingTimer <= 0.0 && fire);

        if (weaponSystem && weaponSystem.currentGun != null)
        {
            weaponSystem.currentGun.fire = firing;
            reloading = weaponSystem.currentGun.reloading;
            currentWeaponName = weaponSystem.currentGun.gunName;
            currentWeapon = weaponSystem.currentWeapon;
        }

        //Check if the user wants the soldier to crouch
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            crouch = !crouch;
            idleTimer = 0.0f;
        }

        crouch |= dead;

        //Check if the user wants the soldier to walk
        walk = (!Input.GetKey(KeyCode.LeftShift) && !dead) || moveDir == Vector3.zero || crouch;
    }
}