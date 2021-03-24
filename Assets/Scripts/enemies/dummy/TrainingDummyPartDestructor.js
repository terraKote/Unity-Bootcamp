#pragma strict
#pragma implicit
#pragma downcast

class TrainingDummyPartDestructor extends MonoBehaviour
{
	private var alpha : float;
	private var timer : float;
	private var color : Color;
	private var r : MeshRenderer;
	private var colorName : String;
	
	function  Start()
	{
		r = renderer;
		
		if(r.material.HasProperty("_MainColor"))
		{
			colorName = "_MainColor";
		}
		else if(r.material.HasProperty("_Color"))
		{
			colorName = "_Color";
		}
		else
		{
			Destroy(gameObject);
			return;
		}
		
		color = r.material.GetColor(colorName);
		
		alpha = 1.0;
		timer = 3.0;
	}
	
	function Update()
	{
		if(timer > 0.0)
		{
			timer -= Time.deltaTime;
		}
		else
		{
			if(alpha > 0.0)
			{
				alpha -= Time.deltaTime;
				color.a = alpha;
				r.material.SetColor(colorName, color);
			}
			else
			{
				Destroy(gameObject);
			}
		}
	}
}