#pragma strict
#pragma implicit
#pragma downcast

class TrainingDummyPartHelper extends MonoBehaviour
{
	public var attached : boolean;
	public var index : int;
	public var dummy : TrainingDummy;
	
	function Hit(hit : RaycastHit)
	{
		if(dummy != null) dummy.Hit(hit, index);
	}
	
	function Destruct()
	{
		if(dummy != null) dummy.Destruct(index);
	}
}