using UnityEngine;

public class SoundObject : MonoBehaviour
{
    [HideInInspector] public int overrideSound = -1;

    private bool playSound;
    private bool generateParticles;

    public float minSpeedToParticle;
    public GameObject genericParticle;
    public GameObject waterParticles;
    public LayerMask waterLayer;
    public bool checkWater;
    private float waterTimer;

    public float minSpeedSound;
    public float maxSpeedSound;

    private float timeToGenerateParticles;
    private float timeToSound;
    private float delta;

    private AudioSource audioPlayer;
    private AudioClip ac;

    void Start()
    {
        playSound = false;
        generateParticles = false;
        checkWater = false;

        if (SoundObjectManager.instance == null)
        {
            Destroy(this);
            return;
        }

        var so = SoundObjectManager.instance;
        minSpeedToParticle = so.minSpeedToParticle;
        minSpeedToParticle *= minSpeedToParticle;

        minSpeedSound = so.minSpeedSound;
        maxSpeedSound = so.maxSpeedSound;
        delta = 1.0f / (maxSpeedSound - minSpeedSound);
        genericParticle = so.genericParticle;

        generateParticles = genericParticle != null;

        waterParticles = so.waterParticles;
        waterLayer = so.waterLayer;
        checkWater = waterParticles != null;

        if (overrideSound == -1)
        {
            switch (gameObject.tag)
            {
                case "wood":
                    ac = so.defaultWoodSound;
                    break;
                case "metal":
                    ac = so.defaultMetalSound;
                    break;
                case "concrete":
                    ac = so.defaultConcreteSound;
                    break;
                default:
                    ac = so.defaultSound;
                    break;
            }
        }

        playSound = ac != null;

        if (!playSound && !generateParticles)
        {
            Destroy(this);
        }

        if (GetComponent<Rigidbody>() == null)
        {
            if (transform.parent != null)
            {
                if (transform.parent.GetComponent<Rigidbody>() != null)
                {
                    SoundObjectAux aux = transform.parent.gameObject.AddComponent<SoundObjectAux>();
                    aux.soundGenerator = this;
                }
                else
                {
                    Destroy(this);
                }
            }
            else
            {
                Destroy(this);
            }
        }
    }

    void Update()
    {
        if (waterTimer > 0.0f)
        {
            waterTimer -= Time.deltaTime;
        }

        timeToGenerateParticles -= Time.deltaTime;
        timeToSound -= Time.deltaTime;
    }

    public void OnCollisionEnter(Collision collisionData)
    {
        var speed = collisionData.relativeVelocity.sqrMagnitude;

        if (checkWater && waterTimer <= 0.0)
        {
            if (collisionData.collider.gameObject.GetComponent<Terrain>() != null)
            {
                RaycastHit hitInfo;

                if (Physics.Raycast(transform.position + new Vector3(0, 4, 0), -Vector3.up, out hitInfo, 4.0f, waterLayer))
                {
                    if (hitInfo.collider.tag == "water")
                    {
                        waterTimer = 1.0f;

                        var go = Instantiate(waterParticles, hitInfo.point, Quaternion.identity) as GameObject;

                        if (go.GetComponent<AudioSource>() != null)
                        {
                            go.GetComponent<AudioSource>().Play();
                        }

                        ParticleEmitter emitter;
                        for (int i = 0; i < go.transform.childCount; i++)
                        {
                            emitter = go.transform.GetChild(i).GetComponent<ParticleEmitter>();

                            if (emitter == null) continue;

                            emitter.emit = false;
                            emitter.Emit();
                        }

                        var aux = go.AddComponent<AutoDestroy>();
                        aux.time = 2;
                        return;
                    }
                }
            }
        }

        if (generateParticles && timeToGenerateParticles <= 0.0f)
        {
            if (minSpeedToParticle < speed)
            {
                timeToGenerateParticles = 0.5f;
                GameObject.Instantiate(genericParticle, collisionData.contacts[0].point, Quaternion.identity);
            }
        }

        if (playSound && timeToSound <= 0.0f)
        {
            if (audioPlayer == null)
            {
                audioPlayer = gameObject.AddComponent<AudioSource>();
                audioPlayer.playOnAwake = false;
                audioPlayer.loop = false;
                audioPlayer.clip = ac;
            }

            if (minSpeedSound * minSpeedSound < speed)
            {
                speed = Mathf.Sqrt(speed);
                var v = Mathf.Clamp((speed - minSpeedSound) * delta, 0.05f, 1.0f);
                audioPlayer.volume = v;
                timeToSound = 0.2f;
                audioPlayer.Play();
            }
        }
    }
}
