using UnityEngine;
using UnityEditor;

public class DecalUVEditorWindow : EditorWindow 
{
	static public void Initialize (Decal d) 
	{
		DecalUVEditorWindow window = (DecalUVEditorWindow)EditorWindow.GetWindow(typeof(DecalUVEditorWindow), false, "Decal UV");
		window.currentDecal = d;
		window.scale = false;
		window.move = false;
		window.texture = d.decalMaterial.mainTexture;
		window.textureSize = new Vector2(window.texture.width, window.texture.height);
		window.boxRect = new Rect(d.offset.x * window.textureSize.x, (1.0f - d.offset.y - d.tiling.y) * window.textureSize.y, d.tiling.x * window.textureSize.x, d.tiling.y * window.textureSize.y);
		window.startPos = new Vector2(window.boxRect.x, window.boxRect.y);
		window.delta = new Vector2(window.boxRect.width, window.boxRect.height);
	}
	
	public Decal currentDecal;
	public Texture texture;
	public Vector2 textureSize;
	
	private Vector2 scrollPosition;
	private Rect windowRect = new Rect(30, 30, 512, 512);
	
	public float zoom = 1.0f;

	public bool scale;
	public bool move;
	
	private Vector2 startPos;
	private Vector2 mousePos;
	private Vector2 movePosDif;
	private Vector2 delta;
	
	private int width;
	private string widthT;
	private int height;
	private string heightT;
	
	public Rect boxRect;
	
	private void OnGUI()
	{
		wantsMouseMove = true;
		
		if(currentDecal != null)
		{
			if(texture != null)
			{
				windowRect = new Rect(0, 0, Screen.width, Screen.height - 22);
				
				ControlMouse();
				
				GUILayout.BeginArea(new Rect(5, 5, Screen.width - 10, 20));
					GUILayout.BeginHorizontal();
						GUILayout.Label("Zoom");
						GUILayout.Space(5);
						GUILayout.BeginVertical();
							GUILayout.Space(4);
							zoom = GUILayout.HorizontalScrollbar(zoom, 0.5f, 1f, 30.0f, GUILayout.Width(Screen.width * 0.7f));
						GUILayout.EndVertical();
						GUILayout.FlexibleSpace();
						GUILayout.Label(zoom.ToString());
					GUILayout.EndHorizontal();
				GUILayout.EndArea();
				
				GUILayout.BeginArea(new Rect(5, 25, Screen.width - 10, 100));
					GUILayout.BeginHorizontal();
						GUILayout.Label("Width");
						GUILayout.Space(5);
						widthT = width.ToString();
						widthT = GUILayout.TextField(widthT, GUILayout.MinWidth(50));
						int.TryParse(widthT, out width);
						GUILayout.Space(10);
						GUILayout.Label("Height");
						GUILayout.Space(5);
						heightT = height.ToString();
						heightT = GUILayout.TextField(heightT, GUILayout.MinWidth(50));
						int.TryParse(heightT, out height);
						GUILayout.FlexibleSpace();
					GUILayout.EndHorizontal();
				GUILayout.EndArea();
				
				windowRect.y += 60;
				windowRect.height -= 90;
				GUILayout.BeginArea(windowRect);
					scrollPosition = GUILayout.BeginScrollView(scrollPosition);
						//FAKE HELPER TO MAKE THE SCROLLVIEW WORK
						GUILayout.Label("", GUILayout.Width(textureSize.x * zoom), GUILayout.Height(textureSize.y * zoom));
						
						mousePos = new Vector2(Event.current.mousePosition.x / zoom, Event.current.mousePosition.y / zoom);
				
						GUI.DrawTexture(new Rect(0, 0, zoom * textureSize.x, zoom * textureSize.y), texture);
						GUI.Box(new Rect(boxRect.x * zoom, boxRect.y * zoom, boxRect.width * zoom, boxRect.height * zoom), ""); 
					GUILayout.EndScrollView();
				GUILayout.EndArea();
				
				GUILayout.BeginArea(new Rect(0, Screen.height - 45, Screen.width, 30));
				GUILayout.BeginHorizontal();
				if(GUILayout.Button("Cancel"))
				{
					Close();
				}
				else if(GUILayout.Button("Apply"))
				{
					if(boxRect.x >= 0.0f && boxRect.y >= 0.0f && boxRect.width > 0.0f && boxRect.height > 0.0f)
					{
						currentDecal.offset = new Vector2(boxRect.x / textureSize.x, Mathf.Abs((boxRect.y / textureSize.y) - 1.0f + (boxRect.height / textureSize.y)));
						currentDecal.tiling = new Vector2(boxRect.width / textureSize.x, boxRect.height / textureSize.y);
					}
					Close();
				}
				GUILayout.EndHorizontal();
				GUILayout.EndArea();
			}
		}
	}
	
	private void ControlMouse()
	{
		if(Event.current.type == EventType.MouseDown)
		{
			Rect r;
			r = new Rect(0, 0, Screen.width, Screen.height - 102);
			Vector2 m = new Vector2(mousePos.x - (scrollPosition.x / zoom), mousePos.y - (scrollPosition.y / zoom));
			
			if(r.Contains(m))
			{
				r = new Rect(startPos.x, startPos.y, delta.x, delta.y);
				if(r.Contains(mousePos) && !move)
				{
					move = true;
					movePosDif = new Vector2(mousePos.x - startPos.x, mousePos.y - startPos.y);
				}
				else
				{
					if(!scale && !move)
					{
						scale = true;
						startPos = mousePos;
					}
				}
			}
		}
		else if(Event.current.type == EventType.MouseUp)
		{
			scale = false;
			move = false;
		}
		
		if(scale)
		{
			delta = new Vector2(mousePos.x - startPos.x, mousePos.y - startPos.y);
			AdjustRect();
			Repaint();
		}
		else if(move)
		{
			startPos = new Vector2(mousePos.x - movePosDif.x, mousePos.y - movePosDif.y);
			AdjustRect();
			Repaint();
		}
	}
	
	private void AdjustRect()
	{
		if(delta.x < 0.0f)
		{
			boxRect.x = startPos.x + delta.x;
		}
		else 
		{
			boxRect.x = startPos.x;
		}
		
		if(delta.y < 0.0f)
		{
			boxRect.y = startPos.y + delta.y;
		}
		else
		{
			boxRect.y = startPos.y;
		}
		
		boxRect.width = Mathf.Abs(delta.x);
		boxRect.height = Mathf.Abs(delta.y);
		
		if(boxRect.x < 0.0f)
		{
			boxRect.width = boxRect.width + boxRect.x;
			boxRect.x = 0.0f;
		}
		
		if(boxRect.y < 0.0f)
		{
			boxRect.height = boxRect.height + boxRect.y;
			boxRect.y = 0.0f;
		}
		
		if(boxRect.width + boxRect.x > textureSize.x)
		{
			boxRect.width = textureSize.x - boxRect.x;
		}
		
		if(boxRect.height + boxRect.y > textureSize.y)
		{
			boxRect.height = textureSize.y - boxRect.y;
		}
	}
}