#pragma strict
#pragma implicit
#pragma downcast

class FriendlyChopper extends MonoBehaviour
{
	public var sarge : SargeManager;
	
    function Start()
    {
        var sargeObject : GameObject = GameObject.Find("SargeManager") as GameObject;

        if(sargeObject != null)
        {
            sarge = sargeObject.GetComponent("SargeManager") as SargeManager;
        }
    }

	function Hit(hitInfo : RaycastHit)
	{
        if(sarge == null) return;

		sarge.FriendlyFire();
	}
}