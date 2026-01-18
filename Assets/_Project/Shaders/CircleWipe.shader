Shader "DigitPark/CircleWipe"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (0,0,0,1)

        [Header(Circle Wipe Settings)]
        _Progress ("Progress", Range(0, 1)) = 0
        _Softness ("Edge Softness", Range(0, 0.2)) = 0.02
        _CenterX ("Center X", Range(0, 1)) = 0.5
        _CenterY ("Center Y", Range(0, 1)) = 0.5

        [Header(Style)]
        _Invert ("Invert", Range(0, 1)) = 0

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
            float _Progress;
            float _Softness;
            float _CenterX;
            float _CenterY;
            float _Invert;
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
                // Calculate distance from center
                float2 center = float2(_CenterX, _CenterY);
                float2 uv = IN.texcoord;

                // Correct for aspect ratio (assuming 16:9ish)
                float aspect = 1.78; // Approximate mobile aspect ratio
                uv.x = (uv.x - 0.5) * aspect + 0.5;
                center.x = (center.x - 0.5) * aspect + 0.5;

                float dist = distance(uv, center);

                // Max distance for full coverage (corner to corner)
                float maxDist = 1.5;

                // Calculate circle radius based on progress
                float radius = _Progress * maxDist;

                // Create circle mask with soft edge
                float circle = smoothstep(radius - _Softness, radius + _Softness, dist);

                // Invert if needed
                if (_Invert > 0.5)
                {
                    circle = 1.0 - circle;
                }

                // Apply color
                fixed4 color = IN.color;
                color.a *= circle;

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
