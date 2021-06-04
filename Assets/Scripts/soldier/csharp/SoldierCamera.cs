using UnityEngine;

public class SoldierCamera : MonoBehaviour
{
    public Transform target;
    public Transform soldier;

    public Vector2 speed = new Vector2(135.0f, 135.0f);
    public Vector2 aimSpeed = new Vector2(100.0f, 100.0f);
    public Vector2 maxSpeed = new Vector2(270.0f, 270.0f);

    public float yMinLimit = -85;
    public float yMaxLimit = 85;

    public float normalFOV = 60;
    public float zoomFOV = 30;

    public float lerpSpeed = 8.0f;
    private float x = 0.0f;
    public float y = 0.0f;

    private Transform camTransform;
    private float deltaTime;
    private SoldierController soldierController;

    public bool orbit;

    public LayerMask hitLayer;

    private Vector3 cPos;

    public Vector3 normalDirection = new Vector3(-1, 0, 0.3f);
    public Vector3 aimDirection = new Vector3(-1, 0, 0.7f);
    public Vector3 crouchDirection = new Vector3(-1, 0, 0.3f);
    public Vector3 aimCrouchDirection = new Vector3(-0.2f, 0, 0.2f);

    public float positionLerp = 6;

    public float normalHeight = 1.5f;
    public float crouchHeight = 1.5f;
    public float normalAimHeight = 1.8f;
    public float crouchAimHeight = 1;
    public float minHeight = 0.5f;
    public float maxHeight = 2;

    public float normalDistance = 2.5f;
    public float crouchDistance = 2.5f;
    public float normalAimDistance = 1;
    public float crouchAimDistance = 1;
    public float minDistance = 0.2f;
    public float maxDistance = 2.5f;

    private float targetDistance;
    private Vector3 camDir;
    private float targetHeight;


    public float minShakeSpeed = 20;
    public float maxShakeSpeed = 20;

    public float minShake = 0.01f;
    public float maxShake = 0.025f;

    public int minShakeTimes = 6;
    public int maxShakeTimes = 20;

    public float maxShakeDistance = 35;

    private bool shake;
    private float shakeSpeed = 2.0f;
    private float cShakePos;
    private float shakeTimes = 8;
    private float cShake;
    private float cShakeSpeed;
    private int cShakeTimes;

    public Transform radar;
    public Transform radarCamera;

    private DepthOfField _depthOfFieldEffect;

    void Start()
    {
        cShakeTimes = 0;
        cShake = 0.0f;
        cShakeSpeed = shakeSpeed;

        _depthOfFieldEffect = gameObject.GetComponent<DepthOfField>();

        if (target == null || soldier == null)
        {
            Destroy(this);
            return;
        }

        target.parent = null;

        camTransform = transform;

        var angles = camTransform.eulerAngles;
        x = angles.y;
        y = angles.x;

        soldierController = soldier.GetComponent<SoldierController>();

        targetDistance = normalDistance;

        cPos = soldier.position + new Vector3(0, normalHeight, 0);
    }

    void GoToOrbitMode(bool state)
    {
        orbit = state;

        soldierController.idleTimer = 0.0f;
    }

    void Update()
    {
        if (GameManager.pause || GameManager.scores) return;

        if (orbit && (Input.GetKeyDown(KeyCode.O) || Input.GetAxis("Horizontal") != 0.0 || Input.GetAxis("Vertical") != 0.0 || soldierController.aim || soldierController.fire))
        {
            GoToOrbitMode(false);
        }

        if (!orbit && soldierController.idleTimer > 0.1)
        {
            GoToOrbitMode(true);
        }
    }

    void LateUpdate()
    {
        if (GameManager.pause || GameManager.scores) return;

        deltaTime = Time.fixedDeltaTime;

        GetInput();

        RotateSoldier();

        CameraMovement();

        DepthOfFieldControl();
    }

