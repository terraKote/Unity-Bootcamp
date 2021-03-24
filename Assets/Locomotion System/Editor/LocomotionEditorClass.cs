/*
Copyright (c) 2008, Rune Skovbo Johansen & Unity Technologies ApS

See the document "TERMS OF USE" included in the project folder for licencing details.
*/
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

class InspectorAnimationGroup {
	public string name;
	public List<MotionAnalyzer> motions = new List<MotionAnalyzer>();
	
	public InspectorAnimationGroup (string name) {
		this.name = name;
	}
}

[CustomEditor(typeof(LegController))]
class LocomotionEditorClass : Editor {
	
	List<bool> legFoldouts = new List<bool>();
	bool legsFoldout = false;
	bool animsFoldout = false;
	
	public override void OnInspectorGUI () {
		LegController lc = target as LegController;
		if (!lc)
			return;
		
		EditorGUIUtility.LookLikeControls();
		
		lc.groundPlaneHeight = EditorGUILayout.FloatField("Ground Height", lc.groundPlaneHeight);
		lc.groundedPose = EditorGUILayout.ObjectField("Grounded Pose", lc.groundedPose, typeof(AnimationClip)) as AnimationClip;
		lc.rootBone = EditorGUILayout.ObjectField("Root Bone", lc.rootBone, typeof(Transform)) as Transform;
		
		EditorGUILayout.Space();
		
		// Handle legs array
		
		List<LegInfo> legs = new List<LegInfo>(lc.legs);
		if (legs.Count != legFoldouts.Count)
			legFoldouts = new List<bool>(new bool[legs.Count]);
		
		legsFoldout = EditorGUILayout.Foldout(legsFoldout, "Legs");
		
		if (legsFoldout) {
			GUI.changed = false;
			
			int removeIndex = -1;
			for (int l=0; l<legs.Count; l++) {
				GUILayout.BeginHorizontal();
				string str = "Leg " + (l+1) + (legs[l].hip != null ? " (" + legs[l].hip.name + ")" : "");
				legFoldouts[l] = EditorGUILayout.Foldout(legFoldouts[l], str);
				if (GUILayout.Button("Remove", GUILayout.Width(80)))
					removeIndex = l;
				GUILayout.EndHorizontal();
				
				if (legFoldouts[l]) {
					EditorGUI.indentLevel++;
					
					LegInfo li = legs[l];
					li.hip = EditorGUILayout.ObjectField("Hip", li.hip, typeof(Transform)) as Transform;
					li.ankle = EditorGUILayout.ObjectField("Ankle", li.ankle, typeof(Transform)) as Transform;
					li.toe = EditorGUILayout.ObjectField("Toe", li.toe, typeof(Transform)) as Transform;
					
					GUILayout.BeginHorizontal();
					EditorGUILayout.PrefixLabel("Foot");
					GUILayout.BeginVertical();
					GUILayout.Label("Width", GUILayout.Width(40));
					GUILayout.Label("Length", GUILayout.Width(40));
					GUILayout.EndVertical();
					GUILayout.BeginVertical();
					li.footWidth = EditorGUILayout.FloatField(li.footWidth, GUILayout.Width(50));
					li.footLength = EditorGUILayout.FloatField(li.footLength, GUILayout.Width(50));
					GUILayout.EndVertical();
					GUILayout.BeginVertical();
					GUILayout.Label("Offset", GUILayout.Width(40));
					GUILayout.Label("Offset", GUILayout.Width(40));
					GUILayout.EndVertical();
					GUILayout.BeginVertical();
					li.footOffset.x = EditorGUILayout.FloatField(li.footOffset.x, GUILayout.Width(50));
					li.footOffset.y = EditorGUILayout.FloatField(li.footOffset.y, GUILayout.Width(50));
					GUILayout.EndVertical();
					GUILayout.EndHorizontal();
					
					EditorGUI.indentLevel--;
					
					EditorGUILayout.Space();
				}
			}
			
			// Remove a leg?
			if (removeIndex >= 0) {
				legs.RemoveAt(removeIndex);
				legFoldouts.RemoveAt(removeIndex);
			}
			
			// Add a leg?
			GUILayout.BeginHorizontal();
			GUILayout.Label("");
			if (GUILayout.Button("Add Leg", GUILayout.Width(80))) {
				LegInfo li = new LegInfo();
				if (legs.Count > 0) {
					li.footWidth = legs[legs.Count-1].footWidth;
					li.footLength = legs[legs.Count-1].footLength;
					li.footOffset = legs[legs.Count-1].footOffset;
				}
				legs.Add(li);
				legFoldouts.Add(true);
			}
			GUILayout.EndHorizontal();
			
			lc.legs = legs.ToArray();
		}
		
		EditorGUILayout.Space();
		
		// Handle animations array
		
		animsFoldout = EditorGUILayout.Foldout(animsFoldout, "Source Animations");
		
		if (animsFoldout) {
			GUI.changed = false;
			
			List<InspectorAnimationGroup> groups = new List<InspectorAnimationGroup>();
			groups.Add(new InspectorAnimationGroup(""));
			if (GUILayout.Button("Reset") || lc.sourceAnimations.Length == 0) {
				AnimationClip[] clips = AnimationUtility.GetAnimationClips(lc.animation);
				for (int c=0; c<clips.Length; c++) {
					MotionAnalyzer motion = new MotionAnalyzer();
					motion.animation = clips[c];
					//motion.motionGroup = "";
					groups[0].motions.Add(motion);
				}
				GUI.changed = true;
			}
			else {
				for (int m=0; m<lc.sourceAnimations.Length; m++) {
					MotionAnalyzer ma = lc.sourceAnimations[m];
					if (ma.motionGroup == null)
						ma.motionGroup = "";
					
					bool found = false;
					for (int g=0; g<groups.Count; g++) {
						if (ma.motionGroup == groups[g].name) {
							groups[g].motions.Add(ma);
							found = true;
						}
					}
					if (!found) {
						InspectorAnimationGroup group = new InspectorAnimationGroup(ma.motionGroup);
						group.motions.Add(ma);
						groups.Add(group);
					}
				}
			}
			groups.Add(new InspectorAnimationGroup("NewGroup"));
			groups.Add(new InspectorAnimationGroup("Remove"));
			
			string[] groupNames = new string[groups.Count];
			for (int g=0; g<groups.Count; g++) {
				groupNames[g] = groups[g].name;
			}
			groupNames[0] = "Ungrouped";
			
			for (int g=0; g<groups.Count-1; g++) {
				InspectorAnimationGroup group = groups[g];
				
				if (group.motions.Count == 0)
					continue;
				
				if (group.name == "") {
					GUILayout.Label("Ungrouped Animations");
				}
				else {
					GUILayout.BeginHorizontal();
					GUILayout.Label("Animation Group:", GUILayout.ExpandWidth(false));
					group.name = GUILayout.TextField(group.name, GUILayout.ExpandWidth(true));
					GUILayout.EndHorizontal();
				}
				
				for (int m=0; m<group.motions.Count; m++) {
					MotionAnalyzer ma = group.motions[m];
					
					GUILayout.BeginHorizontal();
					
					ma.animation = EditorGUILayout.ObjectField(ma.animation, typeof(AnimationClip)) as AnimationClip;
					ma.motionType = (MotionType)EditorGUILayout.EnumPopup(ma.motionType, GUILayout.Width(70));
					
					//ma.alsoUseBackwards = GUILayout.Toggle(ma.alsoUseBackwards, "", GUILayout.Width(15));
					//ma.fixFootSkating = GUILayout.Toggle(ma.fixFootSkating, "", GUILayout.Width(15));
					int selectedGroup = EditorGUILayout.Popup(g, groupNames, GUILayout.Width(70));
					if (selectedGroup != g) {
						group.motions.Remove(ma);
						groups[selectedGroup].motions.Add(ma);
						GUI.changed = true;
					}
					
					GUILayout.Label(""+ma.nativeSpeed, GUILayout.Width(40));
					
					GUILayout.EndHorizontal();
				}
				
				if (GUILayout.Button("Add Animation to "+groupNames[g])) {
					MotionAnalyzer motion = new MotionAnalyzer();
					group.motions.Add(motion);
					GUI.changed = true;
				}
				
				EditorGUILayout.Space();
			}
			
			if (GUI.changed) {
				List<MotionAnalyzer> motions = new List<MotionAnalyzer>();
				for (int g=0; g<groups.Count-1; g++) {
					for (int m=0; m<groups[g].motions.Count; m++) {
						groups[g].motions[m].motionGroup = groups[g].name;
						motions.Add(groups[g].motions[m]);
					}
				}
				lc.sourceAnimations = motions.ToArray();
			}
		}
		
	}
	
