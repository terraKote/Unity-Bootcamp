#pragma strict
#pragma implicit
#pragma downcast

class AchievmentScreen extends MonoBehaviour
{
	public var menuBackground : Texture2D;
	private var menuBackgroundRect : Rect;
	
	public var windowBackground : Texture2D;
	private var windowBackgroundRect : Rect;

	public var dummyHead : Texture2D;
	private var dummyHeadRect : Rect;
	
	public var dummyLArm : Texture2D;
	private var dummyLArmRect : Rect;
	
	public var dummyRArm : Texture2D;
	private var dummyRArmRect : Rect;
	
	public var dummyHeart : Texture2D;
	private var dummyHeartRect : Rect;
	
	public var dummyTorso : Texture2D;
	private var dummyTorsoRect : Rect;
	
	public var dummyChest : Texture2D;
	private var dummyChestRect : Rect;
	
	private var dummyRect : Rect;
	
	public var exploreMap : Texture2D;
	public var exploreMapOver : Texture2D;
	private var exploreMapRect : Rect;
	
	public var restartTraining : Texture2D;
	public var restartTrainingOver : Texture2D;
	private var restartTrainingRect : Rect;
	
	private var scrollPosition : Rect;
	private var scrollView : Rect;
	private var scroll : Vector2;
	
	public var achievmentDone : Texture2D;
	public var achievmentUndone : Texture2D;
	private var achievmentIconBackgroundRect : Rect;
	private var achievmentBackgroundRect : Rect;
	
	public var hudSkin : GUISkin;
	
	public var panelLeft : GUIStyle;
	private var panelLeftRect : Rect;
	
	public var panelRight : GUIStyle;
	private var panelRightRect : Rect;
	
	public var achievments : Achievment[];
	private var activeAchievments : int;
	
	private var descriptionStyle : GUIStyle;
	private var titleStyle : GUIStyle;
	private var dummyInfoStyle : GUIStyle;
	private var customBox : GUIStyle;
	
	private var head : int;
	private var chest : int;
	private var lArm : int;
	private var rArm : int;
	private var heart : int;
	private var torso : int;
	
	private var titleBackground : GUIStyle;
	private var titleRect : Rect;
	
	public var visible : boolean;
	private var evt : Event;
	private var lastMouseTime : float;
	static public var returningToTraining : boolean;
	
	private var overallAlpha : float;
	private var alpha : float;
	
	public var black : Texture2D;
	
	function Start()
	{
		lastMouseTime = 0.0;
		visible = false;
		returningToTraining = false;
		alpha = 0.0;
		
		descriptionStyle = hudSkin.GetStyle("Description");
		titleStyle = hudSkin.GetStyle("Titles");
		dummyInfoStyle = hudSkin.GetStyle("DummyInfo");
		customBox = hudSkin.GetStyle("CustomBox");
		titleBackground = hudSkin.GetStyle("TitleBackground");
		
		menuBackgroundRect = new Rect(0, 0, menuBackground.width, menuBackground.height);
		windowBackgroundRect = new Rect(0, 0, windowBackground.width, windowBackground.height);
		titleRect = new Rect(0, 0, 478, 25);
		panelLeftRect = new Rect(0, 0, 220, 220);
		panelRightRect = new Rect(0, 0, 250, 220);
		dummyRect = new Rect(0, 0, 128, 178);
		exploreMapRect = new Rect(0, 0, exploreMap.width * 0.75, exploreMap.height * 0.75);
		restartTrainingRect = new Rect(0, 0, restartTraining.width * 0.75, restartTraining.height * 0.75);
		
		scrollPosition = new Rect(0, 0, 240, 190);
		achievmentBackgroundRect = new Rect(0, 0, 222, 60);
		
		dummyHeadRect = new Rect(0, 0, dummyHead.width, dummyHead.height);
		dummyLArmRect = new Rect(0, 0, dummyLArm.width, dummyLArm.height);
		dummyRArmRect = new Rect(0, 0, dummyRArm.width, dummyRArm.height);
		dummyTorsoRect = new Rect(0, 0, dummyTorso.width, dummyTorso.height);
		dummyChestRect = new Rect(0, 0, dummyChest.width, dummyChest.height);
		dummyHeartRect = new Rect(0, 0, dummyHeart.width, dummyHeart.height);
		
		activeAchievments = 0;
		for(var i : int = 0; i < achievments.length; i++)
		{
			if(achievments[i].enabled) activeAchievments++;
		}
		
		scrollView = new Rect(0, 0, 223, activeAchievments * 65.0);
		
		achievmentIconBackgroundRect = new Rect(0, 0, achievmentDone.width, achievmentDone.height);
	}
	
