Shader "MoonlightHouse/GlowPulse"
{
    // Animated emission glow — used on interactable objects and the pet when happy
    Properties
    {
        _MainTex     ("Base",        2D)    = "white" {}
        _Color       ("Base Color",  Color)  = (1,1,1,1)
        _GlowColor   ("Glow Color",  Color)  = (0.6,0.3,1,1)
        _GlowMin     ("Glow Min",    Float)  = 0.5
        _GlowMax     ("Glow Max",    Float)  = 2.5
        _PulseSpeed  ("Pulse Speed", Float)  = 1.8
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            Tags { "LightMode"="ForwardBase" }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex; float4 _MainTex_ST;
            fixed4 _Color, _GlowColor;
            float  _GlowMin, _GlowMax, _PulseSpeed;

            struct v2f { float4 pos:SV_POSITION; float2 uv:TEXCOORD0; };
            v2f vert(appdata_base v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv  = TRANSFORM_TEX(v.texcoord, _MainTex);
                return o;
            }
            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col  = tex2D(_MainTex, i.uv) * _Color;
                float  pulse = _GlowMin + (_GlowMax - _GlowMin) *
                               (0.5 + 0.5 * sin(_Time.y * _PulseSpeed));
                col.rgb += _GlowColor.rgb * pulse;
                return col;
            }
            ENDCG
        }
    }
}
