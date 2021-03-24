#pragma strict
#pragma implicit
#pragma downcast

class DownChoperCutscene extends MonoBehaviour
{
	public var soldierWeapon : GameObject;
	public var weaponAnimation : GameObject;
	public var rope : GameObject;
	public var soldier : GameObject;
	public var cutsceneCamera : GameObject;
	public var sarge : SargeManager;
	
	public var timer : float = 2.0;
	
	private var destroy : boolean;
	private var endCutscene : boolean;
	private var currentWeaponParent : Transform;
	private var currentWeaponPosition : Vector3;
	private var currentWeaponRotation : Quaternion;
	
	public var audioSources : AudioSource[];
	private var audioStarted : boolean;

    public var pilot : GameObject;
    public var wingman : GameObject;
    public var particles : ParticleEmitter[];
    public var windZone : GameObject;
    
    public var soldierCamera : GameObject;

	function Start()
	{
        var sargeObject : GameObject = GameObject.Find("SargeManager") as GameObject;

        if(sargeObject != null)
        {
            sarge = sargeObject.GetComponent("SargeManager") as SargeManager;
        }

		audioStarted = false;
		destroy = false;
		SendMessageUpwards("CutsceneStart");
		endCutscene = false;
		rope.animation.Play("RopeAnimation");
		rope.animation["RopeAnimation"].enabled = true;
		rope.animation["RopeAnimation"].time = 0.05;
		rope.animation.Sample();
		rope.animation["RopeAnimation"].enabled = false;
		currentWeaponParent = soldierWeapon.transform.parent;
		currentWeaponPosition = soldierWeapon.transform.localPosition;
		currentWeaponRotation = soldierWeapon.transform.localRotation;
		animation.Play("heli_rapel_cutscene");
	}
	
	function StartAudios()
	{
		if(!audioStarted)
		{
			audioStarted = true;
		}
	}
	
	function Update()
	{
        var p : int;

		if(GameManager.pause)
		{
			if(audioSources != null)
			{
				for(var i : int = 0; i < audioSources.length; i++)
				{
					if(audioSources[i].isPlaying)
					{
						audioSources[i].Pause();
					}
				}
			}

            if(windZone.active)
            {
                windZone.active = false;
            }

			animation["heli_rapel_cutscene"].speed = 0.0;
			rope.animation["RopeAnimation"].speed = 0.0;
			soldier.animation["CS_Rope"].speed = 0.0;
			weaponAnimation.animation["Take 001"].speed = 0.0;
            pilot.animation["CS_Pilot1"].speed = 0.0;
            wingman.animation["CS_Pilot2"].speed = 0.0;

            if(particles != null)
            {
                for(p = 0; p < particles.Length; p++)
                {
                    if(particles[p] == null) continue;

                    if(particles[p].enabled)
                    {
                        particles[p].enabled = false;
                    }
                }
            }

            if(!endCutscene)
            {
                if(Time.timeScale > 0.0)
                {
                    Time.timeScale = 0.0;
                }
            }
		}
		else
		{
            if(!windZone.active)
            {
                windZone.active = true;
            }

			if(animation["heli_rapel_cutscene"].speed < 1.0)
			{
				animation["heli_rapel_cutscene"].speed = 1.0;
				rope.animation["RopeAnimation"].speed = 1.0;
				soldier.animation["CS_Rope"].speed = 1.0;
				weaponAnimation.animation["Take 001"].speed = 1.0;
                pilot.animation["CS_Pilot1"].speed = 1.0;
                wingman.animation["CS_Pilot2"].speed = 1.0;

                if(particles != null)
                {
                    for(p = 0; p < particles.Length; p++)
                    {
                        if(particles[p] == null) continue;

                        if(!particles[p].enabled)
                        {
                            particles[p].enabled = true;
                        }
                    }
                }
			}

			if(!endCutscene)
			{
				if(Input.GetKeyDown(KeyCode.Space))
				{
					animation.Stop();
					audio.Stop();
					animation["heli_rapel_cutscene"].enabled = true;
					animation["heli_rapel_cutscene"].time = 25.0;
					animation.Play("heli_rapel_cutscene");

                    if(sarge != null)
                    {
					    sarge.StopInstructions();
                    }

					EndCutscene();
				}
				
				if(rope.animation["RopeAnimation"].normalizedTime > 0.6)
				{
					soldierWeapon.transform.parent = currentWeaponParent;
					soldierWeapon.transform.localPosition = currentWeaponPosition;
					soldierWeapon.transform.localRotation = currentWeaponRotation;
				}
				
				if(audioSources != null && audioStarted)
				{
					for(var j : int = 0; j < audioSources.length; j++)
					{
						if(!audioSources[j].isPlaying)
						{
							audioSources[j].Play();
						}
					}
				}
			}
			else
			{
				//Handle object destruction
				if(destroy)
				{
					timer -= Time.deltaTime;
					
					if(timer <= 0.0f)
					{
						Destroy(gameObject);
					}
				}
				else
				{
					if(animation["heli_rapel_cutscene"].normalizedTime > 0.99)
					{
						destroy = true;
					}
				}
			}
		}
	}
	
	function StartRapelAnimation()
	{
		if(endCutscene) return;
		
		soldierWeapon.transform.parent = weaponAnimation.transform.GetChild(0);
		soldierWeapon.transform.localPosition = Vector3.zero;
		weaponAnimation.animation.Play("Take 001");
		rope.animation.Play("RopeAnimation");
		soldier.animation.Play("CS_Rope");
	}
	
	function EndCutscene()
	{
		if(endCutscene) return;
		
		audioStarted = false;
		if(audioSources != null)
		{
			for(var j : int = 0; j < audioSources.length; j++)
			{
				audioSources[j].Stop();
			}
		}
		
		soldier.SetActiveRecursively(false);
		soldierWeapon.SetActiveRecursively(false);
		cutsceneCamera.SetActiveRecursively(false);
		
		SendMessageUpwards("StartGame");
		
		var soldCam : Camera = soldierCamera.GetComponentInChildren(Camera) as Camera;
		if(soldCam) {
			soldCam.enabled = true;
		}
		
		endCutscene = true;
	}
	
	function PlayInstruction(i : int)
	{
		switch(i)
		{
			case 0:
				sarge.ShowInstruction("helicopter_base");
				break;
			case 1:
				sarge.ShowInstruction("wait_zone");
				break;
			case 2:
				sarge.ShowInstruction("ropes");
				break;
		}
	}
}