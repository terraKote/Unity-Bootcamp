#pragma strict
#pragma implicit
#pragma downcast

class FadeOnVisible extends MonoBehaviour
{
	public var alpha : float;
	
	public var originalMaterials : Material[];
	public var fadeMaterials : Material[];
	public var colors : Color[];
	
	public var r : MeshRenderer;
	public var fadeShader : Shader;
	
	private var mLength : int;
	private var i : int;
	
	function Start()
	{
		fadeShader = Shader.Find("Transparent/VertexLit");
		
		if(renderer == null) 
		{
			Destroy(this);
			return;
		}
		
		alpha = 0.0;
		
		if(r == null) 
		{
			r = renderer;
			
			mLength = r.materials.length;
			colors = new Color[mLength];
			originalMaterials = new Material[mLength];
			fadeMaterials = new Material[mLength];
			
			for(i = 0; i < mLength; i++)
			{
				originalMaterials[i] = r.materials[i];
				colors[i] = r.materials[i].color;
				fadeMaterials[i] = new Material(fadeShader);
				fadeMaterials[i].color = colors[i];
			}
			
			for(i = 0; i < mLength; i++)
			{
				r.materials[i] = fadeMaterials[i];
				colors[i].a = 0.0;
			}
		}
	}
	
	function OnBecameVisible () 
	{
		if(!enabled && alpha == 0.0 && Time.time > 0.2)
		{
			enabled = true;
		}
	}

	function OnBecameInvisible()
	{
		alpha = 0.0;
		enabled = false;
		
		for(i = 0; i < mLength; i++)
		{
			r.materials[i] = fadeMaterials[i];
			colors[i].a = 0.0;
		}
	}
	
	function Update()
	{
		alpha += Time.deltaTime;
		
		for(i = 0; i < mLength; i++)
		{
			colors[i].a = alpha;
			fadeMaterials[i].color = colors[i];
		}
		
		if(alpha >= 1.0)
		{
			for(i = 0; i < mLength; i++)
			{
				colors[i].a = 1.0;
				r.materials[i] = originalMaterials[i];
				r.materials[i].color = colors[i];
			}
			
			alpha = 1.0;
			enabled = false;
		}
	}
}