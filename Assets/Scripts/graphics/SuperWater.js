@script ExecuteInEditMode

var theLight : Transform;

function Update () 
{
    
    renderer.sharedMaterial.SetVector("_WorldLightDir", -theLight.forward);

}