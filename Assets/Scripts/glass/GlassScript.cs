using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// Type of glass.
/// </summary>
public enum GlassType
{ 
    SINGLE,
    DIVIDED,
}

/// <summary>
/// 
/// </summary>
public class GlassScript : MonoBehaviour
{
    #region Public Data

    //Glass type.
    public GlassType glassType;

    //Number of point in matrix.
    public int tileHorizontal;
    public int tileVertical;

    //Prefab of particle object emmited when break the glass.
    public GameObject particlePrefab;

    //Breaked texture (when hit the first time).
    public Texture2D textureBreaked;

    //Texture with base marks.
    public Texture2D[] textureBaseMarks;

    //Max number bullets to break all glass.
    public int maxNumberToBreak;
       
    #endregion
    
    
    #region Internal Data

    //Used to change mainTexture when happen the first hit.
    private bool firstHit;

    //Size of alpha texture.
    private int sizeTextureX;
    private int sizeTextureY;

    //The base size of a simple "broked" square of glass alpha texture.
    private int baseQuadSizeX;
    private int baseQuadSizeY;

    //Need a simple texture "isReadable", to set later with SetPixels.
    private Texture2D textureCurrentAlpha;

    //Record all squares of matrix, true if is broken, false if isnt broken yet.
    private bool[,] matrixOfGlass;
    private int[,] selectedMarkNumber;

    //Aux transform to place in position of bullet when it collide with glass.
    private GameObject objectColliding;

    //Structs of data glass marks.
    private GlassStruct[] glassStruct;

    //Instance of textureBaseMarks.
    private Texture2D instanceTextBaseMarks;

    //Particle emmiter on center of each point of glass.
    private ParticleEmitter[,] matrixEmmiters; 

    //Number of breaked points.
    private int numberToBreak;

    //TODO DELETE
    private Cubemap cubeMap;
	
	private AudioClip destructAudio;
	private AudioClip hitAudio;
	private bool playDestruction;
    #endregion
	
	private float auxHorizontal;
	private float auxVertical;
	private Transform parentEmmiters;
	
