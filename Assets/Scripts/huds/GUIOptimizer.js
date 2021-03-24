class GUIOptimizer extends MonoBehaviour
{
	public var hudWeapons : HudWeapons;
	public var mainMenu : MainMenuScreen;
	public var sarge : SargeManager;
	public var achievements : AchievmentScreen;

	function OnGUI()
	{
		var evt : Event = Event.current;
		
		if(mainMenu != null) mainMenu.DrawGUI(evt);

        if(achievements != null) achievements.DrawGUI(evt);

		if(evt.type == EventType.Repaint)
		{
			if(hudWeapons != null) hudWeapons.DrawGUI(evt);
			if(sarge != null) sarge.DrawGUI(evt);
		}
	}
}