Shader "WaterShader/Water" {
	Properties {
		_Color ("Main Color", Color) = (1,1,1,1)
		_GlareColor ("Glare Color", Color) = (1,1,1,1)
		_Bias("Bias Glare", Float) = -1
		_Scale("Scale Glare", Float) = 10
		_FadeColor ("Fade Color", Color) = (.9,.9,1,1)
		_ReflectionColor ("Reflection Color", Color) = (1,1,1,1)
		_ReflectionBrightness("Reflection Brightness", Range(0, 1)) = 0.5 

		_Wave1 ("Wave1 Distortion Texture", 2D) = "bump" {}
		_Direction ("Waves Direction 1 & 2", Vector) = (1.0 ,1.0, -1.0, -1.0)
		_Wave2 ("Wave2 Distortion Texture", 2D) = "bump" {}
		_Foam ("Foam Texture", 2D) = "white" {}
		_FoamDirection ("Foam Direction R & G Chanell", Vector) = (1.0 ,1.0, -1.0, -1.0)
		_FoamIntensity("Foam Intensity", Float) = 1
		_FadeBlend1  ("Fade Blend Foam", Float) = 1
		_FadeBlend2  ("Fade Blend Transparency", Float) = 1
		_FadeDepth  ("Fade Depth", Float) = 1
		_TransperentDepth  ("Depth Transperent", Range(0, 1)) = 0
		_Distortion  ("Distortion Normal", Float) = 400
		_DistortionVert  ("Per Vertex Distortion", Float) = 1
		
		_Amplitude ("Wave Amplitude", Vector) = (0.1 ,0.3, 0.2, 0.15)
		_Frequency ("Wave Frequency", Vector) = (0.6, 0.5, 0.5, 1.8)
		_Steepness ("Wave Steepness", Vector) = (1.0, 2.0, 1.5, 1.0)
		_Speed ("Wave Speed", Vector) = (-0.23, -1.25, -3.0, 1.5)
		_DirectionAB ("Wave Direction", Vector) = (0.3 ,0.5, 0.85, 0.25)
		_DirectionCD ("Wave Direction", Vector) = (0.1 ,0.9, 0.5, 0.5)	
		_WaveScale("Waves Scale", Float) = 1
		_TexturesScale("Textures Scale", Float) = 1
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
			#pragma multi_compile editor_on editor_off
			#pragma multi_compile ripples_off ripples_on
			#include "UnityCG.cginc"

			float4 _Color;
			float4 _GlareColor;
			float _Bias;
			float _Scale;
			float4 _FadeColor;
			float4 _ReflectionColor;
			float _ReflectionBrightness;
			//Gerstner Waves
			float4 _Amplitude;
			float4 _Frequency;
			float4 _Steepness; 									
			float4 _Speed;					
			float4 _DirectionAB;		
			float4 _DirectionCD;
			sampler2D _Wave1;
			sampler2D _Wave2;
			sampler2D _Foam;
			float _FoamIntensity;
			float4 _Wave1_ST;
			float4 _Wave2_ST;
			float4 _Foam_ST;
			float4 _Direction;
			float _FadeBlend1;
			float _FadeBlend2;
			float _FadeDepth;
			float _TransperentDepth;
			float _Distortion;
			float _DistortionVert;
			float _WaveScale;
			float _TexturesScale;

			sampler2D _GrabTexture;
			float4 _FoamDirection;
			
			sampler2D _WaterDisplacementTexture;
			float4x4 _projectiveMatrWaves;

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
			v2f vert (appdata_tan v)
			{
				v2f o;
				#if UNITY_UV_STARTS_AT_TOP
					float scale = -1.0;
				#else
					float scale = 1.0;
				#endif
				float4 oPos = UnityObjectToClipPos(v.vertex);
				float3 posWorld = mul(unity_ObjectToWorld, v.vertex).xyz;
				//Gerstner Waves
				half3 offsets;
				half4 AB = _Steepness.xxyy * _Amplitude.xxyy * _DirectionAB.xyzw;
				half4 CD = _Steepness.zzww * _Amplitude.zzww * _DirectionCD.xyzw;
				half4 dotABCD = _Frequency.xyzw * half4(dot(_DirectionAB.xy, posWorld.xz), 
						dot(_DirectionAB.zw, posWorld.xz), dot(_DirectionCD.xy, posWorld.xz), 
						dot(_DirectionCD.zw,  posWorld.xz));
				half4 TIME = fmod(_Time.y * _Speed, 6.2831);
				half4 CosParam = cos (dotABCD + TIME);
				half4 SinParam = sin (dotABCD + TIME);
				offsets.x = dot(CosParam, half4(AB.xz, CD.xz));
				offsets.y = dot(CosParam, half4(AB.yw, CD.yw));
				offsets.z = dot(SinParam, _Amplitude);
				half2 scaleeUv = -posWorld.xz / _TexturesScale;
				o.uvWave2.z = 0;
				#if ripples_on
					float2 texDisp = tex2Dlod(_WaterDisplacementTexture, float4(mul(_projectiveMatrWaves, v.vertex).xz, 0, 0)).rg;
					float2 displ = texDisp.r + texDisp.g;
					v.vertex.y += displ;
					o.uvWave2.z = (displ*displ * 2 + displ) * 1.5 + offsets.y / 10;
				#endif
				v.vertex.xyz += offsets;
				o.vertex = UnityObjectToClipPos(v.vertex);

				o.uvgrabDefault.xy = (float2(o.vertex.x, o.vertex.y*scale) + o.vertex.w) * 0.5;
				o.uvgrabDefault.zw = o.vertex.zw;

				oPos += o.vertex*_DistortionVert;
				o.uvgrab.xy = (float2(oPos.x, oPos.y*scale) + oPos.w) * 0.5;
				o.uvgrab.zw = oPos.zw;
				
				float angle = dot(normalize(posWorld - _WorldSpaceCameraPos.xyz),
				normalize(mul((float3x3)(unity_ObjectToWorld), v.normal).xyz));
				o.viewDir.w = pow(abs(1.0 + angle>0?-1:angle),1)*_Scale + _Bias;		
				o.viewDir.xyz = normalize(WorldSpaceViewDir(o.vertex));
				o.screenPos = ComputeScreenPos(oPos);	
				o.screenPosWithoutVert = ComputeScreenPos (o.vertex);

				float2 time1 = _Time.xx * _Direction.xy;
				float2 time2 = _Time.xx * _Direction.zw;
				float2 time3 = _Time.xx * _FoamDirection.xy;
				
				o.uvWave1.xy = scaleeUv * _Wave1_ST.xy + _Wave1_ST.w  + time1;
				o.uvWave2.xy = scaleeUv * _Wave2_ST.xy + _Wave2_ST.w  + time2;
				o.uvFoam = scaleeUv * _Foam_ST.xy + _Foam_ST.w  + time3;
				o.uvWave1.z = offsets.y;
				COMPUTE_EYEDEPTH (o.screenPos.z);
				return o;
			}

			sampler2D _CameraDepthTexture;
			sampler2D _ReflectionTex;
			float4 _LightColor0; 

			half4 frag( v2f i ) : COLOR
			{
				half4 color;
				//二维投影纹理查询，并进行深度值比较
				half4 grabDefault = tex2Dproj(_GrabTexture, UNITY_PROJ_COORD(i.uvgrabDefault));
				float sceneZDefault = LinearEyeDepth (SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, 
						UNITY_PROJ_COORD(i.screenPosWithoutVert)));
				half2 normal1 = UnpackNormal(tex2D(_Wave1, i.uvWave1.xy)).rg;
				half3 normal2 = UnpackNormal(tex2D(_Wave2, i.uvWave2.xy + normal1));
				half3 normal3 = UnpackNormal(tex2D(_Wave2, i.uvWave2.xy/2-_Time.xx * _Direction.zw + normal2.xy));
				half fresnel = pow(1-max(0,normal2*i.viewDir),4);
				float2 offset = normal2.xy * _Distortion /1000;
				#if ripples_on
				offset += clamp(i.uvWave2.z*normal1 * 4 + i.uvWave2.z* i.uvWave2.z * 5, 0, 0.4);
				#endif
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
				#if ripples_on
					reflection -= i.uvWave2.z*_LightColor0.w / 3;
				#endif
				#if editor_on
					reflection = 0;
				#endif
				half4 grab = tex2Dproj(_GrabTexture, UNITY_PROJ_COORD(i.uvgrab));
				color = grab;
				float sceneZ = LinearEyeDepth (SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(screenPosOffset)));
				float deltaZ =  sceneZ-i.screenPos.z;
				float fadeDepth = saturate (_FadeDepth * deltaZ);
				float fadeBlend1 = saturate (_FadeBlend1 * deltaZ);
				float fadeBlend2 = saturate (_FadeBlend2 * deltaZ);
				if(sceneZ < i.screenPos.z)
				{	
					fadeDepth = saturate (_FadeDepth * (sceneZDefault - i.screenPos.z));
					fadeBlend1 = saturate (_FadeBlend1 * (sceneZDefault - i.screenPos.z));
					fadeBlend2 = saturate (_FadeBlend2 * (sceneZDefault - i.screenPos.z));
					color = grabDefault;
					grab = grabDefault;
				}
				fadeBlend1 = 1 - fadeBlend1;
				half glare = lerp(normal3.r, normal3.g, normal2.x * i.viewDir.w);
				half4 glareColor = _LightColor0*_GlareColor * pow(glare,2);
				color = lerp(color * _Color, lerp(color * _FadeColor + color * i.uvWave1.z *_TransperentDepth,
					_FadeColor*_LightColor0, _TransperentDepth), fadeDepth);
				half4 reflectColor = reflection * _ReflectionColor * fresnel;
				
				color += glareColor;
				color += reflectColor;
				color += color * saturate(i.uvWave2.z)/3;

				half foamDefault = tex2D(_Foam,i.uvFoam);
				half4 foam = foamDefault * _LightColor0 * _FoamIntensity;
				foam = (1 - foam)*color + foam;

				color = lerp (color, foam, fadeBlend1);
				color = lerp(grab, color, fadeBlend2);
				UNITY_APPLY_FOG(i.fogCoord, color);
				return color;
		}
		ENDCG
		}
	}
	FallBack "Diffuse"
}