	function Update()
	{
		if(!visible || GameManager.pause) 
		{
			if(!visible)
			{
				overallAlpha = 0.0;
			}
			
			return;
		}
		
		CheckAchievments();
		
		CheckAccuracy();
		
		if(returningToTraining)
		{
			alpha += Time.deltaTime;
			
			if(alpha >= 1.0)
			{
				Application.LoadLevel("demo_industry");
			}
		}
		
		if(visible)
		{
			if(overallAlpha < 1.0)
			{
				overallAlpha += Time.deltaTime;
			}
		}
	}
	
	function DrawGUI(event : Event)
	{
		if(!visible || GameManager.pause) return;
		
		GUI.color = new Color(1.0, 1.0, 1.0, overallAlpha);
		
		evt = event;//Event.current;
		
		var screenSize : Vector2 = new Vector2(Screen.width, Screen.height);
		var mousePos : Vector2 = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
		
		menuBackgroundRect.x = 0;
		menuBackgroundRect.y = (screenSize.y - menuBackgroundRect.height) * 0.5 - 50;
		
		//playGameRect.x = menuBackgroundRect.x + 93;// + ((menuBackgroundRect.width - playGameRect.width) * 0.5);
		//playGameRect.y = menuBackgroundRect.y + 62
		
		exploreMapRect.x = menuBackgroundRect.x + 93;//((menuBackgroundRect.width - exploreMapRect.width) * 0.5) - 10;
		exploreMapRect.y = menuBackgroundRect.y + 62;//70;
		
		restartTrainingRect.x = exploreMapRect.x;
		restartTrainingRect.y = exploreMapRect.y + exploreMapRect.height + 30;
		
		windowBackgroundRect.x = menuBackgroundRect.x + menuBackground.width;
		windowBackgroundRect.y = (screenSize.y - windowBackgroundRect.height) * 0.5;

		panelLeftRect.x = windowBackgroundRect.x + 15;
		panelLeftRect.y = windowBackgroundRect.y + 80;

		panelRightRect.x = panelLeftRect.x + 10 + panelLeftRect.width;
		panelRightRect.y = panelLeftRect.y;
		
		scrollPosition.x = panelRightRect.x + 5;
		scrollPosition.y = panelRightRect.y + 25;
		
		dummyRect.x = panelLeftRect.x + ((panelLeftRect.width - dummyRect.width) * 0.5);
		dummyRect.y = panelLeftRect.y + ((panelLeftRect.height - dummyRect.height) * 0.5);
				
		//Draw GUI stuff
		GUI.DrawTexture(windowBackgroundRect, windowBackground);
		GUI.Box(panelLeftRect, "", panelLeft);
		GUI.Box(panelRightRect, "", panelRight);
		
		var c : Color = GUI.color;
		var d : Color = c;
		
		var auxV : float = head;
		auxV *= 0.01;
		d.a = 0.2 + (0.8 * auxV);
		GUI.color = d;
		dummyHeadRect.x = dummyRect.x + 45;
		dummyHeadRect.y = dummyRect.y + 0;
		GUI.DrawTexture(dummyHeadRect, dummyHead);
		
		auxV = lArm;
		auxV *= 0.01;
		d.a = 0.2 + (0.8 * auxV);
		GUI.color = d;
		dummyLArmRect.x = dummyRect.x + 0;
		dummyLArmRect.y = dummyRect.y + 58;
		GUI.DrawTexture(dummyLArmRect, dummyLArm);
		
		auxV = rArm;
		auxV *= 0.01;
		d.a = 0.2 + (0.8 * auxV);
		GUI.color = d;
		dummyRArmRect.x = dummyRect.x + 101;
		dummyRArmRect.y = dummyRect.y + 58;
		GUI.DrawTexture(dummyRArmRect, dummyRArm);
		
		auxV = heart;
		auxV *= 0.01;
		d.a = 0.2 + (0.8 * auxV);
		GUI.color = d;
		dummyHeartRect.x = dummyRect.x + 60;
		dummyHeartRect.y = dummyRect.y + 59;
		GUI.DrawTexture(dummyHeartRect, dummyHeart);
		
		auxV = chest;
		auxV *= 0.01;
		d.a = 0.2 + (0.8 * auxV);
		GUI.color = d;
		dummyChestRect.x = dummyRect.x + 20;
		dummyChestRect.y = dummyRect.y + 55;
		GUI.DrawTexture(dummyChestRect, dummyChest);
		
		auxV = torso;
		auxV *= 0.01;
		d.a = 0.2 + (0.8 * auxV);
		GUI.color = d;
		dummyTorsoRect.x = dummyRect.x + 30;
		dummyTorsoRect.y = dummyRect.y + 98;
		GUI.DrawTexture(dummyTorsoRect, dummyTorso);
		
		GUI.color = c;
		//*/
		
		var cSkin : GUISkin = GUI.skin;
		GUI.skin = hudSkin;
		
		achievmentBackgroundRect.x = 1;
		achievmentBackgroundRect.y = 0;

		achievmentIconBackgroundRect.x = 7;
		achievmentIconBackgroundRect.y = 14;
		
		GUI.Label(new Rect(dummyRect.x + 1, dummyRect.y + 14, dummyRect.width, 20), "HEAD\n" + head.ToString() + "%", dummyInfoStyle);
		GUI.Label(new Rect(dummyRect.x + 1, dummyRect.y + 115, dummyRect.width, 20), "TORSO\n" + torso.ToString() + "%", dummyInfoStyle);
		GUI.Label(new Rect(dummyRect.x - 25, dummyRect.y + 55, 30, 20), "L.ARM\n" + lArm.ToString() + "%", dummyInfoStyle);
		GUI.Label(new Rect(dummyRect.x + dummyRect.width - 4, dummyRect.y + 55, 30, 20), "R.ARM\n" + rArm.ToString() + "%", dummyInfoStyle);
		GUI.Label(new Rect(dummyRect.x + 28, dummyRect.y + 65, 35, 20), "CHEST\n" + chest.ToString() + "%", dummyInfoStyle);
		GUI.Label(new Rect(dummyRect.x + 63, dummyRect.y + 69, 35, 20), "HEART\n" + heart.ToString() + "%", dummyInfoStyle);
		
		GUI.Label(new Rect(windowBackgroundRect.x + 20, windowBackgroundRect.y + 15, 200, 20), "TRAINING SCORES", titleStyle);
		
		GUI.Label(new Rect(panelLeftRect.x + 10, panelLeftRect.y + 5, 200, 20), "ACCURACY", titleStyle);
		
		GUI.Label(new Rect(scrollPosition.x + 20, scrollPosition.y - 20, 200, 20), "REWARDS", titleStyle);
		
		titleRect.x = windowBackgroundRect.x + 16;
		titleRect.y = windowBackgroundRect.y + 50;
		GUI.Box(titleRect, "", titleBackground);
		
		var timeRect : Rect = new Rect(panelLeftRect.x + 10, titleRect.y + 4, 200, 20);
		var time : String = "";
		
		var hours : int = GameManager.time;
		hours /= 3600;
		
		var minutes : int = GameManager.time;
		minutes = (minutes / 60) - (hours * 60);
	
		var seconds : int = GameManager.time;
		seconds = seconds % 60;
		
		time += (((hours < 10) ? "0" : "") + hours.ToString()) + ":" + (((minutes < 10) ? "0" : "") + minutes.ToString()) + ":" + (((seconds < 10) ? "0" : "") + seconds.ToString());
		
		GUI.Label(timeRect, "TOTAL TIME: " + time, titleStyle);
		
		scroll = GUI.BeginScrollView(scrollPosition, scroll, scrollView);
		for(var i : int = 0; i < achievments.length; i++)
		{
			if(achievments[i].enabled)
			{
				GUI.Box(achievmentBackgroundRect, "", customBox);
				
				if(achievments[i].done)
				{
					GUI.DrawTexture(achievmentIconBackgroundRect, achievmentDone);
					
					hudSkin.label.normal.textColor = new Color(1.0, 1.0, 1.0, 1.0);	
					descriptionStyle.normal.textColor = new Color(1.0, 1.0, 1.0, 1.0);			
				}
				else
				{
					GUI.DrawTexture(achievmentIconBackgroundRect, achievmentUndone);
					
					if(achievments[i].showProgress || (i == 5 && achievments[i].done))
					{
						var dr : Rect = achievmentIconBackgroundRect;
						
						var p1 : float = achievments[i].progress;
						p1 /= achievments[i].maxProgress;

						dr.height = achievmentIconBackgroundRect.height * p1;
						dr.y = achievmentIconBackgroundRect.y + 35 - dr.height;
						GUI.DrawTexture(dr, achievmentDone);
					}
					
					hudSkin.label.normal.textColor = new Color(0.3, 0.3, 0.3, 1.0);
					descriptionStyle.normal.textColor = new Color(0.3, 0.3, 0.3, 1.0);
				}
				
				GUI.Label(new Rect(achievmentIconBackgroundRect.width + 12, achievmentIconBackgroundRect.y - 4, 200, 20), achievments[i].name.ToUpper());
				GUI.Label(new Rect(achievmentIconBackgroundRect.width + 12, achievmentIconBackgroundRect.y + 12, 170, 50), achievments[i].description, descriptionStyle);
				
				GUI.DrawTexture(new Rect(achievmentIconBackgroundRect.x + (achievmentIconBackgroundRect.width - achievments[i].icon.width) * 0.5 - 1,
									achievmentIconBackgroundRect.y + (achievmentIconBackgroundRect.height - achievments[i].icon.height) * 0.5 - 1,
									achievments[i].icon.width,
									achievments[i].icon.height), achievments[i].icon);
				
				if(achievments[i].showProgress)
				{	
					var progressRect = new Rect(achievmentIconBackgroundRect.width + 10, achievmentBackgroundRect.y + 45, 140, 3);

					GUI.Label(new Rect(progressRect.x + progressRect.width + 7, progressRect.y - 3, 50, 20), achievments[i].progress.ToString() + "/" + achievments[i].maxProgress.ToString(), descriptionStyle);
				}
				
				achievmentIconBackgroundRect.y += 65;
				achievmentBackgroundRect.y += 65;
			}
		}
		GUI.EndScrollView();
		
		GUI.skin = cSkin;
		
		GUI.DrawTexture(menuBackgroundRect, menuBackground);
		
		if(exploreMapRect.Contains(mousePos))
		{
			GUI.DrawTexture(exploreMapRect, exploreMapOver);
			
			if(visible && overallAlpha >= 1.0)
			{
				if(evt.type == EventType.MouseUp && evt.button == 0 && Time.time > lastMouseTime)
				{
					visible = false;
					GameManager.scores = false;
					lastMouseTime = Time.time;
				}
			}
		}
		else
		{
			GUI.DrawTexture(exploreMapRect, exploreMap);
		}
		
		if(restartTrainingRect.Contains(mousePos))
		{
			GUI.DrawTexture(restartTrainingRect, restartTrainingOver);
			
			if(visible && overallAlpha >= 1.0)
			{
				if(evt.type == EventType.MouseUp && evt.button == 0 && Time.time > lastMouseTime)
				{
					returningToTraining = true;
					
					lastMouseTime = Time.time;
				}
			}
		}
		else
		{
			GUI.DrawTexture(restartTrainingRect, restartTraining);
		}
		
		if(returningToTraining)
		{
			GUI.color = new Color(1.0, 1.0, 1.0, alpha);
			GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), black); 
		}
	}
	
	function CheckAchievments()
	{
		achievments[0].progress = TrainingStatistics.headShoot;
		achievments[0].done = achievments[0].progress >= achievments[0].maxProgress;
		achievments[1].done = GameManager.time < 180.0;
		achievments[4].done = (TrainingStatistics.turretsHit == 0) && (TrainingStatistics.dummiesHit == 0);
		achievments[5].progress = TrainingStatistics.turrets;
		achievments[5].done = (TrainingStatistics.turretsHit == 0) && (achievments[5].progress >= achievments[5].maxProgress);
		achievments[8].progress = TrainingStatistics.eaglesEye;
		achievments[8].done = achievments[8].progress >= achievments[8].maxProgress;
		achievments[9].progress = TrainingStatistics.blueLeaf;
		achievments[9].done = achievments[9].progress >= achievments[9].maxProgress;
	}
	
	function CheckAccuracy()
	{
		if(TrainingStatistics.totalHits <= 0) return;
		
		var aux : float = 0.0;
		var tShoots : float = 100.0 / TrainingStatistics.totalHits;
		
		aux = TrainingStatistics.head;
		aux *= tShoots;
		head = aux;
		
		aux = TrainingStatistics.lArm;
		aux *= tShoots;
		lArm = aux;
		
		aux = TrainingStatistics.rArm;
		aux *= tShoots;
		rArm = aux;
		
		aux = TrainingStatistics.heart;
		aux *= tShoots;
		heart = aux;
		
		aux = TrainingStatistics.chest;
		aux *= tShoots;
		chest = aux;
		
		aux = TrainingStatistics.torso;
		aux *= tShoots;
		torso = aux;
	}
}