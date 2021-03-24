Shader "Hidden/AddBrightStuffShader" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "" {}
		_BrightTex ("_BrightTex", 2D) = "" {}
	}
	
	// Helper code used in all of the below subshaders	
	CGINCLUDE

	#pragma fragmentoption ARB_precision_hint_fastest
	
	#include "UnityCG.cginc"
	
	struct v2f {
		float4 pos : POSITION;
		float2 uv[2] : TEXCOORD0;
	};
		
	sampler2D _MainTex;
	sampler2D _BrightTex;
		
	float intensity;
	float4 _MainTex_ST;
	float4 _MainTex_TexelSize;
	float4 _BrightTex_ST;
		
	v2f vert( appdata_img v ) {
		v2f o;
		o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
		
		o.uv[0] = v.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw; 
		o.uv[1] = v.texcoord.xy * _BrightTex_ST.xy + _BrightTex_ST.zw;
		
		// HACK FOR MSAA & WINDOWS
		
		#if SHADER_API_D3D9
		if (_MainTex_ST.w < 0)
			o.uv[1].y = 1-o.uv[1].y;
		#endif	
		
		return o;
	}
	
	half4 frag(v2f i) : COLOR {
		return tex2D(_BrightTex, i.uv[1]) * intensity + tex2D(_MainTex, i.uv[0]);	
	}

	ENDCG
	
Subshader {
 
 Pass {
      // ... the usual pass state setup ...
	  ZTest Always Cull Off ZWrite Off
	  Fog { Mode off }      

      CGPROGRAM
      
      #pragma fragmentoption ARB_precision_hint_fastest
      #pragma vertex vert
      #pragma fragment frag

      // the Cg code itself

      ENDCG
      // ... the rest of pass setup ...
  }
}
	
} // shader