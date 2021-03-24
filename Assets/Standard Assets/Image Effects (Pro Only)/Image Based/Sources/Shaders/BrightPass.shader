Shader "Hidden/BrightPass" {
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
		o.uv = float2(1,1);
		return o;
	}
	
	half4 frag(v2f i) : COLOR {
		return float4 (1, 1, 1, 1) * _Color * tex2D(_MainTex, i.uv);
	}

	ENDCG
	
Subshader { 
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