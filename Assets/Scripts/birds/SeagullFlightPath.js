#pragma strict
#pragma implicit
#pragma downcast

var flySpeed = 15.00;
var highFlyHeight = 80.00;
var normalFlyHeight = 40.00;
var lowFlyHeight = 20.00;
var flyDownSpeed = 0.10;
var circleRadius = 60.00;
var circleSpeed = 0.20;
var circleTime = 15.00;
var awayTime = 20.00;

var offset : Vector3;

private var myT : Transform;
public var player : Transform;
private var awayDir : Vector3;
private var flyHeight = 0.00;
private var col : Collider;
private var hit : RaycastHit;
private var distToTarget = 0.00;
private var lastHeight = 0.00;
private var height = 0.00;
private var terrainSize : Vector3;
private var terrainData : TerrainData;

private var dTime = 0.1;

function Start ()
{
	terrainData = Terrain.activeTerrain.terrainData;
	terrainSize = terrainData.size;
	col = Terrain.activeTerrain.collider;
	myT = transform;
	//player = gameObject.FindWithTag("Player").transform;
	MainRoutine();	
}

function MainRoutine ()
{
	while(true)
	{
		yield ReturnToPlayer();
		yield CirclePlayer();
		yield FlyAway();
	}
}

function ReturnToPlayer()
{
	distToTarget = 100.00;
	while(distToTarget > 10)
	{
		var toPlayer : Vector3 = player.position - myT.position;
		toPlayer.y = 0;
		distToTarget = toPlayer.magnitude;
		
		var targetPos : Vector3;
		var normal : Vector3;
		
		if(distToTarget > 0) targetPos = transform.position + ((toPlayer / distToTarget) * 10);
		else targetPos = Vector3.zero;
		
		targetPos.y = terrainData.GetInterpolatedHeight(targetPos.x / terrainSize.x, targetPos.z / terrainSize.z);
		normal = terrainData.GetInterpolatedNormal(targetPos.x / terrainSize.x, targetPos.z / terrainSize.z);
		offset = Vector3(normal.x * 40, 0, normal.z * 40);
		
		flyHeight = (distToTarget > 80) ? highFlyHeight : lowFlyHeight;
		if(distToTarget > 0) Move(targetPos - transform.position);
		yield WaitForSeconds(dTime);	
	}	
}

function CirclePlayer()
{
	var time = 0.00;
	while(time < circleTime)
	{
		var circlingPos : Vector3 = player.position + Vector3(Mathf.Cos(Time.time * circleSpeed) * circleRadius, 0, Mathf.Sin(Time.time * circleSpeed) * circleRadius);
		circlingPos.y = terrainData.GetInterpolatedHeight(circlingPos.x / terrainSize.x, circlingPos.z / terrainSize.z);
		var normal : Vector3 = terrainData.GetInterpolatedNormal(circlingPos.x / terrainSize.x, circlingPos.z / terrainSize.z);
		offset = Vector3(normal.x * 40, 0, normal.z * 40);

		flyHeight = normalFlyHeight;
		Move(circlingPos - myT.position);
		time += dTime;
		yield WaitForSeconds(dTime);	
	}	
}

function FlyAway()
{
	var radians : float = Random.value * 2 * Mathf.PI;
	awayDir = Vector3(Mathf.Cos(radians), 0, Mathf.Sin(radians));
	var time = 0.00;
	while(time < awayTime)
	{
		var away : Vector3 = player.position + (awayDir * 1000);
		away.y = 0;
		
		var toAway : Vector3 = away - transform.position;
		
		distToTarget = toAway.magnitude;
		
		var targetPos : Vector3;
		var normal : Vector3;
		
		if(distToTarget > 0) targetPos = transform.position + ((toAway / distToTarget) * 10);
		else targetPos = Vector3.zero;
		
		targetPos.y = terrainData.GetInterpolatedHeight(targetPos.x / terrainSize.x, targetPos.z / terrainSize.z);
		normal = terrainData.GetInterpolatedNormal(targetPos.x / terrainSize.x, targetPos.z / terrainSize.z);
		offset = Vector3(normal.x * 40, 0, normal.z * 40);
		
		flyHeight = highFlyHeight;
		Move(targetPos - transform.position);
		time += dTime;
		yield WaitForSeconds(dTime);	
	}	
}

function Move (delta : Vector3)
{
	delta.y = 0;
	delta = delta.normalized * flySpeed * dTime;
	var newPos : Vector3 = Vector3(myT.position.x + delta.x, 1000, myT.position.z + delta.z);
	
	var newHeight : float;
	
	if(col.Raycast(Ray(newPos, -Vector3.up), hit, 2000)) newHeight = hit.point.y;
	else newHeight = 0.00;
	if(newHeight < lastHeight) height = Mathf.Lerp(height, newHeight, flyDownSpeed * dTime);
	else height = newHeight;
	lastHeight = newHeight;
	myT.position = Vector3(newPos.x, Mathf.Clamp(height, 35.28, 1000.00) + flyHeight, newPos.z);
}