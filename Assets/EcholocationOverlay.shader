Shader "Custom/EcholocationOverlay"
{
    Properties
    {
        _Color ("Echolocation Color", Color) = (1, 1, 1, 1)
        _Center ("Center", Vector) = (0, 0, 0)
        _Radius ("Radius", Float) = 0
        _MainTex ("Base Texture", 2D) = "white" {} // GLB's texture
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha // Alpha blending for transparency
        ZWrite Off // Don’t write to depth buffer to overlay

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float4 _Color;
            float3 _Center;
            float _Radius;
            sampler2D _MainTex;
            float4 _MainTex_ST;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float2 uv : TEXCOORD1;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Sample original texture
                fixed4 baseColor = tex2D(_MainTex, i.uv);

                // Echolocation effect
                float dist = distance(_Center, i.worldPos);
                float val = smoothstep(_Radius - 1.5, _Radius, dist) * (1 - smoothstep(_Radius - 0.1, _Radius, dist));
                fixed4 echoColor = fixed4(_Color.rgb, val * _Color.a);

                // Blend echolocation over base texture
                return lerp(baseColor, echoColor, val);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}