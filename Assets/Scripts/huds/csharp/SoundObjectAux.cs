using UnityEngine;
using System.Collections;

public class SoundObjectAux : MonoBehaviour
{
    public SoundObject soundGenerator;

    void Awake()
    {
        if (GetComponent<Rigidbody>() != null) GetComponent<Rigidbody>().Sleep();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (soundGenerator == null)
            Destroy(this);
        else
            soundGenerator.OnCollisionEnter(collision);
    }
}
