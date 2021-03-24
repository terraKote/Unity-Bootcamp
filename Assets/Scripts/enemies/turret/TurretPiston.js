#pragma strict
#pragma implicit
#pragma downcast

class TurretPiston extends MonoBehaviour
{	
	public var objectToFollow : Transform;
	private var t : Transform;
	public var axis : Vector3 = new Vector3(1, 0, 0);
	
	function Start()
	{
		if(objectToFollow == null)
		{
			Destroy(gameObject);
			return;
		}
		
		t = transform;
	}
	
	function Update()
	{
		t.rotation = Quaternion.FromToRotation(axis, (objectToFollow.position - t.position).normalized);
	}
}