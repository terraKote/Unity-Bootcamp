using UnityEngine;

public class GunParticles : MonoBehaviour
{
    private bool cState;
    private ParticleSystem[] emitters;

    void Start()
    {
        cState = true;

        emitters = GetComponentsInChildren<ParticleSystem>();

        ChangeState(false);
    }

    public void ChangeState(bool p_newState)
    {
        if (cState == p_newState) return;

        cState = p_newState;

        if (emitters != null)
        {
            for (int i = 0; i < emitters.Length; i++)
            {
                if (p_newState)
                {
                    emitters[i].Play();
                }
                else
                {
                    emitters[i].Stop();
                }
            }
        }
    }
}