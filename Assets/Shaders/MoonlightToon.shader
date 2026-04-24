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
        _Wrap          ("Wrap",          Range(0,1)) = 0.45
        _MidShadow     ("Mid Shadow",    Color)   = (0.75,0.60,0.88,1)
        _RimColor      ("Rim Color",     Color)   = (1.00,0.92,0.75,1)
        _RimPower      ("Rim Power",     Range(0.5,8)) = 3.0
        _SSSColor      ("SSS Color",     Color)   = (1.00,0.55,0.42,1)
        _SSSStrength   ("SSS Strength",  Range(0,1)) = 0.0
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
            fixed4    _MidShadow;
            float     _ShadowThreshold;
            fixed4    _EmissionColor;
            float     _EmissionIntensity;
            float     _Wrap;
            fixed4    _RimColor;
            float     _RimPower;
            fixed4    _SSSColor;
            float     _SSSStrength;

            struct v2f
            {
                float4 pos     : SV_POSITION;
                float2 uv      : TEXCOORD0;
                float3 normal  : TEXCOORD1;
                float3 worldPos: TEXCOORD2;
            };

            v2f vert(appdata_base v)
            {
                v2f o;
                o.pos      = UnityObjectToClipPos(v.vertex);
                o.uv       = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.normal   = UnityObjectToWorldNormal(v.normal);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 N = normalize(i.normal);
                float3 L = normalize(_WorldSpaceLightPos0.xyz);
                float3 V = normalize(_WorldSpaceCameraPos.xyz - i.worldPos);

                // Wrap lighting: softens shadow falloff, gives faux-SSS feel.
                float NdotL = dot(N, L);
                float wrap  = saturate((NdotL + _Wrap) / (1.0 + _Wrap));

                // Three-tone cel: lit | mid | shadow (smoothstep → no harsh 1-px edge)
                float lit = smoothstep(_ShadowThreshold,       _ShadowThreshold + 0.06, wrap);
                float mid = smoothstep(_ShadowThreshold * 0.5, _ShadowThreshold * 0.5 + 0.06, wrap);

                fixed4 tex    = tex2D(_MainTex, i.uv) * _Color;
                fixed3 shadow = tex.rgb * _ShadowColor.rgb;
                fixed3 middle = tex.rgb * _MidShadow.rgb;
                fixed3 col    = lerp(shadow, lerp(middle, tex.rgb, lit), mid);

                // Faux-SSS: back-lit translucent warm glow (wraps around silhouette)
                float back = saturate(dot(-L, V));
                col += _SSSColor.rgb * _SSSStrength * back * (1.0 - wrap);

                // Stylised rim — amber edge pop on silhouette
                float rim = pow(1.0 - saturate(dot(N, V)), _RimPower);
                col += _RimColor.rgb * rim * 0.35 * wrap;

                col += _EmissionColor.rgb * _EmissionIntensity;
                return fixed4(col, tex.a);
            }
            ENDCG
        }
    }
}
