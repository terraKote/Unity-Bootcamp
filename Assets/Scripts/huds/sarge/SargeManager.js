#pragma strict
#pragma implicit
#pragma downcast

class SargeManager extends MonoBehaviour
{
	public var sarge : Texture2D;
	public var background : Texture2D;
	public var instructions : SargeInstruction[];
	
	private var visible : boolean;
	
	private var sargeRect : Rect;
	private var backgroundRect : Rect;
	private var halfScreen : Vector2;
	private var container : Rect;
	
	private var sargeAlpha : float;
	private var backgroundAlpha : float;
	private var contentAlpha : float;
	
	public var fadeTime : float;
	
	private var auxColor : Color;
	private var oldColor : Color;
	
	private var table : Hashtable;
	
	private var timeToHide : float;
	
	private var currentInstruction : SargeInstruction;
	
	public var textStyle : GUIStyle;
	
	@HideInInspector
	public var debug : boolean;

	private var audioWasPlaying : boolean;
	
	private var messageQueue : Array;
	private var friendlyFire : boolean;
	private var lastInstruction : SargeInstruction;
	
	function Start()
	{
		messageQueue = new Array();
		
		friendlyFire = false;
		audioWasPlaying = false;
		
		table = new Hashtable();
		
		for(var i : int = 0; i < instructions.length; i++)
		{
			if(instructions[i] != null)
			{
				if(!String.IsNullOrEmpty(instructions[i].name))
				{
					if(!table.ContainsKey(instructions[i].name.ToLower()))
					{
						table.Add(instructions[i].name.ToLower(), instructions[i]);
					}
				}
			}
		}
		
		fadeTime = 1.0 / fadeTime;
		
		sargeAlpha = 0.0;
		visible = false;
		
		sargeRect = new Rect(0, 0, sarge.width, sarge.height);
		backgroundRect = new Rect(0, 0, background.width, background.height);
		
		background.wrapMode = TextureWrapMode.Clamp;
		
		container = new Rect(0, 0, sarge.width + background.width, Mathf.Max(sarge.height, background.height));
		
		if(audio == null)
		{
			gameObject.AddComponent("AudioSource");
		}
		
		audio.loop = false;
		audio.playOnAwake = false;
	}
	
	function StopInstructions()
	{
		if(messageQueue != null)
		{
			messageQueue.Clear();
		}
		
		timeToHide = 0.0;
		
		if(audio.isPlaying)
		{
			audio.Stop();
		}
	}
	
	function DrawGUI(event : Event)
	{
		if(contentAlpha <= 0.0) return;
		
		if(GameManager.pause || SoldierController.dead || AchievmentScreen.returningToTraining)
		{
			GUI.color = new Color(0.5, 0.5, 0.5, 0.0);
			return;
		}
		
		auxColor = oldColor = GUI.color;
		
		halfScreen = new Vector2(Screen.width, Screen.height) * 0.5;
		container.x = halfScreen.x - (container.width * 0.5);
		container.y = Screen.height - (container.height * 0.5);
			
		sargeRect.x = container.x;
		sargeRect.y = container.y - (sargeRect.height * 0.5);
			
		backgroundRect.x = sargeRect.x + sargeRect.width;
		backgroundRect.y = container.y - (backgroundRect.height * 0.5);
			
		auxColor.a = backgroundAlpha;
		GUI.color = auxColor;
		GUI.DrawTexture(backgroundRect, background);
			
		auxColor.a = sargeAlpha;
		GUI.color = auxColor;
		GUI.DrawTexture(sargeRect, sarge);
		
		DrawInstruction();
		
		GUI.color = oldColor;
	}
	