	[DrawGizmo (GizmoType.SelectedOrChild)]
	static void RenderGizmo (LegController legC, GizmoType gizmoType) {
		if (Application.isPlaying || AnimationUtility.InAnimationMode())
			return;
		
		Vector3 up = legC.transform.up;
		Vector3 forward = legC.transform.forward;
		Vector3 right = legC.transform.right;
		
		// Draw cross signifying the Ground Plane Height
		Vector3 groundCenter = (
			legC.transform.position
				+ legC.groundPlaneHeight * up * legC.transform.lossyScale.y
		);
		Gizmos.color = (Color.green+Color.white)/2;
		Gizmos.DrawLine(groundCenter-forward, groundCenter+forward);
		Gizmos.DrawLine(groundCenter-right, groundCenter+right);
		
		// Draw rect showing foot boundaries
		if (legC.groundedPose==null) return;
		float scale = legC.transform.lossyScale.z;
		for (int leg=0; leg<legC.legs.Length; leg++) {
			if (legC.legs[leg].ankle==null) continue;
			if (legC.legs[leg].toe==null) continue;
			if (legC.legs[leg].footLength+legC.legs[leg].footWidth==0) continue;
			legC.InitFootData(leg);
			Vector3 heel = legC.legs[leg].ankle.TransformPoint(legC.legs[leg].ankleHeelVector);
			Vector3 toetip = legC.legs[leg].toe.TransformPoint(legC.legs[leg].toeToetipVector);
			Vector3 side = (Quaternion.AngleAxis(90,up) * (toetip-heel)).normalized * legC.legs[leg].footWidth * scale;
			Gizmos.DrawLine(heel+side/2, toetip+side/2);
			Gizmos.DrawLine(heel-side/2, toetip-side/2);
			Gizmos.DrawLine(heel-side/2, heel+side/2);
			Gizmos.DrawLine(toetip-side/2, toetip+side/2);
		}
	}
	
