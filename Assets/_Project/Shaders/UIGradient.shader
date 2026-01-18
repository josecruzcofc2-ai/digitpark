Shader "DigitPark/UIGradient"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        [Header(Gradient Settings)]
        _TopColor ("Top Color", Color) = (1, 0.5, 0, 1)
        _BottomColor ("Bottom Color", Color) = (1, 0, 0.5, 1)
        _GradientAngle ("Gradient Angle", Range(0, 360)) = 0
        _GradientBlend ("Gradient Blend", Range(0, 1)) = 1

        [Header(Animation)]
        _AnimateGradient ("Animate Gradient", Range(0, 1)) = 0
        _AnimationSpeed ("Animation Speed", Range(0, 5)) = 1

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

            #define PI 3.14159265359

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
            fixed4 _TopColor;
            fixed4 _BottomColor;
            float _GradientAngle;
            float _GradientBlend;
            float _AnimateGradient;
            float _AnimationSpeed;
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
                half4 texColor = tex2D(_MainTex, IN.texcoord);

                // Calculate gradient angle
                float angle = _GradientAngle;
                if (_AnimateGradient > 0.5)
                {
                    angle += _Time.y * _AnimationSpeed * 60.0;
                }
                float rad = angle * PI / 180.0;

                // Calculate gradient position based on angle
                float2 uv = IN.texcoord - 0.5;
                float2 dir = float2(cos(rad), sin(rad));
                float gradientPos = dot(uv, dir) + 0.5;

                // Calculate gradient color
                fixed4 gradientColor = lerp(_BottomColor, _TopColor, saturate(gradientPos));

                // Blend gradient with original tint
                fixed4 finalColor = lerp(IN.color, gradientColor, _GradientBlend);
                finalColor *= texColor;

                // Apply UI clipping
                finalColor.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);

                #ifdef UNITY_UI_ALPHACLIP
                clip (finalColor.a - 0.001);
                #endif

                return finalColor;
            }
            ENDCG
        }
    }
}
