#pragma strict
#pragma implicit
#pragma downcast

class DistanceFadeObject
{
	public var renderer : Renderer;
	public var transform : Transform;
	public var materials : Material[];
	public var colors : Color[];
	public var colorName : String[];
	public var fadeColor : Color;
	
	public var enabled : boolean;
	
	//@HideInInspector
	public var minDistance : float;
	
	//@HideInInspector
	public var maxDistance : float;
	
	public var fadeShader : Shader;
	
	//@HideInInspector
	public var alphaMaterial : boolean;
	
	//@HideInInspector
	public var count : int;
	
	//@HideInInspector
	public var normalDistance : float;
	
	function Initialize(r : Renderer)
	{
		renderer = r;
		
		alphaMaterial = false;
		
		enabled = renderer.enabled;
		
		transform = renderer.transform;
		
		count = renderer.sharedMaterials.length;
		
		normalDistance = 1.0 / 10.0;
		
		materials = new Material[count];
		//fadeMaterials = new Material[count];
		colors = new Color[count];
		//fadeColors = new Color[count];
		colorName = new String[count];
		
		for(var i : int = 0; i < count; i++)
		{
			materials[i] = renderer.sharedMaterials[i];
			
			if(materials[i].HasProperty("_Color"))
			{
				colorName[i] = "_Color";
			}
			else if(materials[i].HasProperty("_MainColor"))
			{
				colorName[i] = "_MainColor";
			}
			
			colors[i] = materials[i].GetColor(colorName[i]);
		}
		
		fadeColor = colors[0];
	}
	
	function SetMaxDistance(d : float)
	{
		maxDistance = d;
		minDistance = maxDistance - 5.0;
		normalDistance = 1.0 / 5.0;//(maxDistance - minDistance);
	}
	
	function Disable()
	{
		renderer.enabled = false;
		enabled = false;
	}
	
	function StartFade()
	{
		if(!enabled)
		{
			renderer.enabled = true;
			enabled = true;
		}
		
		if(!alphaMaterial)
		{
			alphaMaterial = true;

			for(var i : int = 0; i < count; i++)
			{
				renderer.materials[i].shader = fadeShader;
			}
		}
		
		fadeColor.a = 0.0;
		
		for(var k : int = 0; k < count; k++)
		{
			renderer.materials[k].SetColor("_Color", fadeColor);
		}
	}
	
	function DoFade(deltaTime : float) : boolean
	{
		fadeColor.a += 0.5 * deltaTime;
		
		for(var k : int = 0; k < count; k++)
		{
			renderer.materials[k].SetColor("_Color", fadeColor);
		}
		
		if(fadeColor.a >= 1.0)
		{
			if(alphaMaterial)
			{
				alphaMaterial = false;
				
				for(var j : int = 0; j < count; j++)
				{
					renderer.materials[j].shader = materials[j].shader;
					renderer.materials[j].SetColor(colorName[j], colors[j]);
				}
			}
			
			return true;
		}
		else
		{
			return false;
		}
	}
	
	function DistanceBased(d : float)
	{
		if(d > maxDistance)
		{
			if(enabled)
			{
				renderer.enabled = false;
				enabled = false;
			}
		}
		else if(d > minDistance)
		{
			if(!enabled)
			{
				renderer.enabled = true;
				enabled = true;
			}
			
			if(!alphaMaterial)
			{
				alphaMaterial = true;

				for(var i : int = 0; i < count; i++)
				{
					renderer.materials[i].shader = fadeShader;
				}
			}
			
			fadeColor.a = 1.0 - ((d - minDistance) * normalDistance);
			
			for(var k : int = 0; k < count; k++)
			{
				renderer.materials[k].SetColor("_Color", fadeColor);
			}
		}
		else
		{
			if(!enabled)
			{
				renderer.enabled = true;
				enabled = true;
			}
			
			if(alphaMaterial)
			{
				alphaMaterial = false;
				
				for(var j : int = 0; j < count; j++)
				{
					renderer.materials[j].shader = materials[j].shader;
					renderer.materials[j].SetColor(colorName[j], colors[j]);
				}
			}
		}
	}
}