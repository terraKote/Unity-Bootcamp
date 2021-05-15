using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(SargeManager))]
public class SargeHelper : Editor
{
    public override void OnInspectorGUI()
	{
		var t = target as SargeManager;

		DrawDefaultInspector();

		if (!EditorApplication.isPlaying) return;

		if (t.instructions != null)
		{
			t.debug = EditorGUILayout.Foldout(t.debug, "Debug voices");

			if (t.debug)
			{
				for (int i = 0; i < t.instructions.Length; i++)
				{
					var inst = t.instructions[i].name;
					if (GUILayout.Button(inst))
					{
						t.gameObject.SendMessage("ShowInstruction", inst);
					}
				}
			}
		}
	}
}
