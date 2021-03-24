Shader "Hidden/PoissonBlur" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "" {}
	}
	
	// Helper code used in all of the below subshaders	
	CGINCLUDE
	
	#pragma target 3.0
	
	#include "UnityCG.cginc"
	
	struct v2f {
		float4 pos : POSITION;
		float2 uv : TEXCOORD0;
	};
		
	sampler2D _MainTex;
	float4 _MainTex_TexelSize;
		
	v2f vert( appdata_img v ) {
		v2f o;
		o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
		o.uv =  MultiplyUV(UNITY_MATRIX_TEXTURE0, v.texcoord);
		return o;
	}
	
	half4 frag(v2f i) : COLOR {
		const float2 poisson[8] = {
			float2( 0.0, 0.0),
			float2( 0.527837,-0.085868),
			float2(-0.040088, 0.536087),
			float2(-0.670445,-0.179949),
			float2(-0.419418,-0.616039),
			float2( 0.440453,-0.639399),
			float2(-0.757088, 0.349334),
			float2( 0.574619, 0.685879),
		};
		
		float4 finalColor = float4(0.0,0.0,0.0,0.0);
		
		for(int j = 0; j < 8; j++) { 
			finalColor += tex2D(_MainTex, i.uv+_MainTex_TexelSize.xy*poisson[j]*1.25);
		}		
		
		finalColor.rgb = finalColor.rgb/8.0;
		return finalColor;
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