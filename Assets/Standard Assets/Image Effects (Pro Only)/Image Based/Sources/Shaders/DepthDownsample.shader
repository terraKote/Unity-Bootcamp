Shader "Hidden/DepthDownsampleShader" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "" {}
	}
	
	// Helper code used in all of the below subshaders	
	CGINCLUDE
	
	#include "UnityCG.cginc"
	
	struct v2f {
		float4 pos : POSITION;
		float2 uv : TEXCOORD0;
	};
	
	sampler2D _MainTex;
	
	uniform float speed;
	
	float4 _MainTex_TexelSize; // thanks 2 aras
		
	v2f vert( appdata_img v ) {
		v2f o;
		o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
		o.uv =  MultiplyUV(UNITY_MATRIX_TEXTURE0, v.texcoord);
		return o;
	} 
	
	half4 frag(v2f i) : COLOR 
	{
		half4 totalColor = half4(0,0,0,0);
		
		totalColor += tex2D (_MainTex, i.uv.xy + half2( 0.2, 0.2));
		totalColor += tex2D (_MainTex, i.uv.xy + half2(-0.2, 0.2));
		totalColor += tex2D (_MainTex, i.uv.xy + half2( 0.2,-0.2));
		totalColor += tex2D (_MainTex, i.uv.xy + half2(-0.2,-0.2));
		
		totalColor += tex2D (_MainTex, i.uv.xy + half2( 0.5, 0.5));
		totalColor += tex2D (_MainTex, i.uv.xy + half2(-0.5, 0.5));
		totalColor += tex2D (_MainTex, i.uv.xy + half2( 0.5,-0.5));
		totalColor += tex2D (_MainTex, i.uv.xy + half2(-0.5,-0.5));
		
		totalColor /= 8.0;
		
		totalColor.a = speed;
		
		return totalColor;
	}

	ENDCG 
	
Subshader {
 Blend SrcAlpha OneMinusSrcAlpha 
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