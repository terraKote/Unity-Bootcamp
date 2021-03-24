#pragma strict
#pragma implicit
#pragma downcast

class TurretPart extends MonoBehaviour
{
	public var turret : Turret;
	
	function Destruct()
	{
		if(turret != null) turret.Destruct();
		
		Destroy(this);
	}
	
	function Hit(hit : RaycastHit)
	{
		if(turret != null) turret.Hit();
	}
}