using UnityEngine;

/// <summary>
/// 
/// </summary>
public struct GlassStruct
{
    private int index;

    //True if side is breaked, False if side is not breaked yet.
    private bool left;
    private bool top;
    private bool right;
    private bool bottom;

    //Record color of a single mark of glass.
    private GlassMark[] glassMarks;

    //Base black square to fill intersection of four breaked points.
    static public Color[] blackFillTexture;
    static public Color[] whiteFillTexture;

    /// <summary>
    /// 
    /// </summary>
    public GlassStruct(int p_index, params GlassMark[] p_glassMarks)
    {
        index = p_index;

        left = false;
        top = false;
        right = false;
        bottom = false;

        //Set marks with one index and array of colors.
        glassMarks = new GlassMark[p_glassMarks.Length];
        for (int i = 0; i < glassMarks.Length; i++)
        {
            glassMarks[i] = new GlassMark(p_glassMarks[i].markTexture);
        }
        
      
        //Set base black / white texture to fill intersections / replacement.
        GlassStruct.blackFillTexture = new Color[glassMarks[0].markTexture.Length];
        for (int i = 0; i < GlassStruct.blackFillTexture.Length; i++)
        {
            GlassStruct.blackFillTexture[i] = Color.black;
        }

        GlassStruct.whiteFillTexture = new Color[glassMarks[0].markTexture.Length];
        for (int i = 0; i < GlassStruct.whiteFillTexture.Length; i++)
        {
            GlassStruct.whiteFillTexture[i] = Color.white;
        }
    }


    /// <summary>
    /// 
    /// </summary>
    public void SetSides(bool p_left, bool p_top, bool p_right, bool p_bottom)
    {
        left = p_left;
        top = p_top;
        right = p_right;
        bottom = p_bottom;
    }



    /// <summary>
    /// Test if is this mark.
    /// </summary>
    public int IsThisMark(bool p_left, bool p_top, bool p_right, bool p_bottom)
    {
        if (p_left == left && p_top == top && p_right == right && p_bottom == bottom)
            return index;
        else
            return -1;
    }


    /// <summary>
    /// Return color array with mark bullet.
    /// </summary>
    /// <returns></returns>
    public Color[] GetColorMark(int p_formIndex)
    {
        return glassMarks[p_formIndex].markTexture;
    }
}




