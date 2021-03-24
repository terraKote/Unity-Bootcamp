Shader "Misc/Mesh Terrain 4 Splats" {
Properties {
	_Control ("SplatMap (RGBA)", 2D) = "red" {}
	_Splat0 ("Layer 0 (R)", 2D) = "white" {}
	_Splat1 ("Layer 1 (G)", 2D) = "white" {}
	_Splat2 ("Layer 2 (B)", 2D) = "white" {}
	_Splat3 ("Layer 3 (A)", 2D) = "white" {}
	_BaseMap ("BaseMap (RGB)", 2D) = "white" {}
}

// Fragment program
SubShader {
	Tags { "RenderType" = "Opaque" }
	Pass { 
		Tags { "LightMode" = "Always" }
		
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#pragma fragmentoption ARB_precision_hint_fastest
		#pragma multi_compile LIGHTMAP_ON LIGHTMAP_OFF

		#include "UnityCG.cginc"

		struct appdata_lightmap {
			float4 vertex : POSITION;
			float2 texcoord : TEXCOORD0;
			float2 texcoord1 : TEXCOORD1;
		};
		
		struct v2f_vertex {
			float4 pos : SV_POSITION;
			float4 uv[3] : TEXCOORD0;
		};
		
		uniform sampler2D _Control;
		uniform float4 _Control_ST;
		
		#ifdef LIGHTMAP_ON
		uniform float4 unity_LightmapST;
		uniform sampler2D unity_Lightmap;
		#endif
		
		uniform sampler2D _Splat0,_Splat1,_Splat2,_Splat3;
		uniform float4 _Splat0_ST,_Splat1_ST,_Splat2_ST,_Splat3_ST;
		
		v2f_vertex vert (appdata_lightmap v) 
		{
			v2f_vertex o;
			o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
			o.uv[0].xy = TRANSFORM_TEX (v.texcoord.xy, _Control);
		#ifdef LIGHTMAP_ON
			o.uv[0].zw = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
		#else
			o.uv[0].zw = half2(0,0);
		#endif
			o.uv[1].xy = TRANSFORM_TEX (v.texcoord.xy, _Splat0);
			o.uv[1].zw = TRANSFORM_TEX (v.texcoord.xy, _Splat1);
			o.uv[2].xy = TRANSFORM_TEX (v.texcoord.xy, _Splat2);
			o.uv[2].zw = TRANSFORM_TEX (v.texcoord.xy, _Splat3);
		
			return o;
		}
		
		half4 frag (v2f_vertex i) : COLOR
		{
			half4 splat_control = tex2D(_Control, i.uv[0].xy);
			half3 splat_color = splat_control.r * tex2D (_Splat0, i.uv[1].xy).rgb;
			splat_color += splat_control.g * tex2D (_Splat1, i.uv[1].zw).rgb;
			splat_color += splat_control.b * tex2D (_Splat2, i.uv[2].xy).rgb;
			splat_color += splat_control.a * tex2D (_Splat3, i.uv[2].zw).rgb;
			#ifdef LIGHTMAP_ON
			splat_color *= DecodeLightmap (tex2D (unity_Lightmap, i.uv[0].zw));
			#endif
		
			return half4 (splat_color, 0.0);
		}
		ENDCG
 	}
}

// Fixed function
SubShader {
	Tags { "RenderType" = "Opaque" }
	Pass { 
		Tags { "LightMode" = "Vertex" }
		SetTexture [_BaseMap] { constantColor(0,0,0,0) combine texture, constant }
 	}
	Pass { 
		Tags { "LightMode" = "VertexLM" }
		SetTexture [unity_Lightmap] { combine texture }
		SetTexture [_BaseMap] { constantColor(0,0,0,0) combine texture * previous, constant }
 	}
	Pass { 
		Tags { "LightMode" = "VertexLMRGBM" }
		SetTexture [unity_Lightmap] { combine texture * texture alpha DOUBLE }
		SetTexture [_BaseMap] { constantColor(0,0,0,0) combine texture * previous DOUBLE, constant }
 	}
}
}
