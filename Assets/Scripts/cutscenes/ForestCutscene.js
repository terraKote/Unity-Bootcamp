#pragma strict
#pragma implicit
#pragma downcast

enum CutsceneBehaviourType
{
	PLAY_ANIM,
	SET_POSITION,
}

class ForestCutsceneBehaviour
{
	public var action : CutsceneBehaviourType;
	public var time : float;
	public var anim : String;
	public var position : Vector3;
	public var rotation : Vector3;
}

class ForestCutscene extends MonoBehaviour
{
	private var played : boolean;
	private var playing : boolean;
	
	public var soldier : GameObject;
	public var soldierFinalRef : Transform;
	public var soldierCam : Camera;
	public var cam : Transform;
	private var camAnimation : Animation;
	
	public var anims : ForestCutsceneBehaviour[];
	
	public var totalTime : float;
	public var timer : float;
	private var cStep : int;
	private var nextAnimTime : float;
	private var part1 : boolean;
	private var part2 : boolean;
	
	function Start()
	{
		part1 = part2 = false;
		
		cStep = 0;
		nextAnimTime = anims[0].time;
		
		camAnimation = cam.animation;
		played = false;
		playing = false;
		timer = 0.0;

		Play();
	}
	
	function Update()
	{
		timer += Time.deltaTime;
		
		if(GameManager.pause)
		{
			soldier.animation["CS_2_Part1"].speed = 0.0;
			soldier.animation["CS_2_Part2"].speed = 0.0;
			cam.animation["industryCutsceneCamera_entire"].speed = 0.0;
		}
		else if(soldier.animation["CS_2_Part1"].speed < 1.0)
		{
			soldier.animation["CS_2_Part1"].speed = 1.0;
			soldier.animation["CS_2_Part2"].speed = 1.0;
			cam.animation["industryCutsceneCamera_entire"].speed = 1.0;
		}
		
		if(soldier.animation["CS_2_Part1"].normalizedTime > 0.965 && !part1)
		{
			part1 = true;
			soldier.animation["CS_2_Part2"].speed = 0.7;
			soldier.animation.Play("CS_2_Part2");
			soldier.transform.localPosition = new Vector3(3.256119, soldier.transform.localPosition.y, soldier.transform.localPosition.z);
		}
		
		if(soldier.animation["CS_2_Part2"].normalizedTime > 0.27 && !part2)
		{
			part2 = true;
			soldier.transform.localPosition = new Vector3(0.6527214, 1.321428, -1.147861);
			soldier.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
		}
		
		if(!GameManager.pause)
		{
			totalTime -= Time.deltaTime;
		
			if(totalTime <= 0.0)
			{
				StartCoroutine(WaitAndDestroy());
			}
			
			if(Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
			{
				StartCoroutine(WaitAndDestroy());
			}
		}
	}
	
	function WaitAndDestroy()
	{
		this.enabled = false;
		
		Destroy(cam.GetComponent("AudioListener"));
		SendMessageUpwards("StartGame");
		soldier.animation.Stop();
		
		if(soldierCam)
			soldierCam.enabled = true;		
		
		yield;
		
		Destroy(gameObject);
	}
	
	function Play()
	{
		if(played) return;
		
		played = true;
		
		gameObject.SetActiveRecursively(true);
		
		SendMessageUpwards("CutsceneStart", SendMessageOptions.DontRequireReceiver);
		
		soldier.animation.CrossFade("CS_2_Part1");
	}
}