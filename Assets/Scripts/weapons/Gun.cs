using UnityEngine;
using System.Collections;

namespace Bootcamp.Weapons
{
    public enum FireType
    {
        RAYCAST,
        PHYSIC_PROJECTILE,
    }

    public enum FireMode
    {
        SEMI_AUTO,
        FULL_AUTO,
        BURST
    }

    public class Gun : MonoBehaviour
    {
        public string gunName;
        public GameObject bulletMark;
        public GameObject projectilePrefab;

        public Transform weaponTransformReference;

        public LayerMask hitLayer;

        public GameObject woodParticle;
        public GameObject metalParticle;
        public GameObject concreteParticle;
        public GameObject sandParticle;
        public GameObject waterParticle;

        //How many shots the gun can take in one second
        public float fireRate;
        public bool useGravity;
        private FireType fireType;
        public FireMode fireMode;

        //Number of shoots to fire when on burst mode
        public int burstRate;

        //Range of fire in meters
        public float fireRange;

        //Speed of the projectile in m/s
        public float projectileSpeed;

        public int clipSize;
        public int totalClips;

        //Time to reload the weapon in seconds
        public float reloadTime;
        public bool autoReload;
        public int currentRounds;

        public float shootVolume = 0.4f;
        public AudioClip shootSound;
        private AudioSource shootSoundSource;

        public AudioClip reloadSound;
        private AudioSource reloadSoundSource;

        public AudioClip outOfAmmoSound;
        private AudioSource outOfAmmoSoundSource;

        private float reloadTimer;

        [HideInInspector] public bool freeToShoot;

        [HideInInspector] public bool reloading;
        private float lastShootTime;
        private float shootDelay;
        private int cBurst;

        [HideInInspector]
        public bool fire;
        public GameObject hitParticles;

        public GunParticles shotingEmitter;
        private Transform shottingParticles;

        public ParticleEmitter[] capsuleEmitter;

        public ShotLight shotLight;

        public bool unlimited = true;

        private float timerToCreateDecal;

        public float pushPower = 3.0f;

        public SoldierCamera soldierCamera;
        private Camera cam;

        void OnDisable()
        {
            if (shotingEmitter != null)
            {
                shotingEmitter.ChangeState(false);
            }

            if (capsuleEmitter != null)
            {
                for (int i = 0; i < capsuleEmitter.Length; i++)
			{
                    if (capsuleEmitter[i] != null)
                        capsuleEmitter[i].emit = false;
                }
            }

            if (shotLight != null)
            {
                shotLight.enabled = false;
            }
        }

        void OnEnable()
        {
            cam = soldierCamera.camera;

            reloadTimer = 0.0f;
            reloading = false;
            freeToShoot = true;
            shootDelay = 1.0f / fireRate;

            cBurst = burstRate;

            totalClips--;
            currentRounds = clipSize;

            if (projectilePrefab != null)
            {
                fireType = FireType.PHYSIC_PROJECTILE;
            }

            if (shotLight != null)
            {
                shotLight.enabled = false;
            }

            shottingParticles = null;
            if (shotingEmitter != null)
            {
                for (int i = 0; i < shotingEmitter.transform.childCount; i++)
			{
                    if (shotingEmitter.transform.GetChild(i).name == "bullet_trace")
                    {
                        shottingParticles = shotingEmitter.transform.GetChild(i);
                        break;
                    }
                }
            }
        }

        void ShotTheTarget()
        {
            if (fire && !reloading)
            {
                if (currentRounds > 0)
                {
                    if (Time.time > lastShootTime && freeToShoot && cBurst > 0)
                    {
                        lastShootTime = Time.time + shootDelay;

                        switch (fireMode)
                        {
                            case FireMode.SEMI_AUTO:
                                freeToShoot = false;
                                break;
                            case FireMode.BURST:
                                cBurst--;
                                break;
                        }

                        if (capsuleEmitter != null)
                        {
                            for (int i  = 0; i < capsuleEmitter.Length; i++)
						{
                                capsuleEmitter[i].Emit();
                            }
                        }

                        PlayShootSound();

                        if (shotingEmitter != null)
                        {
                            shotingEmitter.ChangeState(true);

                        }

                        if (shotLight != null)
                        {
                            shotLight.enabled = true;
                        }

                        switch (fireType)
                        {
                            case FireType.RAYCAST:
                                TrainingStatistics.shootsFired++;
                                CheckRaycastHit();
                                break;
                            case FireType.PHYSIC_PROJECTILE:
                                TrainingStatistics.grenadeFired++;
                                LaunchProjectile();
                                break;
                        }

                        currentRounds--;

                        if (currentRounds <= 0)
                        {
                            Reload();
                        }
                    }
                }
                else if (autoReload && freeToShoot)
                {
                    if (shotingEmitter != null)
                    {
                        shotingEmitter.ChangeState(false);
                    }

                    if (shotLight != null)
                    {
                        shotLight.enabled = false;
                    }

                    if (!reloading)
                    {
                        Reload();
                    }
                }
            }
            else
            {
                if (shotingEmitter != null)
                {
                    shotingEmitter.ChangeState(false);
                }

                if (shotLight != null)
                {
                    shotLight.enabled = false;
                }
            }
        }

