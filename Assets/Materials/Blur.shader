Shader "Custom/GaussianBlur"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BlurSize ("Blur Size", Float) = 1.0
        _BlurCenter ("Blur Center", Vector) = (0.5, 0.5, 0, 0)
        _BlurRadius ("Blur Radius", Float) = 0.3
        _BlurFalloff ("Blur Falloff", Float) = 0.2
        _VignetteStrength ("Vignette Strength", Float) = 0.5
    }
    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float _BlurSize;
            float4 _BlurCenter;
            float _BlurRadius;
            float _BlurFalloff;
            float _VignetteStrength;

            float4 frag(v2f_img i) : SV_Target
            {
                float2 uv = i.uv;
                float4 original = tex2D(_MainTex, uv);

                // gpt weights tyty
                float w0 = 0.196482f;
                float w1 = 0.296906f;
                float w2 = 0.094595f;
                float w3 = 0.010381f;
                float w4 = 0.000957f;

                float4 blurred = original * w0;
                blurred += tex2D(_MainTex, uv + float2(_BlurSize * _MainTex_TexelSize.x, 0)) * w1;
                blurred += tex2D(_MainTex, uv - float2(_BlurSize * _MainTex_TexelSize.x, 0)) * w1;
                blurred += tex2D(_MainTex, uv + float2(2.0 * _BlurSize * _MainTex_TexelSize.x, 0)) * w2;
                blurred += tex2D(_MainTex, uv - float2(2.0 * _BlurSize * _MainTex_TexelSize.x, 0)) * w2;
                blurred += tex2D(_MainTex, uv + float2(3.0 * _BlurSize * _MainTex_TexelSize.x, 0)) * w3;
                blurred += tex2D(_MainTex, uv - float2(3.0 * _BlurSize * _MainTex_TexelSize.x, 0)) * w3;
                blurred += tex2D(_MainTex, uv + float2(4.0 * _BlurSize * _MainTex_TexelSize.x, 0)) * w4;
                blurred += tex2D(_MainTex, uv - float2(4.0 * _BlurSize * _MainTex_TexelSize.x, 0)) * w4;

                blurred += tex2D(_MainTex, uv + float2(0, _BlurSize * _MainTex_TexelSize.y)) * w1;
                blurred += tex2D(_MainTex, uv - float2(0, _BlurSize * _MainTex_TexelSize.y)) * w1;
                blurred += tex2D(_MainTex, uv + float2(0, 2.0 * _BlurSize * _MainTex_TexelSize.y)) * w2;
                blurred += tex2D(_MainTex, uv - float2(0, 2.0 * _BlurSize * _MainTex_TexelSize.y)) * w2;
                blurred += tex2D(_MainTex, uv + float2(0, 3.0 * _BlurSize * _MainTex_TexelSize.y)) * w3;
                blurred += tex2D(_MainTex, uv - float2(0, 3.0 * _BlurSize * _MainTex_TexelSize.y)) * w3;
                blurred += tex2D(_MainTex, uv + float2(0, 4.0 * _BlurSize * _MainTex_TexelSize.y)) * w4;
                blurred += tex2D(_MainTex, uv - float2(0, 4.0 * _BlurSize * _MainTex_TexelSize.y)) * w4;

                blurred /= (w0 + 4.0 * w1 + 4.0 * w2 + 4.0 * w3 + 4.0 * w4); // no more bloom

                // force circle
                float aspect = _MainTex_TexelSize.y / _MainTex_TexelSize.x;
                float2 delta = uv - _BlurCenter.xy;
                delta.x *= aspect;
                float dist = length(delta);

                float blurAmount = smoothstep(_BlurRadius, _BlurRadius + _BlurFalloff, dist);

                float vignetteStrength = 1 - _VignetteStrength;
                float vignette = lerp(1.0, vignetteStrength, blurAmount); // darken based on blur falloff

                float4 color = lerp(original, blurred, blurAmount);
                color.rgb *= vignette; // zutomayo reference

                return color;
            }
            ENDCG
        }
    }
}