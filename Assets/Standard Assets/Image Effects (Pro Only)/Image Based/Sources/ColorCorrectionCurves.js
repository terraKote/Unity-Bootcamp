

public var redChannel : AnimationCurve;
public var greenChannel : AnimationCurve;
public var blueChannel : AnimationCurve;

public var useDepthCorrection : boolean = false;

public var zCurve : AnimationCurve;
public var depthRedChannel : AnimationCurve;
public var depthGreenChannel : AnimationCurve;
public var depthBlueChannel : AnimationCurve;

private var _ccMaterial : Material;
private var _ccDepthMaterial : Material;
private var _selectiveCcMaterial : Material;

private var _rgbChannelTex : Texture2D;
private var _rgbDepthChannelTex : Texture2D;

private var _zCurve : Texture2D;

public var selectiveCc : boolean = false;

public var selectiveFromColor : Color = Color.white;
public var selectiveToColor : Color = Color.white;


public var updateTextures : boolean = true;
public var recreateTextures : boolean = false;

public var curveResolution : int = 256;

// GENERAL stuff

@script ExecuteInEditMode
@script AddComponentMenu ("Image Effects/Color Correction (Curves)")

enum ColorCorrectionMode {
	Simple = 0,
	Advanced = 1	
}

public var mode : ColorCorrectionMode;

// SHADERS

public var colorCorrectionCurvesShader : Shader = null;
public var simpleColorCorrectionCurvesShader : Shader = null;
public var colorCorrectionSelectiveShader : Shader = null;

class ColorCorrectionCurves extends PostEffectsBase 
{
	function Start () {
		updateTextures = true;
		CreateMaterials ();	
	}
	
	function CreateMaterials () {
        if (recreateTextures)
        {
            _rgbChannelTex = null;
            _rgbDepthChannelTex = null;
            _zCurve = null;
            recreateTextures = false;
        }

		if (!_ccMaterial) {
			if(!CheckShader(simpleColorCorrectionCurvesShader)) {
				enabled = false;
				return;
			}
			_ccMaterial = new Material (simpleColorCorrectionCurvesShader);	
			_ccMaterial.hideFlags = HideFlags.HideAndDontSave;
		}

		if (!_ccDepthMaterial) {
			if(!CheckShader(colorCorrectionCurvesShader)) {
				enabled = false;
				return;
			}
			_ccDepthMaterial = new Material (colorCorrectionCurvesShader);	
			_ccDepthMaterial.hideFlags = HideFlags.HideAndDontSave;
		}	
		
		if (!_selectiveCcMaterial) {
			if(!CheckShader(colorCorrectionSelectiveShader)) {
				enabled = false;
				return;
			}
			_selectiveCcMaterial = new Material (colorCorrectionSelectiveShader);
			_selectiveCcMaterial.hideFlags = HideFlags.HideAndDontSave;	
		}
		
		if(!SystemInfo.SupportsRenderTextureFormat (RenderTextureFormat.Depth)) {
			enabled = false;
			return;	
		}
		
		// sample all curves, create textures
		if (!_rgbChannelTex) {
			_rgbChannelTex = new Texture2D(curveResolution, 4, TextureFormat.ARGB32, false);
			_rgbChannelTex.hideFlags = HideFlags.HideAndDontSave;
			updateTextures = true;
		}
		if (!_rgbDepthChannelTex) {
			_rgbDepthChannelTex = new Texture2D(curveResolution, 4, TextureFormat.ARGB32, false);
			_rgbDepthChannelTex.hideFlags = HideFlags.HideAndDontSave;
			updateTextures = true;
		}
		
		if (!_zCurve) {
			_zCurve = new Texture2D (curveResolution, 1, TextureFormat.ARGB32, false);
			_zCurve.hideFlags = HideFlags.HideAndDontSave;
			updateTextures = true;
		}	
		
		_rgbChannelTex.wrapMode = TextureWrapMode.Clamp;
		_rgbDepthChannelTex.wrapMode = TextureWrapMode.Clamp;
		_zCurve.wrapMode = TextureWrapMode.Clamp;		
	}
	
	function OnEnable() {
		if( !CheckSupport() ) {
			enabled = false;
			return;	
		}
	if(useDepthCorrection)
		camera.depthTextureMode |= DepthTextureMode.Depth;	
	}
	
	function OnDisable () {
	}
	