        void LaunchProjectile()
        {
            //Get the launch position (weapon related)
            Ray camRay   = cam.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.6f, 0));

            Vector3 startPosition ;

            if (weaponTransformReference != null)
            {
                startPosition = weaponTransformReference.position;
            }
            else
            {
                startPosition = cam.ScreenToWorldPoint(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0.5f));
            }

            GameObject projectile  = Instantiate(projectilePrefab, startPosition, Quaternion.identity) as GameObject;

            Grenade grenadeObj  = projectile.GetComponent<Grenade>();
            grenadeObj.soldierCamera = soldierCamera;

            projectile.transform.rotation = Quaternion.LookRotation(camRay.direction);

            Rigidbody projectileRigidbody = projectile.rigidbody;

            if (projectile.rigidbody == null)
            {
                projectileRigidbody = projectile.AddComponent<Rigidbody>();
            }
            projectileRigidbody.useGravity = useGravity;

            RaycastHit hit;
            Ray camRay2 = cam.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.55f, 0));

            if (Physics.Raycast(camRay2.origin, camRay2.direction, out hit, fireRange, hitLayer))
            {
                projectileRigidbody.velocity = (hit.point - weaponTransformReference.position).normalized * projectileSpeed;
            }
            else
            {
                projectileRigidbody.velocity = (cam.ScreenToWorldPoint(new Vector3(Screen.width * 0.5f, Screen.height * 0.55f, 40)) - weaponTransformReference.position).normalized * projectileSpeed;
            }
        }

        void CheckRaycastHit()
        {
            RaycastHit hit  ;
            RaycastHit glassHit ;
            Ray camRay  ;
            Vector3 origin ;
            Vector3 glassOrigin = Vector3.zero;
            Vector3 dir ;
            Vector3 glassDir = Vector3.forward;

            if (weaponTransformReference == null)
            {
                camRay = cam.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0));
                origin = camRay.origin;
                dir = camRay.direction;
                origin += dir * 0.1f;
            }
            else
            {
                camRay = cam.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0));

                origin = weaponTransformReference.position + (weaponTransformReference.right * 0.2f);

                if (Physics.Raycast(camRay.origin + camRay.direction * 0.1f, camRay.direction, out hit, fireRange, hitLayer))
                {
                    dir = (hit.point - origin).normalized;

                    if (hit.collider.tag == "glass")
                    {
                        glassOrigin = hit.point + dir * 0.05f;

                        if (Physics.Raycast(glassOrigin, camRay.direction, out glassHit, fireRange - hit.distance, hitLayer))
                        {
                            glassDir = glassHit.point - glassOrigin;
                        }
                    }
                }
                else
                {
                    dir = weaponTransformReference.forward;
                }
            }

            if (shottingParticles != null)
            {
                shottingParticles.rotation = Quaternion.FromToRotation(Vector3.forward, (cam.ScreenToWorldPoint(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, cam.farClipPlane)) - weaponTransformReference.position).normalized);
            }

            if (Physics.Raycast(origin, dir, out hit, fireRange, hitLayer))
            {
                hit.collider.gameObject.SendMessage("Hit", hit, SendMessageOptions.DontRequireReceiver);
                GenerateGraphicStuff(hit);

                if (hit.collider.tag == "glass")
                {
                    if (Physics.Raycast(glassOrigin, glassDir, out glassHit, fireRange - hit.distance, hitLayer))
                    {
                        glassHit.collider.gameObject.SendMessage("Hit", glassHit, SendMessageOptions.DontRequireReceiver);
                        GenerateGraphicStuff(glassHit);
                    }
                }
            }
        }

        void GenerateGraphicStuff(RaycastHit hit)
        {
            HitType hitType ;

            Rigidbody body   = hit.collider.rigidbody;
            if (body == null)
            {
                if (hit.collider.transform.parent != null)
                {
                    body = hit.collider.transform.parent.rigidbody;
                }
            }

            if (body != null)
            {
                if (body.gameObject.layer != 10 && !body.gameObject.name.ToLower().Contains("door"))
                {
                    body.isKinematic = false;
                }

                if (!body.isKinematic)
                {
                    Vector3 direction  = hit.collider.transform.position - weaponTransformReference.position;
                    body.AddForceAtPosition(direction.normalized * pushPower, hit.point, ForceMode.Impulse);
                }
            }

            GameObject go ;

            float delta = -0.02f;
            Vector3 hitUpDir   = hit.normal;
            Vector3 hitPoint  = hit.point + hit.normal * delta;

            switch (hit.collider.tag)
            {
                case "wood":
                    hitType = HitType.WOOD;
                    go = Instantiate(woodParticle, hitPoint, Quaternion.FromToRotation(Vector3.up, hitUpDir)) as GameObject;
                    break;
                case "metal":
                    hitType = HitType.METAL;
                    go = Instantiate(metalParticle, hitPoint, Quaternion.FromToRotation(Vector3.up, hitUpDir)) as GameObject;
                    break;
                case "car":
                    hitType = HitType.METAL;
                    go = Instantiate(metalParticle, hitPoint, Quaternion.FromToRotation(Vector3.up, hitUpDir)) as GameObject;
                    break;
                case "concrete":
                    hitType = HitType.CONCRETE;
                    go = Instantiate(concreteParticle, hitPoint, Quaternion.FromToRotation(Vector3.up, hitUpDir)) as GameObject;
                    break;
                case "dirt":
                    hitType = HitType.CONCRETE;
                    go = Instantiate(sandParticle, hitPoint, Quaternion.FromToRotation(Vector3.up, hitUpDir)) as GameObject;
                    break;
                case "sand":
                    hitType = HitType.CONCRETE;
                    go = Instantiate(sandParticle, hitPoint, Quaternion.FromToRotation(Vector3.up, hitUpDir)) as GameObject;
                    break;
                case "water":
                    go = Instantiate(waterParticle, hitPoint, Quaternion.FromToRotation(Vector3.up, hitUpDir)) as GameObject;
                    break;
                default:
                    return;
            }

            go.layer = hit.collider.gameObject.layer;

            if (hit.collider.renderer == null) return;

            if (timerToCreateDecal < 0.0 && hit.collider.tag != "water")
            {
                go = (GameObject)Instantiate(bulletMark, hit.point, Quaternion.FromToRotation(Vector3.forward, -hit.normal));
                BulletMarks bm  = go.GetComponent<BulletMarks>();
                bm.GenerateDecal(hitType, hit.collider.gameObject);
                timerToCreateDecal = 0.02f;
            }
        }

        void Update()
        {
            timerToCreateDecal -= Time.deltaTime;

            if (Input.GetButtonDown("Fire1") && currentRounds == 0 && !reloading && freeToShoot)
            {
                PlayOutOfAmmoSound();
            }

            if (Input.GetButtonUp("Fire1"))
            {
                freeToShoot = true;
                cBurst = burstRate;
            }

            HandleReloading();

            ShotTheTarget();
        }

        void HandleReloading()
        {
            if (Input.GetKeyDown(KeyCode.R) && !reloading)
            {
                Reload();
            }

            if (reloading)
            {
                reloadTimer -= Time.deltaTime;

                if (reloadTimer <= 0.0f)
                {
                    reloading = false;
                    if (!unlimited)
                    {
                        totalClips--;
                    }
                    currentRounds = clipSize;
                }
            }
        }

        void Reload()
        {
            if (totalClips > 0 && currentRounds < clipSize)
            {
                PlayReloadSound();
                reloading = true;
                reloadTimer = reloadTime;
            }
        }

        //---------------AUDIO METHODS--------
        void PlayOutOfAmmoSound()
        {
            audio.PlayOneShot(outOfAmmoSound, 1.5f);
        }

        void PlayReloadSound()
        {
            audio.PlayOneShot(reloadSound, 1.5f);
        }

        void PlayShootSound()
        {
            audio.PlayOneShot(shootSound);
        }
    }
}
