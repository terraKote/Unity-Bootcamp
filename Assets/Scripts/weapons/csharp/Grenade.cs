using UnityEngine;
using System.Collections;

public class Grenade : MonoBehaviour
{
    private Transform thisTransform;
    public float minY = -10.0f;
    public GameObject smoke;
    public GameObject explosionEmitter;
    public float explosionTime;
    public float explosionRadius;
    public float power = 3200f;
    private float timer;

    public SoldierCamera soldierCamera;

    public AudioClip[] nearSounds;
    public AudioClip[] farSounds;
    public float farSoundDistance = 25.0f;

    private bool exploded;
    private RaycastHit hit;

    void Start()
    {
        exploded = false;

        timer = 0.0f;

        thisTransform = transform;
    }

    void Detonate()
    {
        if (exploded) return;

        exploded = true;

        if (GetComponent<Renderer>() != null)
        {
            GetComponent<Renderer>().enabled = false;

            if (smoke != null)
            {
                Destroy(smoke);
            }
        }
        else
        {
            var renderers = GetComponentsInChildren<Renderer>();

            foreach (Renderer r in renderers)
            {
                r.enabled = false;
            }
        }

        Vector3 _explosionPosition = thisTransform.position;
        var col = Physics.OverlapSphere(_explosionPosition, explosionRadius);

        var distance = Vector3.Distance(soldierCamera.transform.position, _explosionPosition);
        soldierCamera.StartShake(distance);

        Rigidbody body;

        if (col != null)
        {
            for (int c = 0; c < col.Length; c++)
            {
                col[c].gameObject.SendMessage("Destruct", SendMessageOptions.DontRequireReceiver);

                body = null;
                body = col[c].gameObject.GetComponent<Rigidbody>();
                if (body != null)
                {
                    body.isKinematic = false;
                }
                else if (col[c].gameObject.transform.parent != null)
                {
                    body = col[c].gameObject.transform.parent.GetComponent<Rigidbody>();
                    if (body != null)
                    {
                        body.isKinematic = false;
                    }
                }

                if (body != null)
                {
                    body.AddExplosionForce(power, _explosionPosition, explosionRadius, 3.0f);
                }

                if (col[c].GetComponent<Collider>().tag == "glass")
                {
                    col[c].gameObject.SendMessage("BreakAll", SendMessageOptions.DontRequireReceiver);
                }
            }
        }

        gameObject.SendMessage("Explode", SendMessageOptions.DontRequireReceiver);

        PlaySound(distance);

        if (explosionEmitter != null)
        {
            GameObject.Instantiate(explosionEmitter, transform.position, Quaternion.identity);
        }
    }

    void PlaySound(float distance)
    {
        int sIndex = 0;

        if (distance < farSoundDistance)
        {
            sIndex = Random.Range(0, nearSounds.Length);
            GetComponent<AudioSource>().PlayOneShot(nearSounds[sIndex]);
            timer = nearSounds[sIndex].length + 1.0f;
        }
        else
        {
            sIndex = Random.Range(0, farSounds.Length);
            GetComponent<AudioSource>().PlayOneShot(farSounds[sIndex]);
            timer = farSounds[sIndex].length + 1.0f;
        }
    }

    void Update()
    {
        if (thisTransform.position.y < minY)
        {
            Destroy(gameObject);
        }

        if (exploded)
        {
            if (timer > 0.0)
            {
                timer -= Time.deltaTime;

                if (timer <= 0.0)
                {
                    Destroy(gameObject);
                }
            }
        }
    }

    void OnCollisionEnter(Collision c)
    {
        if (exploded) return;

        Detonate();
    }

    void OnCollisionStay(Collision c)
    {
        if (exploded) return;

        Detonate();
    }

    void OnCollisionExit(Collision c)
    {
        if (exploded) return;

        Detonate();
    }
}
