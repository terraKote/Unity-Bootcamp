using UnityEngine;

public class SoldierTarget : MonoBehaviour
{
	public Texture2D target;
	public Texture2D targetOver;

	public bool overEnemy;
	private bool _overEnemy;

	private GUITexture gui;

	public bool aim;
	private bool _aim;

	public LayerMask enemyLayer;
	public LayerMask otherLayer;

public float enemyDistance = 50.0f;

	public Camera soldierCam;

	public Transform soldierTarget;
	public SoldierController soldierController;
	public SoldierCamera soldierCamera;

void OnEnable()
{
	soldierTarget.parent = null;

	gui = GetComponent<GUITexture>();

	gui.pixelInset = new Rect(-target.width * 0.5f, -target.height * 0.5f, target.width, target.height);
	gui.texture = target;

	gui.color = new Color(0.5f, 0.5f, 0.5f, 0.15f);
}

void Update()
{
	if (!soldierCam.gameObject.active)
	{
		gui.color = new Color(0.5f, 0.5f, 0.5f, 0.0f);
		return;
	}

	aim = Input.GetButton("Fire2");

	var ray = soldierCam.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0));

		RaycastHit hit1;
		RaycastHit hit2;

overEnemy = Physics.Raycast(ray.origin, ray.direction, out hit1, enemyDistance, enemyLayer);

if (overEnemy)
{
	if (Physics.Raycast(ray.origin, ray.direction, out hit2, enemyDistance, otherLayer))
	{
		overEnemy = hit1.distance < hit2.distance;
	}
}

var delta = 1.0f - ((soldierCamera.y + 85) * 0.0058823529f);

if (!soldierController.crouch)
{
	if (soldierController.aim)
	{
		soldierTarget.position = soldierCam.ScreenToWorldPoint(new Vector3(Screen.width * 0.5f, Screen.height * (0.3f + (delta * 0.24f)), 10));
	}
	else
	{
		soldierTarget.position = soldierCam.ScreenToWorldPoint(new Vector3(Screen.width * 0.6f, Screen.height * (0.4f + (delta * 0.16f)), 10));
	}
}
else
{
	if (soldierController.aim)
	{
		soldierTarget.position = soldierCam.ScreenToWorldPoint(new Vector3(Screen.width * 0.7f, Screen.height * (0.3f + (delta * 0.24f)), 10));
	}
	else
	{
		soldierTarget.position = soldierCam.ScreenToWorldPoint(new Vector3(Screen.width * 0.7f, Screen.height * (0.4f + (delta * 0.16f)), 10));
	}
}


if (overEnemy != _overEnemy)
{
	_overEnemy = overEnemy;

	if (overEnemy)
	{
		gui.texture = targetOver;
	}
	else
	{
		gui.texture = target;
	}
}

if (aim != _aim)
{
	_aim = aim;

	if (aim)
	{
		gui.color = new Color(0.5f, 0.5f, 0.5f, 0.75f);
	}
	else
	{
		gui.color = new Color(0.5f, 0.5f, 0.5f, 0.15f);
	}
}
	}
}