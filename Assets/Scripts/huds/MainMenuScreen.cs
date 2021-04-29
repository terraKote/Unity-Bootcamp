using UnityEngine;
using System.Collections;

enum MainMenuState
{
    IDLE,
    OPTIONS,
    GRAPHICS,
    ABOUT,
}

public class MainMenuScreen : MonoBehaviour
{
    public Texture2D menuBackground;
    private Rect menuBackgroundRect;

    public Texture2D windowBackground;
    private Rect windowBackgroundRect;

    public Texture2D playGame;
    public Texture2D playGameOver;
    private Rect playGameRect;

    public Texture2D resume;
    public Texture2D resumeOver;
    private Rect resumeRect;

    public Texture2D options;
    public Texture2D optionsOver;
    private Rect optionsRect;

    public Texture2D graphics;
    public Texture2D graphicsOver;
    private Rect graphicsRect;

    public Texture2D about;
    public Texture2D aboutOver;
    private Rect aboutRect;

    public GUISkin hudSkin;

    private GUIStyle panelLeft;
    private Rect panelLeftRect;

    private GUIStyle panelRight;
    private Rect panelRightRect;

    private GUIStyle descriptionStyle;
    private GUIStyle titleStyle;
    private GUIStyle customBox;

    private Vector2 mousePos;
    private Vector2 screenSize;

    private MainMenuState state;
    private float lastMouseTime;

    public Texture2D receiveDamageOn;
    public Texture2D receiveDamageOff;
    public Texture2D dontReceiveDamageOn;
    public Texture2D dontReceiveDamageOff;
    private Rect damageRect;

    private Rect scrollPosition;
    private Rect scrollView;
    private Vector2 scroll;

    public Texture2D black;
    private float alpha;
    static public bool goingToGame;
    static public bool showProgress;

    private Vector2 aboutScroll;
    private Vector2 graphicsScroll;
    private GUIStyle aboutStyle;

    private bool resumeGame;
    public bool visible;

    private GUIStyle sliderBackground;
    private GUIStyle sliderButton;

    public Texture2D greenBar;
    public Texture2D checkOn;
    public Texture2D checkOff;
    public Texture2D whiteMarker;

    private float margin = 30.0f;

    private Rect questionRect;
    private Rect greenRect;
    private GUIStyle tooltipStyle;
    private GUIStyle questionButtonStyle;

    private GUIStyle aquirisLogo;
    private GUIStyle unityLogo;

    public AudioClip overSound;
    public float overVolume = 0.4f;

    public AudioClip clickSound;
    public float clickVolume = 0.7f;

    private bool over;
    private bool hideOptions;
    private bool loadingIndustry;

    public GUIStyle textStyle;
    private float angle;
    public Texture2D loadingBackground;

    void Start()
    {
        angle = 0.0f;
        over = false;
        loadingIndustry = false;
        showProgress = false;
        hideOptions = Application.loadedLevelName != "demo_industry";

        questionButtonStyle = hudSkin.GetStyle("QuestionButton");
        tooltipStyle = hudSkin.GetStyle("TooltipStyle");
        aquirisLogo = hudSkin.GetStyle("AquirisLogo");
        unityLogo = hudSkin.GetStyle("UnityLogo");
        questionRect = new Rect(0, 0, 11, 11);

        alpha = 1.0f;
        goingToGame = false;
        resumeGame = false;

        state = MainMenuState.IDLE;

        descriptionStyle = hudSkin.GetStyle("Description");
        titleStyle = hudSkin.GetStyle("Titles");
        customBox = hudSkin.GetStyle("CustomBox");
        panelLeft = hudSkin.GetStyle("PanelLeft");
        panelRight = hudSkin.GetStyle("PanelRight");
        aboutStyle = hudSkin.GetStyle("About");

        menuBackgroundRect = new Rect(0, 0, menuBackground.width, menuBackground.height);
        windowBackgroundRect = new Rect(0, 0, windowBackground.width, windowBackground.height);
        panelLeftRect = new Rect(0, 0, 235, 240);
        panelRightRect = new Rect(0, 0, 235, 240);
        playGameRect = new Rect(0, 0, playGame.width * 0.75f, playGame.height * 0.75f);
        optionsRect = new Rect(0, 0, options.width * 0.75f, options.height * 0.75f);
        graphicsRect = new Rect(0, 0, graphics.width * 0.75f, graphics.height * 0.75f);
        aboutRect = new Rect(0, 0, about.width * 0.75f, about.height * 0.75f);
        resumeRect = new Rect(0, 0, resume.width * 0.75f, resume.height * 0.75f);
        damageRect = new Rect(0, 0, receiveDamageOn.width * 0.7f, receiveDamageOn.height * 0.7f);
    }

