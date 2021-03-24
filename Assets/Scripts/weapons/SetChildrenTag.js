#pragma strict
#pragma implicit
#pragma downcast

class SetChildrenTag extends MonoBehaviour
{
	public var desiredTag : String;
	
	function Start()
	{
		if(String.IsNullOrEmpty(desiredTag)) return;
		
		gameObject.tag = desiredTag;
		
		for(var i : int = 0; i < transform.childCount; i++)
		{
			SetTag(transform.GetChild(i));
		}
	}
	
	function SetTag(t : Transform)
	{
		t.tag = desiredTag;
		
		for(var i : int = 0; i < t.childCount; i++)
		{
			SetTag(t.GetChild(i));
		}
	}
}