class UnderwaterCamera extends MonoBehaviour
{
	public var effectComponents : MonoBehaviour[];
	public var waterLayer : LayerMask;
	
	private var thisT : Transform;
	private var effectState : boolean;
	private var hitInfo : RaycastHit;
	
	function OnEnable()
	{
		effectState = false;
		
		if(effectComponents == null) 
		{
			Destroy(this);
			return;
		}
		
		if(effectComponents.Length <= 0)
		{
			Destroy(this);
			return;
		}
		
		for(var i : int = 0; i < effectComponents.Length; i++)
		{
			effectComponents[i].enabled = false;
		}
		
		thisT = transform;
	}
	
	function OnDisable()
	{
		SwitchEffect(false);
	}
	
	function Update()
	{
		if(thisT == null) return;
		
        if(!GameQualitySettings.underwater)
        {
            SwitchEffect(false);
            return;
        }

		if(Physics.Raycast(thisT.position + new Vector3(0, 4, 0), -Vector3.up, hitInfo, 4.0, waterLayer))
		{
			if(hitInfo.collider.tag == "water")
			{
				SwitchEffect(true);
			}
			else
			{
				SwitchEffect(false);
			}
		}
		else
		{
			SwitchEffect(false);
		}
	}
	
	function SwitchEffect(b : boolean)
	{
		if(b == effectState) return;
		
		effectState = b;
		
		for(var i : int = 0; i < effectComponents.Length; i++)
		{
			effectComponents[i].enabled = b;
		}
	}
}