using UnityEngine;
using System.Collections;

public class SoldierAnimations : MonoBehaviour
{
    [SerializeField] private PlayerInputService playerInputService;

    public Transform aimPivot;
    public Transform aimTarget;
    public HeadLookController headLookController;

    public float jumpAnimStretch = 5;
    public float jumpLandCrouchAmount = 1.6f;

    private SoldierController soldier;
    private CharacterMotor motor;
    private float lastNonRelaxedTime;
    private float aimAngleY = 0.0f;

    private bool aim;
    private bool fire;
    private bool walk;
    private bool crouch;
    private Vector3 moveDir;
    private bool reloading;
    private int currentWeapon;
    private bool inAir;

    private float groundedWeight = 1;
    private float crouchWeight = 0;
    private float relaxedWeight = 1;
    private float aimWeight = 0;
    private float fireWeight = 0;

    void OnEnable()
    {
        soldier = gameObject.GetComponent<SoldierController>();
        motor = gameObject.GetComponent<CharacterMotor>();

        SetAnimationProperties();
    }

    void Update()
    {
        CheckSoldierState();

        if (crouch)
            crouchWeight = CrossFadeUp(crouchWeight, 0.4f);
        else if (inAir && jumpLandCrouchAmount > 0)
            crouchWeight = CrossFadeUp(crouchWeight, 1 / jumpLandCrouchAmount);
        else
            crouchWeight = CrossFadeDown(crouchWeight, 0.45f);
        var uprightWeight = 1 - crouchWeight;

        if (fire)
        {
            aimWeight = CrossFadeUp(aimWeight, 0.2f);
            fireWeight = CrossFadeUp(fireWeight, 0.2f);
        }
        else if (aim)
        {
            aimWeight = CrossFadeUp(aimWeight, 0.3f);
            fireWeight = CrossFadeDown(fireWeight, 0.3f);
        }
        else
        {
            aimWeight = CrossFadeDown(aimWeight, 0.5f);
            fireWeight = CrossFadeDown(fireWeight, 0.5f);
        }
        var nonAimWeight = (1 - aimWeight);
        var aimButNotFireWeight = aimWeight - fireWeight;

        if (inAir)
            groundedWeight = CrossFadeDown(groundedWeight, 0.1f);
        else
            groundedWeight = CrossFadeUp(groundedWeight, 0.2f);

        // Method that computes the idle timer to control IDLE and RELAXEDWALK animations
        if (aim || fire || crouch || !walk || (moveDir != Vector3.zero && moveDir.normalized.z < 0.8))
            lastNonRelaxedTime = Time.time;

        if (Time.time > lastNonRelaxedTime + 2)
            relaxedWeight = CrossFadeUp(relaxedWeight, 1.0f);
        else
            relaxedWeight = CrossFadeDown(relaxedWeight, 0.3f);
        var nonRelaxedWeight = 1 - relaxedWeight;

        GetComponent<Animation>()["NormalGroup"].weight = uprightWeight * nonAimWeight * groundedWeight * nonRelaxedWeight;
        GetComponent<Animation>()["RelaxedGroup"].weight = uprightWeight * nonAimWeight * groundedWeight * relaxedWeight;
        GetComponent<Animation>()["CrouchGroup"].weight = crouchWeight * nonAimWeight * groundedWeight;

        GetComponent<Animation>()["NormalAimGroup"].weight = uprightWeight * aimButNotFireWeight * groundedWeight;
        GetComponent<Animation>()["CrouchAimGroup"].weight = crouchWeight * aimButNotFireWeight * groundedWeight;

        GetComponent<Animation>()["NormalFireGroup"].weight = uprightWeight * fireWeight * groundedWeight;
        GetComponent<Animation>()["CrouchFireGroup"].weight = crouchWeight * fireWeight * groundedWeight;

        var runningJump = Mathf.Clamp01(Vector3.Dot(motor.movement.velocity, transform.forward) / 2.0f);
        GetComponent<Animation>()["StandingJump"].weight = (1 - groundedWeight) * (1 - runningJump);
        GetComponent<Animation>()["RunJump"].weight = (1 - groundedWeight) * runningJump;
        if (inAir)
        {
            //var normalizedTime = Mathf.Lerp(0.15, 0.65, Mathf.InverseLerp(jumpAnimStretch, -jumpAnimStretch, motor.movement.velocity.y));
            var normalizedTime = Mathf.InverseLerp(jumpAnimStretch, -jumpAnimStretch, motor.movement.velocity.y);
            GetComponent<Animation>()["StandingJump"].normalizedTime = normalizedTime;
            GetComponent<Animation>()["RunJump"].normalizedTime = normalizedTime;
        }

        //Debug.Log("motor.movement.velocity.y="+motor.movement.velocity.y+" - "+animation["StandingJump"].normalizedTime);

        float locomotionWeight = 1;
        locomotionWeight *= 1 - GetComponent<Animation>()["Crouch"].weight;
        locomotionWeight *= 1 - GetComponent<Animation>()["CrouchAim"].weight;
        locomotionWeight *= 1 - GetComponent<Animation>()["CrouchFire"].weight;

        GetComponent<Animation>()["LocomotionSystem"].weight = locomotionWeight;

        // Aiming up/down
        var aimDir = (aimTarget.position - aimPivot.position).normalized;
        var targetAngle = Mathf.Asin(aimDir.y) * Mathf.Rad2Deg;
        aimAngleY = Mathf.Lerp(aimAngleY, targetAngle, Time.deltaTime * 8);


        // Use HeadLookController when not aiming/firing
        headLookController.effect = nonAimWeight;

        // Use additive animations for aiming when aiming and firing
        GetComponent<Animation>()["StandingAimUp"].weight = uprightWeight * aimWeight;
        GetComponent<Animation>()["StandingAimDown"].weight = uprightWeight * aimWeight;
        GetComponent<Animation>()["CrouchAimUp"].weight = crouchWeight * aimWeight;
        GetComponent<Animation>()["CrouchAimDown"].weight = crouchWeight * aimWeight;

        // Set time of animations according to current vertical aiming angle
        GetComponent<Animation>()["StandingAimUp"].time = Mathf.Clamp01(aimAngleY / 90);
        GetComponent<Animation>()["StandingAimDown"].time = Mathf.Clamp01(-aimAngleY / 90);
        GetComponent<Animation>()["CrouchAimUp"].time = Mathf.Clamp01(aimAngleY / 90);
        GetComponent<Animation>()["CrouchAimDown"].time = Mathf.Clamp01(-aimAngleY / 90);


        if (reloading)
        {
            GetComponent<Animation>().CrossFade("Reload" + soldier.currentWeaponName, 0.1f);
        }

        if (currentWeapon > 0 && fire)
        {
            GetComponent<Animation>().CrossFade("FireM203");
        }
    }

