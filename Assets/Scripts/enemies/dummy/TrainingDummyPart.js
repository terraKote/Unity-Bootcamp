#pragma strict
#pragma implicit
#pragma downcast

class TrainingDummyPart
{
	@HideInInspector
	public var name : String;
	
	public var gameObject : GameObject;
	//public var boxCollider : BoxCollider;
	public var siblings : GameObject[];
	//public var siblingParts : TrainingDummyPartHelper[];
	public var brokeParts : GameObject[];
	public var shootsTaken : int;
	public var dummyPart : DummyPart;
	
	function Start()
	{
		shootsTaken = 0;
	}
}