    /// <summary>
    /// 
    /// </summary>
    void Start()
    {
		playDestruction = true;
        //Warning.
        if (tileHorizontal % 2 != 0 || tileVertical % 2 != 0)
        {
            Debug.LogWarning("Use power of 2 tile horizontal and vertical!");
        }
		
		destructAudio = GameSoundManager.GetClip("glass_destruction");
		hitAudio = GameSoundManager.GetClip("glass_hit");
		
		if(audio == null)
		{
			gameObject.AddComponent<AudioSource>();
		}
		audio.loop = false;
		audio.playOnAwake = false;
		
        //Intialize with false the first hit (used to change texture to breaked).
        firstHit = false;

        //Set base alpha texture size.
        sizeTextureX = (int)((textureBaseMarks[0].width / 4.0f) * tileHorizontal);
        sizeTextureY = (int)((textureBaseMarks[0].height / 4.0f) * tileVertical);

        textureCurrentAlpha = new Texture2D(2, 2,  TextureFormat.ARGB32, true);//(Texture2D)renderer.material.GetTexture("_AlphaTex");
		renderer.material.SetTexture("_AlphaTex", textureCurrentAlpha);
        
        //Resize the base alpha color of glass.
        textureCurrentAlpha.Resize(sizeTextureX, sizeTextureY);

        //Set all glass with white color and apply.
        Color[] auxTexColors = new Color[(int)(textureCurrentAlpha.width * textureCurrentAlpha.height)];
        for (int i = 0; i < auxTexColors.Length; i++)
        {
            auxTexColors[i] = Color.white;
        }
        textureCurrentAlpha.SetPixels(0, 0, textureCurrentAlpha.width, textureCurrentAlpha.height, auxTexColors);
        textureCurrentAlpha.Apply();

        //Create the glass matrix.
        matrixOfGlass = new bool[tileHorizontal + 2, tileVertical + 2];
        selectedMarkNumber = new int[tileHorizontal + 2, tileVertical + 2];

        for (int i = 0; i < matrixOfGlass.GetLength(0); i++)
        {
            for (int j = 0; j < matrixOfGlass.GetLength(1); j++)
            {
                matrixOfGlass[i, j] = false;
                selectedMarkNumber[i, j] = -1;
            }
        }
        selectedMarkNumber = new int[tileHorizontal + 2, tileVertical + 2];


        //Create object that simulate colliding bullet.
        objectColliding = new GameObject("object_colliding");
        objectColliding.transform.parent = transform;
        objectColliding.transform.localPosition = Vector3.zero;


        //Calculate the size of one recorte base alpha texture.
        baseQuadSizeX = (int)(textureBaseMarks[0].width * 0.25f);
        baseQuadSizeY = (int)(textureBaseMarks[0].height * 0.25f);


        //Start all marks squares.
        glassStruct = new GlassStruct[16];
        for (int i = 0; i < 4; i++)
        {
            for(int j = 0; j < 4; j++)
            {
                glassStruct[(i * 4) + j] = new GlassStruct((i * 4) + j, 
                    new GlassMark(textureBaseMarks[0].GetPixels(i * baseQuadSizeX, j * baseQuadSizeY, baseQuadSizeX, baseQuadSizeY)),
                    new GlassMark(textureBaseMarks[1].GetPixels
                        (i * baseQuadSizeX, j * baseQuadSizeY, baseQuadSizeX, baseQuadSizeY)));
            }
        }

        //Create a basic black square to fill intersections of four breaked points.
        GlassStruct.blackFillTexture = new Color[baseQuadSizeX * baseQuadSizeY];
        for (int i = 0; i < baseQuadSizeX; i++)
        {
            for (int j = 0; j < baseQuadSizeY; j++)
            {
                GlassStruct.blackFillTexture[(i * baseQuadSizeX) + j] = Color.black;
            }
        }


        //Set base marks with sides (left side, top side, right side, bottom side) -> True if breaked, False if not breaked.
        glassStruct[0].SetSides(true, false, true, true);
        glassStruct[1].SetSides(false, true, true, false);
        glassStruct[2].SetSides(false, true, false, false);
        glassStruct[3].SetSides(false, false, false, false);
        glassStruct[4].SetSides(true, true, false, true);
        glassStruct[5].SetSides(false, false, true, true);
        glassStruct[6].SetSides(true, false, true, false);
        glassStruct[7].SetSides(true, false, false, false);
        glassStruct[8].SetSides(true, true, true, false);
        glassStruct[9].SetSides(true, false, false, true);
        glassStruct[10].SetSides(false, true, false, true);
        glassStruct[11].SetSides(false, false, true, false);
        glassStruct[12].SetSides(true, true, true, true);
        glassStruct[13].SetSides(false, true, true, true);
        glassStruct[14].SetSides(true, true, false, false);
        glassStruct[15].SetSides(false, false, false, true);


        //Create particle system.
        parentEmmiters = new GameObject("particle_emmiters").transform;
        parentEmmiters.parent = transform;
        parentEmmiters.localPosition = Vector3.zero;
        parentEmmiters.rotation = transform.rotation;
        parentEmmiters.localScale = Vector3.one;

        matrixEmmiters = new ParticleEmitter[tileHorizontal, tileVertical];

        auxHorizontal = 10f / (float)tileHorizontal;
        auxVertical = 10f / (float)tileVertical;

		/*
        for (int i = 0; i < tileHorizontal; i++)
        {
            for (int j = 0; j < tileVertical; j++)
            {
                //Crate emmiters and set position.
                matrixEmmiters[i, j] = ((GameObject)GameObject.Instantiate(particlePrefab, Vector3.zero, Quaternion.identity)).GetComponent<ParticleEmitter>();
				
                matrixEmmiters[i, j].name = string.Format("emmiter{0}{1}", i, j);
                matrixEmmiters[i, j].transform.parent = parentEmmiters;

                matrixEmmiters[i, j].transform.forward = parentEmmiters.forward;
                matrixEmmiters[i, j].transform.localPosition = new Vector3((auxHorizontal * (tileHorizontal - i - 1)) + (auxHorizontal * 0.5f) - 5f, 0.0f, (auxVertical * (tileVertical - j - 1)) + (auxVertical * 0.5f) - 5f);
				
				matrixEmmiters[i, j].gameObject.active = false;
            }
        }
        //*/

        //Initialize number of break points with 0.
        numberToBreak = 0;
    }


