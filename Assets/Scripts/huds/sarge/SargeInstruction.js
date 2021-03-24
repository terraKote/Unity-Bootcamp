#pragma strict
#pragma implicit
#pragma downcast

class SargeInstruction
{
	public var name : String;
	public var text : String;
	public var texture : Texture2D;
	public var timeToDisplay : float = 3.0;
	public var audio : AudioClip;
	public var queuable : boolean = true;
	public var overridable : boolean = false;
	public var volume : float = 1.0;
}