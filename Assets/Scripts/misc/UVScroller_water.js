// Scroll main texture based on time

var scrollSpeed = 0.1;
function Update () 
{
    var offset = Time.time * scrollSpeed;
    //renderer.material.SetTextureOffset ("_LightMap", Vector2(offset/20, offset));
    
    var mat : Material = GetComponent.<Renderer>().material;
    mat.SetTextureOffset ("_MainTex", Vector2(offset*0.5, offset*1));
	mat.SetTextureOffset ("_HeightTex", Vector2(offset/2, offset));
	mat.SetTextureOffset ("_FoamTex", Vector2(offset/4, offset*1));
}