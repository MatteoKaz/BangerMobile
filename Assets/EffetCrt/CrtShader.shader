Shader "FullScreen/CRT_Local_Chromatic"
{
    Properties
    {
        _ZoneX              ("Zone X", Range(0,1)) = 0.1
        _ZoneY              ("Zone Y", Range(0,1)) = 0.1
        _ZoneW              ("Zone Width", Range(0,1)) = 0.5
        _ZoneH              ("Zone Height", Range(0,1)) = 0.5

        _CornerRadius       ("Corner Radius", Range(0,0.5)) = 0.1
        _EdgeSoftness       ("Edge Softness", Range(0.001,0.1)) = 0.01

        _ScanlineCount      ("Scanline Count", Float) = 200
        _ScanlineIntensity  ("Scanline Intensity", Range(0,1)) = 0.3
        _ScanlineSpeed      ("Scanline Speed", Float) = 5

        _DistortionStrength ("Distortion Strength", Range(0,0.3)) = 0.05
        _DistortionChromatic("Chromatic Distortion", Range(0,0.05)) = 0.01
        _VignetteIntensity  ("Vignette Intensity", Range(0,3)) = 0.5

        _BlurSize           ("Blur Size", Range(0,0.01)) = 0.002
        _BloomThreshold     ("Bloom Threshold", Range(0,1)) = 0.7
        _BloomIntensity     ("Bloom Intensity", Range(0,3)) = 1.0
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
        ZWrite Off
        Cull Off
        ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Name "CRT_Local_Chromatic"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            float _ZoneX, _ZoneY, _ZoneW, _ZoneH;
            float _CornerRadius, _EdgeSoftness;

            float _ScanlineCount, _ScanlineIntensity, _ScanlineSpeed;
            float _DistortionStrength, _DistortionChromatic;
            float _VignetteIntensity;
            float _BlurSize;
            float _BloomThreshold, _BloomIntensity;

            float RoundedBoxSDF(float2 uv, float2 center, float2 size, float radius)
            {
                float2 d = abs(uv - center) - size + radius;
                return length(max(d, 0.0)) - radius;
            }

            float ZoneMask(float2 uv)
            {
                float2 center = float2(_ZoneX + _ZoneW * 0.5, _ZoneY + _ZoneH * 0.5);
                float2 size = float2(_ZoneW * 0.5, _ZoneH * 0.5);
                float dist = RoundedBoxSDF(uv, center, size, _CornerRadius);
                return 1.0 - smoothstep(0.0, _EdgeSoftness, dist);
            }

            float2 Distort(float2 uv)
            {
                float2 cc = uv - 0.5;
                float dist = dot(cc, cc) * _DistortionStrength;
                return uv + cc * dist;
            }

            half3 Blur(float2 uv)
            {
                half3 col = 0;
                col += SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + float2(-_BlurSize, -_BlurSize)).rgb;
                col += SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + float2(0, -_BlurSize)).rgb;
                col += SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + float2(_BlurSize, -_BlurSize)).rgb;
                col += SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + float2(-_BlurSize, 0)).rgb;
                col += SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv).rgb;
                col += SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + float2(_BlurSize, 0)).rgb;
                col += SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + float2(-_BlurSize, _BlurSize)).rgb;
                col += SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + float2(0, _BlurSize)).rgb;
                col += SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + float2(_BlurSize, _BlurSize)).rgb;
                return col / 9.0;
            }

            half3 Bloom(float2 uv, half3 col)
            {
                half brightness = dot(col, half3(0.2126, 0.7152, 0.0722));
                half3 bright = col * max(0, brightness - _BloomThreshold);
                half3 blurred = Blur(uv);
                return col + blurred * bright * _BloomIntensity;
            }

            half Scanlines(float2 uv)
            {
                float s = sin(uv.y * _ScanlineCount + _Time.y * _ScanlineSpeed) * 0.5 + 0.5;
                return 1.0 - pow(s, 2.0) * _ScanlineIntensity;
            }

            half Vignette(float2 uv)
            {
                float2 v = uv - 0.5;
                return 1.0 - dot(v, v) * _VignetteIntensity;
            }

            half3 ChromaticAberration(float2 uv)
            {
                float2 offsetR = float2(_DistortionChromatic, 0);
                float2 offsetB = float2(-_DistortionChromatic, 0);

                half r = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + offsetR).r;
                half g = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv).g;
                half b = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + offsetB).b;

                return half3(r, g, b);
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float2 uv = input.texcoord;

                half4 original = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv);

                float mask = ZoneMask(uv);

                float2 uvDist = Distort(uv);

                half3 col = ChromaticAberration(uvDist);

                col = lerp(col, Blur(uvDist), saturate(_BlurSize * 200));
                col = Bloom(uvDist, col);
                col *= Scanlines(uvDist);
                col *= Vignette(uvDist);

                half3 finalCol = lerp(original.rgb, col, mask);

                return half4(finalCol, mask);
            }
            ENDHLSL
        }
    }
}