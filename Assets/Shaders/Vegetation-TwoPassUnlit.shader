/*
Renders doubled sides objects without lighting. Useful for
grass, trees or foliage.

This shader renders two passes for all geometry, one
for opaque parts and one with semitransparent details.

This makes it possible to render transparent objects
like grass without them being sorted by depth.
*/

Shader "Nature/Vegetation Two Pass unlit_2.x" {
Properties {
	_Color ("Main Color", Color) = (1, 1, 1, 1)
	_MainTex ("Base (RGB) Alpha (A)", 2D) = "white" {}
	_Cutoff ("Base Alpha cutoff", Range (0,.9)) = .5
}
SubShader {
	Tags {"Queue"="Transparent-110" "IgnoreProjector"="True" }
	Alphatest Greater 0
	ZWrite Off
	Blend SrcAlpha OneMinusSrcAlpha 
	ColorMask RGB
	
	// Render both front and back facing polygons.
	Cull Off
	
	// first pass:
	//   render any pixels that are more than [_Cutoff] opaque
	Pass {  
		ZWrite On
		AlphaTest Greater [_Cutoff]
		SetTexture [_MainTex] {
			constantColor [_Color]
			combine texture * constant, texture * constant 
		}
	}

	// Second pass:
	//   render the semitransparent details.
	Pass {
		//Tags { "RequireOption" = "SoftVegetation" }
		// Dont write to the depth buffer
		ZWrite Off
		
		// Only render pixels less or equal to the value
		AlphaTest LEqual [_Cutoff]
		
		// Set up alpha blending
		Blend SrcAlpha OneMinusSrcAlpha
		
		SetTexture [_MainTex] {
			constantColor [_Color]
			Combine texture * constant, texture * constant 
		}
	}
}
}
