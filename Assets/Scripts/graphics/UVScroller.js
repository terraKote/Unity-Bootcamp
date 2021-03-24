#pragma strict
#pragma implicit
#pragma downcast

@script AddComponentMenu("Aquiris/Graphics/UVScroller")
class UVScroller extends MonoBehaviour
{
	var scrollSpeed : Vector2;
	var texturesToScroll : String[];

	private var offset : Vector2;
	private var cRenderer : Renderer;

	function Start()
	{
		if(renderer == null)
		{
			Debug.LogWarning("UVScroller[\"" + gameObject.name + "\"]: There is no renderer attached to the gameObject.");
			Destroy(this);
			return;
		}
		else if(texturesToScroll == null)
		{
			Debug.LogWarning("UVScroller[\"" + gameObject.name + "\"]: You need to specify at least one texture to scroll.");
			Destroy(this);
			return;
		}
		else if(texturesToScroll.Length <= 0)
		{
			Debug.LogWarning("UVScroller[\"" + gameObject.name + "\"]: You need to specify at least one texture to scroll.");
			Destroy(this);
			return;
		}

		offset = Vector2.zero;
		cRenderer = renderer;
	}

	function Update () 
	{
		offset += scrollSpeed * 0.1 * Time.deltaTime;
	
		for(var textureName : String in texturesToScroll)
		{
			cRenderer.material.SetTextureOffset(textureName, new Vector2(offset.x, offset.y));
		}
	}
}