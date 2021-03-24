/*
Copyright (c) 2010, Raphael Lopes Baldi & Aquiris Game Experience LTDA.

See the document "TERMS OF USE" included in the project folder for licencing details.
*/

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(Decal))]
public class DecalEditor : Editor 
{
	//Used to process DecalDecorator changes.
	private void OnSceneGUI()
	{
		//Selected instance.
		Decal decal = (Decal)target;
		
		//Will only calculate changes and process the new Decal
		//on Repaint and MouseDrag events.
		//MouseDrag will allow us to handle changes made to scene gizmos, while 
		//Repaint will allow us to handle changes made to the Inspector.
		
		//If the user want the objects to be updated, let's do it.
		bool hasChanged = decal.HasChanged();
		
		if(!hasChanged && ((Event.current.type != EventType.MouseDrag && Event.current.type != EventType.Repaint) || Event.current.modifiers != 0)) return;
			
		//On the editor, we'll use only MeshFilter.
		decal.decalMode = DecalMode.MESH_FILTER;
		
		if(hasChanged)
		{
			decal.ClearDecals();

			if(decal.checkAutomatically)
			{
				GetAffectedObjects(decal);
			}

			decal.CalculateDecal();
			
			GenerateUV(decal);
		}
	}
	
	private void GenerateUV(Decal d)
	{
		if(d.GetComponent<MeshFilter>() == null) return;
		
		Mesh m = d.GetComponent<MeshFilter>().sharedMesh;
		
		Vector2[] uv = Unwrapping.GeneratePerTriangleUV(m);
			
        MeshUtility.SetPerTriangleUV2(m, uv);

        Unwrapping.GenerateSecondaryUVSet(m);
	}
	
	//Draw custom Inspector.
	public override void OnInspectorGUI ()
	{
		Decal decal = (Decal)target;
		/*
		GUILayout.BeginVertical();
		decal.showAffectedObjectsOptions = EditorGUILayout.Foldout(decal.showAffectedObjectsOptions, "Affected Objects");
		if(decal.showAffectedObjectsOptions)
		{
			DrawAffectedObjects(decal);
		}
		
		decal.pushDistance = EditorGUILayout.FloatField(decal.pushDistance);
		
		GUILayout.FlexibleSpace();
		GUILayout.EndVertical();*/
		base.OnInspectorGUI ();
		
		if(decal.decalMaterial != null)
		{
			if(decal.decalMaterial.mainTexture != null)
			{
				if(GUILayout.Button("Edit UV"))
				{
					DecalUVEditorWindow.Initialize(decal);
				}
			}
		}
		//if(decal.decalMaterial == null) return;
		
		//GUILayout.BeginVertical();
		//GUILayout.FlexibleSpace();
		//GUILayout.Label(decal.decalMaterial.mainTexture);
		//GUILayout.FlexibleSpace();
		//GUILayout.EndVertical();
		//GUILayout.BeginArea(new Rect(0, 400, 512, 1024));
		//GUILayout.EndArea();
		
		//Graphics.DrawTexture(new Rect(10, 10, 100, 100), decal.decalMaterial.mainTexture);
	}
	
	private void DrawAffectedObjects(Decal decal)
	{
		GUILayout.BeginHorizontal();
		GUILayout.Space(15);
		
		GUILayout.BeginVertical();
		
		GUILayout.BeginHorizontal();
		GUILayout.Label("Check Automatically");
		GUILayout.Space(60);
		decal.checkAutomatically = GUILayout.Toggle(decal.checkAutomatically, "");
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();

		decal.showObjects = EditorGUILayout.Foldout(decal.showObjects, "Affected Objects");
		
		if(decal.showObjects)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Space(45);
			GUILayout.BeginVertical();
			
			GUILayout.BeginHorizontal();
			GUILayout.Label("Size");
			GUILayout.Space(60);
			
			int size = 0;
			
			if(decal.affectedObjects != null)
			{
				size = decal.affectedObjects.Length;
			}
			
			if(!decal.checkAutomatically)
			{
				size = Mathf.Max(EditorGUILayout.IntField(size), 0);
				
				if(size > 0 && decal.affectedObjects == null)
				{
					decal.affectedObjects = new GameObject[size];
				}
				else if(decal.affectedObjects != null && size != decal.affectedObjects.Length)
				{
					if(size == 0)
					{
						decal.affectedObjects = null;
					}
					else if(decal.affectedObjects.Length > 0)
					{
						GameObject[] objs = new GameObject[decal.affectedObjects.Length];
						for(int i = 0; i < objs.Length; i++)
						{
							objs[i] = decal.affectedObjects[i];
						}
						
						decal.affectedObjects = new GameObject[size];
						
						for(int i = 0; i < size; i++)
						{
							if(i < objs.Length)
							{
								decal.affectedObjects[i] = objs[i];
							}
							else
							{
								decal.affectedObjects[i] = objs[objs.Length - 1];
							}
						}
					}
					else
					{
						decal.affectedObjects = new GameObject[size];
					}
				}
			}
			else
			{
				GUILayout.Label(size.ToString());
			}
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			
			GUILayout.BeginVertical();
			
			for(int i = 0; i < size; i++)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label("Element " + i.ToString());
				GUILayout.Space(30);
				if(!decal.checkAutomatically)
				{
					decal.affectedObjects[i] = (GameObject)EditorGUILayout.ObjectField(decal.affectedObjects[i], typeof(GameObject));
				}
				else
				{
					EditorGUILayout.ObjectField(decal.affectedObjects[i], typeof(GameObject));
				}
				GUILayout.EndHorizontal();
			}
			GUILayout.EndVertical();

			GUILayout.EndVertical();
			
			GUILayout.EndHorizontal();
		}
		GUILayout.EndVertical();
		
		GUILayout.EndHorizontal();
	}	

	//Get the objects that will be affected by the decal.
	private void GetAffectedObjects(Decal decal)
	{
		int affectedLayers = (int)decal.affectedLayers;
		
		decal.affectedObjects = null;
		
		if(affectedLayers == 0) return;
		
		MeshRenderer[] renderers = (MeshRenderer[])(Object.FindObjectsOfType(typeof(MeshRenderer)));

		if(renderers != null)
		{
			Bounds decalBounds = decal.bounds;

			List<GameObject> affectedObjects = new List<GameObject>();
			
			int mLength = renderers.Length;
			
			GameObject decalGO = decal.gameObject;
			GameObject auxGO;
			int cLayer;
			
			if(mLength > 0)
			{
				for(int i = 0; i < mLength; i++)
				{
					auxGO = renderers[i].gameObject;
					
					//Do not affect the projector
					if(auxGO == decalGO) continue;
					
					//Layer check.
					//-1 means everything will be affected, so we don't need to check this case.
					if(affectedLayers != -1)
					{
						cLayer = 1<<auxGO.layer;
						
						if((cLayer & affectedLayers) == 0) continue;
					}
					
					if(!decal.affectOtherDecals)
					{
						if(auxGO.GetComponent<Decal>() != null) continue;
					}
					
					if(!decal.affectInactiveRenderers)
					{
						if(!renderers[i].enabled) continue;
					}
					
					Bounds b = renderers[i].bounds;
					
					if(decalBounds.Intersects(b))
					{
						affectedObjects.Add(auxGO);
					}
				}
				
				mLength = affectedObjects.Count;
				decal.affectedObjects = new GameObject[mLength];
				
				for(int i = 0; i < mLength; i++)
				{
					decal.affectedObjects[i] = affectedObjects[i];
				}
				
				affectedObjects.Clear();
				affectedObjects = null;
				
				System.GC.Collect();
			}
		}
	}
}