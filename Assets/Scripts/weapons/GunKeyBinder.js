#pragma strict
#pragma implicit
#pragma downcast

@script System.Serializable
class GunKeyBinder
{
	public var gun : Gun;
	public var keyToActivate : KeyCode;
	public var switchModesOnKey : boolean;
}