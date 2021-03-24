#pragma strict
#pragma implicit
#pragma downcast

class HelicopterCutscene extends MonoBehaviour
{
	public var inChopperCamera : GameObject;
	public var cutsceneCamera : GameObject;
	
	public var soldier : GameObject;
	public var coleague : GameObject;
	public var soldierWeapon : GameObject;
	public var weaponAnimation : GameObject;
	public var rope : GameObject;
	
	public var childActive : boolean;
	
	private var currentPlaying : int;
	
	function Start()
	{
		currentPlaying = -1;
		rope.animation.Play("RopeAnimation");
		rope.animation["RopeAnimation"].enabled = true;
		rope.animation["RopeAnimation"].time = 0.05;
		rope.animation.Sample();
		rope.animation["RopeAnimation"].enabled = false;
		
		for(var t : Transform in transform)
		{
			t.gameObject.SetActiveRecursively(false);
			childActive = false;
		}
		
		animation["heli_load_animation"].wrapMode = WrapMode.Loop;
	}
	
	function Update()
	{
		switch(currentPlaying)
		{
			case 0:
				if(animation["heli_start_animation"].normalizedTime > 0.99)
				{
					currentPlaying = -1;
					SendMessageUpwards("HeliCutsceneEnd", 0);
				}
				break;
			case 1:
				break;
			case 2:
				if(coleague.animation["CS_Pointing"].normalizedTime > 0.99)
				{
					currentPlaying = -1;
					SendMessageUpwards("HeliCutsceneEnd", 2);
				}
				break;
			case 3:
				if(rope.animation["RopeAnimation"].normalizedTime > 0.99)
				{
					currentPlaying = -1;
					SendMessageUpwards("HeliCutsceneEnd", 3);
				}
				break;
		}
		
		if(Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
		{
			SendMessageUpwards("HeliCutsceneEnd", 3);
		}
	}
	
	function Play(step : int)
	{
		if(step > 3 || step < 0)
		{
			return;
		}
		
		currentPlaying = step;
		
		if(!childActive)
		{
			for(var t : Transform in transform)
			{
				t.gameObject.SetActiveRecursively(true);
				childActive = true;
			}
		}
				
		switch(step)
		{
			case 0:
				inChopperCamera.SetActiveRecursively(false);
				animation.Play("heli_start_animation");
				coleague.animation.CrossFade("CS_ColeagueIdle");
				break;
			case 1:
				inChopperCamera.SetActiveRecursively(true);
				cutsceneCamera.SetActiveRecursively(false);
				soldier.SetActiveRecursively(false);
				animation.Play("heli_load_animation");
				coleague.animation.CrossFade("CS_ColeagueIdle");
				break;
			case 2:
				inChopperCamera.SetActiveRecursively(true);
				cutsceneCamera.SetActiveRecursively(false);
				soldier.SetActiveRecursively(false);
				coleague.animation.CrossFade("CS_Pointing");
				break;
			case 3:
				inChopperCamera.SetActiveRecursively(false);
				cutsceneCamera.SetActiveRecursively(true);
				soldier.SetActiveRecursively(true);
				coleague.animation.CrossFade("CS_ColeagueIdle");
				soldierWeapon.transform.parent = weaponAnimation.transform.GetChild(0);
				soldierWeapon.transform.localPosition = Vector3.zero;
				weaponAnimation.animation.Play("Take 001");
				rope.animation.Play("RopeAnimation");
				soldier.animation.Play("CS_Rope");
				break;
		}
	}
	
	function DestroyScene()
	{
		Destroy(gameObject);
	}
}