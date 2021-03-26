using UnityEngine;

namespace Bootcamp.Misc.WaterFX
{
    [AddComponentMenu("Bootcamp/Misc/Water FX/Water Shader Helper")]
    public class WaterShaderHelper : MonoBehaviour
    {
        [SerializeField] Transform _lightDirection;
        private Material _material;

        private void Start()
        {
            _material = GetComponent<SkinnedMeshRenderer>().material;
        }

        public void Update()
        {
            _material.shader.maximumLOD = QualitySettings.GetQualityLevel() * 100;

            if (_lightDirection)
                _material.SetVector("_WorldLightDir", _lightDirection.forward);
            else
                _material.SetVector("_WorldLightDir", new Vector3(0.7f, 0.7f, 0.0f));
        }
    }
}
