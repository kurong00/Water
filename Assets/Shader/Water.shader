Shader "WaterShader/Water" {
	Properties{
		_Color("Main Color", Color) = (1,1,1,1)
		_ReflectColor("Reflect Color", Color) = (1,1,1,1)
		_ReflectLevel("Reflect Level", Range(0,1)) = 0.5
		_FadeColor("Fade Color",Color) = (1,1,1,1)
		_ReflectionBrightness("Reflection Brightness", Range(0, 1)) = 0.5 
		_Direction ("Waves Direction", Vector) = (1.0 ,1.0, -1.0, -1.0)
		_TexturesScale("Textures Scale", Float) = 1
		//Gerstner Waves
		_Amplitude ("Wave Amplitude", Vector) = (0.05 ,0.05, 0.02, 0.03)
		_Speed ("Wave Speed", Vector) = (1, 1.5, 1, 0.2)
		_Frequency ("Wave Frequency", Vector) = (5, -7, 2, 3)
		_Steepness ("Wave Steepness", Vector) = (0.5, 0.05, 0.5, 0.23)
		_DirectionAB ("Wave Direction1", Vector) = (0.3 ,0.5, 0.85, 0.25)
		_DirectionCD ("Wave Direction2", Vector) = (0.1 ,0.9, 0.5, 0.5)	
		_WaveScale("Waves Scale", Float) = 1
		_Direction ("Waves Direction 1 & 2", Vector) = (0.05 ,0.3, 15, -3)
		_FoamDirection ("Foam Direction R & G Chanell", Vector) = (0 ,5, -1.0, -1.0)
		_FadeDepth  ("Fade Depth", Float) = 1
		_DistortionVert  ("Vertex Distortion", Float) = 2
		_Bias("Bias Glare", Float) = -1
		_Wave1 ("Wave1 Distortion Texture", 2D) = "bump" {}
		_Wave2 ("Wave2 Distortion Texture", 2D) = "bump" {}
		_Foam ("Foam Texture", 2D) = "white" {}
		_FadeBlend  ("Fade Blend Foam", Float) = 1
		_FadeBlend2  ("Fade Blend Transparency", Float) = 1
		_FadeDepth  ("Fade Depth", Float) = 1
		//Fresnel
		_OffsetFresnel("Offset Fresnel", Float) = 0.1
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
			float4 _Amplitude;
			float4 _Frequency;
			float4 _Steepness; 									
			float4 _Speed;					
			float4 _DirectionAB;		
			float4 _DirectionCD;
			float4 _Direction;
			float4 _FoamDirection;
			float _ReflectionBrightness;
			sampler2D _Wave1;
			sampler2D _Wave2;
			sampler2D _Foam;
			float _FadeBlend;
			float _FadeBlend2;
			float _FadeDepth;
			float4 _Wave1_ST;
			float4 _Wave2_ST;
			float4 _Foam_ST;
			float _OffsetFresnel;
			float _DistortionVert;
			float _TexturesScale;
			float4x4 _projectiveMatrWaves;
			struct v2f {
				float4 vertex : POSITION;//done
				float4 uvgrab : TEXCOORD0;//done
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
			#if UNITY_UV_STARTS_AT_TOP
				float scale = -1.0;
			#else
				float scale = 1.0;
			#endif
			//Gerstner Waves
			half4 directionAB = _Steepness.xxyy * _Amplitude.xxyy * _DirectionAB.xyzw;
			half4 directionCD = _Steepness.zzww * _Amplitude.zzww * _DirectionCD.xyzw;
			half4 time = fmod(_Time.y * _Speed, 6.2831);
			half4 CosParam = cos(directionAB*directionCD+time);
			half4 SinParam = sin(directionAB*directionCD+time);
			half3 offsets;
			offsets.x = dot(CosParam,half4(directionAB.xz, directionCD.xz));
			offsets.y = dot(SinParam,_Amplitude);
			offsets.z = dot(CosParam,half4(directionAB.yw, directionCD.yw));
			
			float4 oPos = UnityObjectToClipPos(v.vertex);
			float3 posWorld = mul(unity_ObjectToWorld, v.vertex).xyz;			
			o.uvgrab.xy = (float2(oPos.x, oPos.y*scale) + oPos.w) * 0.5;
			o.uvgrab.zw = oPos.zw;
			v.vertex.xyz += offsets;
			o.vertex = UnityObjectToClipPos(v.vertex);
			float2 time1 = _Time.xx * _Direction.xy;
			float2 time2 = _Time.xx * _Direction.zw;
			float2 time3 = _Time.xx * _FoamDirection.xy;
			half2 scaleeUv = -posWorld.xz / _TexturesScale;
			o.uvWave1.xy = scaleeUv * _Wave1_ST.xy + _Wave1_ST.w  + time1;
			o.uvWave2.xy = scaleeUv * _Wave2_ST.xy + _Wave2_ST.w  + time2;
			o.uvWave1.z = offsets.y;
			o.uvWave2.z = 0;
			o.uvFoam = scaleeUv * _Foam_ST.xy + _Foam_ST.w  + time3;
			oPos += o.vertex*_DistortionVert;
			float angle = dot(normalize(posWorld - _WorldSpaceCameraPos.xyz),
				normalize(mul((float3x3)(unity_ObjectToWorld), v.normal).xyz));
			o.viewDir.w = abs(1.0 + angle>0?-1:angle);		
			o.viewDir.xyz = normalize(WorldSpaceViewDir(o.vertex));
			o.screenPos = ComputeScreenPos(oPos);	
			o.screenPosWithoutVert = ComputeScreenPos (o.vertex);
			o.uvgrabDefault.xy = (float2(o.vertex.x, o.vertex.y*scale) + o.vertex.w) * 0.5;
			o.uvgrabDefault.zw = o.vertex.zw;
			return o;
		}
		sampler2D _CameraDepthTexture;
		sampler2D _GrabTexture;
		sampler2D _ReflectionTex;
		float4 _LightColor; 
		half4 frag( v2f i ) : COLOR {
			half4 color;
			//二维投影纹理查询，并进行深度值比较
			half4 grabDefault = tex2Dproj(_GrabTexture, UNITY_PROJ_COORD(i.uvgrabDefault));
			float sceneZDefault = LinearEyeDepth (SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, 
				UNITY_PROJ_COORD(i.screenPosWithoutVert)));
			half2 normal1 = UnpackNormal(tex2D(_Wave1, i.uvWave1.xy)).rg;
			half3 normal2 = UnpackNormal(tex2D(_Wave2, i.uvWave2.xy + normal1));
			half3 normal3 = UnpackNormal(tex2D(_Wave2, i.uvWave2.xy/2-_Time.xx * _Direction.zw + normal2.xy));
			half foam1 = tex2D(_Foam,i.uvFoam);
			half fresnel = pow(1-max(0,normal2*i.viewDir),4);
			float2 offset = normal2.xy*_Direction /1000;
			offset += clamp(i.uvWave2.z*normal1*4 +  
				i.uvWave2.z* i.uvWave2.z*5, 0, 0.4);
			float offsetFadeBlend = saturate ((sceneZDefault - i.screenPos.z));
			offset = pow(offset,3)  + offset/30 + normal1.xy * i.uvWave1.z/8;
			offset *= offsetFadeBlend * offsetFadeBlend;
			i.uvgrab.xy = offset * i.uvgrab.z + i.uvgrab.xy;
			float4 screenPosOffset;
			screenPosOffset.xy = offset * i.uvgrab.z + i.screenPos.xy;
			screenPosOffset.zw = i.screenPos.zw;
			
			float4 screenPosOffsetWithRipples;
			screenPosOffsetWithRipples.xy = (offset +  i.uvWave2.z/3) * i.uvgrab.z + i.screenPos.xy;
			screenPosOffsetWithRipples.zw = i.screenPos.zw;
			half4 reflection = tex2Dproj(_ReflectionTex, UNITY_PROJ_COORD(screenPosOffsetWithRipples));
			reflection = lerp((reflection+_ReflectionBrightness)/2, reflection, 1-_LightColor0.w);
			reflection -= i.uvWave2.z*_LightColor.w/3;
			half4 grab = tex2Dproj(_GrabTexture, UNITY_PROJ_COORD(i.uvgrab));
			color = grab;
			float sceneZ = LinearEyeDepth (SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(screenPosOffset)));
			float deltaZ =  sceneZ-i.screenPos.z;
			float fadeDepth = saturate (_FadeDepth * deltaZ);
			float fadeBlend = saturate (_FadeBlend * deltaZ);
			float fadeBlend2 = saturate (_FadeBlend2 * deltaZ);

			if(sceneZ < i.screenPos.z)
			{	
				fadeDepth = saturate (_FadeDepth * (sceneZDefault - i.screenPos.z));
				fadeBlend = saturate (_FadeBlend * (sceneZDefault - i.screenPos.z));
				fadeBlend2 = saturate (_FadeBlend2 * (sceneZDefault - i.screenPos.z));
				col = grabDefault;
				grab = grabDefault;
			}
			fadeBlend = 1 - fadeBlend;
			half glare = lerp(normal3.r, normal3.g, normal2.x * i.viewDir.w);
			half4 glareColor = _LightColor*_GlareColor * pow(glare,2);
			color = lerp(color * _Color, lerp(color * _FadeColor + color * i.uvWave1.z *_DepthTransperent,
				 _FadeColor*_LightColor, _DepthTransperent), fadeDepth);
			half4 reflectColor = reflection * _ReflectionColor * fresnel;

			return color;
		}
		ENDCG
		}	
	}
	FallBack "Diffuse"
}
