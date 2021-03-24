#pragma strict
#pragma implicit
#pragma downcast

class DestroyOnHit extends MonoBehaviour
{
	public var hitsToDestroy : int = 1;
	public var destructionParticles : GameObject;
	public var destroyOnExplosion : boolean = true;
	public var destroyParent : boolean = true;
	
	function Start()
	{
		gameObject.layer = 11;
	}
	
	function Destruct()
	{
		if(destroyOnExplosion)
		{
			DestroyObject();
		}
	}
	
	function Hit(hit : RaycastHit)
	{
		hitsToDestroy--;
		
		if(hitsToDestroy <= 0)
		{
			DestroyObject();
		}
	}
	
	function DestroyObject()
	{
		if(destructionParticles != null)
		{
			GameObject.Instantiate(destructionParticles, transform.position, Quaternion.identity);
		}
		
		if(destroyParent)
		{
			if(transform.parent != null)
			{
				Destroy(transform.parent.gameObject);
			}
			else
			{
				Destroy(gameObject);
			}
		}
		else
		{
			Destroy(gameObject);
		}
	}
}