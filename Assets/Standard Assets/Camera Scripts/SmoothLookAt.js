public var target : Transform;
public var damping = 6.0;
public var smooth = true;
private var alpha : float = 1.0f;
public var minDistance : float = 10.0f;
private var color : Color;
public var property : String;

//@script AddComponentMenu("Camera-Control/Smooth Look At")
function LateUpdate () 
{
	if (target) 
	{
		if (smooth)
		{
			// Look at and dampen the rotation
			var rotation = Quaternion.LookRotation(target.position - transform.position);
			transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * damping);
		}
		else
		{
			// Just lookat
		    transform.rotation = Quaternion.FromToRotation(-Vector3.forward, (new Vector3(target.position.x, transform.position.y, target.position.z) - transform.position).normalized);
			
			var distance : float = (target.position - transform.position).magnitude;
			
			if(distance < minDistance)
			{
				alpha = Mathf.Lerp(alpha, 0.0f, Time.deltaTime * 2f);
			}
			else
			{
				alpha = Mathf.Lerp(alpha, 1.0f, Time.deltaTime * 2f);
			}
			
			if(!String.IsNullOrEmpty(property))
			{
				color.a = Mathf.Clamp(alpha, 0.0f, 1.0f);
				
				renderer.material.SetColor(property, color);
			}
		}
	}
}

function Start () {

	if(renderer.material.HasProperty(property))
	{
		color = renderer.material.GetColor(property);
	}
	else
	{
		property = "";
	}
		
	// Make the rigid body not change rotation
   	if (rigidbody)
		rigidbody.freezeRotation = true;
}