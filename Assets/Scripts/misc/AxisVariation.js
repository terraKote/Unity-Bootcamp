public var amplitude : Vector3;
public var speed : float;
private var startPosition : Vector3;

private var runningTime : float;

function OnEnable()
{
    runningTime = (Mathf.PI / speed);// * 0.5;
    startPosition = transform.localPosition;
}

function LateUpdate() 
{
    if(GameManager.pause) return;

    transform.localPosition = startPosition + Mathf.Sin(runningTime * speed) * amplitude;
    runningTime += Time.deltaTime;
}