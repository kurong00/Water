Shader "WaterShader/Water" {
	Properties{
		_Color("Main Color", Color) = (1,1,1,1)
		_ReflectColor("Reflect Color", Color) = (1,1,1,1)
		_ReflectLevel("Reflect Level", Range(0,1)) = 0.5
		_FadeColor("Fade Color",Color) = (1,1,1,1)
		_Direction ("Waves Direction", Vector) = (1.0 ,1.0, -1.0, -1.0)
		//Gerstner Waves
		_Amplitude ("Wave Amplitude", Vector) = (0.05 ,0.05, 0.02, 0.03)
		_Speed ("Wave Speed", Vector) = (1, 1.5, 1, 0.2)
		_Frequency ("Wave Frequency", Vector) = (5, -7, 2, 3)
		_Steepness ("Wave Steepness", Vector) = (0.5, 0.05, 0.5, 0.23)
		_GDirectionAB ("Wave Direction", Vector) = (0.3 ,0.5, 0.85, 0.25)
		_GDirectionCD ("Wave Direction", Vector) = (0.1 ,0.9, 0.5, 0.5)	

		_FadeDepth  ("Fade Depth", Float) = 1
		_DistortionVert  ("Vertex Distortion", Float) = 2
		_Bias("Bias Glare", Float) = -1
	}
	SubShader{
		GrabPass {
			"_GrabTexture"
			Tags { "LightMode" = "Always" "IgnoreProjector"="True" }
 		}
		Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		LOD 200
		Cull Off
		ZWrite Off
		Pass {
			CGPROGRAM

			#pragma vertex vert 
			#pragma fragment frag
			#pragma target 3.0
			#include "UnityCG.cginc"

			struct v2f {
			float4 vertex : POSITION;
			float4 uvgrab : TEXCOORD0;
			float3 uvWave1 : TEXCOORD1;
			float3 uvWave2 : TEXCOORD2;
			float4 viewDir : TEXCOORD3;
			float4 screenPos : TEXCOORD4;
			float2 uvFoam : TEXCOORD5;	
			float4 uvgrabDefault : TEXCOORD6;
			float4 screenPosWithoutVert : TEXCOORD7;
			};
		v2f vert (appdata_tan v) {
			v2f o;
			float4 oPos = UnityObjectToClipPos(v.vertex);
			float3 posWorld = mul(unity_ObjectToWorld,v.vertex).xyz;
			//Gerstner Waves
			half2
			//TODO
			return o;
		}
		half4 frag( v2f i ) : COLOR {
			half4 color;
			//TODO
			return color;
		}
		ENDCG
		}	
	}
	FallBack "Diffuse"
}
