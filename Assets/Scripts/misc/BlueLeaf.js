#pragma strict
#pragma implicit
#pragma downcast

class BlueLeaf extends MonoBehaviour
{
	private var registered : boolean;
	
	public var audios : AudioClip[];
	static private var played : boolean[];
	static private var totalPlayed : int;
	static private var totalAudios : int;
	static private var lastPlayed : int;
	
	public var soundDelay : float = 0.5;
	public var volume : float = 0.18;
	
	function Start()
	{
		registered = false;
		
		if(audios != null)
		{
			if(audios.length > 0)
			{
				if(played == null)
				{
					totalAudios = audios.length;
					
					totalPlayed = 0;
					
					played = new boolean[totalAudios];
					for(var i : int = 0; i < totalAudios; i++)
					{
						played[i] = false;
					}
				}
			}
			else
			{
				totalAudios = 0;
			}
		}
		else
		{
			totalAudios = 0;
		}
	}
	
	function Update()
	{
		if(!registered)
		{
			TrainingStatistics.totalBlueLeaf++;
			registered = true;
		}	
	}
	
	function PlaySound()
	{
		if(totalAudios <= 0) return;
		
		var sAudio : int = 0;
		
		if(totalPlayed >= totalAudios)
		{
			totalPlayed = 0;
			for(var i : int = 0; i < totalAudios; i++)
			{
				played[i] = false;
			}
			
			played[lastPlayed] = true;
		}
		
		sAudio = Random.Range(0, totalAudios);
		
		while(played[sAudio])
		{
			sAudio = Random.Range(0, totalAudios);
		}
		
		if(totalPlayed == 0)
		{
			played[lastPlayed] = false;
		}
		
		lastPlayed = sAudio;
		played[sAudio] = true;
		totalPlayed++;
		
		var go : GameObject = new GameObject("_LeafSound");
		var audioS : AudioSource = go.AddComponent("AudioSource") as AudioSource;
		audioS.volume = volume;
		
		audioS.playOnAwake = false;
		
		var ad : AutoDestroy = go.AddComponent("AutoDestroy") as AutoDestroy;
		ad.time = audios[sAudio].length + soundDelay + 1.0;
		audioS.clip = audios[sAudio];
		var bSound : BlueLeafSound = go.AddComponent("BlueLeafSound") as BlueLeafSound;
		bSound.delay = soundDelay;
		
		/*
		if(audio != null)
		{
			audio.PlayOneShot(audios[sAudio]);
		}
		//*/
	}
	
	function Hit(hit : RaycastHit)
	{
		TrainingStatistics.blueLeaf++;
		
		PlaySound();
		
		Destroy(this);
	}
}