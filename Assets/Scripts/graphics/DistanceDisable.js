#pragma strict
#pragma implicit
#pragma downcast

enum ObjectState
{
	ENABLED,
	DISABLED,
	ENABLING,
	DISABLING
}

class ObjectInfo
{
	public var transform : Transform;
	public var renderer : Renderer;
	public var material : Material;
	public var baseShader : Shader;
	public var state : ObjectState;
	public var color : Color;
	public var lastUpdate : float;
	public var distance : float;
	public var oColor : Color;
	public var gameObject : GameObject;
}

class DistanceDisable extends MonoBehaviour
{
	public var objectsPerFrame : int = 30;
	public var minObjectDistance : float = 50.0;
	public var maxObjectDistance : float = 100.0;
	public var objects : Array;
	public var soldierRef : Transform;
	private var index : int;
	public var alphaShader : Shader;
	private var maxObjectDistanceNormal : float;
	
	function Start()
	{
		alphaShader = Shader.Find("Transparent/VertexLit");
		
		if(soldierRef == null)
		{
			Destroy(this);
		}
		else
		{
			index = 0;
			
			maxObjectDistance *= maxObjectDistance;
			minObjectDistance *= minObjectDistance;
			maxObjectDistanceNormal = 1.0 / maxObjectDistance;
			
			objects = new Array();
		
			GetAllChilds(transform);
		}
	}
	
	function GetAllChilds(t : Transform)
	{
		var auxGO = t.gameObject;

		if(auxGO.renderer != null)
		{
			var dObject : ObjectInfo = new ObjectInfo();
			dObject.transform = t;
			dObject.renderer = auxGO.renderer;
			dObject.state = ObjectState.ENABLED;
			dObject.material = dObject.renderer.material;
			dObject.baseShader = dObject.material.shader;
			dObject.color = dObject.material.color;
			dObject.oColor = dObject.material.color;
			dObject.gameObject = auxGO;
			objects.Add(dObject);
		}
		
		for(var i : int = 0; i < t.childCount; i++)
		{
			GetAllChilds(t.GetChild(i));
		}
	}
	
	function Update()
	{
		if(objects == null) return;
		
		var soldierPos = soldierRef.position;
		
		var cObject : ObjectInfo;
		
		for(var i : int = index; (i < objects.length) && (i < index + objectsPerFrame); i++)
		{
			cObject = objects[i];
			
			if(!cObject.gameObject.active) continue;
			
			cObject.distance = (cObject.transform.position - soldierPos).sqrMagnitude;
			
			if(cObject.distance > maxObjectDistance)
			{
				if(cObject.renderer.enabled)
				{
					cObject.renderer.enabled = false;
				}
			}
			else if(cObject.distance > minObjectDistance)
			{
				if(!cObject.renderer.enabled)
				{
					cObject.renderer.enabled = true;
				}
				
				cObject.material.shader = alphaShader;
				cObject.color.a = Mathf.Clamp(1.0 - ((cObject.distance - minObjectDistance) * maxObjectDistanceNormal), 0.0, 1.0);
				cObject.material.color = cObject.color;
			}
			else
			{
				if(!cObject.renderer.enabled)
				{
					cObject.renderer.enabled = true;
				}
				
				cObject.material.shader = cObject.baseShader;
				cObject.material.color = cObject.oColor;
			}
			
			cObject.lastUpdate = Time.time;
		}
		
		index += objectsPerFrame;
		
		if(index >= objects.length) index = 0;
	}
}