#pragma strict
#pragma implicit
#pragma downcast

private var timer : float; 

public var cols : Collider[];

function Start()
{
	for(var i : int = 0; i < cols.length; i++)
	{
		Physics.IgnoreCollision(cols[i], collider);
	}
}

function OnCollisionEnter (collision : Collision) 
{
	if(timer <= 0.0)
	{
		timer = 0.2;
		SendMessageUpwards("OnFootStrike", SendMessageOptions.DontRequireReceiver);
	}
}

function Update()
{
	timer -= Time.deltaTime;
}