    void Update()
    {
        if (goingToGame)
        {
            alpha += Time.deltaTime;

            if (alpha >= 1.0)
            {
                if (!loadingIndustry)
                {
                    loadingIndustry = true;
                    Application.LoadLevelAsync("demo_industry");
                }
            }
        }
        else
        {
            if (alpha > 0)
            {
                alpha -= Time.deltaTime * 0.5f;
            }
        }

        if (Time.timeScale == 0.0 || GameManager.pause)
        {
            lastMouseTime -= 0.01f;
        }

        if (showProgress)
        {
            angle -= Time.deltaTime * 360;

            if (angle < -360.0f)
            {
                angle += 360.0f;
            }
        }
    }

    void DrawGUI(Event e)
    {
        screenSize = new Vector2(Screen.width, Screen.height);
        mousePos = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);

        if (visible)
        {
            if (state != MainMenuState.IDLE)
            {
                windowBackgroundRect.x = menuBackgroundRect.x + menuBackgroundRect.width;
                windowBackgroundRect.y = (screenSize.y - windowBackgroundRect.height) * 0.5f;

                GUI.DrawTexture(windowBackgroundRect, windowBackground);

                if (state == MainMenuState.GRAPHICS || state == MainMenuState.ABOUT)
                {
                    panelLeftRect.width = 475;
                    panelLeftRect.x = windowBackgroundRect.x + 15;
                    panelLeftRect.y = windowBackgroundRect.y + 55;

                    GUI.Box(panelLeftRect, "", panelLeft);
                }
            }

            if (state == MainMenuState.OPTIONS)
            {
                DrawGameOptions();
            }
            else if (state == MainMenuState.GRAPHICS)
            {
                DrawGraphicOptions();
            }
            else if (state == MainMenuState.ABOUT)
            {
                DrawAbout();
            }

            DrawMenu();
        }

        if (showProgress)
        {
            float currentProgress = IndustryLoader.industryProgress;//Application.GetStreamProgressForLevel("demo_industry");
            currentProgress *= 100.0f;
            var aux = currentProgress;
            currentProgress = aux;

            if (currentProgress < 1.0)
            {
                GUIUtility.RotateAroundPivot(angle, new Vector2(Screen.width - 28, Screen.height - 28));
                GUI.DrawTexture(new Rect(Screen.width - 56, Screen.height - 56, 56, 56), loadingBackground, ScaleMode.ScaleToFit, true, 0);

                GUI.matrix = Matrix4x4.identity;
                GUI.Label(new Rect(Screen.width - 52, Screen.height - 36, 50, 20), currentProgress.ToString(), textStyle);
            }
        }

