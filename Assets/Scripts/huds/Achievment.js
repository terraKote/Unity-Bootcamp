#pragma strict
#pragma implicit
#pragma downcast

class Achievment
{
	public var name : String;
	public var enabled : boolean;
	public var done : boolean;
	public var icon : Texture2D;
	public var description : String;
	public var progress : int;
	public var maxProgress : int;
	public var showProgress : boolean;
}