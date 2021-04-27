using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class CharacterMotorMovement
{
    // The maximum horizontal speed when moving.
    public float maxForwardSpeed;
    public float maxSidewaysSpeed;
    public float maxBackwardsSpeed;

    [Tooltip("Curve for multiplying speed based on slope (negative = downwards).")]
    public AnimationCurve slopeSpeedMultiplier;

    [Tooltip("How fast does the character change speed?  Higher is faster.")]
    public float maxGroundAcceleration;
    public float maxAirAcceleration;

    // The gravity for the character.
    public float gravity;
    public float maxFallSpeed;

    // The last collision flags returned from controller.Move.
    [NonSerialized] public CollisionFlags collisionFlags;

    // We will keep track of the character's current velocity.
    [NonSerialized] public Vector3 velocity;

    // This keeps track of our current velocity while we're not grounded.
    [NonSerialized] public Vector3 frameVelocity;
    [NonSerialized] public Vector3 hitPoint;
    [NonSerialized] public Vector3 lastHitPoint;

    public CharacterMotorMovement()
    {
        // The maximum horizontal speed when moving.
        maxForwardSpeed = 10f;
        maxSidewaysSpeed = 10f;
        maxBackwardsSpeed = 10f;

        // Curve for multiplying speed based on slope (negative = downwards).
        slopeSpeedMultiplier = new AnimationCurve(new Keyframe[] { new Keyframe(-90, 1), new Keyframe(0, 1), new Keyframe(90, 0) });
        maxGroundAcceleration = 30f;
        maxAirAcceleration = 20f;
        gravity = 10f;
        maxFallSpeed = 20f;
        frameVelocity = Vector3.zero;
        hitPoint = Vector3.zero;
        lastHitPoint = new Vector3(Mathf.Infinity, 0, 0);
    }
}


public enum MovementTransferOnJump
{
    None = 0,           // The jump is not affected by velocity of floor at all.
    InitTransfer = 1,   // Jump gets its initial velocity from the floor, then gradually comes to a stop.
    PermaTransfer = 2,  // Jump gets its initial velocity from the floor and keeps that velocity until landing.
    PermaLocked = 3     // Jump is relative to the movement of the last touched floor and will move together with that floor.
}


// We will contain all the jumping related variables in one helper class for clarity.
[Serializable]
public class CharacterMotorJumping
{
    [Tooltip("Can the character jump?")]
    public bool enabled = true;

    [Tooltip("How high do we jump when pressing jump and letting go immediately.")]
    public float baseHeight = 1f;

    [Tooltip("We add extraHeight units (meters) on top when holding the button down longer while jumping.")]
    public float extraHeight = 2f;

    [Tooltip("How much does the character jump out perpendicular to walkable surfaces. 0 = fully vertical, 1 = fully perpendicular.")]
    [Range(0, 1)]
    public float perpAmount = 0f;

    [Tooltip("How much does the character jump out perpendicular to steep surfaces. 0 = fully vertical, 1 = fully perpendicular.")]
    [Range(0, 1)]
    public float steepPerpAmount = 0.5f;

    // Are we jumping? (Initiated with jump button and not grounded yet)
    // To see if we are just in the air (initiated by jumping OR falling) see the grounded variable.
    [NonSerialized] public bool jumping;
    [NonSerialized] public bool holdingJumpButton;

    // The time we jumped at (Used to determine for how long to apply extra jump power after jumping.)
    [NonSerialized] public float lastStartTime;
    [NonSerialized] public float lastButtonDownTime = -100;
    [NonSerialized] public Vector3 jumpDir = Vector3.up;
}


[Serializable]
public class CharacterMotorMovingPlatform
{
    public bool enabled;
    public MovementTransferOnJump movementTransfer;
    [NonSerialized] public Transform hitPlatform;
    [NonSerialized] public Transform activePlatform;
    [NonSerialized] public Vector3 activeLocalPoint;
    [NonSerialized] public Vector3 activeGlobalPoint;
    [NonSerialized] public Quaternion activeLocalRotation;
    [NonSerialized] public Quaternion activeGlobalRotation;
    [NonSerialized] public Matrix4x4 lastMatrix;
    [NonSerialized] public Vector3 platformVelocity;
    [NonSerialized] public bool newPlatform;