    /// <summary>
    /// 
    /// </summary>
    public void Hit(RaycastHit p_hit)
    {
        Vector3 p_point = p_hit.point;
		
        //If is the first hit, change the mainTexture to breaked.
        if (firstHit == false && glassType == GlassType.SINGLE)
        {
            renderer.material.SetTexture("_MainTex", textureBreaked);
            firstHit = true;
        }

        //Translate aux transform to position of hit with glass collision.
        objectColliding.transform.position = p_point;
        objectColliding.transform.localPosition = new Vector3(objectColliding.transform.localPosition.x, objectColliding.transform.localPosition.y, objectColliding.transform.localPosition.z);

        //Calculate relative value of hit position (0 - 1).
        Vector2 hitPosition = new Vector2(Mathf.Abs(objectColliding.transform.localPosition.x - 5) * 0.1f, Mathf.Abs(objectColliding.transform.localPosition.z - 5) * 0.1f);

        //Select position on alpha texture to SetPixels.
        int posX = (int)(hitPosition.x * tileHorizontal) * (textureCurrentAlpha.width / tileHorizontal);
        int posY = (int)(hitPosition.y * tileVertical) * (textureCurrentAlpha.height / tileVertical); 

        //Position X and Y in matrix (Ex: 0, 1, 2..)
        int matrixX = Mathf.Min((posX / baseQuadSizeX) + 1, tileHorizontal);
        int matrixY = Mathf.Min((posY / baseQuadSizeY) + 1, tileVertical);

        bool oldBreak = matrixOfGlass[matrixX, matrixY];

        //Break a point.
        BreakAPoint(matrixX, matrixY);

        if (oldBreak  == false && glassType == GlassType.SINGLE)
        {
            //Test if will break all glass.
            TestBreakAll();

            //Random Breaks.
            RandomBreak(matrixX, matrixY);

            //Test if gravity will break the glass.
            TestGravityBreak();
        }

        //Apply all current changes in the alpha texture.
        textureCurrentAlpha.Apply();
    }


