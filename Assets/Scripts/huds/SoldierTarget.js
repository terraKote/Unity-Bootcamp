#pragma strict
#pragma implicit
#pragma downcast

class SoldierTarget extends MonoBehaviour
{
	public var target : Texture2D;
	public var targetOver : Texture2D;
	
	public var overEnemy : boolean;
	private var _overEnemy : boolean;
	
	private var gui : GUITexture;
	
	public var aim : boolean;
	private var _aim : boolean;
	
	public var enemyLayer : LayerMask;
	public var otherLayer : LayerMask;
	
	public var enemyDistance : float = 50.0;
	
	public var soldierCam : Camera;
	
	public var soldierTarget : Transform;
	public var soldierController : SoldierController;
	public var soldierCamera : SoldierCamera;
	
	function OnEnable()
	{
		soldierTarget.parent = null;
		
		gui = guiTexture;
		
		gui.pixelInset = new Rect(-target.width * 0.5, -target.height * 0.5, target.width, target.height);
		gui.texture = target;
		
		gui.color = new Color(0.5, 0.5, 0.5, 0.15);
	}
	
	function Update()
	{	
		if(!soldierCam.gameObject.active) 
		{
			gui.color = new Color(0.5, 0.5, 0.5, 0.0);
			return;
		}
	
		aim = Input.GetButton("Fire2");
		
		var ray : Ray = soldierCam.ScreenPointToRay(new Vector3(Screen.width * 0.5, Screen.height * 0.5, 0));
		
		var hit1 : RaycastHit;
		var hit2 : RaycastHit;
		
		overEnemy = Physics.Raycast(ray.origin, ray.direction, hit1, enemyDistance, enemyLayer);
		
		if(overEnemy)
		{
			if(Physics.Raycast(ray.origin, ray.direction, hit2, enemyDistance, otherLayer))
			{
				overEnemy = hit1.distance < hit2.distance;
			}
		}
		
		var delta : float = 1.0 - ((soldierCamera.y + 85) * 0.0058823529);
		
		if(!soldierController.crouch)
		{
			if(soldierController.aim)
			{
				soldierTarget.position = soldierCam.ScreenToWorldPoint(new Vector3 (Screen.width * 0.5, Screen.height * (0.3 + (delta * 0.24)), 10));
			}
			else
			{
				soldierTarget.position = soldierCam.ScreenToWorldPoint(new Vector3 (Screen.width * 0.6, Screen.height * (0.4 + (delta * 0.16)), 10));
			}
		}
		else
		{
			if(soldierController.aim)
			{
				soldierTarget.position = soldierCam.ScreenToWorldPoint(new Vector3 (Screen.width * 0.7, Screen.height * (0.3 + (delta * 0.24)), 10));
			}
			else
			{
				soldierTarget.position = soldierCam.ScreenToWorldPoint(new Vector3 (Screen.width * 0.7, Screen.height * (0.4 + (delta * 0.16)), 10));
			}
		}
		
		
		if(overEnemy != _overEnemy)
		{
			_overEnemy = overEnemy;
			
			if(overEnemy)
			{
				gui.texture = targetOver;
			}
			else
			{
				gui.texture = target;
			}
		}
		
		if(aim != _aim)
		{
			_aim = aim;
			
			if(aim)
			{
				gui.color = new Color(0.5, 0.5, 0.5, 0.75);
			}
			else
			{
				gui.color = new Color(0.5, 0.5, 0.5, 0.15);
			}
		}
	}
}