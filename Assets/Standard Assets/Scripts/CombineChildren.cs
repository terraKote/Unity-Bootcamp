using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
Attach this script as a parent to some game objects. The script will then combine the meshes at startup.
This is useful as a performance optimization since it is faster to render one big mesh than many small meshes. See the docs on graphics performance optimization for more info.

Different materials will cause multiple meshes to be created, thus it is useful to share as many textures/material as you can.
*/

[AddComponentMenu("Mesh/Combine Children")]
public class CombineChildren : MonoBehaviour {
	
	/// Usually rendering with triangle strips is faster.
	/// However when combining objects with very low triangle counts, it can be faster to use triangles.
	/// Best is to try out which value is faster in practice.
	public bool generateTriangleStrips = true;
	
	//[HideInInspector]
	public List<GameObject> currentMeshes;
	
	//[HideInInspector]
	public List<Renderer> allRenderers;
	
	public bool combineAtStart = false;
	public bool destroyOnDisable = false;
	
	[HideInInspector]
	public bool areaCombine = false;
	
	public bool removeColliders = false;
	
	private void Start()
	{
		if(combineAtStart)
		{
			Combine();
		}
	}
	
	public void DisableCombine()
	{
		if(currentMeshes != null)
		{
			for(int i = 0; i < currentMeshes.Count; i++)
			{
				if(currentMeshes[i] != null)
				{
					if(Application.isEditor)
					{
						DestroyImmediate(currentMeshes[i]);	
					}
					else
					{
						Destroy(currentMeshes[i]);
					}
				}
			}
			
			currentMeshes.Clear();
		}
		
		if(allRenderers != null)
		{
			for(int i = 0; i < allRenderers.Count; i++)
			{
				if(allRenderers[i] != null)
				{
					allRenderers[i].enabled = true;
				}
			}
			
			allRenderers.Clear();
		}
		
		if (GetComponent(typeof(MeshFilter)) != null)
		{
			if(Application.isEditor)
			{
				DestroyImmediate(GetComponent(typeof(MeshFilter)));	
			}
			else
			{
				Destroy(GetComponent(typeof(MeshFilter)));
			}
		}
		
		if(GetComponent("MeshRenderer"))
		{
			if(Application.isEditor)
			{
				DestroyImmediate(GetComponent(typeof(MeshRenderer)));
			}
			else
			{
				Destroy(GetComponent("MeshRenderer"));
			}
		}
	}
	
	/// This option has a far longer preprocessing time at startup but leads to better runtime performance.
	public void Combine () 
	{		
		if(!Application.isEditor)
		{
			DisableCombine();
		}
		
		currentMeshes = new List<GameObject>();
		allRenderers = new List<Renderer>();
				
		Component[] filters  = GetComponentsInChildren(typeof(MeshFilter));
		Matrix4x4 myTransform = transform.worldToLocalMatrix;
		Hashtable materialToMesh= new Hashtable();
		
		MeshFilter f = GetComponent<MeshFilter>();
		
		for (int i=0;i<filters.Length;i++) 
		{
			MeshFilter filter = (MeshFilter)filters[i];
			
			if(f != null)
			{
				if(f == filter) continue;
			}
			
			if(removeColliders)
			{
				if(filter.collider != null)
				{
					if(Application.isEditor)
					{
						DestroyImmediate(filter.collider);
					}
					else
					{
						Destroy(filter.collider);
					}
				}
			}
			
			Renderer curRenderer  = filters[i].renderer;
			MeshCombineUtility.MeshInstance instance = new MeshCombineUtility.MeshInstance ();
			instance.mesh = filter.sharedMesh;
			if (curRenderer != null && curRenderer.enabled && instance.mesh != null) 
			{
				instance.transform = myTransform * filter.transform.localToWorldMatrix;
				
				Material[] materials = curRenderer.sharedMaterials;
				for (int m=0;m<materials.Length;m++) {
					instance.subMeshIndex = System.Math.Min(m, instance.mesh.subMeshCount - 1);
	
					ArrayList objects = (ArrayList)materialToMesh[materials[m]];
					if (objects != null) {
						objects.Add(instance);
					}
					else
					{
						objects = new ArrayList ();
						objects.Add(instance);
						materialToMesh.Add(materials[m], objects);
					}
				}
				
				allRenderers.Add(curRenderer);
				curRenderer.enabled = false;
			}
		}
	
		foreach (DictionaryEntry de  in materialToMesh) {
			ArrayList elements = (ArrayList)de.Value;
			MeshCombineUtility.MeshInstance[] instances = (MeshCombineUtility.MeshInstance[])elements.ToArray(typeof(MeshCombineUtility.MeshInstance));

			// We have a maximum of one material, so just attach the mesh to our own game object
			if (materialToMesh.Count == 1)
			{
				// Make sure we have a mesh filter & renderer
				if (GetComponent(typeof(MeshFilter)) == null)
					gameObject.AddComponent(typeof(MeshFilter));
				if (!GetComponent("MeshRenderer"))
					gameObject.AddComponent("MeshRenderer");
	
				MeshFilter filter = (MeshFilter)GetComponent(typeof(MeshFilter));
				filter.mesh = MeshCombineUtility.Combine(instances, generateTriangleStrips);
				renderer.material = (Material)de.Key;
				renderer.enabled = true;
			}
			// We have multiple materials to take care of, build one mesh / gameobject for each material
			// and parent it to this object
			else
			{
				GameObject go = new GameObject("Combined mesh");
				currentMeshes.Add(go);
				go.transform.parent = transform;
				go.transform.localScale = Vector3.one;
				go.transform.localRotation = Quaternion.identity;
				go.transform.localPosition = Vector3.zero;
				go.AddComponent(typeof(MeshFilter));
				go.AddComponent("MeshRenderer");
				go.renderer.material = (Material)de.Key;
				MeshFilter filter = (MeshFilter)go.GetComponent(typeof(MeshFilter));
				filter.mesh = MeshCombineUtility.Combine(instances, generateTriangleStrips);
			}
		}	
	}	
}