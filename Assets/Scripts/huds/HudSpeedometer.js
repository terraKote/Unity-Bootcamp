#pragma strict
#pragma implicit
#pragma downcast

class HudSpeedometer extends MonoBehaviour
{
	public var currentSpeed : float;
	public var currentGear : int;
	public var currentRPM : float;
	
	public var minSpeed : float;
	public var maxSpeed : float;
	public var minSpeedAngle : float;
	public var maxSpeedAngle : float;
	
	public var pointer : Texture;
	public var rpmText : GUIText;
	public var gearText : GUIText;
	
	private var targetAngle : float;
	private var _maxSpeed : float;
	private var pointerPos : Vector2;
	private var pivot : Vector2 = new Vector2(17, 17);
	
	function Start()
	{
		_maxSpeed = 1.0 / maxSpeed;
	}
	
	function Update()
	{
		currentSpeed = Mathf.Clamp(currentSpeed, minSpeed, maxSpeed);
		
		targetAngle = Mathf.Lerp(minSpeedAngle, maxSpeedAngle, (currentSpeed - minSpeed) * _maxSpeed);
		
		rpmText.text = parseInt(currentRPM).ToString();
		
		if(currentGear > 0)
		{
			gearText.text = currentGear.ToString();
		}
		else if(currentGear == 0)
		{
			gearText.text = "R";
		}
		else if(currentGear < 0)
		{
			gearText.text = "N";
		}
	}
	
	function OnGUI()
	{
		pointerPos = new Vector2(Screen.width - 110, Screen.height - 84);
		
		GUIUtility.RotateAroundPivot(targetAngle, pointerPos + pivot);
		GUI.DrawTexture(new Rect(pointerPos.x, pointerPos.y, 103, 34), pointer);
	}
}