    void CameraMovement()
    {
        if (soldierController.aim)
        {
            camera.GetComponent<DepthOfField>().enabled = true;
            camera.fieldOfView = Mathf.Lerp(camera.fieldOfView, zoomFOV, deltaTime * lerpSpeed);

            if (soldierController.crouch)
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
            camera.GetComponent<DepthOfField>().enabled = false;
            camera.fieldOfView = Mathf.Lerp(camera.fieldOfView, normalFOV, deltaTime * lerpSpeed);

            if (soldierController.crouch)
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

        RaycastHit hit;
        if (Physics.Raycast(cPos, camDir, out hit, targetDistance + 0.2f, hitLayer))
        {
            var t = hit.distance - 0.1f;
            t -= minDistance;
            t /= (targetDistance - minDistance);

            targetHeight = Mathf.Lerp(maxHeight, targetHeight, Mathf.Clamp(t, 0.0f, 1.0f));
            cPos = soldier.position + new Vector3(0, targetHeight, 0);
        }

        if (Physics.Raycast(cPos, camDir, out hit, targetDistance + 0.2f, hitLayer))
        {
            targetDistance = hit.distance - 0.1f;
        }
        if (radar != null)
        {
            radar.position = cPos;
            radarCamera.rotation = Quaternion.Euler(90, x, 0);
        }

        Vector3 lookPoint = cPos;
        lookPoint += (target.right * Vector3.Dot(camDir * targetDistance, target.right));

        camTransform.position = cPos + (camDir * targetDistance);
        camTransform.LookAt(lookPoint);

        target.position = cPos;
        target.rotation = Quaternion.Euler(y, x, 0);
    }

    void HandleCameraShake()
    {
        if (shake)
        {
            cShake += cShakeSpeed * deltaTime;

            if (Mathf.Abs(cShake) > cShakePos)
            {
                cShakeSpeed *= -1.0f;
                cShakeTimes++;

                if (cShakeTimes >= shakeTimes)
                {
                    shake = false;
                }

                if (cShake > 0.0)
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

    public void StartShake(float distance)
    {
        float proximity = distance / maxShakeDistance;

        if (proximity > 1.0) return;

        proximity = Mathf.Clamp(proximity, 0.0f, 1.0f);

        proximity = 1.0f - proximity;

        cShakeSpeed = Mathf.Lerp(minShakeSpeed, maxShakeSpeed, proximity);
        shakeTimes = Mathf.Lerp(minShakeTimes, maxShakeTimes, proximity);
        cShakeTimes = 0;
        cShakePos = Mathf.Lerp(minShake, maxShake, proximity);

        shake = true;
    }

    void GetInput()
    {
        var a = soldierController.aim ? aimSpeed : speed;
        Debug.Log(deltaTime);
        x += Mathf.Clamp(Input.GetAxis("Mouse X") * a.x, -maxSpeed.x, maxSpeed.x) * deltaTime;
        y -= Mathf.Clamp(Input.GetAxis("Mouse Y") * a.y, -maxSpeed.y, maxSpeed.y) * deltaTime;
        y = ClampAngle(y, yMinLimit, yMaxLimit);
    }

    void DepthOfFieldControl()
    {
        if (_depthOfFieldEffect == null) return;
        if (soldierController == null) return;

        if (soldierController.aim && GameQualitySettings.depthOfField)
        {
            if (!_depthOfFieldEffect.enabled)
            {
                _depthOfFieldEffect.enabled = true;
            }
        }
        else
        {
            if (_depthOfFieldEffect.enabled)
            {
                _depthOfFieldEffect.enabled = false;
            }
        }
    }

    void RotateSoldier()
    {
        if (!orbit)
            soldierController.targetYRotation = x;
    }

    static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360)
        {
            angle += 360;
        }

        if (angle > 360)
        {
            angle -= 360;
        }

        return Mathf.Clamp(angle, min, max);
    }
}