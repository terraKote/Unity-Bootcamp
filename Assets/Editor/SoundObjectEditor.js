#pragma strict

@CustomEditor(SoundObject)
class SoundObjectEditor extends Editor
{
	function OnInspectorGUI()
	{
		var managers : SoundObjectManager[] = UnityEngine.Object.FindObjectsOfType(SoundObjectManager) as SoundObjectManager[];
		
		if(managers != null)
		{
			if(managers.length > 0)
			{
				var m : SoundObjectManager = managers[0];
				
				if(m.additionalSounds != null)
				{
					var optionNames : String[] = new String[m.additionalSounds.length + 1];
					var optionValues : int[] = new int[m.additionalSounds.length + 1];
					
					optionNames[0] = "USE TAG";
					optionValues[0] = -1;
					
					for(var i : int = 0; i < m.additionalSounds.length; i++)
					{
						optionNames[i+1] = m.additionalSounds[i].name;
						optionValues[i+1] = i;
					}
					
					var so : SoundObject = target as SoundObject;
					
					so.overrideSound = EditorGUILayout.IntPopup(so.overrideSound, optionNames, optionValues);
					
					DrawDefaultInspector();
				}
			}
		}
	}
}