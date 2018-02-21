Shader "WaterShader/Ripples" {
	Properties {
	_TintColor ("Tint Color", Color) = (1,1,1,1)
	_RiplesStrength ("Riples strength", Float) = 1.0
	_FalloffTex ("FallOff", 2D) = "white" {}
	_RampStrength ("RampStrength", 2D) = "white" {}
	}
	Subshader {
		Tags {"Queue"="Transparent"}
		Pass {
			ZWrite Off
			Blend SrcAlpha One
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile editor_on editor_off
			#include "UnityCG.cginc"

			fixed4 _TintColor;
			fixed _RiplesStrength;
			sampler2D _FalloffTex;
			sampler2D _RampStrength;
			
			float4x4 unity_Projector;
			float4 _LightColor0; 
			sampler2D _WaterDisplacementTexture;
			float4x4 unity_ProjectorClip;
			float4 _WaterDisplacementTexture_ST;

			struct v2f {
				float4 uvFalloff : TEXCOORD0;
				float4 uv_Offset : TEXCOORD1;
				float4 vertex : POSITION;
			};

			v2f vert (float4 vertex : POSITION)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos (vertex);
				o.uvFalloff = mul (unity_ProjectorClip, vertex);
				o.uv_Offset = mul (unity_Projector, vertex);
				return o;
			}

			fixed4 frag (v2f i) : COLOR
			{
				#if editor_on
				return 0;
				#endif
				fixed4 texF = tex2Dproj (_FalloffTex, UNITY_PROJ_COORD(i.uvFalloff));
				float2 texDisp = tex2Dproj (_WaterDisplacementTexture, UNITY_PROJ_COORD(i.uv_Offset)).rg;
				float displ = texDisp.r + texDisp.g;
				displ = abs(displ);
				displ = tex2D(_RampStrength, fixed2(displ, 0)).r/3 + displ*2;
				fixed4 res = lerp(half4(0,0,0,0), displ * _TintColor * _RiplesStrength, texF.a)*_LightColor0;
				return res;
			}
			ENDCG
		}
	}
}
