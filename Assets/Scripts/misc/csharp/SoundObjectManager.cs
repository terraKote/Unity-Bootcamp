using UnityEngine;

public class SoundObjectManager : MonoBehaviour
{
    public float minSpeedToParticle = 3.0f;
    public GameObject genericParticle;
    public GameObject waterParticles;
    public LayerMask waterLayer;

    public float minSpeedSound = 2.0f;
    public float maxSpeedSound = 10.0f;

    public AudioClip defaultSound;
    public AudioClip defaultMetalSound;
    public AudioClip defaultWoodSound;
    public AudioClip defaultConcreteSound;

    public AudioClip[] additionalSounds;

    static public SoundObjectManager instance;

    void Awake()
    {
        instance = this;
    }
}