    /// <summary>
    /// Break a point on glass (fill the alpha texture with mark of breaked).
    /// </summary>
    void BreakAPoint(int p_matrixX, int p_matrixY)
    {
        p_matrixX = Mathf.Min(p_matrixX, tileHorizontal);
        p_matrixY = Mathf.Min(p_matrixY, tileVertical);

        //If this hitted point is breaked, return.
        if (matrixOfGlass[p_matrixX, p_matrixY] == true)
            return;
		
		if(hitAudio != null)
		{
			audio.PlayOneShot(hitAudio);
		}        
		
		//Set in matrix true in element that received hit.
        matrixOfGlass[p_matrixX, p_matrixY] = true;
        selectedMarkNumber[p_matrixX, p_matrixY] = UnityEngine.Random.Range(0, textureBaseMarks.Length);

        //Emit particles.
		if(p_matrixX - 1 >= 0 && p_matrixY - 1 >= 0 && p_matrixX - 1 < tileHorizontal && p_matrixY - 1 < tileVertical)
		{
			if(matrixEmmiters[p_matrixX - 1, p_matrixY - 1] == null)
			{
				matrixEmmiters[p_matrixX - 1, p_matrixY - 1] = ((GameObject)GameObject.Instantiate(particlePrefab, Vector3.zero, Quaternion.identity)).GetComponent<ParticleEmitter>();
				
                matrixEmmiters[p_matrixX - 1, p_matrixY - 1].name = string.Format("emmiter{0}{1}", p_matrixX - 1, p_matrixY - 1);
                matrixEmmiters[p_matrixX - 1, p_matrixY - 1].transform.parent = parentEmmiters;

                matrixEmmiters[p_matrixX - 1, p_matrixY - 1].transform.forward = parentEmmiters.forward;
                matrixEmmiters[p_matrixX - 1, p_matrixY - 1].transform.localPosition = new Vector3((auxHorizontal * (tileHorizontal - p_matrixX)) + (auxHorizontal * 0.5f) - 5f, 0.0f, (auxVertical * (tileVertical - p_matrixY)) + (auxVertical * 0.5f) - 5f);//new Vector3((auxHorizontal * (tileHorizontal - p_matrixX - 2)) + (auxHorizontal * 0.5f) - 5f, 0.0f, (auxVertical * (tileVertical - p_matrixY - 2)) + (auxVertical * 0.5f) - 5f);
			}
			
            matrixEmmiters[p_matrixX - 1, p_matrixY - 1].Emit();
		}

        //Set hitted mark.
        int number = SelectMarkNumber(p_matrixX, p_matrixY);
        if (number != -1) textureCurrentAlpha.SetPixels((p_matrixX - 1) * baseQuadSizeX, (p_matrixY - 1) * baseQuadSizeY, baseQuadSizeX, baseQuadSizeY, glassStruct[number].GetColorMark(selectedMarkNumber[p_matrixX, p_matrixY]));

        //If type is SINGLE, test sides.
        if (glassType == GlassType.SINGLE)
        {
            //Test side points.
            TestSidePoints(p_matrixX, p_matrixY);

            //Test intersection points.
            TestIntersectionPoints(p_matrixX, p_matrixY);
        }
    }

    /// <summary>
    /// If left, top, right, bottom marks is true, change marks too.
    /// </summary>
    private void TestSidePoints(int p_matrixX, int p_matrixY)
    {
        p_matrixX = Mathf.Min(p_matrixX, tileHorizontal);
        p_matrixY = Mathf.Min(p_matrixY, tileVertical);

        int p_posX = (p_matrixX - 1) * baseQuadSizeX;
        int p_posY = (p_matrixY - 1) * baseQuadSizeY;

        //Record old color to multiply by new changed side mark (because no override intersections).
        Color[] oldColor = new Color[baseQuadSizeX * baseQuadSizeY];
        Color[] newColor = new Color[baseQuadSizeX * baseQuadSizeY];

        //Left point.
        if (matrixOfGlass[p_matrixX - 1, p_matrixY] == true)
        {
            int numberLeft = SelectMarkNumber(p_matrixX - 1, p_matrixY);

            oldColor = textureCurrentAlpha.GetPixels(p_posX - baseQuadSizeX, p_posY, baseQuadSizeX, baseQuadSizeY);
            MultiplyColorArrays(glassStruct[numberLeft].GetColorMark(selectedMarkNumber[p_matrixX - 1, p_matrixY]), oldColor, ref newColor);
            
            if (numberLeft != -1) textureCurrentAlpha.SetPixels(p_posX - baseQuadSizeX, p_posY, baseQuadSizeX, baseQuadSizeY, newColor);
        }
        //Top point.
        if (matrixOfGlass[p_matrixX, p_matrixY + 1] == true)
        {
            int numberTop = SelectMarkNumber(p_matrixX, p_matrixY + 1);

            oldColor = textureCurrentAlpha.GetPixels(p_posX, p_posY + baseQuadSizeY, baseQuadSizeX, baseQuadSizeY);
            MultiplyColorArrays(glassStruct[numberTop].GetColorMark(selectedMarkNumber[p_matrixX, p_matrixY + 1]), oldColor, ref newColor);
            
            if (numberTop != -1) textureCurrentAlpha.SetPixels(p_posX, p_posY + baseQuadSizeY, baseQuadSizeX, baseQuadSizeY, newColor);
        }
        //Right point.
        if (matrixOfGlass[p_matrixX + 1, p_matrixY] == true)
        {
            int numberRight = SelectMarkNumber(p_matrixX + 1, p_matrixY);
            
            oldColor = textureCurrentAlpha.GetPixels(p_posX + baseQuadSizeX, p_posY, baseQuadSizeX, baseQuadSizeY);
            MultiplyColorArrays(glassStruct[numberRight].GetColorMark(selectedMarkNumber[p_matrixX + 1, p_matrixY]), oldColor, ref newColor);
            
            if (numberRight != -1) textureCurrentAlpha.SetPixels(p_posX + baseQuadSizeX, p_posY, baseQuadSizeX, baseQuadSizeY, newColor);
        }
        //Bottom point.
        if (matrixOfGlass[p_matrixX, p_matrixY - 1] == true)
        {
            int numberBottom = SelectMarkNumber(p_matrixX, p_matrixY - 1);
            
            oldColor = textureCurrentAlpha.GetPixels(p_posX, p_posY - baseQuadSizeY, baseQuadSizeX, baseQuadSizeY);
            MultiplyColorArrays(glassStruct[numberBottom].GetColorMark(selectedMarkNumber[p_matrixX, p_matrixY - 1]), oldColor, ref newColor);

            if (numberBottom != -1) textureCurrentAlpha.SetPixels(p_posX, p_posY - baseQuadSizeY, baseQuadSizeX, baseQuadSizeY, newColor);
        }
    }


