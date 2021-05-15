#pragma strict
#pragma implicit
#pragma downcast

@CustomEditor(SargeManager)
class SargeHelper extends Editor
{
	function OnInspectorGUI()
	{
		var t : SargeManager = target as SargeManager;
		
		DrawDefaultInspector();
		
		if(!EditorApplication.isPlaying) return;

		if(t.instructions != null)
		{
			t.debug = EditorGUILayout.Foldout(t.debug, "Debug voices");
			
			if(t.debug)
			{
				for(var i : int = 0; i < t.instructions.length; i++)
				{
					var inst = t.instructions[i].name;
					if(GUILayout.Button(inst))
					{
						t.gameObject.SendMessage("ShowInstruction", inst);
					}
				}
			}
		}
	}
}