#pragma strict
#pragma implicit
#pragma downcast

class SimpleRotation extends MonoBehaviour
{
	public var desiredAxis : DesiredAxis;
	public var visualSlowSpeed : float = 0.5;
	public var framePerVisualRotation : int = 4;
	
	private var t : Transform;
	private var axis : Vector3;
	
	function Start()
	{
		t = transform;
		
		axis = new Vector3(1, 0, 0);
		
		switch(desiredAxis)
		{
			case DesiredAxis.Y:
				axis = new Vector3(0, 1, 0);
				break;
			case DesiredAxis.Z:
				axis = new Vector3(0, 0, 1);
				break;
		}
	}
	
	function Update()
	{
        if(GameManager.pause) return;
		
		t.Rotate(axis * (visualSlowSpeed * 360f * Time.deltaTime + 360f / framePerVisualRotation));
	}
}

enum DesiredAxis
{
	X,
	Y,
	Z
}