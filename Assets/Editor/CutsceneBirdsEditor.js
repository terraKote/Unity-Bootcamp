
@CustomEditor(typeof(CutsceneBirds))
class CutsceneBirdsEditor extends Editor
{
	function OnInspectorGUI()
	{
		DrawDefaultInspector();
		
		if(GUILayout.Button("SET BIRDS PROPERTIES"))
		{
			SetupBirds(target as CutsceneBirds);
		}
	}
	
	function SetupBirds(cBirds : CutsceneBirds)
	{
		var go : GameObject = cBirds.gameObject;
		
		var birdGroup : Transform;
		var bird : Seagull;
		
		for(var j : int = 0; j < go.transform.childCount; j++)
		{
			birdGroup = go.transform.GetChild(j);
			
			for(var i : int = 0; i < birdGroup.childCount; i++)
			{
				bird = birdGroup.GetChild(i).gameObject.GetComponent("Seagull");
				
				if(bird == null) continue;
				
				bird.sounds = cBirds.sounds;
				bird.soundFrequency = cBirds.soundFrequency;
				bird.minSpeed = cBirds.minSpeed;
				bird.turnSpeed = cBirds.turnSpeed;
				bird.randomFreq = cBirds.randomFreq;
				bird.randomForce = cBirds.randomForce;
				bird.toOriginForce = cBirds.toOriginForce;
				bird.toOriginRange = cBirds.toOriginRange;
				bird.damping = cBirds.damping;
				bird.gravity = cBirds.gravity;
				bird.avoidanceRadius = cBirds.avoidanceRadius;
				bird.avoidanceForce = cBirds.avoidanceForce;
				bird.followVelocity = cBirds.followVelocity;
				bird.followRadius = cBirds.followRadius;
				bird.bankTurn = cBirds.bankTurn;
				bird.raycast = cBirds.raycast;
				bird.bounce = cBirds.bounce;
				bird.animationSpeed = cBirds.animationSpeed;
			}
		}
	}
}