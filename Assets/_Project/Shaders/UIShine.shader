Shader "DigitPark/UIShine"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        [Header(Shine Settings)]
        _ShineColor ("Shine Color", Color) = (1, 1, 1, 0.8)
        _ShineWidth ("Shine Width", Range(0.01, 0.5)) = 0.1
        _ShineSpeed ("Shine Speed", Range(0, 5)) = 1
        _ShineAngle ("Shine Angle", Range(-1, 1)) = 0.5
        _ShineIntensity ("Shine Intensity", Range(0, 3)) = 1

        [Header(Animation)]
        _ShineProgress ("Shine Progress", Range(-1, 2)) = 0
        _AutoAnimate ("Auto Animate", Range(0, 1)) = 1
        _AnimationDelay ("Animation Delay", Range(0, 10)) = 3

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
            fixed4 _ShineColor;
            float _ShineWidth;
            float _ShineSpeed;
            float _ShineAngle;
            float _ShineIntensity;
            float _ShineProgress;
            float _AutoAnimate;
            float _AnimationDelay;
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

                // Calculate shine position
                float progress = _ShineProgress;
                if (_AutoAnimate > 0.5)
                {
                    // Create looping animation with delay
                    float cycleTime = 1.0 / _ShineSpeed + _AnimationDelay;
                    float t = fmod(_Time.y, cycleTime);
                    progress = t * _ShineSpeed * 3.0 - 1.0;
                }

                // Calculate shine line position
                float2 uv = IN.texcoord;
                float shinePos = uv.x + uv.y * _ShineAngle;

                // Create shine mask
                float dist = abs(shinePos - progress);
                float shine = 1.0 - smoothstep(0, _ShineWidth, dist);
                shine = pow(shine, 2.0) * _ShineIntensity;

                // Only apply shine where there's alpha
                shine *= color.a;

                // Add shine to color
                color.rgb += _ShineColor.rgb * shine * _ShineColor.a;

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