	function DrawInstruction()
	{
		if(currentInstruction == null) return;
	
		auxColor.a = contentAlpha;
		GUI.color = auxColor;
		
		if(currentInstruction.texture != null)
		{
			var auxRect = new Rect((backgroundRect.width - currentInstruction.texture.width) * 0.5 + backgroundRect.x, (backgroundRect.height - currentInstruction.texture.height) * 0.5 + backgroundRect.y, currentInstruction.texture.width, currentInstruction.texture.height);
			GUI.DrawTexture(auxRect, currentInstruction.texture);
		}
		else
		{
			GUI.TextArea(new Rect(backgroundRect.x + 30, backgroundRect.y + 10, backgroundRect.width - 60, backgroundRect.height - 20), currentInstruction.text, textStyle);
		}
	}
	
	var tex : Texture2D;
	function Update()
	{
		if(GameManager.pause || SoldierController.dead || AchievmentScreen.returningToTraining)
		{
			if(audio.isPlaying)
			{
				audioWasPlaying = true;
				audio.Pause();
			}
			return;
		}
		else if(audioWasPlaying)
		{
			audio.Play();
			audioWasPlaying = false;
		}
		
		if(!visible)
		{
			if(contentAlpha > 0.0) contentAlpha -= Time.deltaTime * fadeTime;
			else if(backgroundAlpha > 0.0) backgroundAlpha -= Time.deltaTime * fadeTime;
			else if(sargeAlpha > 0.0) sargeAlpha -= Time.deltaTime * fadeTime;
		}
		else
		{
			if(sargeAlpha < 1.0) sargeAlpha += Time.deltaTime * fadeTime;
			else if(backgroundAlpha < 1.0) backgroundAlpha += Time.deltaTime * fadeTime;
			else if(contentAlpha < 1.0) contentAlpha += Time.deltaTime * fadeTime;
			
			if(timeToHide >= 0) 
			{
				timeToHide -= Time.deltaTime;
				
				if(timeToHide < 0.0)
				{
					if(friendlyFire)
					{
						friendlyFire = false;
						
						if(lastInstruction != null)
						{
							ShowInstruction(lastInstruction.name);
							lastInstruction = null;
						}
					}
					else
					{
						if(messageQueue.length > 0)
						{
							var m : String = messageQueue[0] as String;
							messageQueue.RemoveAt(0);
							ShowInstruction(m);
						}
						else
						{
							visible = false;
						}
					}
				}
			}
		}
	}
	
	function FriendlyFire()
	{
		if(friendlyFire) return;
		
		if(audio.isPlaying)
		{
			var i : int = Random.Range(0, 2);
			var m : String;
			
			if(i == 0)
			{
				m = "friendly_fire1";
			}
			else
			{
				m = "friendly_fire2";
			}
			
			if(table.ContainsKey(m.ToLower()))
			{
				lastInstruction = currentInstruction;
				friendlyFire = true;
				currentInstruction = table[m];
				timeToHide = currentInstruction.timeToDisplay + ((1.0 - sargeAlpha) + (1.0 - backgroundAlpha) + (1.0 - contentAlpha)) * (1.0 / fadeTime);
				
				if(currentInstruction.audio != null)
				{
					audio.clip = currentInstruction.audio;
					audio.volume = currentInstruction.volume;
					audio.Play();
				}
				
				visible = true;
			}
		}
	}
	
	function ShowInstruction(instruction : String)
	{
		if(table == null) return;
		
		if(table.ContainsKey(instruction.ToLower()))
		{
			if(timeToHide > 0.0 || friendlyFire)
			{
				if(!currentInstruction.overridable)
				{
					if((table[instruction] as SargeInstruction).queuable)
					{	
						messageQueue.Add(instruction);
					}
					
					return;
				}
			}

			currentInstruction = table[instruction];
			timeToHide = currentInstruction.timeToDisplay + ((1.0 - sargeAlpha) + (1.0 - backgroundAlpha) + (1.0 - contentAlpha)) * (1.0 / fadeTime);
			
			if(currentInstruction.audio != null)
			{
				audio.clip = currentInstruction.audio;
				audio.volume = currentInstruction.volume;
				audio.Play();
			}
			
			visible = true;
		}
	}
}