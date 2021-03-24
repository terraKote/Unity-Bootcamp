Shader "Hidden/RadialBlurDepthWeighted" 
{
	Properties 
	{ 
		_MainTex("Base (RGB)", 2D) = "" {}
		_LowRezDepth("Depth (RGBA encoded)", 2D) = "" {}
	}
	
	// Helper code used in all of the below subshaders	
	CGINCLUDE
	
	#include "UnityCG.cginc"
	#pragma target 3.0
	
	struct v2f {
		float4 pos : POSITION;
		float2 uv : TEXCOORD0;
	};
	
	float4 blurTo;
	float4 _MainTex_TexelSize;
	
	sampler2D _MainTex;
	sampler2D _LowRezDepth;
	
	sampler2D _CameraDepthTexture;
		
	v2f vert (appdata_img v) 
	{
		v2f o;
		o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
		o.uv =  MultiplyUV( UNITY_MATRIX_TEXTURE0, v.texcoord );
		return o;
	}
	
	// @WORK IN PROGRESS
		
	half4 frag (v2f i) : COLOR 
	{
		float4 color = float4 (0,0,0,0); 
		float3 ofs = float3(0,0,0);
		ofs.xy = blurTo.xy-i.uv.xy;
		ofs.z = blurTo.z; // * blurTo.w;
		
		ofs.xy *= _MainTex_TexelSize.xy * blurTo.w; 
		
		float4 depthVals = float4(0,0,0,0);
		
		depthVals.r = (tex2D(_CameraDepthTexture, i.uv));
		depthVals.g = (tex2D(_CameraDepthTexture, i.uv + 1.0 * ofs.xy));
		depthVals.b = (tex2D(_CameraDepthTexture, i.uv + 2.0 * ofs.xy));
		depthVals.a = (tex2D(_CameraDepthTexture, i.uv + 3.0 * ofs.xy));
		
		color += 0.400 * tex2D(_MainTex, i.uv) ;
		color += 0.275 * tex2D(_MainTex, i.uv + 1.0 * ofs.xy) ;
		color += 0.200 * tex2D(_MainTex, i.uv + 2.0 * ofs.xy) ;
		color += 0.125 * tex2D(_MainTex, i.uv + 3.0 * ofs.xy) ;

		 
		return color;
	}

	ENDCG
	
Subshader {
 Pass {
      // ... the usual pass state setup ...
	  ZTest Always Cull Off ZWrite Off
	  Fog { Mode off }      

      CGPROGRAM
      // compilation directives for this snippet, e.g.:
      #pragma vertex vert
      #pragma fragment frag

      // the Cg code itself

      ENDCG
      // ... the rest of pass setup ...
  }
}
	
} // shader