    public CharacterMotorMovingPlatform()
    {
        enabled = true;
        movementTransfer = MovementTransferOnJump.PermaTransfer;
    }
}


[Serializable]
public class CharacterMotorSliding
{
    [Tooltip("Does the character slide on too steep surfaces?")]
    public bool enabled;

    [Tooltip("How fast does the character slide on steep surfaces?")]
    public float slidingSpeed;

    [Tooltip("How much can the player control the sliding direction? If the value is 0.5 the player can slide sideways with half the speed of the downwards sliding speed.")]
    [Range(0, 1)]
    public float sidewaysControl;

    [Tooltip("How much can the player influence the sliding speed? If the value is 0.5 the player can speed the sliding up to 150% or slow it down to 50%.")]
    [Range(0, 1)]
    public float speedControl;

    public CharacterMotorSliding()
    {
        enabled = true;
        slidingSpeed = 15;
        sidewaysControl = 1f;
        speedControl = 0.4f;
    }
}


[AddComponentMenu("Character Controller/Character Motor")]
[RequireComponent(typeof(CharacterController))]
public class CharacterMotor : MonoBehaviour
{
    [Tooltip("Does this script currently respond to input?")]
    public bool canControl;
    public bool useFixedUpdate;

    // The current global direction we want the character to move in.
    [NonSerialized] public Vector3 inputMoveDirection = Vector3.zero;

    // Is the jump button held down? We use this interface instead of checking.
    // For the jump button directly so this script can also be used by AIs.
    [NonSerialized] public bool inputJump;

    public CharacterMotorMovement movement;
    public CharacterMotorJumping jumping;
    public CharacterMotorMovingPlatform movingPlatform;
    public CharacterMotorSliding sliding;

    [NonSerialized] public bool grounded;
    [NonSerialized] public Vector3 groundNormal;

