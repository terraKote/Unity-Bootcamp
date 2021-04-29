using UnityEngine;
using System.Collections;

public enum DummyPart
{
	HEAD = 0,
	NECK,
	FACE,
	CHEST,
	STOMACH,
	LOWER_STOMACH,
	LEFT_HAND,
	LEFT_FOREARM,
	LEFT_ARM,
	RIGHT_HAND,
	RIGHT_FOREARM,
	RIGHT_ARM,
	OTHER,
	HEART,
}

public class TrainingStatistics : MonoBehaviour {
	public static TrainingStat[] statistics ;
	public static int shootsFired ;
	public static int totalHits ;
	public static int grenadeFired ;
	public static int headShoot ;
	public static int turrets ;
	public static int dummies ;
	public static int turretsHit ;
	public static int dummiesHit ;
	public static int eaglesEye ;
	public static int totalEaglesEye ;
	public static int blueLeaf ;
	public static int totalBlueLeaf ;
	
	public static int head ;
	public static int chest ;
	public static int heart ;
	public static int lArm ;
	public static int rArm ;
	public static int torso ;
	
	public static void ResetStatistics()
	{
		statistics = new TrainingStat[14];

		head = 0;
		chest = 0;
		heart = 0;
		lArm = 0;
		rArm = 0;
		torso = 0;

		totalHits = 0;
		shootsFired = 0;
		grenadeFired = 0;
		headShoot = 0;
		turrets = 0;
		dummies = 0;
		turretsHit = 0;
		dummiesHit = 0;
		eaglesEye = 0;
		blueLeaf = 0;
		totalEaglesEye = 0;
		totalBlueLeaf = 0;

		TrainingStat t ;

		for (var i = 0; i < 14; i++)
		{
			t = new TrainingStat();
			t.dummyPart = (DummyPart)i;
			t.name = t.dummyPart.ToString().Replace("_", " ");
			t.points = 0;

			statistics[i] = t;
		}
	}

	static void Register(DummyPart part )
	{
		if (statistics == null) return;

		int i = (int)part;

		if (i < 0 || i > statistics.Length - 1) return;

		totalHits++;

		if (statistics[i] != null)
		{
			statistics[i].points++;
		}

		switch (part)
		{
			case DummyPart.HEAD:
				head++;
				break;
			case DummyPart.NECK:
				head++;
				break;
			case DummyPart.FACE:
				head++;
				break;
			case DummyPart.CHEST:
				chest++;
				break;
			case DummyPart.STOMACH:
				torso++;
				break;
			case DummyPart.LOWER_STOMACH:
				torso++;
				break;
			case DummyPart.LEFT_HAND:
				lArm++;
				break;
			case DummyPart.LEFT_FOREARM:
				lArm++;
				break;
			case DummyPart.LEFT_ARM:
				lArm++;
				break;
			case DummyPart.RIGHT_HAND:
				rArm++;
				break;
			case DummyPart.RIGHT_FOREARM:
				rArm++;
				break;
			case DummyPart.RIGHT_ARM:
				rArm++;
				break;
			case DummyPart.HEART:
				heart++;
				break;
			case DummyPart.OTHER:
				torso++;
				break;
		}
	}
}

