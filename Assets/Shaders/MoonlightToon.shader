Shader "MoonlightHouse/Toon"
{
    Properties
    {
        _MainTex       ("Base Texture",  2D)    = "white" {}
        _Color         ("Base Color",    Color)  = (1,1,1,1)
        _ShadowColor   ("Shadow Color",  Color)  = (0.35,0.2,0.55,1)
        _ShadowThreshold("Shadow Threshold", Range(0,1)) = 0.4
        _OutlineWidth  ("Outline Width", Range(0,0.05))  = 0.01
        _OutlineColor  ("Outline Color", Color)  = (0.1,0.05,0.2,1)
        _EmissionColor ("Emission",      Color)  = (0,0,0,1)
        _EmissionIntensity("Emission Intensity", Float) = 0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }

        // ── Outline pass ────────────────────────────────────────
        Pass
        {
            Name "OUTLINE"
            Cull Front
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float _OutlineWidth;
            float4 _OutlineColor;

            struct v2f { float4 pos : SV_POSITION; };
            v2f vert(appdata_base v)
            {
                v2f o;
                float3 norm = normalize(mul((float3x3)UNITY_MATRIX_IT_MV, v.normal));
                float2 offset = TransformViewToProjection(norm.xy);
                o.pos = UnityObjectToClipPos(v.vertex);
                o.pos.xy += offset * o.pos.z * _OutlineWidth;
                return o;
            }
            fixed4 frag(v2f i) : SV_Target { return _OutlineColor; }
            ENDCG
        }

        // ── Toon shading pass ───────────────────────────────────
        Pass
        {
            Name "TOON"
            Tags { "LightMode"="ForwardBase" }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase
            #include "UnityCG.cginc"
            #include "Lighting.cginc"

            sampler2D _MainTex;
            float4    _MainTex_ST;
            fixed4    _Color;
            fixed4    _ShadowColor;
            float     _ShadowThreshold;
            fixed4    _EmissionColor;
            float     _EmissionIntensity;

            struct v2f
            {
                float4 pos   : SV_POSITION;
                float2 uv    : TEXCOORD0;
                float3 normal : TEXCOORD1;
            };

            v2f vert(appdata_base v)
            {
                v2f o;
                o.pos    = UnityObjectToClipPos(v.vertex);
                o.uv     = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.normal = UnityObjectToWorldNormal(v.normal);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 N = normalize(i.normal);
                float3 L = normalize(_WorldSpaceLightPos0.xyz);
                float NdotL = dot(N, L);
                float light = step(_ShadowThreshold, NdotL);

                fixed4 tex = tex2D(_MainTex, i.uv) * _Color;
                fixed4 col = lerp(tex * _ShadowColor, tex, light);
                col.rgb += _EmissionColor.rgb * _EmissionIntensity;
                return col;
            }
            ENDCG
        }
    }
}
