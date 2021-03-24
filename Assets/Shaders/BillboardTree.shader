Shader "Hidden/TerrainEngine/BillboardTree" {
	Properties {
		_MainTex ("Base (RGB) Alpha (A)", 2D) = "white" {}
	}
	
	SubShader {
		Tags { "Queue" = "Transparent-100" "IgnoreProjector"="True" "RenderType"="TreeBillboard" }
		
		Pass {
			ColorMask rgb
			Blend SrcAlpha OneMinusSrcAlpha
			//ZWrite Off
			Cull Off
			
			CGPROGRAM
			#pragma vertex vert
			#include "UnityCG.cginc"
			#include "TerrainEngine.cginc"
			#pragma fragment frag

			struct v2f {
				float4 pos : POSITION;
				float fog : FOGC;
				float4 color : COLOR0;
				float2 uv : TEXCOORD0;
			};

			v2f vert (appdata_tree_billboard v) {
				v2f o;
				TerrainBillboardTree(v.vertex, v.texcoord1.xy, v.texcoord.y);	
				o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
				o.fog = o.pos.z;
				o.uv.x = v.texcoord.x;
				o.uv.y = v.texcoord.y > 0;
				o.color = v.color;
				return o;
			}

			sampler2D _MainTex;
			half4 frag(v2f input) : COLOR
			{
				half4 col = tex2D( _MainTex, input.uv);
				col.rgb *= input.color.rgb;
				clip(col.a - 0.0001);
				return col;
			}
			ENDCG			
		}
	}

	SubShader {
		Tags { "Queue" = "Transparent-100" "IgnoreProjector"="True" "RenderType"="TreeBillboard" }
		
		Pass {

			CGPROGRAM
			#pragma vertex vert
			#pragma exclude_renderers gles
			#include "UnityCG.cginc"
			#include "TerrainEngine.cginc"

			struct v2f {
				float4 pos : POSITION;
				float fog : FOGC;
				float4 color : COLOR0;
				float2 uv : TEXCOORD0;
			};

			v2f vert (appdata_tree_billboard v) {
				v2f o;
				TerrainBillboardTree(v.vertex, v.texcoord1.xy, v.texcoord.y);	
				o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
				o.fog = o.pos.z;
				o.uv.x = v.texcoord.x;
				o.uv.y = v.texcoord.y > 0;
				o.color = v.color;
				return o;
			}
			ENDCG			

			ColorMask rgb
			Blend SrcAlpha OneMinusSrcAlpha
			//ZWrite Off
			Cull Off
			
			AlphaTest Greater 0.0001
			SetTexture [_MainTex] { combine texture * primary, texture }
		}
	}
	
	Fallback Off
}