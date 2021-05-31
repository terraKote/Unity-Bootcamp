#pragma strict
#pragma implicit
#pragma downcast

class HudWeapons extends MonoBehaviour
{
	public var weapon : Texture2D[];
	public var ammunition : Texture2D[];
	public var ammunitionBackground : Texture2D[];

	public var selectedWeapon : int;
	public var maxAmmo : int[];
	public var ammoRemaining : int[];
	public var maxIcons : int[];
	public var clipsRemaining : int[];

	private var startCorner : Vector2;
	private var weaponRect : Rect[];
	private var ammunitionRect : Rect[];
	
	public var totalAmmoStyle : GUIStyle;
	public var ammoRemainingStyle : GUIStyle;
	
	private var alphaWeapon : float;
	private var alphaAmmo : float;
	private var auxColor : Color;
	private var cColor : Color;
	
	private var state : int;
	private var currentWeapon : int;
	private var currentAmmo : int;
	private var hideTime : float;
	public var fadeTime : float = 0.2;
	public var showTime : float = 2.0;
	
	function Start()
	{
		fadeTime = 1.0 / fadeTime;
		
		var i : int;
		state = 0;
		alphaWeapon = 0.0;
		alphaAmmo = 0.0;
		
		currentWeapon = selectedWeapon;
		currentAmmo = ammoRemaining[selectedWeapon];
		
		weaponRect = new Rect[weapon.length];
		for(i = 0; i < weaponRect.length; i++)
		{
			weaponRect[i] = new Rect(0, 0, weapon[i].width, weapon[i].height);
		}
		
		ammunitionRect = new Rect[ammunitionBackground.length];
		for(i = 0; i < ammunitionBackground.length; i++)
		{
			ammunitionRect[i] = new Rect(0, 0, ammunitionBackground[i].width, ammunitionBackground[i].height);
		}
	}
	
	function DrawGUI(event : Event)
	{
		if(alphaAmmo <= 0.0) return;
		
		auxColor = cColor = GUI.color;
		
		startCorner = new Vector2(Screen.width, Screen.height) - new Vector2(5, 5);
		
		selectedWeapon = Mathf.Clamp(selectedWeapon, 0, 1);
		
		ShowAmmunition();
		ShowSelectedWeapon();
		
		GUI.color = cColor;
	}
	
	function Update()
	{
		selectedWeapon = Mathf.Clamp(selectedWeapon, 0, 1);
		
		switch(state)
		{
			case 0:
				if(alphaAmmo > 0.0) alphaAmmo -= Time.deltaTime * fadeTime;
				if(alphaWeapon > 0.0) alphaWeapon -= Time.deltaTime * fadeTime;
				break;
			case 1:
				alphaAmmo = 0.0;
				
				if(alphaWeapon < 1.0) alphaWeapon += Time.deltaTime * fadeTime;
				break;
			case 2:
				alphaWeapon = 0.0;
				
				if(alphaAmmo < 1.0) alphaAmmo += Time.deltaTime * fadeTime;
				break;
		}
		
		if(selectedWeapon != currentWeapon)
		{
			currentWeapon = selectedWeapon;
			currentAmmo = ammoRemaining[selectedWeapon];
			state = 1;
			hideTime = showTime + ((1.0 - alphaWeapon) * (1 / fadeTime));
		}
		else if(currentAmmo != ammoRemaining[selectedWeapon])
		{
			currentAmmo = ammoRemaining[selectedWeapon];
			state = 2;
			hideTime = showTime + ((1.0 - alphaAmmo) * (1 / fadeTime));
		}
		else if(hideTime > 0.0)
		{
			hideTime -= Time.deltaTime;
			
			if(hideTime <= 0.0)
			{
				state = 0;
			}
		}
	}
	
	function ShowAmmunition()
	{
		auxColor.a = alphaAmmo;
		GUI.color = auxColor;
		
		ammunitionRect[selectedWeapon].x = startCorner.x - ammunitionRect[selectedWeapon].width;
		ammunitionRect[selectedWeapon].y = startCorner.y - ammunitionRect[selectedWeapon].height;
		
		GUI.DrawTexture(ammunitionRect[selectedWeapon], ammunitionBackground[selectedWeapon]);
		
		var delta : float = Mathf.Clamp(ammoRemaining[selectedWeapon], 0, maxAmmo[selectedWeapon]);
		delta /= maxAmmo[selectedWeapon];
		delta *= maxIcons[selectedWeapon]; 
		
		var length : int = delta;
		for(var i : int = 0; i < length; i++)
		{
			GUI.DrawTexture(new Rect(ammunitionRect[selectedWeapon].x + 40 + (i * (ammunition[selectedWeapon].width - 1)), ammunitionRect[selectedWeapon].y + 28, ammunition[selectedWeapon].width, ammunition[selectedWeapon].height), ammunition[selectedWeapon]);
		}
		
		var auxRect = new Rect(ammunitionRect[selectedWeapon].x + 40, ammunitionRect[selectedWeapon].y + 2, 20, 20);

		GUI.Label(auxRect, clipsRemaining[selectedWeapon].ToString(), totalAmmoStyle);
		auxRect.x += 17;
		auxRect.y -= 1;
		GUI.Label(auxRect, "|", totalAmmoStyle);
		auxRect.x += 6;
		auxRect.y += 4;
		GUI.Label(auxRect, ammoRemaining[selectedWeapon].ToString(), ammoRemainingStyle);
	}
	
	function ShowSelectedWeapon()
	{
		auxColor.a = alphaWeapon;
		GUI.color = auxColor;
		
		weaponRect[selectedWeapon].x = startCorner.x - weaponRect[selectedWeapon].width;
		weaponRect[selectedWeapon].y = startCorner.y - weaponRect[selectedWeapon].height;
		
		GUI.DrawTexture(weaponRect[selectedWeapon], weapon[selectedWeapon]);
	}
}