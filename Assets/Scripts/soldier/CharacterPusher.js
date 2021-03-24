#pragma strict
#pragma implicit
#pragma downcast

class CharacterPusher extends MonoBehaviour
{
	public var pushPower : float = 2.0;
	
	function OnControllerColliderHit(hit : ControllerColliderHit)
	{
		 var body : Rigidbody = hit.collider.attachedRigidbody;
		 
	    if (body == null || body.isKinematic) return;
	        
	    if (hit.moveDirection.y < -0.3) return;
	    
	    var pushDir : Vector3 = Vector3 (hit.moveDirection.x, 0, hit.moveDirection.z);

	    body.velocity = pushDir * pushPower;
	}
}