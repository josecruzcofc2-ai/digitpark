Shader "DigitPark/UIGlow"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        [Header(Glow Settings)]
        _GlowColor ("Glow Color", Color) = (0, 1, 1, 1)
        _GlowIntensity ("Glow Intensity", Range(0, 5)) = 1.5
        _GlowSize ("Glow Size", Range(0, 0.1)) = 0.02
        _GlowPulseSpeed ("Pulse Speed", Range(0, 10)) = 2
        _GlowPulseAmount ("Pulse Amount", Range(0, 1)) = 0.3

        [Header(Animation)]
        _AnimateGlow ("Animate Glow", Range(0, 1)) = 1

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _GlowColor;
            float _GlowIntensity;
            float _GlowSize;
            float _GlowPulseSpeed;
            float _GlowPulseAmount;
            float _AnimateGlow;
            float4 _MainTex_ST;
            float4 _ClipRect;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                OUT.color = v.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                // Sample main texture
                half4 color = tex2D(_MainTex, IN.texcoord) * IN.color;

                // Calculate pulsing intensity
                float pulse = 1.0;
                if (_AnimateGlow > 0.5)
                {
                    pulse = 1.0 + sin(_Time.y * _GlowPulseSpeed) * _GlowPulseAmount;
                }

                // Sample surrounding pixels for glow
                float glowAlpha = 0;
                float2 offsets[8] = {
                    float2(-1, 0), float2(1, 0), float2(0, -1), float2(0, 1),
                    float2(-1, -1), float2(1, -1), float2(-1, 1), float2(1, 1)
                };

                for (int i = 0; i < 8; i++)
                {
                    float2 offset = offsets[i] * _GlowSize;
                    glowAlpha += tex2D(_MainTex, IN.texcoord + offset).a;
                }
                glowAlpha /= 8.0;

                // Create glow effect
                float glowMask = saturate(glowAlpha - color.a);
                fixed4 glow = _GlowColor * glowMask * _GlowIntensity * pulse;

                // Combine original color with glow
                color.rgb += glow.rgb * glow.a;
                color.a = saturate(color.a + glow.a * 0.5);

                // Apply UI clipping
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);

                #ifdef UNITY_UI_ALPHACLIP
                clip (color.a - 0.001);
                #endif

                return color;
            }
            ENDCG
        }
    }
}
