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
			Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
			LOD 200
			Cull Off
			ZWrite Off

		CGPROGRAM
#pragma surface surf Standard fullforwardshadows

#pragma target 3.0
	
		sampler2D _MainTex;

	struct Input {
		float2 uv_MainTex;
	};

	half _Glossiness;
	half _Metallic;
	fixed4 _Color;


		void surf(Input IN, inout SurfaceOutputStandard o) {
	}
	ENDCG
	}
		FallBack "Diffuse"
}