	private static bool SanityCheckAnimationCurves(LegController legC, AnimationClip animation) {
		AnimationClipCurveData[] curveData = AnimationUtility.GetAllCurves(animation,false);
		
		bool hasRootPosition = false;
		bool hasRootRotation = false;
		
		// Check each joint from hip to ankle in each leg
		bool[][] hasJointRotation = new bool[legC.legs.Length][];
		for (int i=0; i<legC.legs.Length; i++) {
			hasJointRotation[i] = new bool[legC.legs[i].legChain.Length];
		}
		
		foreach (AnimationClipCurveData data in curveData) {
			Transform bone = legC.transform.Find(data.path);
			if (bone==legC.root && data.propertyName=="m_LocalPosition.x") hasRootPosition = true;
			if (bone==legC.root && data.propertyName=="m_LocalRotation.x") hasRootRotation = true;
			for (int i=0; i<legC.legs.Length; i++) {
				for (int j=0; j<legC.legs[i].legChain.Length; j++) {
					if (bone==legC.legs[i].legChain[j] &&  data.propertyName=="m_LocalRotation.x") {
						hasJointRotation[i][j] = true;
					}
				}
			}
		}
		
		bool success = true;
		
		if (!hasRootPosition) {
			Debug.LogError("AnimationClip \""+animation.name+"\" is missing animation curve for the position of the root bone \""+legC.root.name+"\".");
			success = false;
		}
		if (!hasRootRotation) {
			Debug.LogError("AnimationClip \""+animation.name+"\" is missing animation curve for the rotation of the root bone \""+legC.root.name+"\".");
			success = false;
		}
		for (int i=0; i<legC.legs.Length; i++) {
			for (int j=0; j<legC.legs[i].legChain.Length; j++) {
				if (!hasJointRotation[i][j]) {
					Debug.LogError("AnimationClip \""+animation.name+"\" is missing animation curve for the rotation of the joint \""+legC.legs[i].legChain[j].name+"\" in leg "+i+".");
					success = false;
				}
			}
		}
		
		return success;
	}
	
	[MenuItem ("Tools/Locomotion Initialization")]
    static void DoToggle()
    {
        Debug.Log("Menu item selected");
        GameObject activeGO = Selection.activeGameObject;
        LegController legC = activeGO.GetComponent(typeof(LegController)) as LegController;
        
        legC.Init();
        
        bool success = true;
        foreach (MotionAnalyzer analyzer in legC.sourceAnimations) {
        	if (!SanityCheckAnimationCurves(legC,analyzer.animation)) success = false;
        }
        if (!success) return;
        
        legC.Init2();
    }
	
}
