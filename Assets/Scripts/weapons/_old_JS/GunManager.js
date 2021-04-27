#pragma strict
#pragma implicit
#pragma downcast

class GunManager extends MonoBehaviour
{
	public var guns : GunKeyBinder[];
	
	@HideInInspector
	public var currentGun : Gun;
	
	@HideInInspector
	public var currentWeapon : int;
	
	public var soldier : SoldierController;
	
	public var hud : HudWeapons;
	
	function Start()
	{
		for(var i : int = 0; i < guns.length; i++)
		{
			guns[i].gun.enabled = false;
		}
		currentWeapon = 0;
		guns[0].gun.enabled = true;
		currentGun = guns[0].gun;
	}
	
	function Update()
	{
		for(var i : int = 0; i < guns.length; i++)
		{
			if(Input.GetKeyDown(guns[i].keyToActivate))
			{
				ChangeToGun(i);
			}
		}
		
		hud.selectedWeapon = currentWeapon;
		hud.ammoRemaining[currentWeapon] = guns[currentWeapon].gun.currentRounds;
	}
	
	function ChangeToGun(gunIndex : int)
	{
		var cGun : Gun = guns[gunIndex].gun;
		
		if(cGun.enabled)
		{
			if(guns[gunIndex].switchModesOnKey)
			{
				switch(cGun.fireMode)
				{
					case FireMode.SEMI_AUTO:
						cGun.fireMode = FireMode.FULL_AUTO;
						break;
					case FireMode.FULL_AUTO:
						cGun.fireMode = FireMode.BURST;
						break;
					case FireMode.BURST:
						cGun.fireMode = FireMode.SEMI_AUTO;
						break;
				}
			}
		}
		else
		{
			for(var j : int = 0; j < guns.length; j++)
			{
				guns[j].gun.enabled = false;
			}
					
			cGun.enabled = true;
			currentGun = cGun;
			currentWeapon = gunIndex;
		}
	}
}