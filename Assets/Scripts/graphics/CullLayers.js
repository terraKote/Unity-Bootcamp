#pragma strict
#pragma implicit
#pragma downcast

class CullLayer
{
	public var layer : int;
	public var distance : float;
}

class CullLayers extends MonoBehaviour
{
	public var layers : CullLayer[];
	public var cameras : Camera[];
	
	function Start () 
	{
		var distances = new float[32];
		
		for(var i : int = 0; i < layers.length; i++)
		{
			if(layers[i].layer < 32)
			{
				distances[layers[i].layer] = Mathf.Max(0.0, layers[i].distance);
			}
		}
		
		if(cameras != null)
		{
			for(var j : int = 0; j < cameras.length; j++)
			{
				if (cameras[j] != null)
					cameras[j].layerCullDistances = distances;
			}
		}
	}
}