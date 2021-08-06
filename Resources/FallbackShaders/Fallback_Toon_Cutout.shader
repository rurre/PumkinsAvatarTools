Shader "Fallback/Toon/Cutout Double"
{
    Properties
    {
        [HideInInspector]_MainTex("Texture", 2D) = "white" {}
        [HideInInspector]_Cutoff("Cutoff", Range(0,1)) = 0.5
        [HideInInspector]_Color("Color", Color) = (1,1,1,1)
        [HideInInspector]_EmissionMap("Emission Map", 2D) = "black" {}
        [HideInInspector]_EmissionColor("Emission Color", Color) = (0,0,0,0)
    }
    SubShader
    {
        Tags { "Queue"="AlphaTest" "LightMode"="ForwardBase"}
        AlphaToMask On
        Cull Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #pragma multi_compile_fwdbase
            #include "UnityPBSLighting.cginc"
            #include "AutoLight.cginc"
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : TEXCOORD1;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 worldPos : TEXCOORD1;
                float4 color : TEXCOORD2;
                UNITY_FOG_COORDS(1)
                SHADOW_COORDS(3)
            };

            sampler2D _MainTex;
            sampler2D _EmissionMap;
            float4 _EmissionColor;
            half4 _MainTex_ST;
            float _Cutoff;
            float4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                TRANSFER_SHADOW(o);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                UNITY_LIGHT_ATTENUATION(attenuation, i, i.worldPos.xyz);
                half3 indirectDiffuse = ShadeSH9(float4(0, 0, 0, 1)); // We don't care about anything other than the color from GI, so only feed in 0,0,0, rather than the normal
                half4 lightCol = _LightColor0;

                //If we don't have a directional light or realtime light in the scene, we can derive light color from a slightly modified indirect color.
                int lightEnv = int(any(_WorldSpaceLightPos0.xyz));
                if(lightEnv != 1)
                    lightCol = indirectDiffuse.xyzz * 0.2;

                float4 lighting = lightCol;

                // sample the texture
                float4 albedo = tex2D(_MainTex, TRANSFORM_TEX(i.uv, _MainTex));
                float4 emission = tex2D(_EmissionMap, i.uv) * _EmissionColor;
                half4 final = albedo * (lighting * attenuation + indirectDiffuse.xyzz) + emission;

                clip(albedo.a * _Color.a - 0.5);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return float4(final.rgb, albedo.a);
            }
            ENDCG
        }
    }
    Fallback "Transparent/Cutout/Diffuse"
}
