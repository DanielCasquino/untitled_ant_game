Shader "Custom/FogMask"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _Radius ("Radius", Float) = 0.5
        _Falloff ("Falloff", Float) = 0.2
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        ZWrite Off
        Cull Off
        Lighting Off
        Blend SrcAlpha OneMinusSrcAlpha

        Stencil
        {
            Ref 1
            Comp always
            Pass replace
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            fixed4 _Color;
            float _Radius;
            float _Falloff;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.vertex.xy;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 center = float2(0,0);
                float dist = length(i.uv - center);
                float alpha = alpha = smoothstep(_Radius, _Radius - _Falloff, dist);
                fixed4 col = _Color;
                col.a *= alpha;
                return col;
            }
            ENDCG
        }
    }
}