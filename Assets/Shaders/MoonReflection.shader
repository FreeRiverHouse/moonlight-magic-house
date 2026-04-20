Shader "MoonlightHouse/MoonReflection"
{
    // Animated moon reflection on floor/water surfaces
    Properties
    {
        _Color       ("Base",         Color) = (0.10,0.07,0.20,1)
        _ReflColor   ("Reflection",   Color) = (0.85,0.90,1.00,1)
        _ReflSpeed   ("Ripple Speed", Float) = 0.6
        _ReflScale   ("Ripple Scale", Float) = 4.0
        _ReflIntensity("Intensity",   Range(0,1)) = 0.35
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _Color, _ReflColor;
            float  _ReflSpeed, _ReflScale, _ReflIntensity;

            struct v2f { float4 pos:SV_POSITION; float3 worldPos:TEXCOORD0; };
            v2f vert(appdata_base v)
            {
                v2f o;
                o.pos      = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }
            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.worldPos.xz * _ReflScale;
                // Cheap ripple via sin
                float ripple = sin(uv.x * 3.1 + _Time.y * _ReflSpeed)
                             * sin(uv.y * 2.7 + _Time.y * _ReflSpeed * 0.7);
                ripple = ripple * 0.5 + 0.5;
                return lerp(_Color, _ReflColor, ripple * _ReflIntensity);
            }
            ENDCG
        }
    }
}