    private Vector3 lastGroundNormal;
    private Transform tr;
    private CharacterController controller;


    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        tr = GetComponent<Transform>();
    }


    private void UpdateFunction()
    {
        // We copy the actual velocity into a temporary variable that we can manipulate.
        Vector3 velocity = movement.velocity;

        // Update velocity based on input
        velocity = ApplyInputVelocityChange(velocity);

        // Apply gravity and jumping force
        velocity = ApplyGravityAndJumping(velocity);

        // Moving platform support
        Vector3 moveDistance = Vector3.zero;

        if (MoveWithPlatform())
        {
            Vector3 newGlobalPoint = movingPlatform.activePlatform.TransformPoint(movingPlatform.activeLocalPoint);
            moveDistance = newGlobalPoint - movingPlatform.activeGlobalPoint;

            if (moveDistance != Vector3.zero)
            {
                controller.Move(moveDistance);
            }

            // Support moving platform rotation as well:
            Quaternion newGlobalRotation = movingPlatform.activePlatform.rotation * movingPlatform.activeLocalRotation;
            Quaternion rotationDiff = newGlobalRotation * Quaternion.Inverse(movingPlatform.activeGlobalRotation);
            float yRotation = rotationDiff.eulerAngles.y;

            if (yRotation != 0)
            {
                // Prevent rotation of the local up vector
                tr.Rotate(0, yRotation, 0);
            }
        }

        // Save lastPosition for velocity calculation.
        Vector3 lastPosition = tr.position;

        // We always want the movement to be frame-rate independent.  Multiplying by Time.deltaTime does this.
        Vector3 currentMovementOffset = velocity * Time.deltaTime;

        // Find out how much we need to push towards the ground to avoid loosing grounding
        // when walking down a step or over a sharp change in slope.
        float pushDownOffset = Mathf.Max(controller.stepOffset, new Vector3(currentMovementOffset.x, 0, currentMovementOffset.z).magnitude);

        if (grounded)
        {
            currentMovementOffset = currentMovementOffset - (pushDownOffset * Vector3.up);
        }

        // Reset variables that will be set by collision function
        movingPlatform.hitPlatform = null;
        groundNormal = Vector3.zero;

        // Move our character!
        movement.collisionFlags = controller.Move(currentMovementOffset);
        movement.lastHitPoint = movement.hitPoint;
        lastGroundNormal = groundNormal;

        if (movingPlatform.enabled && (movingPlatform.activePlatform != movingPlatform.hitPlatform))
        {
            if (movingPlatform.hitPlatform != null)
            {
                movingPlatform.activePlatform = movingPlatform.hitPlatform;
                movingPlatform.lastMatrix = movingPlatform.hitPlatform.localToWorldMatrix;
                movingPlatform.newPlatform = true;
            }
        }

        // Calculate the velocity based on the current and previous position.
        // This means our velocity will only be the amount the character actually moved as a result of collisions.
        Vector3 oldHVelocity = new Vector3(velocity.x, 0, velocity.z);
        movement.velocity = (tr.position - lastPosition) / Time.deltaTime;
        Vector3 newHVelocity = new Vector3(movement.velocity.x, 0, movement.velocity.z);

        // The CharacterController can be moved in unwanted directions when colliding with things.
        // We want to prevent this from influencing the recorded velocity.
        if (oldHVelocity == Vector3.zero)
        {
            movement.velocity = new Vector3(0, movement.velocity.y, 0);
        }
        else
        {
            float projectedNewVelocity = Vector3.Dot(newHVelocity, oldHVelocity) / oldHVelocity.sqrMagnitude;
            movement.velocity = (oldHVelocity * Mathf.Clamp01(projectedNewVelocity)) + (movement.velocity.y * Vector3.up);
        }

        if (movement.velocity.y < (velocity.y - 0.001f))
        {
            if (movement.velocity.y < 0)
            {
                // Something is forcing the CharacterController down faster than it should.
                // Ignore this
                movement.velocity.y = velocity.y;
            }
            else
            {
                // The upwards movement of the CharacterController has been blocked.
                // This is treated like a ceiling collision - stop further jumping here.
                jumping.holdingJumpButton = false;
            }
        }

        // We were grounded but just loosed grounding
        if (grounded && !IsGroundedTest())
        {
            grounded = false;

            // Apply inertia from platform
            if (movingPlatform.enabled && ((movingPlatform.movementTransfer == MovementTransferOnJump.InitTransfer) || (movingPlatform.movementTransfer == MovementTransferOnJump.PermaTransfer)))
            {
                movement.frameVelocity = movingPlatform.platformVelocity;
                movement.velocity = movement.velocity + movingPlatform.platformVelocity;
            }

            //SendMessage ("OnFall", SendMessageOptions.DontRequireReceiver);

            // We pushed the character down to ensure it would stay on the ground if there was any.
            // But there wasn't so now we cancel the downwards offset to make the fall smoother.
            tr.position = tr.position + (pushDownOffset * Vector3.up);
        }
        else // We were not grounded but just landed on something
        {
            if (!grounded && IsGroundedTest())
            {
                grounded = true;
                jumping.jumping = false;
                StartCoroutine(SubtractNewPlatformVelocity());
                //SendMessage ("OnLand", SendMessageOptions.DontRequireReceiver);
            }
        }

        // Moving platforms support
        if (MoveWithPlatform())
        {
            // Use the center of the lower half sphere of the capsule as reference point.
            // This works best when the character is standing on moving tilting platforms.
            movingPlatform.activeGlobalPoint = tr.position + (Vector3.up * ((controller.center.y - (controller.height * 0.5f)) + controller.radius));
            movingPlatform.activeLocalPoint = movingPlatform.activePlatform.InverseTransformPoint(movingPlatform.activeGlobalPoint);

            // Support moving platform rotation as well:
            movingPlatform.activeGlobalRotation = tr.rotation;
            movingPlatform.activeLocalRotation = Quaternion.Inverse(movingPlatform.activePlatform.rotation) * movingPlatform.activeGlobalRotation;
        }
    }


    private void FixedUpdate()
    {
        if (movingPlatform.enabled)
        {
            if (movingPlatform.activePlatform != null)
            {
                if (!movingPlatform.newPlatform)
                {
                    Vector3 lastVelocity = movingPlatform.platformVelocity;
                    movingPlatform.platformVelocity = (movingPlatform.activePlatform.localToWorldMatrix.MultiplyPoint3x4(movingPlatform.activeLocalPoint) - movingPlatform.lastMatrix.MultiplyPoint3x4(movingPlatform.activeLocalPoint)) / Time.deltaTime;
                }
                movingPlatform.lastMatrix = movingPlatform.activePlatform.localToWorldMatrix;
                movingPlatform.newPlatform = false;
            }
            else
            {
                movingPlatform.platformVelocity = Vector3.zero;
            }
        }
        if (useFixedUpdate)
        {
            UpdateFunction();
        }
    }


    private void Update()
    {
        if (!useFixedUpdate)
        {
            UpdateFunction();
        }
    }


    private Vector3 ApplyInputVelocityChange(Vector3 velocity)
    {
        if (!canControl)
        {
            inputMoveDirection = Vector3.zero;
        }

        // Find desired velocity.
        Vector3 desiredVelocity = default(Vector3);

        if (grounded && TooSteep())
        {
            // The direction we're sliding in.
            desiredVelocity = new Vector3(groundNormal.x, 0, groundNormal.z).normalized;

            // Find the input movement direction projected onto the sliding direction.
            Vector3 projectedMoveDir = Vector3.Project(inputMoveDirection, desiredVelocity);

            // Add the sliding direction, the speed control, and the sideways control vectors.
            desiredVelocity = (desiredVelocity + (projectedMoveDir * sliding.speedControl)) + ((inputMoveDirection - projectedMoveDir) * sliding.sidewaysControl);

            // Multiply with the sliding speed.
            desiredVelocity = desiredVelocity * sliding.slidingSpeed;
        }
        else
        {
            desiredVelocity = GetDesiredHorizontalVelocity();
        }

        if (movingPlatform.enabled && (movingPlatform.movementTransfer == MovementTransferOnJump.PermaTransfer))
        {
            desiredVelocity = desiredVelocity + movement.frameVelocity;
            desiredVelocity.y = 0;
        }

        if (grounded)
        {
            desiredVelocity = AdjustGroundVelocityToNormal(desiredVelocity, groundNormal);
        }
        else
        {
            velocity.y = 0;
        }

        // Enforce max velocity change
        float maxVelocityChange = GetMaxAcceleration(grounded) * Time.deltaTime;
        Vector3 velocityChangeVector = desiredVelocity - velocity;

        if (velocityChangeVector.sqrMagnitude > (maxVelocityChange * maxVelocityChange))
        {
            velocityChangeVector = velocityChangeVector.normalized * maxVelocityChange;
        }

        // If we're in the air and don't have control, don't apply any velocity change at all.
        // If we're on the ground and don't have control we do apply it - it will correspond to friction.
        if (grounded || canControl)
        {
            velocity += velocityChangeVector;
        }

        if (grounded)
        {
            // When going uphill, the CharacterController will automatically move up by the needed amount.
            // Not moving it upwards manually prevent risk of lifting off from the ground.
            // When going downhill, DO move down manually, as gravity is not enough on steep hills.
            velocity.y = Mathf.Min(velocity.y, 0);
        }

        return velocity;
    }


    private Vector3 ApplyGravityAndJumping(Vector3 velocity)
    {
        if (!inputJump || !canControl)
        {
            jumping.holdingJumpButton = false;
            jumping.lastButtonDownTime = -100;
        }

        if ((inputJump && (jumping.lastButtonDownTime < 0)) && canControl)
        {
            jumping.lastButtonDownTime = Time.time;
        }

        if (grounded)
        {
            velocity.y = Mathf.Min(0, velocity.y) - (movement.gravity * Time.deltaTime);
        }
        else
        {
            velocity.y = movement.velocity.y - (movement.gravity * Time.deltaTime);

            // When jumping up we don't apply gravity for some time when the user is holding the jump button.
            // This gives more control over jump height by pressing the button longer.
            if (jumping.jumping && jumping.holdingJumpButton)
            {
                // Calculate the duration that the extra jump force should have effect.
                // If we're still less than that duration after the jumping time, apply the force.
                if (Time.time < (jumping.lastStartTime + (jumping.extraHeight / CalculateJumpVerticalSpeed(jumping.baseHeight))))
                {
                    // Negate the gravity we just applied, except we push in jumpDir rather than jump upwards.
                    velocity += ((jumping.jumpDir * movement.gravity) * Time.deltaTime);
                }
            }

            // Make sure we don't fall any faster than maxFallSpeed. This gives our character a terminal velocity.
            velocity.y = Mathf.Max(velocity.y, -movement.maxFallSpeed);
        }

        if (grounded)
        {
            // Jump only if the jump button was pressed down in the last 0.2 seconds.
            // We use this check instead of checking if it's pressed down right now because players will often
            // try to jump in the exact moment when hitting the ground after a jump and if they hit the button
            // a fraction of a second too soon and no new jump happens as a consequence, it's confusing and it
            // feels like the game is buggy.
            if ((jumping.enabled && canControl) && ((Time.time - jumping.lastButtonDownTime) < 0.2f))
            {
                grounded = false;
                jumping.jumping = true;
                jumping.lastStartTime = Time.time;
                jumping.lastButtonDownTime = -100;
                jumping.holdingJumpButton = true;

                // Calculate the jumping direction.
                if (TooSteep())
                {
                    jumping.jumpDir = Vector3.Slerp(Vector3.up, groundNormal, jumping.steepPerpAmount);
                }
                else
                {
                    jumping.jumpDir = Vector3.Slerp(Vector3.up, groundNormal, jumping.perpAmount);
                }

                // Apply the jumping force to the velocity. Cancel any vertical velocity first.
                velocity.y = 0;
                velocity = velocity + (jumping.jumpDir * CalculateJumpVerticalSpeed(jumping.baseHeight));

                // Apply inertia from platform.
                if (movingPlatform.enabled && ((movingPlatform.movementTransfer == MovementTransferOnJump.InitTransfer)
                    || (movingPlatform.movementTransfer == MovementTransferOnJump.PermaTransfer)))
                {
                    movement.frameVelocity = movingPlatform.platformVelocity;
                    velocity = velocity + movingPlatform.platformVelocity;
                }

                //SendMessage ("OnJump", SendMessageOptions.DontRequireReceiver);
            }
            else
            {
                jumping.holdingJumpButton = false;
            }
        }
        return velocity;
    }


    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (((hit.normal.y > 0) && (hit.normal.y > groundNormal.y)) && (hit.moveDirection.y < 0))
        {
            if (((hit.point - movement.lastHitPoint).sqrMagnitude > 0.001f) || (lastGroundNormal == Vector3.zero))
            {
                groundNormal = hit.normal;
            }
            else
            {
                groundNormal = lastGroundNormal;
            }

            movingPlatform.hitPlatform = hit.collider.transform;
            movement.hitPoint = hit.point;
            movement.frameVelocity = Vector3.zero;
        }
    }


    private IEnumerator SubtractNewPlatformVelocity()
    {
        // When landing, subtract the velocity of the new ground from the character's velocity
        // since movement in ground is relative to the movement of the ground.
        if (movingPlatform.enabled &&
            ((movingPlatform.movementTransfer == MovementTransferOnJump.InitTransfer) ||
            (movingPlatform.movementTransfer == MovementTransferOnJump.PermaTransfer)))
        {
            // If we landed on a new platform, we have to wait for two FixedUpdates
            // before we know the velocity of the platform under the character
            if (movingPlatform.newPlatform)
            {
                Transform platform = movingPlatform.activePlatform;
                yield return new WaitForFixedUpdate();
                yield return new WaitForFixedUpdate();
                if (grounded && (platform == movingPlatform.activePlatform))
                {
                    yield return 1;
                }
            }

            movement.velocity = movement.velocity - movingPlatform.platformVelocity;
        }
    }


    private bool MoveWithPlatform()
    {
        return (movingPlatform.enabled && (grounded
            || (movingPlatform.movementTransfer == MovementTransferOnJump.PermaLocked)))
            && (movingPlatform.activePlatform != null);
    }


    private Vector3 GetDesiredHorizontalVelocity()
    {
        // Find desired velocity
        Vector3 desiredLocalDirection = tr.InverseTransformDirection(inputMoveDirection);
        float maxSpeed = MaxSpeedInDirection(desiredLocalDirection);

        if (grounded)
        {
            // Modify max speed on slopes based on slope speed multiplier curve
            float movementSlopeAngle = Mathf.Asin(movement.velocity.normalized.y) * Mathf.Rad2Deg;
            maxSpeed = maxSpeed * movement.slopeSpeedMultiplier.Evaluate(movementSlopeAngle);
        }

        return tr.TransformDirection(desiredLocalDirection * maxSpeed);
    }


    private Vector3 AdjustGroundVelocityToNormal(Vector3 hVelocity, Vector3 groundNormal)
    {
        Vector3 sideways = Vector3.Cross(Vector3.up, hVelocity);
        return Vector3.Cross(sideways, groundNormal).normalized * hVelocity.magnitude;
    }


    private bool IsGroundedTest()
    {
        return groundNormal.y > 0.01f;
    }


    public float GetMaxAcceleration(bool grounded)
    {
        // Maximum acceleration on ground and in air
        if (grounded)
        {
            return movement.maxGroundAcceleration;
        }
        else
        {
            return movement.maxAirAcceleration;
        }
    }


    public float CalculateJumpVerticalSpeed(float targetJumpHeight)
    {
        // From the jump height and gravity we deduce the upwards speed
        // for the character to reach at the apex.
        return Mathf.Sqrt((2 * targetJumpHeight) * movement.gravity);
    }


    public bool IsJumping()
    {
        return jumping.jumping;
    }


    public bool IsSliding()
    {
        return (grounded && sliding.enabled) && TooSteep();
    }


    public bool IsTouchingCeiling()
    {
        return (movement.collisionFlags & CollisionFlags.CollidedAbove) != 0;
    }


    public bool IsGrounded()
    {
        return grounded;
    }


    public bool TooSteep()
    {
        return groundNormal.y <= Mathf.Cos(controller.slopeLimit * Mathf.Deg2Rad);
    }


    public Vector3 GetDirection()
    {
        return inputMoveDirection;
    }


    public void SetControllable(bool controllable)
    {
        canControl = controllable;
    }


    // Project a direction onto elliptical quarter segments based on forward, sideways, and backwards speed.
    // The function returns the length of the resulting vector.
    public float MaxSpeedInDirection(Vector3 desiredMovementDirection)
    {
        if (desiredMovementDirection == Vector3.zero)
        {
            return 0;
        }
        else
        {
            float zAxisEllipseMultiplier = (desiredMovementDirection.z > 0 ? movement.maxForwardSpeed : movement.maxBackwardsSpeed) / movement.maxSidewaysSpeed;
            Vector3 temp = new Vector3(desiredMovementDirection.x, 0, desiredMovementDirection.z / zAxisEllipseMultiplier).normalized;
            float length = new Vector3(temp.x, 0, temp.z * zAxisEllipseMultiplier).magnitude * movement.maxSidewaysSpeed;
            return length;
        }
    }


    public void SetVelocity(Vector3 velocity)
    {
        grounded = false;
        movement.velocity = velocity;
        movement.frameVelocity = Vector3.zero;
        //SendMessage ("OnExternalVelocity");
    }


    public CharacterMotor()
    {
        canControl = true;
        useFixedUpdate = true;
        inputMoveDirection = Vector3.zero;
        movement = new CharacterMotorMovement();
        jumping = new CharacterMotorJumping();
        movingPlatform = new CharacterMotorMovingPlatform();
        sliding = new CharacterMotorSliding();
        grounded = true;
        groundNormal = Vector3.zero;
        lastGroundNormal = Vector3.zero;
    }
}