using System.Collections;
using UnityEngine;

namespace Bootcamp.Weapons
{
    public class GunParticles : MonoBehaviour
    {
        private bool cState;
        private ParticleEmitter[] emitters;

        void Start()
        {
            cState = true;

            emitters = GetComponentsInChildren<ParticleEmitter>();

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
                    emitters[i].emit = p_newState;
                }
            }
        }
    }
}