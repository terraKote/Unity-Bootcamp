using UnityEngine;

namespace Bootcamp.misc.WaterFX
{
    [AddComponentMenu("Bootcamp/Misc/Water FX/UV Scroller")]
    public class UVScroller : MonoBehaviour
    {
        [SerializeField] float _scrollSpeed = -0.3f;
        private Material _material;

        private void Start()
        {
            _material = GetComponent<SkinnedMeshRenderer>().material;
        }

        private void Update()
        {
            float offset = Time.time * _scrollSpeed;

            _material.SetTextureOffset("_MainTex", new Vector2(offset * 0.5f, offset * 1));
            _material.SetTextureOffset("_HeightTex", new Vector2(offset / 2, offset));
            _material.SetTextureOffset("_FoamTex", new Vector2(offset / 4, offset * 1));
        }
    }
}
