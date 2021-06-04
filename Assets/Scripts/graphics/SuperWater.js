@script ExecuteInEditMode

var theLight : Transform;

function Update () 
{
    
    GetComponent.<Renderer>().sharedMaterial.SetVector("_WorldLightDir", -theLight.forward);

}