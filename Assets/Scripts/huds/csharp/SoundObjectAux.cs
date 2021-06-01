using UnityEngine;
using System.Collections;

public class SoundObjectAux : MonoBehaviour
{
    public SoundObject soundGenerator;

    void Awake()
    {
        if (rigidbody != null) rigidbody.Sleep();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (soundGenerator == null)
            Destroy(this);
        else
            soundGenerator.OnCollisionEnter(collision);
    }
}
