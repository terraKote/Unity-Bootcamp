Shader "Hidden/AddAlphaHack" {
	Properties {
		_Color ("Main Color", Color) = (1,1,1,1)
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	
	// Helper code used in all of the below subshaders	
	CGINCLUDE
	
	#include "UnityCG.cginc"
	
	struct v2f {
		float4 pos : POSITION;
		float2 uv : TEXCOORD0;
	};
	
	sampler2D _MainTex;
	float4 _Color;
		
	v2f vert (appdata_base v) {
		v2f o; 
		o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
		o.uv =  MultiplyUV(UNITY_MATRIX_TEXTURE0, v.texcoord);
		return o;
	}
	
	half4 frag(v2f i) : COLOR {
		return half4(0, 0, 0, 1) * /* _Color * */ tex2D(_MainTex, i.uv);
	}

	ENDCG
	
Subshader { 
 Blend One One
 Tags { "RenderType"="Opaque" }
  Pass {

      CGPROGRAM
      // compilation directives for this snippet, e.g.:
      #pragma vertex vert
      #pragma fragment frag

      // the Cg code itself

      ENDCG
      // ... the rest of pass setup ...
   }
 }
 
Subshader { 
 Blend One One
 Tags { "RenderType"="Transparent" }
  Pass {

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