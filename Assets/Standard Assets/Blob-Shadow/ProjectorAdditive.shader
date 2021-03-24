Shader "Projector/Additive" { 
	Properties {
		_ShadowTex ("Cookie", 2D) = "gray" { TexGen ObjectLinear }
		_FalloffTex ("FallOff", 2D) = "white" { TexGen ObjectLinear	}
	}

	Subshader {
		Tags { "RenderType"="Transparent" }
		Pass {
			Blend SrcAlpha One
			
			SetTexture [_ShadowTex] {
				combine texture
				Matrix [_Projector]
			}
			
			SetTexture [_FalloffTex] {
				constantColor (1,1,1,0)
				combine previous lerp (texture) constant
				Matrix [_ProjectorClip]
			}
		}
	}
}