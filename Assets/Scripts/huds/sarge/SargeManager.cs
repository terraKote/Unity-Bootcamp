using UnityEngine;
using System.Collections;
using Bootcamp.Soldier;
using System.Collections.Generic;

namespace Bootcamp.Hud.Sarge
{
	public class SargeManager : MonoBehaviour
	{
		public Texture2D sarge;
		public Texture2D background;
		public SargeInstruction[] instructions;

		private bool visible;

		private Rect sargeRect;
		private Rect backgroundRect;
		private Vector2 halfScreen;
		private Rect container;

		private float sargeAlpha;
		private float backgroundAlpha;
		private float contentAlpha;

		public float fadeTime;

		private Color auxColor;
		private Color oldColor;

		private Hashtable table;

		private float timeToHide;

		private SargeInstruction currentInstruction;

		public GUIStyle textStyle;

		[HideInInspector] public bool debug;

		private bool audioWasPlaying;

		private List<string> messageQueue;
		private bool friendlyFire;
		private SargeInstruction lastInstruction;

		void Start()
		{
			messageQueue = new List<string>();

			friendlyFire = false;
			audioWasPlaying = false;

			table = new Hashtable();

			for (int i = 0; i < instructions.Length; i++)
			{
				if (instructions[i] != null)
				{
					if (!string.IsNullOrEmpty(instructions[i].name))
					{
						if (!table.ContainsKey(instructions[i].name.ToLower()))
						{
							table.Add(instructions[i].name.ToLower(), instructions[i]);
						}
					}
				}
			}

			fadeTime = 1.0f / fadeTime;

			sargeAlpha = 0.0f;
			visible = false;

			sargeRect = new Rect(0, 0, sarge.width, sarge.height);
			backgroundRect = new Rect(0, 0, background.width, background.height);

			background.wrapMode = TextureWrapMode.Clamp;

			container = new Rect(0, 0, sarge.width + background.width, Mathf.Max(sarge.height, background.height));

			if (audio == null)
			{
				gameObject.AddComponent("AudioSource");
			}

			audio.loop = false;
			audio.playOnAwake = false;
		}

		void StopInstructions()
		{
			if (messageQueue != null)
			{
				messageQueue.Clear();
			}

			timeToHide = 0.0f;

			if (audio.isPlaying)
			{
				audio.Stop();
			}
		}

		public void DrawGUI(Event e)
		{
			if (contentAlpha <= 0.0) return;

			if (GameManager.pause || SoldierController.dead || AchievmentScreen.returningToTraining)
			{
				GUI.color = new Color(0.5f, 0.5f, 0.5f, 0.0f);
				return;
			}

			auxColor = oldColor = GUI.color;

			halfScreen = new Vector2(Screen.width, Screen.height) * 0.5f;
			container.x = halfScreen.x - (container.width * 0.5f);
			container.y = Screen.height - (container.height * 0.5f);

			sargeRect.x = container.x;
			sargeRect.y = container.y - (sargeRect.height * 0.5f);

			backgroundRect.x = sargeRect.x + sargeRect.width;
			backgroundRect.y = container.y - (backgroundRect.height * 0.5f);

			auxColor.a = backgroundAlpha;
			GUI.color = auxColor;
			GUI.DrawTexture(backgroundRect, background);

			auxColor.a = sargeAlpha;
			GUI.color = auxColor;
			GUI.DrawTexture(sargeRect, sarge);

			DrawInstruction();

			GUI.color = oldColor;
		}

		void DrawInstruction()
		{
			if (currentInstruction == null) return;

			auxColor.a = contentAlpha;
			GUI.color = auxColor;

			if (currentInstruction.texture != null)
			{
				var auxRect = new Rect((backgroundRect.width - currentInstruction.texture.width) * 0.5f + backgroundRect.x, (backgroundRect.height - currentInstruction.texture.height) * 0.5f + backgroundRect.y, currentInstruction.texture.width, currentInstruction.texture.height);
				GUI.DrawTexture(auxRect, currentInstruction.texture);
			}
			else
			{
				GUI.TextArea(new Rect(backgroundRect.x + 30, backgroundRect.y + 10, backgroundRect.width - 60, backgroundRect.height - 20), currentInstruction.text, textStyle);
			}
		}

		Texture2D tex;
		void Update()
		{
			if (GameManager.pause || SoldierController.dead || AchievmentScreen.returningToTraining)
			{
				if (audio.isPlaying)
				{
					audioWasPlaying = true;
					audio.Pause();
				}
				return;
			}
			else if (audioWasPlaying)
			{
				audio.Play();
				audioWasPlaying = false;
			}

			if (!visible)
			{
				if (contentAlpha > 0.0) contentAlpha -= Time.deltaTime * fadeTime;
				else if (backgroundAlpha > 0.0) backgroundAlpha -= Time.deltaTime * fadeTime;
				else if (sargeAlpha > 0.0) sargeAlpha -= Time.deltaTime * fadeTime;
			}
			else
			{
				if (sargeAlpha < 1.0) sargeAlpha += Time.deltaTime * fadeTime;
				else if (backgroundAlpha < 1.0) backgroundAlpha += Time.deltaTime * fadeTime;
				else if (contentAlpha < 1.0) contentAlpha += Time.deltaTime * fadeTime;

				if (timeToHide >= 0)
				{
					timeToHide -= Time.deltaTime;

					if (timeToHide < 0.0)
					{
						if (friendlyFire)
						{
							friendlyFire = false;

							if (lastInstruction != null)
							{
								ShowInstruction(lastInstruction.name);
								lastInstruction = null;
							}
						}
						else
						{
							if (messageQueue.Count > 0)
							{
								string m = messageQueue[0];
								messageQueue.RemoveAt(0);
								ShowInstruction(m);
							}
							else
							{
								visible = false;
							}
						}
					}
				}
			}
		}

		void FriendlyFire()
		{
			if (friendlyFire) return;

			if (audio.isPlaying)
			{
				int i = Random.Range(0, 2);
				string m;

				if (i == 0)
				{
					m = "friendly_fire1";
				}
				else
				{
					m = "friendly_fire2";
				}

				if (table.ContainsKey(m.ToLower()))
				{
					lastInstruction = currentInstruction;
					friendlyFire = true;
					currentInstruction = (SargeInstruction)table[m];
					timeToHide = currentInstruction.timeToDisplay + ((1.0f - sargeAlpha) + (1.0f - backgroundAlpha) + (1.0f - contentAlpha)) * (1.0f / fadeTime);

					if (currentInstruction.audio != null)
					{
						audio.clip = currentInstruction.audio;
						audio.volume = currentInstruction.volume;
						audio.Play();
					}

					visible = true;
				}
			}
		}

		public void ShowInstruction(string instruction)
		{
			if (table == null) return;

			if (table.ContainsKey(instruction.ToLower()))
			{
				if (timeToHide > 0.0 || friendlyFire)
				{
					if (!currentInstruction.overridable)
					{
						if ((table[instruction] as SargeInstruction).queuable)
						{
							messageQueue.Add(instruction);
						}

						return;
					}
				}

				currentInstruction = (SargeInstruction)table[instruction];
				timeToHide = currentInstruction.timeToDisplay + ((1.0f - sargeAlpha) + (1.0f - backgroundAlpha) + (1.0f - contentAlpha)) * (1.0f / fadeTime);

				if (currentInstruction.audio != null)
				{
					audio.clip = currentInstruction.audio;
					audio.volume = currentInstruction.volume;
					audio.Play();
				}

				visible = true;
			}
		}
	}
}