	public function UpdateParameters() 
	{
		if (updateTextures && redChannel && greenChannel && blueChannel) 
		{
            var rgbC : Vector3 = Vector3(0,0,0);
            var rgbDC : Vector3 = Vector3(0,0,0);
            var zC : float = 0;
            
            var curveResolutionAsFloat : float = curveResolution;
            var curveStep : float = 1.0 / curveResolutionAsFloat;
            
            var step : float = 1.0 / 256.0;
            var subStep : float = 0.0;
            
            var texelIndex : int = 0;
			for (var i : float = 0.0; i <= 1.0; i += step) 
			{
                rgbC.x += Mathf.Clamp01(redChannel.Evaluate(i));
                rgbC.y += Mathf.Clamp01(greenChannel.Evaluate(i));
                rgbC.z += Mathf.Clamp01(blueChannel.Evaluate(i));
                
                zC += Mathf.Clamp01(zCurve.Evaluate(i));
                    
                rgbDC.x += Mathf.Clamp01(depthRedChannel.Evaluate(i));
                rgbDC.y += Mathf.Clamp01(depthGreenChannel.Evaluate(i));
                rgbDC.z += Mathf.Clamp01(depthBlueChannel.Evaluate(i));
                
                subStep += step;
                if (subStep >= curveStep)
                {
                    rgbC *= step / curveStep;
                    zC *= step / curveStep;
                    rgbDC *= step / curveStep;
                   
                    _rgbChannelTex.SetPixel( texelIndex, 0, Color(rgbC.x,rgbC.x,rgbC.x) );
                    _rgbChannelTex.SetPixel( texelIndex, 1, Color(rgbC.y,rgbC.y,rgbC.y) );
                    _rgbChannelTex.SetPixel( texelIndex, 2, Color(rgbC.z,rgbC.z,rgbC.z) );
                    
                    _zCurve.SetPixel( texelIndex, 0, Color(zC,zC,zC) );
                
                    _rgbDepthChannelTex.SetPixel( texelIndex, 0, Color(rgbDC.x,rgbDC.x,rgbDC.x) );
                    _rgbDepthChannelTex.SetPixel( texelIndex, 1, Color(rgbDC.y,rgbDC.y,rgbDC.y) );
                    _rgbDepthChannelTex.SetPixel( texelIndex, 2, Color(rgbDC.z,rgbDC.z,rgbDC.z) );
                    
                    rgbC = Vector3(0,0,0);
                    rgbDC = Vector3(0,0,0);
                    zC = 0;

                    texelIndex++;
                    subStep = 0.0;                    
                }
			}
			
			_rgbChannelTex.Apply();
			_rgbDepthChannelTex.Apply();

			_zCurve.Apply();
				
			updateTextures = false;
		}
	}
	
	function OnRenderImage2 (source : RenderTexture, destination : RenderTexture)
	{
		CreateMaterials ();
		UpdateParameters();	
		
		// force disable anisotropic filtering
		if(_rgbChannelTex)
			_rgbChannelTex.anisoLevel = 0;
		if(_rgbDepthChannelTex)
			_rgbDepthChannelTex.anisoLevel = 0;
						
		var renderTarget2Use : RenderTexture = destination;
		
		if (selectiveCc) {
			// we need an extra render target
			renderTarget2Use = RenderTexture.GetTemporary (source.width, source.height);
		}
		
		if (useDepthCorrection) 
		{
			_ccDepthMaterial.SetTexture ("_RgbTex", _rgbChannelTex);
	
			_ccDepthMaterial.SetTexture ("_ZCurve", _zCurve);
			
			_ccDepthMaterial.SetTexture ("_RgbDepthTex", _rgbDepthChannelTex);
	
			Graphics.Blit (source, renderTarget2Use, _ccDepthMaterial); 	
		} 
		else 
		{
			_ccMaterial.SetTexture ("_RgbTex", _rgbChannelTex);
	
			Graphics.Blit (source, renderTarget2Use, _ccMaterial); 			
		}
		
		if (selectiveCc) {
			_selectiveCcMaterial.SetVector ("selColor", Vector4(selectiveFromColor.r,selectiveFromColor.g,selectiveFromColor.b,selectiveFromColor.a) );
			_selectiveCcMaterial.SetVector ("targetColor", Vector4(selectiveToColor.r,selectiveToColor.g,selectiveToColor.b,selectiveToColor.a) );
			Graphics.Blit (renderTarget2Use, destination, _selectiveCcMaterial); 	
			
			RenderTexture.ReleaseTemporary (renderTarget2Use);
		}
				
	}

}