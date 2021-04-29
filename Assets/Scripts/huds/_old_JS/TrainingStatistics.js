#pragma strict
#pragma implicit
#pragma downcast

class TrainingStatistics extends MonoBehaviour
{
	public static var statistics : TrainingStat[];
	public static var shootsFired : int;
	public static var totalHits : int;
	public static var grenadeFired : int;
	public static var headShoot : int;
	public static var turrets : int;
	public static var dummies : int;
	public static var turretsHit : int;
	public static var dummiesHit : int;
	public static var eaglesEye : int;
	public static var totalEaglesEye : int;
	public static var blueLeaf : int;
	public static var totalBlueLeaf : int;
	
	public static var head : int;
	public static var chest : int;
	public static var heart : int;
	public static var lArm : int;
	public static var rArm : int;
	public static var torso : int;
	
	static function ResetStatistics()
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
		
		var t : TrainingStat;
		
		for(var i : int = 0; i < 14; i++)
		{
			t = new TrainingStat();
			t.dummyPart = i;
			t.name = t.dummyPart.ToString().Replace("_", " ");
			t.points = 0;
			
			statistics[i] = t;	
		}
	}
	
	static function Register(part : DummyPart)
	{
		if(statistics == null) return;
		
		var i : int = part;
		
		if(i < 0 || i > statistics.length - 1) return;
		
		totalHits++;
		
		if(statistics[i] != null)
		{
			statistics[i].points++;
		}
		
		switch(part)
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

enum DummyPart
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