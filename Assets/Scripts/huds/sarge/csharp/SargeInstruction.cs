using UnityEngine;
using System.Collections;

public class SargeInstruction  {

	public string name ;
	public string text ;
	public Texture2D texture;
	public float timeToDisplay = 3.0f;
	public AudioClip audio;
	public bool queuable = true;
	public bool overridable = false;
	public float volume = 1.0f;
}
