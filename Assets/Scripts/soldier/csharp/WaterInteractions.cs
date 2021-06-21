using UnityEngine;
using System.Collections;

public class WaterInteractions : PausableBehaviour
{
    public Transform soldier;
    private SoldierController controller;

    private bool emitMovement;
    public Transform movementContainer;
    public ParticleEmitter[] movementEmitters;

    private bool emitStand;
    public Transform standingContainer;
    public ParticleEmitter[] standingEmitters;

    public float jumpHitDistance = 1.4f;
    public GameObject jumpParticle;

    private Transform thisT;

    public LayerMask affectedLayers;
    private RaycastHit hitInfo;
    private bool jumped;
    private bool emittedHit;
    private float jumpTimer;

    private float runSpeed;
    private float runStrafeSpeed;
    private float walkSpeed;
    private float walkStrafeSpeed;
    private float crouchRunSpeed;
    private float crouchRunStrafeSpeed;
    private float crouchWalkSpeed;
    private float crouchWalkStrafeSpeed;
    private float currentAmount;

    public float depthToReduceSpeed = 0.9f;
    public float speedUnderWater = 0.8f;

    public AudioClip waterImpactSound;
    public AudioClip waterJumpingSound;

    public float fadeSpeed = 0.6f;

    private Vector3 lastPositon;
    private Vector3 currentPosition;

    void Start()
    {
        controller = soldier.GetComponent<SoldierController>();

        currentAmount = 1.0f;

        runSpeed = controller.runSpeed;
        runStrafeSpeed = controller.runStrafeSpeed;
        walkSpeed = controller.walkSpeed;
        walkStrafeSpeed = controller.walkStrafeSpeed;
        crouchRunSpeed = controller.crouchRunSpeed;
        crouchRunStrafeSpeed = controller.crouchRunStrafeSpeed;
        crouchWalkSpeed = controller.crouchWalkSpeed;
        crouchWalkStrafeSpeed = controller.crouchWalkStrafeSpeed;

        jumpTimer = 0.0f;
        emitMovement = false;
        jumped = false;
        int i;

        movementContainer.parent = null;
        movementContainer.GetComponent<AudioSource>().volume = 0.0f;
        for (i = 0; i < movementEmitters.Length; i++)
        {
            movementEmitters[i].emit = false;
        }

        emitStand = false;

        standingContainer.parent = null;
        for (i = 0; i < standingEmitters.Length; i++)
        {
            standingEmitters[i].emit = false;
        }

        thisT = transform;
    }

    void Update()
    {
        if (!soldier.gameObject.active) return;

        lastPositon = currentPosition;
        currentPosition = new Vector3(soldier.position.x, 0.0f, soldier.position.z);

        var dir = (currentPosition - lastPositon).normalized;

        thisT.position = soldier.position + new Vector3(0, 1.8f, 0);

        if (!IsPaused)
        {
            jumped = Input.GetButtonDown("Jump");
        }

        if (!controller.inAir)
        {
            jumpTimer = 0.0f;
            emittedHit = false;
        }
        else
        {
            jumpTimer += Time.deltaTime;
        }

        if (Physics.Raycast(thisT.position, -Vector3.up, out hitInfo, Mathf.Infinity, affectedLayers))
        {
            if (hitInfo.collider.tag == "water")
            {
                if (hitInfo.distance < depthToReduceSpeed)
                {
                    ChangeSpeed(speedUnderWater);
                }
                else
                {
                    ChangeSpeed(1.0f);
                }

                if (controller.inAir)
                {
                    if (hitInfo.distance < jumpHitDistance && !emittedHit && jumpTimer > 0.5)
                    {
                        emittedHit = true;
                        EmitJumpParticles(true, hitInfo);
                        ChangeMovementState(false);
                        ChangeStandingState(false);
                    }
                }
                else
                {
                    if (jumped)
                    {
                        EmitJumpParticles(false, hitInfo);
                        ChangeMovementState(false);
                        ChangeStandingState(false);
                    }
                    else if (!controller.inAir)
                    {
                        if (dir.magnitude > 0.2f)
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
                ChangeSpeed(1.0f);
                ChangeMovementState(false);
                ChangeStandingState(false);
            }
        }
        else
        {
            ChangeSpeed(1.0f);
            ChangeMovementState(false);
            ChangeStandingState(false);
        }

        if (emitMovement)
        {
            if (movementContainer.GetComponent<AudioSource>().volume < 0.65f)
            {
                if (!movementContainer.GetComponent<AudioSource>().isPlaying) movementContainer.GetComponent<AudioSource>().Play();

                movementContainer.GetComponent<AudioSource>().volume += Time.deltaTime * fadeSpeed;
            }
            else
            {
                movementContainer.GetComponent<AudioSource>().volume = 0.65f;
            }
        }
        else
        {
            if (movementContainer.GetComponent<AudioSource>().isPlaying)
            {
                if (movementContainer.GetComponent<AudioSource>().volume > 0.0)
                {
                    movementContainer.GetComponent<AudioSource>().volume -= Time.deltaTime * fadeSpeed * 2.0f;
                }
                else
                {
                    movementContainer.GetComponent<AudioSource>().Pause();
                }
            }
        }
    }

    void ChangeSpeed(float amount)
    {
        if (currentAmount == amount) return;

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

    void EmitJumpParticles(bool b, RaycastHit hitInfo)
    {
        var go = Instantiate(jumpParticle, hitInfo.point, Quaternion.identity) as GameObject;

        if (go.GetComponent<AudioSource>() != null)
        {
            if (b)
            {
                go.GetComponent<AudioSource>().PlayOneShot(waterImpactSound, 0.5f);
            }
            else
            {
                go.GetComponent<AudioSource>().PlayOneShot(waterJumpingSound, 1);
            }
        }

        ParticleEmitter emitter;
        for (int i = 0; i < go.transform.childCount; i++)
        {
            emitter = go.transform.GetChild(i).GetComponent<ParticleEmitter>();

            if (emitter == null) continue;

            emitter.emit = false;
            emitter.Emit();
        }

        AutoDestroy aux = go.AddComponent<AutoDestroy>();
        aux.time = 2f;
    }

    void ChangeMovementState(bool b)
    {
        if (b == emitMovement) return;

        emitMovement = b;

        for (int i = 0; i < movementEmitters.Length; i++)
        {
            movementEmitters[i].emit = b;
        }
    }

    void ChangeStandingState(bool b)
    {
        if (b == emitStand) return;

        emitStand = b;

        for (int i = 0; i < standingEmitters.Length; i++)
        {
            standingEmitters[i].emit = b;
        }
    }
}
