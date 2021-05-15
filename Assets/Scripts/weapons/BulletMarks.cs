using UnityEngine;
using System.Collections;

public enum HitType
{
	CONCRETE,
	WOOD,
	METAL,
	OLD_METAL,
	GLASS,
	GENERIC
}

public class BulletMarks : MonoBehaviour {
	public Texture2D[] concrete;
	public Texture2D[] wood;
	public Texture2D[] metal;
	public Texture2D[] oldMetal;
	public Texture2D[] glass;
	public Texture2D[] generic;
	
	public void GenerateDecal(HitType type, GameObject go )
	{
		Texture2D useTexture;
		int random;

		switch (type)
		{
			case HitType.CONCRETE:
				if (concrete == null) return;
				if (concrete.Length == 0) return;

				random = Random.Range(0, concrete.Length);

				useTexture = concrete[random];
				break;
			case HitType.WOOD:
				if (wood == null) return;
				if (wood.Length == 0) return;

				random = Random.Range(0, wood.Length);

				useTexture = wood[random];
				break;
			case HitType.METAL:
				if (metal == null) return;
				if (metal.Length == 0) return;

				random = Random.Range(0, metal.Length);

				useTexture = metal[random];
				break;
			case HitType.OLD_METAL:
				if (oldMetal == null) return;
				if (oldMetal.Length == 0) return;

				random = Random.Range(0, oldMetal.Length);

				useTexture = oldMetal[random];
				break;
			case HitType.GLASS:
				if (glass == null) return;
				if (glass.Length == 0) return;

				random = Random.Range(0, glass.Length);

				useTexture = glass[random];
				break;
			case HitType.GENERIC:
				if (generic == null) return;
				if (generic.Length == 0) return;

				random = Random.Range(0, generic.Length);

				useTexture = generic[random];
				break;
			default:
				if (wood == null) return;
				if (wood.Length == 0) return;

				random = Random.Range(0, wood.Length);

				useTexture = wood[random];
				return;
		}

		transform.Rotate(new Vector3(0, 0, Random.Range(-180.0f, 180.0f)));

		Decal.dCount++;
		Decal d = gameObject.GetComponent<Decal>();
		d.affectedObjects = new GameObject[1];
		d.affectedObjects[0] = go;
		d.decalMode = DecalMode.MESH_COLLIDER;
		d.pushDistance = 0.009 + BulletMarkManager.Add(gameObject);
		Material m = new Material(d.decalMaterial);
		m.mainTexture = useTexture;
		d.decalMaterial = m;
		d.CalculateDecal();
		d.transform.parent = go.transform;
	}
}
