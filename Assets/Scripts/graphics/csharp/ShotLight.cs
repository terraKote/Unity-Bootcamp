using UnityEngine;
using System.Collections;

public class ShotLight : MonoBehaviour {

	public float time = 0.02f;
	private float timer;
	
	void OnEnable()
	{
		if (GetComponent<Light>() == null)
		{
			Destroy(this);
		}
		else
		{
			timer = time;
			GetComponent<Light>().enabled = false;
		}
	}

	void OnDisable()
	{
		if (GetComponent<Light>() == null)
		{
			Destroy(this);
		}
		else
		{
			timer = time;
			GetComponent<Light>().enabled = false;
		}
	}

	void LateUpdate()
	{
		timer -= Time.deltaTime;

		if (timer <= 0.0)
		{
			timer = time;
			GetComponent<Light>().enabled = !GetComponent<Light>().enabled;
		}
	}
}
