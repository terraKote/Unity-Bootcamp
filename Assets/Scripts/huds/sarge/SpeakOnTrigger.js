#pragma strict
#pragma implicit
#pragma downcast

class SpeakOnTrigger extends MonoBehaviour
{
	public var instructions : TriggerInstruction[];
	
	private var playing : boolean;
	private var timer : float;
	private var instructionsToPlay : int;
	
	function Start()
	{
		playing = false;
		gameObject.layer = 2;
		instructionsToPlay = instructions.length;
	}
	
	function Update()
	{
		if(playing && instructionsToPlay > 0)
		{
			timer += Time.deltaTime;
			
			for(var i : int = 0; i < instructions.length; i++)
			{
				if(!instructions[i].playing)
				{
					if(instructions[i].instructionDelay < timer)
					{
						instructions[i].playing = true;
						instructionsToPlay--;
						SendMessageUpwards("ShowInstruction", instructions[i].instructionName, SendMessageOptions.DontRequireReceiver);
					}
				}
			}
		}	
		else if(instructionsToPlay <= 0)
		{
			Destroy(gameObject);
		}
	}
	
	function OnTriggerEnter(other : Collider)
	{
		if(!playing)
		{
			if(other.name.ToLower() == "soldier")
			{
				playing = true;
				timer = 0.0;
			}
		}
	}
}

class TriggerInstruction
{
	public var instructionName : String;
	public var instructionDelay : float = 0.0;
	
	@HideInInspector
	public var playing : boolean = false;
}