    /// <summary>
    /// Use to multiply each by each Color of array of Colors.
    /// </summary>
    private void MultiplyColorArrays(Color[] p_color1, Color[] p_color2, ref Color[] p_resultColor)
    {
        for (int i = 0; i < p_resultColor.Length; i++)
        {
            p_resultColor[i] = p_color1[i] * p_color2[i];
        }
    }
    

    /// <summary>
    /// If three intesection points is true, mark with black box.
    /// </summary>
    private void TestIntersectionPoints(int p_matrixX, int p_matrixY)
    {
        p_matrixX = Mathf.Min(p_matrixX, tileHorizontal);
        p_matrixY = Mathf.Min(p_matrixY, tileVertical);

        int p_posX = (p_matrixX - 1) * baseQuadSizeX;
        int p_posY = (p_matrixY - 1) * baseQuadSizeY;

        //Left / top three points.
        if (matrixOfGlass[p_matrixX - 1, p_matrixY] == true && matrixOfGlass[p_matrixX - 1, p_matrixY + 1] == true && matrixOfGlass[p_matrixX, p_matrixY + 1] == true)
        {
            textureCurrentAlpha.SetPixels(p_posX - (int)(baseQuadSizeX * 0.5f), p_posY + (int)(baseQuadSizeY * 0.5f), baseQuadSizeX, baseQuadSizeY, GlassStruct.blackFillTexture);
        }
        //Right / top three points.
        if (matrixOfGlass[p_matrixX, p_matrixY + 1] == true && matrixOfGlass[p_matrixX + 1, p_matrixY + 1] == true && matrixOfGlass[p_matrixX + 1, p_matrixY] == true)
        {
            textureCurrentAlpha.SetPixels(p_posX + (int)(baseQuadSizeX * 0.5f), p_posY + (int)(baseQuadSizeY * 0.5f), baseQuadSizeX, baseQuadSizeY, GlassStruct.blackFillTexture);
        }
        //Right / bottom three points.
        if (matrixOfGlass[p_matrixX + 1, p_matrixY] == true && matrixOfGlass[p_matrixX + 1, p_matrixY - 1] == true && matrixOfGlass[p_matrixX, p_matrixY - 1] == true)
        {
            textureCurrentAlpha.SetPixels(p_posX + (int)(baseQuadSizeX * 0.5f), p_posY - (int)(baseQuadSizeY * 0.5f), baseQuadSizeX, baseQuadSizeY, GlassStruct.blackFillTexture);
        }
        //Left / bottom three points.
        if (matrixOfGlass[p_matrixX, p_matrixY - 1] == true && matrixOfGlass[p_matrixX - 1, p_matrixY - 1] == true && matrixOfGlass[p_matrixX - 1, p_matrixY] == true)
        {
            textureCurrentAlpha.SetPixels(p_posX - (int)(baseQuadSizeX * 0.5f), p_posY - (int)(baseQuadSizeY * 0.5f), baseQuadSizeX, baseQuadSizeY, GlassStruct.blackFillTexture);
        }
    }
    

