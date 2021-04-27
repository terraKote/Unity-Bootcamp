using UnityEngine;
using System.Collections;

namespace Bootcamp.Soldier
{
    /// <summary>
    /// Class responsible for processing all soldier's _animations
    /// </summary>
    public class SoldierAnimations : MonoBehaviour
    {
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

        private Animation _animation;

        void OnEnable()
        {
            soldier = gameObject.GetComponent<SoldierController>();
            motor = gameObject.GetComponent<CharacterMotor>();
            _animation = GetComponent<Animation>();

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

            // Method that computes the idle timer to control IDLE and RELAXEDWALK _animations
            if (aim || fire || crouch || !walk || (moveDir != Vector3.zero && moveDir.normalized.z < 0.8))
                lastNonRelaxedTime = Time.time;

            if (Time.time > lastNonRelaxedTime + 2)
                relaxedWeight = CrossFadeUp(relaxedWeight, 1.0f);
            else
                relaxedWeight = CrossFadeDown(relaxedWeight, 0.3f);
            var nonRelaxedWeight = 1 - relaxedWeight;

            _animation["NormalGroup"].weight = uprightWeight * nonAimWeight * groundedWeight * nonRelaxedWeight;
            _animation["RelaxedGroup"].weight = uprightWeight * nonAimWeight * groundedWeight * relaxedWeight;
            _animation["CrouchGroup"].weight = crouchWeight * nonAimWeight * groundedWeight;

            _animation["NormalAimGroup"].weight = uprightWeight * aimButNotFireWeight * groundedWeight;
            _animation["CrouchAimGroup"].weight = crouchWeight * aimButNotFireWeight * groundedWeight;

            _animation["NormalFireGroup"].weight = uprightWeight * fireWeight * groundedWeight;
            _animation["CrouchFireGroup"].weight = crouchWeight * fireWeight * groundedWeight;

            var runningJump = Mathf.Clamp01(Vector3.Dot(motor.movement.velocity, transform.forward) / 2.0);
            _animation["StandingJump"].weight = (1 - groundedWeight) * (1 - runningJump);
            _animation["RunJump"].weight = (1 - groundedWeight) * runningJump;
            if (inAir)
            {
                //var normalizedTime = Mathf.Lerp(0.15, 0.65, Mathf.InverseLerp(jumpAnimStretch, -jumpAnimStretch, motor.movement.velocity.y));
                var normalizedTime = Mathf.InverseLerp(jumpAnimStretch, -jumpAnimStretch, motor.movement.velocity.y);
                _animation["StandingJump"].normalizedTime = normalizedTime;
                _animation["RunJump"].normalizedTime = normalizedTime;
            }

            //Debug.Log("motor.movement.velocity.y="+motor.movement.velocity.y+" - "+_animation["StandingJump"].normalizedTime);

            var locomotionWeight = 1.0f;
            locomotionWeight *= 1 - _animation["Crouch"].weight;
            locomotionWeight *= 1 - _animation["CrouchAim"].weight;
            locomotionWeight *= 1 - _animation["CrouchFire"].weight;

            _animation["LocomotionSystem"].weight = locomotionWeight;

            // Aiming up/down
            var aimDir = (aimTarget.position - aimPivot.position).normalized;
            var targetAngle = Mathf.Asin(aimDir.y) * Mathf.Rad2Deg;
            aimAngleY = Mathf.Lerp(aimAngleY, targetAngle, Time.deltaTime * 8);


            // Use HeadLookController when not aiming/firing
            headLookController.effect = nonAimWeight;

            // Use additive _animations for aiming when aiming and firing
            _animation["StandingAimUp"].weight = uprightWeight * aimWeight;
            _animation["StandingAimDown"].weight = uprightWeight * aimWeight;
            _animation["CrouchAimUp"].weight = crouchWeight * aimWeight;
            _animation["CrouchAimDown"].weight = crouchWeight * aimWeight;

            // Set time of _animations according to current vertical aiming angle
            _animation["StandingAimUp"].time = Mathf.Clamp01(aimAngleY / 90);
            _animation["StandingAimDown"].time = Mathf.Clamp01(-aimAngleY / 90);
            _animation["CrouchAimUp"].time = Mathf.Clamp01(aimAngleY / 90);
            _animation["CrouchAimDown"].time = Mathf.Clamp01(-aimAngleY / 90);


            if (reloading)
            {
                _animation.CrossFade("Reload" + soldier.currentWeaponName, 0.1);
            }

            if (currentWeapon > 0 && fire)
            {
                _animation.CrossFade("FireM203");
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
            moveDir = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

            inAir = !GetComponent("CharacterController").isGrounded;
        }

        //Method that initializes _animations properties
        void SetAnimationProperties()
        {
            _animation.AddClip(_animation["StandingReloadM4"].clip, "ReloadM4");
            _animation["ReloadM4"].AddMixingTransform(transform.Find("Pelvis/Spine1/Spine2"));
            _animation["ReloadM4"].wrapMode = WrapMode.Clamp;
            _animation["ReloadM4"].layer = 3;
            _animation["ReloadM4"].time = 0;
            _animation["ReloadM4"].speed = 1.0f;

            _animation.AddClip(_animation["StandingReloadRPG1"].clip, "ReloadM203");
            _animation["ReloadM203"].AddMixingTransform(transform.Find("Pelvis/Spine1/Spine2"));
            _animation["ReloadM203"].wrapMode = WrapMode.Clamp;
            _animation["ReloadM203"].layer = 3;
            _animation["ReloadM203"].time = 0;
            _animation["ReloadM203"].speed = 1.0f;

            _animation.AddClip(_animation["StandingFireRPG"].clip, "FireM203");
            _animation["FireM203"].AddMixingTransform(transform.Find("Pelvis/Spine1/Spine2"));
            _animation["FireM203"].wrapMode = WrapMode.Clamp;
            _animation["FireM203"].layer = 3;
            _animation["FireM203"].time = 0;
            _animation["FireM203"].speed = 1.0f;

            _animation["StandingJump"].layer = 2;
            _animation["StandingJump"].weight = 0;
            _animation["StandingJump"].speed = 0;
            _animation["StandingJump"].enabled = true;
            _animation["RunJump"].layer = 2;
            _animation["RunJump"].weight = 0;
            _animation["RunJump"].speed = 0;
            _animation["RunJump"].enabled = true;
            _animation.SyncLayer(2);

            SetupAdditiveAiming("StandingAimUp");
            SetupAdditiveAiming("StandingAimDown");
            SetupAdditiveAiming("CrouchAimUp");
            SetupAdditiveAiming("CrouchAimDown");
        }

        void SetupAdditiveAiming(string anim)
        {
            _animation[anim].blendMode = AnimationBlendMode.Additive;
            _animation[anim].enabled = true;
            _animation[anim].weight = 1;
            _animation[anim].layer = 4;
            _animation[anim].time = 0;
            _animation[anim].speed = 0;
        }
    }
}