    float CrossFadeUp(float weight, float fadeTime)
    {
        return Mathf.Clamp01(weight + Time.deltaTime / fadeTime);
    }

    float CrossFadeDown(float weight, float fadeTime)
    {
        return Mathf.Clamp01(weight - Time.deltaTime / fadeTime);
    }

    void CheckSoldierState()
    {
        aim = soldier.aim;
        fire = soldier.fire;
        walk = soldier.walk;
        crouch = soldier.crouch;
        reloading = soldier.reloading;
        currentWeapon = soldier.currentWeapon;
        moveDir = playerInputService.moveDirection;

        inAir = !GetComponent<CharacterController>().isGrounded;
    }

    //Method that initializes animations properties
    void SetAnimationProperties()
    {
        GetComponent<Animation>().AddClip(GetComponent<Animation>()["StandingReloadM4"].clip, "ReloadM4");
        GetComponent<Animation>()["ReloadM4"].AddMixingTransform(transform.Find("Pelvis/Spine1/Spine2"));
        GetComponent<Animation>()["ReloadM4"].wrapMode = WrapMode.Clamp;
        GetComponent<Animation>()["ReloadM4"].layer = 3;
        GetComponent<Animation>()["ReloadM4"].time = 0;
        GetComponent<Animation>()["ReloadM4"].speed = 1.0f;

        GetComponent<Animation>().AddClip(GetComponent<Animation>()["StandingReloadRPG1"].clip, "ReloadM203");
        GetComponent<Animation>()["ReloadM203"].AddMixingTransform(transform.Find("Pelvis/Spine1/Spine2"));
        GetComponent<Animation>()["ReloadM203"].wrapMode = WrapMode.Clamp;
        GetComponent<Animation>()["ReloadM203"].layer = 3;
        GetComponent<Animation>()["ReloadM203"].time = 0;
        GetComponent<Animation>()["ReloadM203"].speed = 1.0f;

        GetComponent<Animation>().AddClip(GetComponent<Animation>()["StandingFireRPG"].clip, "FireM203");
        GetComponent<Animation>()["FireM203"].AddMixingTransform(transform.Find("Pelvis/Spine1/Spine2"));
        GetComponent<Animation>()["FireM203"].wrapMode = WrapMode.Clamp;
        GetComponent<Animation>()["FireM203"].layer = 3;
        GetComponent<Animation>()["FireM203"].time = 0;
        GetComponent<Animation>()["FireM203"].speed = 1.0f;

        GetComponent<Animation>()["StandingJump"].layer = 2;
        GetComponent<Animation>()["StandingJump"].weight = 0;
        GetComponent<Animation>()["StandingJump"].speed = 0;
        GetComponent<Animation>()["StandingJump"].enabled = true;
        GetComponent<Animation>()["RunJump"].layer = 2;
        GetComponent<Animation>()["RunJump"].weight = 0;
        GetComponent<Animation>()["RunJump"].speed = 0;
        GetComponent<Animation>()["RunJump"].enabled = true;
        GetComponent<Animation>().SyncLayer(2);

        SetupAdditiveAiming("StandingAimUp");
        SetupAdditiveAiming("StandingAimDown");
        SetupAdditiveAiming("CrouchAimUp");
        SetupAdditiveAiming("CrouchAimDown");
    }

    void SetupAdditiveAiming(string anim)
    {
        GetComponent<Animation>()[anim].blendMode = AnimationBlendMode.Additive;
        GetComponent<Animation>()[anim].enabled = true;
        GetComponent<Animation>()[anim].weight = 1;
        GetComponent<Animation>()[anim].layer = 4;
        GetComponent<Animation>()[anim].time = 0;
        GetComponent<Animation>()[anim].speed = 0;
    }
}
