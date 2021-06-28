using UnityEngine;
using UnityEngine.UI;

public class RadarHudService : PausableBehaviour
{
    [Header("Shared Settings")]
    [SerializeField] private Transform soliderCamera;
    [SerializeField] private Transform player;

    [Header("Map Settings")]
    [SerializeField] private Transform radarCamera;
    [SerializeField] private float targetHeight = 10.0f;

    [Header("Arrow Settings")]
    [SerializeField] private RectTransform arrowContainer;
    [SerializeField] private Transform arrowTarget;
    [SerializeField] private float fadeDistance = 60.0f;
    [SerializeField] private float fadeOffset = 10.0f;
    [SerializeField] Image arrowRenderer;

    private void LateUpdate()
    {
        RotateMap();
        RotateCompassArrow();
        FadeCompassArrow();
    }

    private void FadeCompassArrow()
    {
        float distanceToTarget = Vector3.Distance(player.position, arrowTarget.position);
        float fade = Mathf.Clamp01((distanceToTarget + fadeOffset) / fadeDistance);
        Color color = arrowRenderer.color;
        color.a = fade;
        arrowRenderer.color = color;
    }

    private void RotateCompassArrow()
    {
        Vector3 directionToTarget = soliderCamera.InverseTransformPoint(arrowTarget.position);
        var angle = Mathf.Atan2(directionToTarget.x, directionToTarget.z) * Mathf.Rad2Deg;
        arrowContainer.eulerAngles = new Vector3(0, 0, -angle);
    }

    private void RotateMap()
    {
        radarCamera.position = player.position + new Vector3(0, targetHeight, 0);
        radarCamera.localRotation = Quaternion.Euler(90, soliderCamera.localRotation.eulerAngles.y, 0);
    }

    public void OnPause()
    {
        gameObject.SetActive(false);
    }

    public void OnUnPause()
    {
        gameObject.SetActive(true);
    }
}