    /// <summary>
    /// Test if sides is broken and return the number of correct mark.
    /// </summary>
    private int SelectMarkNumber(int p_matrixX, int p_matrixY)
    {
        p_matrixX = Mathf.Min(p_matrixX, tileHorizontal);
        p_matrixY = Mathf.Min(p_matrixY, tileVertical);

		//Test if SINGLE, calculate side points and choose the correct mark.
        if (glassType == GlassType.SINGLE)
        {
            //Get if sides is breaked or not.
            bool left = matrixOfGlass[p_matrixX - 1, p_matrixY];
            bool top = matrixOfGlass[p_matrixX, p_matrixY + 1];
            bool right = matrixOfGlass[p_matrixX + 1, p_matrixY];
            bool bottom = matrixOfGlass[p_matrixX, p_matrixY - 1];

            int number = -1;

            //Test all marks to get the correct mark depending of four sides marks (left, top, right, bottom), and return the correct Color[] mark.
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    number = glassStruct[(i * 4) + j].IsThisMark(left, top, right, bottom);

                    if (number != -1)
                    {
                        return number;
                    }
                }
            }

            //If dont returned yet, have error.
            return 3;
        }
        //If DIVIDED, return 3 (single mark).
        else
        {
            return 3;
        }
    }


    /// <summary>
    /// Random break 2 and 3 points above.
    /// </summary>
    void RandomBreak(int p_matrixX, int p_matrixY)
    {
        //Random try break above 2 or 3 points.
        if (p_matrixY < 4)
        {
            int numberRandom = UnityEngine.Random.Range(0, 4);

            //Break more one point above (25% chance).
            if (numberRandom == 0)
            {
                BreakAPoint(p_matrixX, p_matrixY + 1);
            }
            //Break more two points above (25% chance).
            else if (numberRandom == 1)
            {
                BreakAPoint(p_matrixX, p_matrixY + 1);

                //Test if will break above of glass matrix.
                if (p_matrixY < tileVertical - 1)
                {
                    BreakAPoint(p_matrixX, p_matrixY + 2);
                }
            }
        }

        //If is base line, try to break sides.
        if (p_matrixY == 1)
        {
            //33% chance break side point.
            if (UnityEngine.Random.Range(0, 3) == 0)
            { 
                //50% chance break left side.
                if (UnityEngine.Random.Range(0, 2) == 0)
                {
                    //If not matrix aux point 0, 0.
                    if (p_matrixX - 1 > 0)
                    {
                        BreakAPoint(p_matrixX - 1, p_matrixY);
                    }
                    else
                    {
                        BreakAPoint(p_matrixX + 1, p_matrixY);
                    }
                }
                else
                {
                    //If not matrix aux point tileHorizontal + 1, 0.
                    if (p_matrixX < tileHorizontal)
                    {
                        BreakAPoint(p_matrixX + 1, p_matrixY);
                    }
                    else
                    {
                        BreakAPoint(p_matrixX - 1, p_matrixY);
                    }
                }
            }
        }
    }
	
    /// <summary>
    /// 
    /// </summary>
	public void BreakAll()
	{
		if(!firstHit)
		{
			renderer.material.SetTexture("_MainTex", textureBreaked);
			firstHit = true;
		}
		
		if(destructAudio != null)
		{
			if(playDestruction)
			{
				playDestruction = false;
				
				if(GlassManager.timeToPlayAudio <= 0.0f)
				{
					GlassManager.timeToPlayAudio = destructAudio.length;
					audio.PlayOneShot(destructAudio);
				}
			}
		}
		
		//Set all point to true.
        for (int i = 1; i < tileHorizontal + 1; i++)
        {
            for (int j = 1; j < tileVertical + 1; j++)
            {
                //Emmit particles.
                if (matrixOfGlass[i, j] == false)
                {
					if(matrixEmmiters[i - 1, j - 1] == null)
					{
						matrixEmmiters[i - 1, j - 1] = ((GameObject)GameObject.Instantiate(particlePrefab, Vector3.zero, Quaternion.identity)).GetComponent<ParticleEmitter>();
						
		                matrixEmmiters[i - 1, j - 1].name = string.Format("emmiter{0}{1}", i - 1, j - 1);
		                matrixEmmiters[i - 1, j - 1].transform.parent = parentEmmiters;
		
		                matrixEmmiters[i - 1, j - 1].transform.forward = parentEmmiters.forward;
		                matrixEmmiters[i - 1, j - 1].transform.localPosition = new Vector3((auxHorizontal * (tileHorizontal - i)) + (auxHorizontal * 0.5f) - 5f, 0.0f, (auxVertical * (tileVertical - j)) + (auxVertical * 0.5f) - 5f);//new Vector3((auxHorizontal * (tileHorizontal - i - 2)) + (auxHorizontal * 0.5f) - 5f, 0.0f, (auxVertical * (tileVertical - j - 2)) + (auxVertical * 0.5f) - 5f);
					}

                    matrixEmmiters[i - 1, j - 1].Emit();
                }

                matrixOfGlass[i, j] = true;
            }
        }
		
		//Mark correct mark to points.
            for (int i = 1; i < tileHorizontal + 1; i++)
            {
                for (int j = 1; j < tileVertical + 1; j++)
                {
                    int num = SelectMarkNumber(i, j);
                    textureCurrentAlpha.SetPixels((i - 1) * baseQuadSizeX, (j - 1) * baseQuadSizeY, baseQuadSizeX, baseQuadSizeY, glassStruct[num].GetColorMark(UnityEngine.Random.Range(0, textureBaseMarks.Length)));
                }
            }

            //Set all black intersections.
            for (int i = 0; i < tileHorizontal - 1; i++)
            {
                for (int j = 0; j < tileVertical - 1; j++)
                {
                    textureCurrentAlpha.SetPixels(i * baseQuadSizeX + (int)(baseQuadSizeX * 0.5f), j * baseQuadSizeY + (int)(baseQuadSizeY * 0.5f), baseQuadSizeX, baseQuadSizeY, GlassStruct.blackFillTexture);
                }
            }

            //Apply all current changes in the alpha texture.
            textureCurrentAlpha.Apply();

            enabled = false;
	}

    /// <summary>
    /// Test if need break all glass.
    /// </summary>
    void TestBreakAll()
    {
        //Sum and test if break all glass.
        numberToBreak++;
        if (numberToBreak > maxNumberToBreak)
        {
            //Set all point to true.
            for (int i = 1; i < tileHorizontal + 1; i++)
            {
                for (int j = 1; j < tileVertical + 1; j++)
                {
                    //Emmit particles.
                    if (matrixOfGlass[i, j] == false)
                    {
						if(matrixEmmiters[i - 1, j - 1] == null)
						{
							matrixEmmiters[i - 1, j - 1] = ((GameObject)GameObject.Instantiate(particlePrefab, Vector3.zero, Quaternion.identity)).GetComponent<ParticleEmitter>();
							
			                matrixEmmiters[i - 1, j - 1].name = string.Format("emmiter{0}{1}", i - 1, j - 1);
			                matrixEmmiters[i - 1, j - 1].transform.parent = parentEmmiters;
			
			                matrixEmmiters[i - 1, j - 1].transform.forward = parentEmmiters.forward;
			                matrixEmmiters[i - 1, j - 1].transform.localPosition = new Vector3((auxHorizontal * (tileHorizontal - i)) + (auxHorizontal * 0.5f) - 5f, 0.0f, (auxVertical * (tileVertical - j)) + (auxVertical * 0.5f) - 5f);//new Vector3((auxHorizontal * (tileHorizontal - i - 2)) + (auxHorizontal * 0.5f) - 5f, 0.0f, (auxVertical * (tileVertical - j - 2)) + (auxVertical * 0.5f) - 5f);//new Vector3((auxHorizontal * (tileHorizontal - i - 2)) + (auxHorizontal * 0.5f) - 5f, 0.0f, (auxVertical * (tileVertical - j - 2)) + (auxVertical * 0.5f) - 5f);
						}
						
                        matrixEmmiters[i - 1, j - 1].Emit();
                    }

                    matrixOfGlass[i, j] = true;
                }
            }

            //Mark correct mark to points.
            for (int i = 1; i < tileHorizontal + 1; i++)
            {
                for (int j = 1; j < tileVertical + 1; j++)
                {
                    int num = SelectMarkNumber(i, j);
                    textureCurrentAlpha.SetPixels((i - 1) * baseQuadSizeX, (j - 1) * baseQuadSizeY, baseQuadSizeX, baseQuadSizeY, glassStruct[num].GetColorMark(UnityEngine.Random.Range(0, textureBaseMarks.Length)));
                }
            }

            //Set all black intersections.
            for (int i = 0; i < tileHorizontal - 1; i++)
            {
                for (int j = 0; j < tileVertical - 1; j++)
                {
                    textureCurrentAlpha.SetPixels(i * baseQuadSizeX + (int)(baseQuadSizeX * 0.5f), j * baseQuadSizeY + (int)(baseQuadSizeY * 0.5f), baseQuadSizeX, baseQuadSizeY, GlassStruct.blackFillTexture);
                }
            }

            //Apply all current changes in the alpha texture.
            textureCurrentAlpha.Apply();

            enabled = false;

            return;
        }
    }


    /// <summary>
    /// Test if gravity will break the points of glass.
    /// </summary>
    void TestGravityBreak ()
    {
    	//Base size of glass to break by gravity (only center points).
    	if (tileHorizontal > 4 && tileVertical > 2)
        {
    		//Only center pieces.
    		for (int i = 2; i < tileHorizontal; i++)
            {
    			for (int j = tileVertical - 1; j > 1; j--)
                {
    				//Test1 (only one in center or only on side).
    				if (matrixOfGlass[i, j + 1] && matrixOfGlass[i, j - 1] && (matrixOfGlass[i - 1, j] || matrixOfGlass[i + 1, j]))
    					BreakAPoint (i, j);
    				
                    if (matrixOfGlass[i, j + 1] && (matrixOfGlass[i - 1, j] || matrixOfGlass[i + 1, j]) && UnityEngine.Random.Range (0, 2) == 1)
    					BreakAPoint (i, j);
    				

                    //Test2 (if all up dont have sides)
    				if (matrixOfGlass[i, j - 1] && matrixOfGlass[i - 1, j] && matrixOfGlass[i + 1, j]
                        && matrixOfGlass[i, j + 1] == false && matrixOfGlass[i + 1, j + 1] && matrixOfGlass[i - 1, j + 1])
                    {
    					BreakAPoint (i, j);
    					BreakAPoint (i, j + 1);
    				}
    			}
    		}
    	}
    }
}