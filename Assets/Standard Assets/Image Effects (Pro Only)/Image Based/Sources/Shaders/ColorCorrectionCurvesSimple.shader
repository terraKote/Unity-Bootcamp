Shader "Hidden/ColorCorrectionCurvesSimple" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "" {}
		_RgbTex ("_RgbTex (RGB)", 2D) = "" {}
	}
	
	// Shader code pasted into all further CGPROGRAM blocks
	CGINCLUDE

	#pragma fragmentoption ARB_precision_hint_fastest
	
	#include "UnityCG.cginc"
	
	struct v2f {
		float4 pos : POSITION;
		float2 uv : TEXCOORD0;
	};
	
	sampler2D _MainTex;
	
	sampler2D _RgbTex;
	
	v2f vert( appdata_img v ) 
	{
		v2f o;
		o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
		o.uv = v.texcoord.xy;
		return o;
	} 
	
	half4 frag(v2f i) : COLOR 
	{
		half4 color = tex2D(_MainTex, i.uv); 
		half red = tex2D(_RgbTex, half2(color.r, 0.5/4.0)).r;
		half green = tex2D(_RgbTex, half2(color.g, 1.5/4.0)).g;
		half blue = tex2D(_RgbTex, half2(color.b, 2.5/4.0)).b;
		#if SHADER_API_D3D9 // work around Cg codegen bug for D3D9...
		red += 0.0001;
		green += 0.0001;
		blue += 0.0001;
		#endif
		color = half4(red, green, blue, color.a);
				
		return color;
	}

	ENDCG 
	
Subshader {
 Pass {
	  ZTest Always Cull Off ZWrite Off
	  Fog { Mode off }      

      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag
      ENDCG
  }
}

Fallback off
	
} // shader