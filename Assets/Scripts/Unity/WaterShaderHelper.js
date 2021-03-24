

var lightDir : Transform;

function Update () 
{
    var mat : Material = renderer.material;
	mat.shader.maximumLOD = GameQualitySettings.water ? 600 : 300;
	if(lightDir)
		mat.SetVector("_WorldLightDir", lightDir.forward);
	else
		mat.SetVector("_WorldLightDir", Vector3(0.7,0.7,0.0));
}
