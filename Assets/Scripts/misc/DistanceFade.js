#pragma strict
#pragma implicit
#pragma downcast

class DistanceFade extends MonoBehaviour
{
	public var getDisabled : boolean;
	public var objectsToProcessPerFrame : int = 30;
	public var minDistance : float;
	public var maxDistance : float;
	public var soldierRef : Transform;
	public var fadeShader : Shader;
	public var objects : DistanceFadeObject[];
	
	private var currentObject : int;
	private var totalCount : int;
	
	private var fadingArray : Array;
	
	private var count : int;
	private var sPos : Vector3;
	private var cTransform : Transform;
	private var distance : float;
	private var obj : DistanceFadeObject;
	
	
	function Start()
	{
		if(objects == null)
		{
			Destroy(this);
			return;
		}
		
		if(objects.length <= 0)
		{
			Destroy(this);
			return;
		}
		
		fadingArray = new Array();
		
		objectsToProcessPerFrame = Mathf.Min(objectsToProcessPerFrame, objects.length);
		
		if(objectsToProcessPerFrame == 0)
		{
			objectsToProcessPerFrame = objects.length;
		}
		
		totalCount = objects.length;
		currentObject = 0;
	}
	
	function Update()
	{
		if(!soldierRef.gameObject.active) return;
		if(maxDistance <= 0.0) return;
		
		count = 0;
		sPos = soldierRef.position;
		
		while(count < objectsToProcessPerFrame)
		{
			count++;
			
			obj = objects[currentObject];
			
			if(obj != null)
			{
				cTransform = obj.transform;
				
				if(cTransform != null)
				{
					distance = (cTransform.position - sPos).magnitude;
					
					/*
					if(distance > maxDistance && obj.enabled)
					{
						obj.Disable();
					} 
					else if(distance <= maxDistance && !obj.enabled)
					{
						obj.StartFade();
						fadingArray.Add(obj);
					}
					//*/
					
					obj.DistanceBased(distance);
				}
				/*
				else
				{
					//objects[currentObject] = null;
				}
				//*/
			}
			
			NextObject();
		}
		
		/*
		var length : int = fadingArray.length;
		var deltaTime : float = Time.deltaTime;
		for(var i : int = 0; i < length; i++)
		{
			obj = fadingArray[i] as DistanceFadeObject;
			
			if(obj.DoFade(deltaTime))
			{
				fadingArray.RemoveAt(i);
				i--;
				length--;
			}
		}
		//*/
	}
	
	function NextObject()
	{
		currentObject++;
			
		if(currentObject >= totalCount)
		{
			currentObject = 0;
		}
	}
	
	function SetMaxDistance(d : float)
	{
		maxDistance = d;
		minDistance = d - 4.0;
		
		for(var i : int = 0; i < totalCount; i++)
		{
			objects[i].SetMaxDistance(d);
		}
	}
}