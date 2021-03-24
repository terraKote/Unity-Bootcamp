Shader "Hidden/DepthRecord" {
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
		float4 projPos : TEXCOORD1;
	};
	
	sampler2D _MainTex;
	float4 _Color; 
		
	v2f vert (appdata_base v) {
		v2f o;
		o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
		o.uv = MultiplyUV(UNITY_MATRIX_TEXTURE0, v.texcoord);
		o.projPos = o.pos;
		return o;
	}
	
	half4 frag(v2f i) : COLOR 
	{
		i.projPos /= i.projPos.w; // -> .w space		
		float d = 0.5*(i.projPos.z+1.0); // unsign

		//	return pow(d,120.5); // tex2D(_MainTex,i.uv.xy); //d; // pow(d,12.5); // * tex2D(_MainTex, i.uv);
		//	if(d>0.9983)
		//		return 0.3;
		//	return 0.0;

		// this should be a float texture ?
		if(d>0.9999)
			return half4(1,1,1,1);
		else
			return EncodeFloatRGBA(d); 
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