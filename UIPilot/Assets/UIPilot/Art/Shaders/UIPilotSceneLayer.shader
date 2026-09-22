// One layer of a menu's scene: a white sprite tinted by the time of day.
//
// The colour blends between four keyframes as the day turns: Night (0), Dawn
// (0.25), Day (0.5) and Dusk (0.75), each with its own alpha, so a layer can fade
// away (stars by day) as well as change colour. Rise moves the layer up at noon
// and down at midnight, for a sun or, with a negative value, a moon.
//
// The hour comes from one global float, _UIPilotTimeOfDay, which the generated
// UIPilot_GameManager sets from its Time Of Day field and its SetTimeOfDay
// method, and the Editor sets for its preview. No layer needs a script.
//
// Otherwise this is Unity's UI/Default: it clips to a RectMask2D, respects
// stencil masks, and takes the Image's colour as a further tint.
Shader "UIPilot/Scene Layer"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite", 2D) = "white" {}

        _Night ("Night", Color) = (1, 1, 1, 1)
        _Dawn  ("Dawn",  Color) = (1, 1, 1, 1)
        _Day   ("Day",   Color) = (1, 1, 1, 1)
        _Dusk  ("Dusk",  Color) = (1, 1, 1, 1)
        _Rise  ("Rise (fraction of height, up at noon)", Range(-1, 1)) = 0

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
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
        Blend One OneMinusSrcAlpha
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

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex        : SV_POSITION;
                fixed4 color         : COLOR;
                float2 texcoord      : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _ClipRect;
            fixed4 _Night, _Dawn, _Day, _Dusk;
            float _Rise;
            float _UIPilotTimeOfDay;

            // The keyframe colour for the hour, eased between neighbours.
            fixed4 ColorForHour(float hour)
            {
                float t = frac(hour) * 4.0;
                float k = floor(t);
                float f = smoothstep(0.0, 1.0, t - k);
                fixed4 a = k < 1.0 ? _Night : k < 2.0 ? _Dawn : k < 3.0 ? _Day : _Dusk;
                fixed4 b = k < 1.0 ? _Dawn  : k < 2.0 ? _Day  : k < 3.0 ? _Dusk : _Night;
                return lerp(a, b, f);
            }

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(v.vertex);
                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                OUT.color = v.color * ColorForHour(_UIPilotTimeOfDay);
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                // Rise: sample lower in the sprite at noon, so the art appears higher.
                float2 uv = IN.texcoord;
                uv.y -= _Rise * -cos(_UIPilotTimeOfDay * 6.2831853);
                float inside = step(0.0, uv.y) * step(uv.y, 1.0);

                half4 color = tex2D(_MainTex, uv) * IN.color;
                color.a *= inside;

                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(color.a - 0.001);
                #endif

                color.rgb *= color.a;
                return color;
            }
            ENDCG
        }
    }
}
