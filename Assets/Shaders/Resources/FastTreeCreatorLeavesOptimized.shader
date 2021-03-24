Shader "Hidden/Nature/Fast/Tree Creator Leaves Optimized" {
	
Properties {
	_Color ("Main Color", Color) = (1,1,1,1)
	_TranslucencyColor ("Translucency Color", Color) = (0.73,0.85,0.41,1) // (187,219,106,255)
	_Cutoff ("Alpha cutoff", Range(0,1)) = 0.3
	_TranslucencyViewDependency ("View dependency", Range(0,1)) = 0.7
	_ShadowStrength("Shadow Strength", Range(0,1)) = 0.8
	_ShadowOffsetScale ("Shadow Offset Scale", Float) = 1
	
	_MainTex ("Base (RGB) Alpha (A)", 2D) = "white" {}
	_ShadowTex ("Shadow (RGB)", 2D) = "white" {}

	// These are here only to provide default values
	_Scale ("Scale", Vector) = (1,1,1,1)
	_SquashAmount ("Squash", Float) = 1
}
SubShader { 
	Tags {
		"IgnoreProjector"="True"
		"RenderType" = "TreeLeaf"
	}
	LOD 200


	Pass {
		Tags { "LightMode" = "ForwardBase" }

	CGPROGRAM
		#include "UnityCG.cginc"
		#include "TreeVertexLit.cginc"
		#include "TerrainEngine.cginc"

		#pragma vertex VertexLeaf
		#pragma fragment FragmentLeaf
		#pragma multi_compile_fwdbase_nolightmap
		
		
		
		sampler2D _MainTex;
		float4 _MainTex_ST;
		
		float _Cutoff;
		sampler2D _ShadowMapTexture;

		struct v2f_leaf {
			float4 pos : SV_POSITION;
			float4 diffuse : COLOR0;
		#if defined(SHADOWS_SCREEN)
			float4 mainLight : COLOR1;
		#endif
			float2 uv : TEXCOORD0;
		#if defined(SHADOWS_SCREEN)
			float4 screenPos : TEXCOORD1;
		#endif
		};
		v2f_leaf VertexLeaf (appdata_full v)
		{
			v2f_leaf o;
			TreeVertLeaf(v);
			o.pos = mul(UNITY_MATRIX_MVP, v.vertex);

			float ao = v.color.a;
			ao += 0.1; ao = saturate(ao * ao * ao); // hack to emphasize AO, just for kicks now! ;)
						
			float3 color = v.color.rgb * ao;
			
			float3 worldN = mul ((float3x3)_Object2World, SCALED_NORMAL);

			float4 mainLight;
			mainLight.rgb = ShadeTranslucentMainLight (v.vertex, worldN) * color;
			mainLight.a = v.color.a;
			o.diffuse.rgb = ShadeTranslucentLights (v.vertex, worldN) * color;
			o.diffuse.a = 1;
		#if defined(SHADOWS_SCREEN)
			o.mainLight = mainLight;
			o.screenPos = ComputeScreenPos (o.pos);
		#else
			o.diffuse *= 0.5;
			o.diffuse += mainLight;
		#endif			
			o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
			return o;
		}
		
		half4 FragmentLeaf (v2f_leaf IN) : COLOR
		{
			half4 albedo = tex2D(_MainTex, IN.uv); //albedo.xyz = 1;
			half alpha = albedo.a;
			clip (alpha - _Cutoff);
						
		#if defined(SHADOWS_SCREEN)
			half4 light = IN.mainLight;
			half atten = tex2Dproj(_ShadowMapTexture, UNITY_PROJ_COORD(IN.screenPos)).r;
			light.rgb *= lerp(2, 2*atten, _ShadowStrength);
			light.rgb += IN.diffuse.rgb;
		#else
			half4 light = IN.diffuse;
			light.rgb *= 2.0;
		#endif
		
			return half4 (albedo.rgb * light, 0.0);
		}
	ENDCG
	}


	// Pass to render object as a shadow caster
	Pass {
		Name "ShadowCaster"
		Tags { "LightMode" = "ShadowCaster" }
		
		Fog {Mode Off}
		ZWrite On ZTest Less
		Offset 1, 1

	CGPROGRAM
		#pragma vertex vert_surf
		#pragma fragment frag_surf
		#pragma fragmentoption ARB_precision_hint_fastest
		#pragma multi_compile_shadowcaster
		#include "HLSLSupport.cginc"
		#include "UnityCG.cginc"
		#include "Lighting.cginc"

		#define INTERNAL_DATA
		#define WorldReflectionVector(data,normal) data.worldRefl
		#line 22

		#include "Tree.cginc"

		sampler2D _ShadowTex;

		struct Input {
			float2 uv_MainTex;
			float4 color : COLOR; // color.a = AO
		};

		struct v2f_surf {
			V2F_SHADOW_CASTER;
			float2 hip_pack0 : TEXCOORD1;
		};
		float4 _ShadowTex_ST;
		float4 _MainTex_ST;
		v2f_surf vert_surf (appdata_full v) {
			v2f_surf o;
			TreeVertLeaf (v);
			o.hip_pack0.xy = TRANSFORM_TEX(v.texcoord, _ShadowTex);
			TRANSFER_SHADOW_CASTER(o)
			return o;
		}
		float _Cutoff;
		half4 frag_surf (v2f_surf IN) : COLOR {
			half alpha = tex2D(_ShadowTex, IN.hip_pack0.xy).r;
			clip (alpha - _Cutoff);
			SHADOW_CASTER_FRAGMENT(IN)
		}

	ENDCG

	}

	// Pass to render object as a shadow collector
	Pass {
		Name "ShadowCollector"
		Tags { "LightMode" = "ShadowCollector" }
		
		Fog {Mode Off}
		ZWrite On ZTest Less

	CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#pragma fragmentoption ARB_precision_hint_fastest
		#pragma multi_compile_shadowcollector

		#define SHADOW_COLLECTOR_PASS
		#include "UnityCG.cginc"
		#include "TerrainEngine.cginc"

		struct v2f {
			V2F_SHADOW_COLLECTOR;
			float2  uv : TEXCOORD5;
		};

		uniform float4 _MainTex_ST;

		v2f vert (appdata_full v)
		{
			v2f o;
			TreeVertLeaf(v);
			TRANSFER_SHADOW_COLLECTOR(o)
			o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
			return o;
		}

		uniform sampler2D _MainTex;
		uniform float _Cutoff;
		uniform float4 _Color;

		half4 frag (v2f i) : COLOR
		{
			half4 texcol = tex2D( _MainTex, i.uv );
			clip( texcol.a - _Cutoff );
			
			SHADOW_COLLECTOR_FRAGMENT(i)
		}
	ENDCG

	}
	
}

SubShader {
	Tags {
		"IgnoreProjector"="True"
		"RenderType"="TreeLeaf"
	}
	
	Lighting On
	
	Pass {
		CGPROGRAM
		#pragma exclude_renderers gles xbox360 ps3
		#pragma vertex TreeVertLit
		#include "UnityCG.cginc"
		#include "TreeVertexLit.cginc"
		#include "TerrainEngine.cginc"
		struct v2f {
			float4 pos : SV_POSITION;
			float4 color : COLOR;
			float4 uv : TEXCOORD0;
		};
		v2f TreeVertLit (appdata_full v) {
			v2f o;
			TreeVertLeaf(v);

			o.color.rgb = ShadeVertexLights (v.vertex, v.normal);
				
			o.pos = mul (UNITY_MATRIX_MVP, v.vertex);	
			o.uv = v.texcoord;
			o.color.a = 1.0f;
			return o;
		}
		ENDCG

		AlphaTest Greater [_Cutoff]
		ColorMask RGB
		SetTexture [_MainTex] { combine texture * primary DOUBLE, texture }
		SetTexture [_MainTex] {
			ConstantColor [_Color]
			Combine previous * constant, previous
		} 
	}
}

SubShader {
	Tags { "RenderType"="TreeLeaf" }
	Pass {		
		Material {
			Diffuse (1,1,1,1)
			Ambient (1,1,1,1)
		} 
		Lighting On
		AlphaTest Greater [_Cutoff]
		ColorMask RGB
		SetTexture [_MainTex] { Combine texture * primary DOUBLE, texture }
		SetTexture [_MainTex] {
			ConstantColor [_Color]
			Combine previous * constant, previous
		} 
	}
} 

Dependency "BillboardShader" = "Hidden/Nature/Tree Creator Leaves Rendertex"
}
