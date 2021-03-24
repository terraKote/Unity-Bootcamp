// Scroll main texture based on time

var scrollSpeed = 0.1;
function Update () 
{
    var offset = Time.time * scrollSpeed;
    //renderer.material.SetTextureOffset ("_LightMap", Vector2(offset/20, offset));
    
    renderer.material.SetTextureOffset ("_BumpMap", Vector2(offset/3, offset/-3));
}