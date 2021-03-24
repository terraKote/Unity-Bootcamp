static var heat = 0.00;

function Update ()
{
	if(heat > 0) heat -= Time.deltaTime;
}