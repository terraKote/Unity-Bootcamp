using UnityEngine;

/// <summary>
/// Have the same side mark with different form.
/// </summary>
public struct GlassMark
{
    public Color[] markTexture;


    /// <summary>
    /// 
    /// </summary>
    /// <param name="p_markNumber"></param>
    /// <param name="p_markTexture"></param>
    public GlassMark(Color[] p_markTexture)
    {
        markTexture = p_markTexture;
    }
}