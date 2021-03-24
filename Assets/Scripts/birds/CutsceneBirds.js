#pragma strict
#pragma implicit
#pragma downcast

class CutsceneBirds extends MonoBehaviour
{
	public var sounds : AudioClip[] = new AudioClip[0];
	public var soundFrequency = 1.00;
	
	public var animationSpeed : float = 1.00;
	
	public var minSpeed = 0.00;
	public var turnSpeed = 0.00;
	public var randomFreq = 0.00;
	
	public var randomForce = 0.00;
	public var toOriginForce = 0.00;
	public var toOriginRange = 0.00;
	
	public var damping = 0.00;
	
	public var gravity = 0.00;
	
	public var avoidanceRadius = 0.00;
	public var avoidanceForce = 0.00;
	
	public var followVelocity = 0.00;
	public var followRadius = 0.00;
	
	public var bankTurn = 0.00;
	
	public var raycast = false;
	public var bounce = 0.80;
}