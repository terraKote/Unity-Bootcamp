#pragma strict
#pragma implicit
#pragma downcast

class SoldierDamageControl extends MonoBehaviour
{
	public var life : float;
	
	public var hitTexture : Texture2D;
	public var blackTexture : Texture2D;
	
	private var hitAlpha : float;
	private var blackAlpha : float;
	
	private var recoverTime : float;
	
	public var hitSounds : AudioClip[];
	public var dyingSound : AudioClip;
	
	function Start()
	{
		SoldierController.dead = false;
		hitAlpha = 0.0;
		blackAlpha = 0.0;
		life = 1.0;
	}
	
	function HitSoldier(hit : String)
	{
		if(GameManager.receiveDamage)
		{
			life -= 0.05;
			
			if(!audio.isPlaying)
			{
				if(life < 0.5 && (Random.Range(0, 100) < 30))
				{
					audio.clip = dyingSound;
				}
				else
				{
					audio.clip = hitSounds[Random.Range(0, hitSounds.length)];
				}
				
				audio.Play();
			}
			
			recoverTime = (1.0 - life) * 10.0;
			
			if(hit == "Dummy")
			{
				TrainingStatistics.dummiesHit++;
			}
			else if(hit == "Turret")
			{
				TrainingStatistics.turretsHit++;
			}
			
			if(life <= 0.0)
			{
				SoldierController.dead = true;
			}
		}
	}
	
	function Update()
	{
		recoverTime -= Time.deltaTime;
		
		if(recoverTime <= 0.0)
		{
			life += life * Time.deltaTime;
			
			life = Mathf.Clamp(life, 0.0, 1.0);
			
			hitAlpha = 0.0;
		}
		else
		{
			hitAlpha = recoverTime / ((1.0 - life) * 10.0);
		}
	
		if(!SoldierController.dead) return;
		
		blackAlpha += Time.deltaTime;
		
		if(blackAlpha >= 1.0)
		{
			Application.LoadLevel(1);
		}
	}
	
	function OnGUI()
	{
		if(!GameManager.receiveDamage) return;
		
		var oldColor : Color;
		var auxColor : Color;
		oldColor = auxColor = GUI.color;
		
		auxColor.a = hitAlpha;
		GUI.color = auxColor;
		GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), hitTexture);
		
		auxColor.a = blackAlpha;
		GUI.color = auxColor;
		GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), blackTexture);
		
		GUI.color = oldColor;
	}	
}