        if (alpha > 0.0f)
        {
            var c = GUI.color;
            var d = c;
            d.a = alpha;
            GUI.color = d;

            GUI.DrawTexture(new Rect(0, 0, screenSize.x, screenSize.y), black);

            if (goingToGame)
            {
                GUI.Label(new Rect(Screen.width - 120, Screen.height - 40, 90, 20), "Loading...", textStyle);
            }
            GUI.color = c;
        }
    }

    void DrawGameOptions()
    {
        panelLeftRect.width = 235;
        panelLeftRect.x = windowBackgroundRect.x + 15;
        panelLeftRect.y = windowBackgroundRect.y + 55;

        panelRightRect.x = panelLeftRect.x + 5 + panelLeftRect.width;
        panelRightRect.y = panelLeftRect.y;

        damageRect.x = panelLeftRect.x + ((panelLeftRect.width - damageRect.width) * 0.5f);
        damageRect.y = panelLeftRect.y + ((panelLeftRect.height - damageRect.height) * 0.5f);

        var dRect = damageRect;
        dRect.x = panelRightRect.x + ((panelRightRect.width - damageRect.width) * 0.5f);

        Event e = Event.current;

        if (e.type == EventType.MouseUp && e.button == 0 && Time.time > lastMouseTime)
        {
            if (damageRect.Contains(mousePos))
            {
                if (!GameManager.receiveDamage)
                {
                    audio.volume = clickVolume;
                    audio.PlayOneShot(clickSound);
                    GameManager.receiveDamage = true;
                    lastMouseTime = Time.time;
                }
            }
            else if (dRect.Contains(mousePos))
            {
                if (GameManager.receiveDamage)
                {
                    audio.volume = clickVolume;
                    audio.PlayOneShot(clickSound);
                    GameManager.receiveDamage = false;
                    lastMouseTime = Time.time;
                }
            }
        }

        if (GameManager.receiveDamage)
        {
            GUI.DrawTexture(damageRect, receiveDamageOn);
            GUI.DrawTexture(dRect, dontReceiveDamageOff);
        }
        else
        {
            GUI.DrawTexture(damageRect, receiveDamageOff);
            GUI.DrawTexture(dRect, dontReceiveDamageOn);
        }

        GUI.Label(new Rect(windowBackgroundRect.x + 20, windowBackgroundRect.y + 15, 200, 20), "GAME OPTIONS", titleStyle);
    }


    private SceneSettings sceneConf;
    void GetSceneRef()
    {
        //var currentScene : int = Application.loadedLevel;
        int currentScene = 0;// = Application.loadedLevel;

        if (Application.loadedLevelName == "demo_start_cutscene")
        {
            currentScene = 0;
        }
        else if (Application.loadedLevelName == "demo_forest")
        {
            currentScene = 1;
        }
        else if (Application.loadedLevelName == "demo_industry")
        {
            currentScene = 2;
        }

        if (GameQualitySettings.scenes != null)
        {
            if (currentScene >= 0 && currentScene < GameQualitySettings.scenes.Length)
            {
                sceneConf = GameQualitySettings.scenes[currentScene];
            }
            else
            {
                currentScene = -1;
            }
        }
        else
        {
            currentScene = -1;
        }
    }

    private void DrawSliderOverlay(Rect rect, float val)
    {
        rect.width = Mathf.Clamp(val * 199.0f, 0.0f, 199.0f);
        GUI.DrawTexture(rect, greenBar);
    }

    private float SettingsSlider(string name, int nameLen, string tooltip, float v, float vmin, float vmax, string dispname, float dispmul, float dispadd)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Space(margin);
        GUILayout.BeginVertical();
        GUILayout.Label(name);

        questionRect.x = margin + nameLen;
        questionRect.y += 39;
        GUI.Button(questionRect, new GUIContent(string.Empty, tooltip), questionButtonStyle);

        v = GUILayout.HorizontalSlider(v, vmin, vmax, GUILayout.Width(210));
        greenRect.y += 39;
        DrawSliderOverlay(greenRect, Mathf.InverseLerp(vmin, vmax, v));

        var disp = v * dispmul + dispadd;
        GUI.Label(new Rect(greenRect.x + 220, greenRect.y - 7, 200, 20), dispname + disp.ToString("0.00"));

        GUILayout.EndVertical();
        GUILayout.EndHorizontal();

        return v;
    }

    private int SettingsIntSlider(string name, int nameLen, string tooltip, int v, int vmin, int vmax, string[] dispnames)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Space(margin);
        GUILayout.BeginVertical();
        GUILayout.Label(name);

        questionRect.x = margin + nameLen;
        questionRect.y += 39;
        GUI.Button(questionRect, new GUIContent(string.Empty, tooltip), questionButtonStyle);

        v = (int)GUILayout.HorizontalSlider(v, vmin, vmax, GUILayout.Width(210));
        greenRect.y += 39;
        DrawSliderOverlay(greenRect, Mathf.InverseLerp(vmin, vmax, v));

        GUI.Label(new Rect(greenRect.x + 220, greenRect.y - 7, 200, 20), dispnames == null ? v.ToString() : dispnames[v]);

        if (Mathf.Abs(vmin - vmax) < 8)
            DrawMarker(greenRect.y + 5, Mathf.Abs(vmin - vmax));

        GUILayout.EndVertical();
        GUILayout.EndHorizontal();

        return v;
    }

    private void SettingsSpace(int pix)
    {
        GUILayout.Space(pix);
        questionRect.y += pix;
        greenRect.y += pix;
    }

    private bool SettingsToggle(bool v, string name, int nameLen, string tooltip)
    {
        GUILayout.BeginVertical();
        GUILayout.Space(7);
        v = GUILayout.Toggle(v, v ? checkOn : checkOff, GUILayout.Width(14), GUILayout.Height(14));
        GUILayout.EndVertical();
        GUILayout.Space(5);
        GUILayout.Label(name);
        questionRect.x = margin + nameLen;
        GUI.Button(questionRect, new GUIContent(string.Empty, tooltip), questionButtonStyle);
        return v;
    }

    private void BeginToggleRow()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Space(margin);
        GUILayout.BeginVertical();
        GUILayout.BeginHorizontal(GUILayout.Width(400));
        questionRect.y += 21;
    }

    private void EndToggleRow(int pix)
    {
        GUILayout.Space(pix);
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
    }

    void DrawGraphicOptions()
    {
        GetSceneRef();

        int currentQualityLevel = (int)QualitySettings.currentLevel;
        var originalColor = GUI.color;

        if (sceneConf == null) return;

        GUI.Label(new Rect(windowBackgroundRect.x + 20, windowBackgroundRect.y + 15, 200, 20), "GRAPHICS OPTIONS", titleStyle);

        var graphicRect = new Rect(panelLeftRect.x + 7, panelLeftRect.y + 30, panelLeftRect.width - 9, panelLeftRect.height - 35);

        var cSkin = GUI.skin;
        GUI.skin = hudSkin;

        greenRect = new Rect(margin, 0, 210, 5);

        GUILayout.BeginArea(graphicRect);
        graphicsScroll = GUILayout.BeginScrollView(graphicsScroll, GUILayout.Width(graphicRect.width));

        var boxRect = new Rect(17, 0, 430, 0);
        // overall level
        boxRect.height = 18 + 39;
        GUI.Box(boxRect, "", customBox);
        // post-fx
        boxRect.y += 10 + boxRect.height;
        boxRect.height = 93;
        GUI.Box(boxRect, "", customBox);
        // distances
        boxRect.y += 10 + boxRect.height;
        boxRect.height = 18 + 39;
        GUI.Box(boxRect, "", customBox);
        // shadow distance
        boxRect.y += 10 + boxRect.height;
        boxRect.height = 18 + 39;
        GUI.Box(boxRect, "", customBox);
        // texture limit
        boxRect.y += 10 + boxRect.height;
        boxRect.height = 18 + 39;
        GUI.Box(boxRect, "", customBox);
        // terrain
        boxRect.y += 10 + boxRect.height;
        boxRect.height = 18 + 39 * 7;
        GUI.Box(boxRect, "", customBox);

        GUILayout.BeginVertical();
        questionRect.y = -31;
        greenRect.y = -9;

        GameQualitySettings.overallQuality = SettingsIntSlider(
            "Overall Quality Level", 125, "Overall quality level of the game.",
            GameQualitySettings.overallQuality, 0, 5, new string[]

            { "QUALITY: FASTEST", "QUALITY: FAST", "QUALITY: SIMPLE", "QUALITY: GOOD", "QUALITY: BEAUTIFUL", "QUALITY: FANTASTIC"});

        GUILayout.Space(29);
        questionRect.y += 47;

        BeginToggleRow();
        GameQualitySettings.anisotropicFiltering = SettingsToggle(GameQualitySettings.anisotropicFiltering, "Anisotropic Filtering", 153, "Anisotropic filtering for the textures.");
        GUILayout.FlexibleSpace();
        GameQualitySettings.ambientParticles = SettingsToggle(GameQualitySettings.ambientParticles, "Ambient Particles", 355, "Smoke & dust particles.");
        EndToggleRow(50);

        BeginToggleRow();
        GameQualitySettings.colorCorrection = SettingsToggle(GameQualitySettings.colorCorrection, "Color Correction", 128, "Color correction image effect.");
        GUILayout.FlexibleSpace();
        GameQualitySettings.bloomAndFlares = SettingsToggle(GameQualitySettings.bloomAndFlares, "Bloom & Flares", 336, "Bloom & Lens Flares image effect.");
        EndToggleRow(71);

        BeginToggleRow();
        GameQualitySettings.sunShafts = SettingsToggle(GameQualitySettings.sunShafts, "Sun Shafts", 100, "Sun Shafts image effect.");
        GUILayout.FlexibleSpace();
        GameQualitySettings.depthOfFieldAvailable = SettingsToggle(GameQualitySettings.depthOfFieldAvailable, "Depth Of Field", 336, "Depth Of Field image effect while aiming.");
        EndToggleRow(73);

        BeginToggleRow();
        var ssaoEnable = SettingsToggle(GameQualitySettings.ssao, "SSAO", 60, "Screen Space Ambient Ccclusion image effect.");
        if (GameQualitySettings.overallQuality > 3)
            GameQualitySettings.ssao = ssaoEnable;
        GUILayout.FlexibleSpace();
        EndToggleRow(0);

        greenRect.y += 113;
        questionRect.y -= 18;

        SettingsSpace(25);

        GameQualitySettings.dynamicObjectsFarClip = SettingsSlider(
            "Dynamic Objects Far Distance", 180, "Drawing distance of dynamic objects.",
            GameQualitySettings.dynamicObjectsFarClip, 0.0f, 1.0f, "DYNAMIC: ",
            GameQualitySettings._dynamicLayersRange.y - GameQualitySettings._dynamicLayersRange.x, GameQualitySettings._dynamicLayersRange.x);

        SettingsSpace(27);

        GameQualitySettings.shadowDistance = SettingsSlider(
            "Shadow Distance", 108, "Realtime shadows drawing distance.",
            GameQualitySettings.shadowDistance, 0.0f, 30.0f, "",
            1.0f, 0.0f);

        SettingsSpace(28);

        GameQualitySettings.masterTextureLimit = SettingsIntSlider(
            "Texture Quality", 100, "Overall texture detail.",
            GameQualitySettings.masterTextureLimit, 3, 0, new string[]

            { "FULL RESOLUTION", "HALF RESOLUTION", "QUARTER RESOLUTION", "1/8 RESOLUTION"});

        SettingsSpace(27);

        sceneConf.detailObjectDensity = SettingsSlider(
            "Terrain Grass Density", 136, "Grass density.",
            sceneConf.detailObjectDensity, 0.0f, 1.0f, "",
            1.0f, 0.0f);

        sceneConf.detailObjectDistance = SettingsSlider(
            "Terrain Grass Distance", 141, "Grass drawing distance.",
            sceneConf.detailObjectDistance, 0.0f, 50.0f, "",
            1.0f, 0.0f);

        sceneConf.nearTerrainPixelError = SettingsSlider(
            "Terrain Pixel Error", 146, "Set terrain pixel error.",
            sceneConf.nearTerrainPixelError, 50.0f, 5.0f, "",
            1.0f, 0.0f);

        sceneConf.treeDistance = SettingsSlider(
            "Terrain Tree Distance", 137, "Tree drawing distance.",
            sceneConf.treeDistance, 200.0f, 400.0f, "",
            1.0f, 0.0f);

        sceneConf.heightmapMaximumLOD = SettingsIntSlider(
            "Terrain Level of Detail", 139, "Overall terrain Level of Detail.",
            sceneConf.heightmapMaximumLOD, 2, 0, new string[]


            { "FULL RESOLUTION", "QUARTER RESOLUTION", "1/16 RESOLUTION"});

        sceneConf.terrainTreesBillboardStart = SettingsSlider(
            "Terrain Billboard Start", 140, "Distance from the camera where trees will be rendered as billboards.",
            sceneConf.terrainTreesBillboardStart, 10.0f, 70.0f, "",
            1.0f, 0.0f);

        sceneConf.maxMeshTrees = SettingsIntSlider(
            "Max Mesh Trees", 100, "Set the maximum number of trees rendered at full LOD.",
            sceneConf.maxMeshTrees, 5, 60,
            null);

        GUILayout.Space(20);

        GUILayout.EndVertical();

        GUILayout.EndScrollView();
        GUILayout.EndArea();

        if (GUI.tooltip != "")
        {
            GUI.Label(new Rect(mousePos.x + 15, mousePos.y - 60, 150, 70), GUI.tooltip, tooltipStyle);
        }

        GUI.skin = cSkin;
    }

    void DrawMarker(float y, int steps)
    {
        var markerRect = new Rect(margin, y + 2, 1, 5);
        float aux;
        var s = steps;

        for (int i = 0; i <= steps; i++)
        {
            aux = i;
            aux /= s;
            markerRect.x = margin + 5 + aux * 196;

            GUI.DrawTexture(markerRect, whiteMarker);
        }
    }

    void DrawAbout()
    {
        GUI.Label(new Rect(windowBackgroundRect.x + 20, windowBackgroundRect.y + 15, 200, 20), "ABOUT", titleStyle);

        var abRect = new Rect(panelLeftRect.x + 7, panelLeftRect.y + 30, panelLeftRect.width - 12, panelLeftRect.height - 40);

        var cSkin = GUI.skin;
        GUI.skin = hudSkin;

        GUILayout.BeginArea(abRect);
        aboutScroll = GUILayout.BeginScrollView(aboutScroll, GUILayout.Width(abRect.width));

        GUILayout.BeginHorizontal();
        GUILayout.Space(17);
        GUILayout.BeginVertical();
        GUILayout.Label("Developed by Aquiris Game Experience and Unity Technologies ApS.", aboutStyle, GUILayout.Width(423));
        GUILayout.Space(5);
        string names;
        names = "Alessandro Peixoto Lima, ";
        names += "Amilton Diesel, ";
        names += "Andre Schaan, ";
        names += "Aras Pranckevicius, ";
        names += "Bret Church, ";
        names += "Ethan Vosburgh, ";
        names += "Gustavo Allebrandt, ";
        names += "Israel Mendes, ";
        names += "Henrique Geremia Nievinski, ";
        names += "Jakub Cupisz, ";
        names += "Joe Robins, ";
        names += "Marcelo Ferranti, ";
        names += "Mauricio Longoni, ";
        names += "Ole Ciliox, ";
        names += "Rafael Rodrigues, ";
        names += "Raphael Lopes Baldi, ";
        names += "Robert Cupisz, ";
        names += "Rodrigo Peixoto Lima, ";
        names += "Rune Skovbo Johansen, ";
        names += "Wagner Monticelli.";
        GUILayout.Label(names, GUILayout.Width(400));
        GUILayout.Space(20);
        GUILayout.Label("Special thanks to:", aboutStyle, GUILayout.Width(423));
        GUILayout.Space(5);
        names = "Cristiano Bartel, ";
        names += "Daniel Merkel, ";
        names += "Felipe Lahti, ";
        names += "Kely Cunha, ";
        names += "Otto Lopes, ";
        names += "Rory Jennings.";
        GUILayout.Label(names, GUILayout.Width(400));

        GUILayout.Space(70);
        GUI.DrawTexture(new Rect(170, 180, 339 * 0.75f, 94 * 0.75f), aquirisLogo.normal.background);

        GUILayout.EndVertical();
        GUILayout.EndHorizontal();

        GUILayout.EndScrollView();

        GUILayout.EndArea();

        GUI.skin = cSkin;
    }

    void DrawMenu()
    {
        Event evt = Event.current;

        menuBackgroundRect.x = 0;
        menuBackgroundRect.y = (screenSize.y - menuBackgroundRect.height) * 0.5f - 50;

        playGameRect.x = menuBackgroundRect.x + 93;

        if (hideOptions)
        {
            playGameRect.y = menuBackgroundRect.y + 80;
        }
        else
        {
            playGameRect.y = menuBackgroundRect.y + 62;
        }

        resumeRect.x = playGameRect.x;
        resumeRect.y = playGameRect.y;

        optionsRect.x = playGameRect.x;
        optionsRect.y = playGameRect.y + playGameRect.height + 15;

        graphicsRect.x = playGameRect.x;

        if (hideOptions)
        {
            graphicsRect.y = playGameRect.y + playGameRect.height + 15;
        }
        else
        {
            graphicsRect.y = optionsRect.y + optionsRect.height + 15;
        }

        aboutRect.x = playGameRect.x;
        aboutRect.y = graphicsRect.y + graphicsRect.height + 15;

        GUI.DrawTexture(menuBackgroundRect, menuBackground);

        var overButton = false;

        //		if(Application.loadedLevelName == "main_menu")
        //		{
        //			if(playGameRect.Contains(mousePos))
        //			{
        //				overButton = true;
        //						
        //				if(!over)
        //				{
        //					over = true;
        //					audio.volume = overVolume;
        //					audio.PlayOneShot(overSound);
        //				}
        //				
        //				GUI.DrawTexture(playGameRect, playGameOver);
        //				
        //				if(alpha <= 0.0 && !goingToGame)
        //				{
        //					if(evt.type == EventType.MouseUp && evt.button == 0 && Time.time > lastMouseTime)
        //					{
        //						audio.volume = clickVolume;
        //						audio.PlayOneShot(clickSound);
        //						
        //						goingToGame = true;
        //						
        //						lastMouseTime = Time.time;
        //					}
        //				}
        //			}
        //			else
        //			{
        //				GUI.DrawTexture(playGameRect, playGame);
        //			}
        //		}
        //		else
        //		{
        if (resumeRect.Contains(mousePos))
        {
            overButton = true;

            if (!over)
            {
                over = true;
                audio.volume = overVolume;
                audio.PlayOneShot(overSound);
            }

            GUI.DrawTexture(resumeRect, resumeOver);

            if (alpha <= 0.0 && GameManager.pause)
            {
                if (evt.type == EventType.MouseUp && evt.button == 0 && Time.time > lastMouseTime)
                {
                    audio.volume = clickVolume;
                    audio.PlayOneShot(clickSound);

                    GameManager.pause = false;
                    Time.timeScale = 1.0f;
                    //Time.timeScale = 1.0;
                    visible = false;
                    lastMouseTime = Time.time;
                }
            }
        }
        else
        {
            GUI.DrawTexture(resumeRect, resume);
        }
        //}

        if (!hideOptions)
        {
            if (optionsRect.Contains(mousePos))
            {
                overButton = true;

                if (!over)
                {
                    over = true;
                    audio.volume = overVolume;
                    audio.PlayOneShot(overSound);
                }

                GUI.DrawTexture(optionsRect, optionsOver);

                if (alpha <= 0.0 && !goingToGame)
                {
                    if (evt.type == EventType.MouseUp && evt.button == 0 && Time.time > lastMouseTime)
                    {
                        audio.volume = clickVolume;
                        audio.PlayOneShot(clickSound);

                        if (state != MainMenuState.OPTIONS)
                        {
                            state = MainMenuState.OPTIONS;
                        }
                        else
                        {
                            state = MainMenuState.IDLE;
                        }

                        lastMouseTime = Time.time;
                    }
                }
            }
            else
            {
                GUI.DrawTexture(optionsRect, options);
            }
        }

        if (graphicsRect.Contains(mousePos))
        {
            overButton = true;

            if (!over)
            {
                over = true;
                audio.volume = overVolume;
                audio.PlayOneShot(overSound);
            }

            GUI.DrawTexture(graphicsRect, graphicsOver);

            if (alpha <= 0.0 && !goingToGame)
            {
                if (evt.type == EventType.MouseUp && evt.button == 0 && Time.time > lastMouseTime)
                {
                    audio.volume = clickVolume;
                    audio.PlayOneShot(clickSound);

                    if (state != MainMenuState.GRAPHICS)
                    {
                        state = MainMenuState.GRAPHICS;
                    }
                    else
                    {
                        state = MainMenuState.IDLE;
                    }

                    lastMouseTime = Time.time;
                }
            }
        }
        else
        {
            GUI.DrawTexture(graphicsRect, graphics);
        }

        if (aboutRect.Contains(mousePos))
        {
            overButton = true;

            if (!over)
            {
                over = true;
                audio.volume = overVolume;
                audio.PlayOneShot(overSound);
            }

            GUI.DrawTexture(aboutRect, aboutOver);

            if (alpha <= 0.0 && !goingToGame)
            {
                if (evt.type == EventType.MouseUp && evt.button == 0 && Time.time > lastMouseTime)
                {
                    audio.volume = clickVolume;
                    audio.PlayOneShot(clickSound);

                    if (state != MainMenuState.ABOUT)
                    {
                        state = MainMenuState.ABOUT;
                    }
                    else
                    {
                        state = MainMenuState.IDLE;
                    }

                    lastMouseTime = Time.time;
                }
            }
        }
        else
        {
            GUI.DrawTexture(aboutRect, about);
        }

        if (!overButton)
        {
            over = false;
        }
    }
}
