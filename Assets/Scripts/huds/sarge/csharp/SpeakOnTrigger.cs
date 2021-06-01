using UnityEngine;
using System.Collections;

[System.Serializable]
public class TriggerInstruction
{
    public string instructionName;
    public float instructionDelay = 0.0f;
    [HideInInspector] public bool playing = false;
}

public class SpeakOnTrigger : MonoBehaviour
{
    public TriggerInstruction[] instructions;
	
	private bool playing;
	private float timer ;
	private int instructionsToPlay ;
	
	void Start()
    {
        playing = false;
        gameObject.layer = 2;
        instructionsToPlay = instructions.Length;
    }

    void Update()
    {
        if (playing && instructionsToPlay > 0)
        {
            timer += Time.deltaTime;

            for (int i = 0; i < instructions.Length; i++)
			{
                if (!instructions[i].playing)
                {
                    if (instructions[i].instructionDelay < timer)
                    {
                        instructions[i].playing = true;
                        instructionsToPlay--;
                        SendMessageUpwards("ShowInstruction", instructions[i].instructionName, SendMessageOptions.DontRequireReceiver);
                    }
                }
            }
        }
        else if (instructionsToPlay <= 0)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!playing)
        {
            if (other.name.ToLower() == "soldier")
            {
                playing = true;
                timer = 0.0f;
            }
        }
    }
}
