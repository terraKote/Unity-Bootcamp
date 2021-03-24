using UnityEngine;

public class GlassManager : MonoBehaviour
{
	static public float timeToPlayAudio;
	
	private void Start()
	{
		timeToPlayAudio = 0.0f;
	}
	
	private void Update()
	{
		timeToPlayAudio -= Time.deltaTime;
	}
}