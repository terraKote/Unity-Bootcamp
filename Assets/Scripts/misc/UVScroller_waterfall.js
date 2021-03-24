// Scroll main texture based on time

var scrollSpeed = 0.1;
function Update () 
{
    var offset = Time.time * scrollSpeed;
    //renderer.material.SetTextureOffset ("_LightMap", Vector2(offset/20, offset));
    
    var mat : Material = renderer.material;
    mat.SetTextureOffset ("_MainTex", Vector2(offset/3, offset));
	mat.SetTextureOffset ("_BumpMap", Vector2(offset/3.5, offset));
	mat.SetTextureOffset ("_FoamTex", Vector2(offset/4, offset*2));
}