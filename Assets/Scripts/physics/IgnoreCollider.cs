using UnityEngine;
using System.Collections;

public class IgnoreCollider : MonoBehaviour 
{
	public Collider otherCollider;
	
	// Use this for initialization
	void Start () 
	{
		Physics.IgnoreCollision(transform.collider, otherCollider, true);
	}
}
