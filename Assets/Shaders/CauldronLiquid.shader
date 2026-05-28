// CauldronLiquid.shader
// URP Unlit shader for a cauldron liquid surface mesh.
//
// Features:
//   - Dual-layer counter-rotating swirl (normal maps)
//   - Ripple ring expanding outward after a splash
//   - Boil distortion (vertex displacement + chromatic wobble)
//   - Fresnel rim glow
//   - Emission for a glowing magical look
//
// Driven at runtime by CauldronController.cs via:
//   liquidMaterial.SetFloat / SetColor
//
// Setup:
//   1. Create a Material using this shader.
//   2. Assign it to the flat quad / disc mesh inside your cauldron.
//   3. Assign a seamless normal map to _NormalMap (e.g. a water normal).
//   4. Optionally assign a second normal map to _NormalMapB for variety.

Shader "Custom/CauldronLiquid"
{
    Properties
    {
        // ── Color ──────────────────────────────────────────────────────────
        [HDR] _MixColor       ("Liquid color",            Color)  = (0.1, 0.6, 0.4, 1)
        [HDR] _EmissionColor  ("Emission / glow color",   Color)  = (0.05, 0.4, 0.25, 1)
        _EmissionStrength     ("Emission strength",        Float)  = 1.2
        _FresnelColor         ("Fresnel rim color",        Color)  = (0.3, 1.0, 0.6, 1)
        _FresnelPower         ("Fresnel power",            Float)  = 3.0

        // ── Normal maps ────────────────────────────────────────────────────
        _NormalMap            ("Normal map A",             2D)     = "bump" {}
        _NormalMapB           ("Normal map B (optional)",  2D)     = "bump" {}
        _NormalStrength       ("Normal blend strength",    Float)  = 0.6

        // ── Swirl ──────────────────────────────────────────────────────────
        // Controlled by CauldronController at runtime.
        _SwirlSpeed           ("Swirl speed",              Float)  = 0.4
        _SwirlTiling          ("Swirl tiling",             Float)  = 1.8

        // ── Ripple (splash) ────────────────────────────────────────────────
        // _RippleStrength is set to 1 on splash, fades to 0 via coroutine.
        _RippleStrength       ("Ripple strength",          Float)  = 0.0
        _RippleSpeed          ("Ripple expand speed",      Float)  = 1.2
        _RippleWidth          ("Ripple ring width",        Float)  = 0.08
        _RippleColor          ("Ripple color",             Color)  = (0.6, 1.0, 0.8, 1)

        // ── Boil distortion ────────────────────────────────────────────────
        // _BoilDistortion driven 0→0.3 by CauldronController.SetHeat().
        _BoilDistortion       ("Boil distortion",          Float)  = 0.0
        _BoilSpeed            ("Boil noise speed",         Float)  = 1.5
        _BoilTiling           ("Boil noise tiling",        Float)  = 3.0

        // ── Transparency ───────────────────────────────────────────────────
        _Alpha                ("Surface opacity",          Range(0,1)) = 0.92
    }

    SubShader
    {
        Tags
        {
            "RenderType"      = "Transparent"
            "Queue"           = "Transparent"
            "RenderPipeline"  = "UniversalPipeline"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Back

        Pass
        {
            Name "CauldronLiquidPass"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            // ── Samplers ──────────────────────────────────────────────────────
            TEXTURE2D(_NormalMap);   SAMPLER(sampler_NormalMap);
            TEXTURE2D(_NormalMapB);  SAMPLER(sampler_NormalMapB);

            // ── Uniforms ──────────────────────────────────────────────────────
            CBUFFER_START(UnityPerMaterial)
                float4 _MixColor;
                float4 _EmissionColor;
                float  _EmissionStrength;
                float4 _FresnelColor;
                float  _FresnelPower;
                float  _NormalStrength;
                float  _SwirlSpeed;
                float  _SwirlTiling;
                float  _RippleStrength;
                float  _RippleSpeed;
                float  _RippleWidth;
                float4 _RippleColor;
                float  _BoilDistortion;
                float  _BoilSpeed;
                float  _BoilTiling;
                float  _Alpha;
            CBUFFER_END

            // ── Vertex input / output ─────────────────────────────────────────
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS  : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float3 normalWS    : TEXCOORD1;
                float3 viewDirWS   : TEXCOORD2;
                float  fogFactor   : TEXCOORD3;
            };

            // ── Helpers ───────────────────────────────────────────────────────

            // Rotate a 2D UV around the center (0.5, 0.5) by angle (radians).
            float2 RotateUV(float2 uv, float angle)
            {
                float2 centered = uv - 0.5;
                float  s = sin(angle);
                float  c = cos(angle);
                float2 rotated = float2(
                    c * centered.x - s * centered.y,
                    s * centered.x + c * centered.y
                );
                return rotated + 0.5;
            }

            // Simple hash-based value noise (no texture needed).
            float Hash(float2 p)
            {
                p = frac(p * float2(127.1, 311.7));
                p += dot(p, p + 19.19);
                return frac(p.x * p.y);
            }

            float ValueNoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                float2 u = f * f * (3.0 - 2.0 * f); // smoothstep

                return lerp(
                    lerp(Hash(i),             Hash(i + float2(1, 0)), u.x),
                    lerp(Hash(i + float2(0,1)), Hash(i + float2(1, 1)), u.x),
                    u.y
                );
            }

            // ── Vertex shader ─────────────────────────────────────────────────
            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                // Boil: displace surface vertices vertically using noise
                float3 posOS = IN.positionOS.xyz;
                if (_BoilDistortion > 0.001)
                {
                    float2 boilUV   = posOS.xz * _BoilTiling + _Time.y * _BoilSpeed * float2(0.3, 0.7);
                    float  boilNoise = ValueNoise(boilUV) * 2.0 - 1.0; // -1..1
                    posOS.z += boilNoise * _BoilDistortion * 0.04;
                }

                VertexPositionInputs posInputs = GetVertexPositionInputs(posOS);
                VertexNormalInputs   nrmInputs = GetVertexNormalInputs(IN.normalOS);

                OUT.positionCS = posInputs.positionCS;
                OUT.uv         = IN.uv;
                OUT.normalWS   = nrmInputs.normalWS;
                OUT.viewDirWS  = GetWorldSpaceViewDir(posInputs.positionWS);
                OUT.fogFactor  = ComputeFogFactor(posInputs.positionCS.z);

                return OUT;
            }

            // ── Fragment shader ───────────────────────────────────────────────
            float4 frag(Varyings IN) : SV_Target
            {
                float2 uv = IN.uv;
                float  t  = _Time.y;

                // ── Swirl: two counter-rotating normal map layers ──────────────
                float  angleA  =  t * _SwirlSpeed;
                float  angleB  = -t * _SwirlSpeed * 0.7; // slightly different speed

                float2 uvA = RotateUV(uv * _SwirlTiling, angleA);
                float2 uvB = RotateUV(uv * _SwirlTiling * 1.3 + 0.5, angleB);

                // Add gentle scroll on top of the rotation for organic feel
                uvA += float2( t * 0.04,  t * 0.03);
                uvB += float2(-t * 0.02, -t * 0.05);

                float3 normalA = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap,  sampler_NormalMap,  uvA));
                float3 normalB = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMapB, sampler_NormalMapB, uvB));

                // Blend the two normal layers
                float3 blendedNormal = normalize(float3(
                    normalA.xy + normalB.xy,
                    normalA.z  * normalB.z
                ) * float3(_NormalStrength, _NormalStrength, 1.0));

                // ── Boil: additional chromatic distortion in UV space ──────────
                float2 boilOffset = float2(0, 0);
                if (_BoilDistortion > 0.001)
                {
                    float2 boilUV  = uv * _BoilTiling + t * _BoilSpeed * float2(0.2, 0.4);
                    float  boilN   = ValueNoise(boilUV) * 2.0 - 1.0;
                    boilOffset     = float2(boilN, ValueNoise(boilUV + 3.7)) * _BoilDistortion * 0.04;
                }

                // ── Base color with normal-driven specular highlight ───────────
                float3 viewDir = normalize(IN.viewDirWS);
                float3 reflDir = reflect(-viewDir, normalize(blendedNormal + float3(0, 0, 1)));

                // Simple Blinn-Phong-like highlight from the view direction
                float highlight = pow(saturate(dot(reflDir, viewDir)), 32.0) * 0.4;

                float3 baseColor = _MixColor.rgb + highlight;

                // ── Emission ──────────────────────────────────────────────────
                // Slightly pulse emission with noise for a magical shimmer
                float2 emisUV    = uv * 2.5 + boilOffset + t * 0.1;
                float  emisNoise = ValueNoise(emisUV) * 0.3 + 0.7; // 0.7..1.0
                float3 emission  = _EmissionColor.rgb * _EmissionStrength * emisNoise;

                // ── Fresnel rim ───────────────────────────────────────────────
                float  NdotV    = saturate(dot(normalize(IN.normalWS), viewDir));
                float  fresnel  = pow(1.0 - NdotV, _FresnelPower);
                float3 rimColor = _FresnelColor.rgb * fresnel;

                // ── Ripple ring (expands outward from center after splash) ─────
                float3 rippleContrib = float3(0, 0, 0);
                if (_RippleStrength > 0.001)
                {
                    float2 centered    = uv - 0.5;
                    float  dist        = length(centered);
                    // Ring front expands over time, modulated by strength (1→0 as it fades)
                    float  ringFront   = (1.0 - _RippleStrength) * _RippleSpeed;
                    float  ringEdge    = abs(dist - ringFront);
                    float  ringMask    = 1.0 - smoothstep(0.0, _RippleWidth, ringEdge);
                    ringMask          *= _RippleStrength;
                    rippleContrib      = _RippleColor.rgb * ringMask;
                }

                // ── Compose final color ───────────────────────────────────────
                float3 finalColor = baseColor + emission + rimColor + rippleContrib;

                // Fog
                finalColor = MixFog(finalColor, IN.fogFactor);

                // Alpha: base opacity + fresnel edge opacity boost
                float alpha = _Alpha + fresnel * 0.05;
                alpha       = saturate(alpha);

                return float4(finalColor, alpha);
            }

            ENDHLSL
        }
    }

   //FallbackError "Hidden/InternalErrorShader";
}
