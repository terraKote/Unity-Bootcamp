Shader "Hidden/LightShafts" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "" {}
		_ShadowMap ("_ShadowMap", 2D) = "" {}
		_LowRezDepth("_LowRezDepth", 2D) = "" {}
		_Noise ("_Noise", 2D) = "" {}
		_Cookie ("_Cookie", 2D) = "" {}
	}
	
	// Helper code used in all of the below subshaders	
	CGINCLUDE
	
	#include "UnityCG.cginc"
	
	#pragma target 3.0
	
	struct v2f {
		float4 pos : POSITION;
		float2 uv : TEXCOORD0;
		float4 projPos : TEXCOORD1;
		float4 screenPos : TEXCOORD2;
	};
	
	uniform float4x4 camToLight;
	uniform float4x4 unProject;

	uniform float planeIntensity;
	
	sampler2D _MainTex;
	sampler2D _ShadowMap;
	sampler2D _LowRezDepth;
	sampler2D _Noise;
	sampler2D _Cookie;
	
	float4 _ShadowMap_TexelSize;
		
	v2f vert( appdata_img v ) {
		v2f o;
		o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
		o.projPos = mul(camToLight, v.vertex); 
		o.uv =  MultiplyUV(UNITY_MATRIX_TEXTURE0, v.texcoord);
		o.screenPos = o.pos;
		return o;
	}
	
	half4 CheckShadowMap(half2 smUv, half refDepth)  
	{ 
		half4 texMap = tex2D/*proj*/(_ShadowMap, smUv);
		half d = DecodeFloatRGBA(texMap);
		
		// check if visible from light's perspective
		if(d < refDepth + 0.0001) 
			return half4(0,0,0,0); 
		else
			return half4(1,1,1,1);		
	}
	
	// note: this shader is to be optimized (away) but kept here due to shipping time requirements
	// also: name pretty confusing (historical reasons) 
	
	half4 frag(v2f i) : COLOR 
	{ 
		// project uvs (in view frustum space) to shadowmapspace
		half4 uvs = i.projPos/i.projPos.w; // -> tex2dproj

		uvs.xyz = 0.5*uvs.xyz+0.5; // unsign
		uvs.y = 1.0-uvs.y;

		// obtain pixels depth value from light's perspective
		half4 texMap = CheckShadowMap(uvs.xy, uvs.z);
 
			
		// and a stupid zbuffer check-hack
		half d2 = DecodeFloatRGBA(tex2D(_LowRezDepth, i.uv * half2(1,-1) + half2(0,1) ));	
		if((i.screenPos/i.screenPos.w).z*0.5+0.5 > d2)
			texMap = half4(0,0,0,0); 
		
		// cookie multiplication
		half4 cookie = tex2D(_Cookie, uvs.xy);
		texMap *= cookie; 

		texMap *= planeIntensity;
		 
		if(uvs.z < 0.5)
			texMap.a = 0;
		else 
			texMap.a = saturate(dot(texMap.rgb,float3(1,1,1)));
		
		return texMap;
	}
	
	// graveyard:
	//texMap *= saturate(0.275 + 100.0 / (dot((i.projPos).xyz,(i.projPos).xyz)));
	//texMap *= saturate(0.125+tex2D(_Noise, uvs.xy+_Time.w*half2(0.1,0.1)));
	//texMap *= saturate(0.125+tex2D(_Noise, uvs.xy+_Time.w*half2(-0.2,-0.2)));
		
	ENDCG
	
Subshader {
	// Blend SMARTER OLE (!!!!)
	Blend SrcAlpha One 
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