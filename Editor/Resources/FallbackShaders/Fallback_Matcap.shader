Shader "Fallback/Matcap"
{
	Properties
	{
		[HideInInspector]_MatcapTextureFallbackThisIsARediculousNameSoThatItDoesntGetReplaced("Matcap Texture", 2D) = "white" {}
		[HideInInspector]_MatcapColorTintHolyMolyDontReadThis("Color", Color) = (0,0,0,1)
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" "LightMode"="ForwardBase"}
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "AutoLight.cginc"
			#include "Lighting.cginc"
			#include "UnityCG.cginc"
			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float3 worldPos : TEXCOORD1;
				float3 normal : NORMAL;
			};

			sampler2D _MatcapTextureFallbackThisIsARediculousNameSoThatItDoesntGetReplaced;
			float4 _MatcapColorTintHolyMolyDontReadThis;

			half2 matcapSample(half3 worldUp, half3 viewDirection, half3 normalDirection)
			{
				half3 worldViewUp = normalize(worldUp - viewDirection * dot(viewDirection, worldUp));
				half3 worldViewRight = normalize(cross(viewDirection, worldViewUp));
				half2 matcapUV = half2(dot(worldViewRight, normalDirection), dot(worldViewUp, normalDirection)) * 0.5 + 0.5;
				return matcapUV;
			}

			half3 calcStereoViewDir(half3 worldPos)
			{
				#if UNITY_SINGLE_PASS_STEREO
					half3 cameraPos = half3((unity_StereoWorldSpaceCameraPos[0] + unity_StereoWorldSpaceCameraPos[1])*.5);
				#else
					half3 cameraPos = _WorldSpaceCameraPos;
				#endif
				half3 viewDir = cameraPos - worldPos;
				return normalize(viewDir);
			}

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				o.normal = UnityObjectToWorldNormal(v.normal);

				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				i.normal = normalize(i.normal);

				float3 ambientColor = ShadeSH9(float4(i.normal.xyz, 1));
				float3 viewDir = calcStereoViewDir(i.worldPos);
				float2 matcapUV = matcapSample(float3(0,1,0), viewDir, i.normal);
				fixed4 matcap = tex2D(_MatcapTextureFallbackThisIsARediculousNameSoThatItDoesntGetReplaced, matcapUV);
				float4 color = (_MatcapColorTintHolyMolyDontReadThis + matcap) * ambientColor.xyzz;

				return color;
			}
			ENDCG
		}
	}
}
