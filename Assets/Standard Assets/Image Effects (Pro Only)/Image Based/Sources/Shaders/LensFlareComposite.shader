Shader "Hidden/LensFlareComposite" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "" {}
		_MainTexQuartered ("BaseQ (RGB)", 2D) = "" {}
	}
	
	// Helper code used in all of the below subshaders	
	CGINCLUDE
	
	#include "UnityCG.cginc"
	
	struct v2f {
		float4 pos : POSITION;
		float2 uv : TEXCOORD0;
	};
	
	sampler2D _MainTex;
	sampler2D _MainTexQuartered;
		
	v2f vert( appdata_img v ) {
		v2f o;
		o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
		o.uv =  MultiplyUV(UNITY_MATRIX_TEXTURE0, v.texcoord);
		return o;
	}
	
	half4 frag(v2f i) : COLOR {
		half4 color = tex2D (_MainTex, i.uv);
		half4 lfBlur = tex2D (_MainTexQuartered, (i.uv));
		
		color += lfBlur;	
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