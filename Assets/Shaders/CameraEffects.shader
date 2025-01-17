Shader "Custom/CameraEffects"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _EffectType ("Effect Type", Float) = 0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "IgnoreProjector"="True" "Queue"="Geometry" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            sampler2D _MainTex;
            float _EffectType;

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);

                if (_EffectType == 1) // Effect_A (Grey Theme with Pencil Effect)
                {
                    // Convert color to grayscale
                    float grayscale = dot(col.rgb, float3(0.3, 0.59, 0.11));
                    float edge = abs(grayscale - tex2D(_MainTex, i.uv + float2(0.001, 0.001)).r); // Edge detection for pencil effect

                    // Map grayscale values to Grey-themed colors with pencil effect
                    if (grayscale < 0.25)
                        col.rgb = float3(0.0, 0.0, 0.0); // Black
                    else if (grayscale < 0.5)
                        col.rgb = float3(0.3, 0.3, 0.3) * edge; // Dark Grey with pencil effect
                    else if (grayscale < 0.75)
                        col.rgb = float3(0.6, 0.6, 0.6) * edge; // Medium Grey with pencil effect
                    else
                        col.rgb = float3(0.9, 0.9, 0.9) * edge; // Light Grey with pencil effect
                }
                else if (_EffectType == 2) // Effect_B (Red Theme with Pencil Effect)
                {
                    // Convert color to grayscale
                    float grayscale = dot(col.rgb, float3(0.3, 0.59, 0.11));
                    float edge = abs(grayscale - tex2D(_MainTex, i.uv + float2(0.001, 0.001)).r); // Edge detection for pencil effect

                    // Map grayscale values to Red-themed colors with pencil effect
                    if (grayscale < 0.25)
                        col.rgb = float3(0.0, 0.0, 0.0); // Black
                    else if (grayscale < 0.5)
                        col.rgb = float3(0.5, 0.0, 0.0) * edge; // Dark Red with pencil effect
                    else if (grayscale < 0.75)
                        col.rgb = float3(1.0, 0.2, 0.2) * edge; // Bright Red with pencil effect
                    else
                        col.rgb = float3(1.0, 0.6, 0.6) * edge; // Light Red with pencil effect
                }
                else if (_EffectType == 3) // Effect_C (Green Theme with Pencil Effect)
                {
                    // Convert color to grayscale
                    float grayscale = dot(col.rgb, float3(0.3, 0.59, 0.11));
                    float edge = abs(grayscale - tex2D(_MainTex, i.uv + float2(0.001, 0.001)).r); // Edge detection for pencil effect

                    // Map grayscale values to Green-themed colors with pencil effect
                    if (grayscale < 0.25)
                        col.rgb = float3(0.0, 0.0, 0.0); // Black
                    else if (grayscale < 0.5)
                        col.rgb = float3(0.0, 0.5, 0.0) * edge; // Dark Green with pencil effect
                    else if (grayscale < 0.75)
                        col.rgb = float3(0.2, 1.0, 0.2) * edge; // Bright Green with pencil effect
                    else
                        col.rgb = float3(0.6, 1.0, 0.6) * edge; // Light Green with pencil effect
                }
                else if (_EffectType == 4) // Effect_D (Cyan Theme with Pencil Effect)
                {
                    // Convert color to grayscale
                    float grayscale = dot(col.rgb, float3(0.3, 0.59, 0.11));
                    float edge = abs(grayscale - tex2D(_MainTex, i.uv + float2(0.001, 0.001)).r); // Edge detection for pencil effect

                    // Map grayscale values to Cyan-themed colors with pencil effect
                    if (grayscale < 0.25)
                        col.rgb = float3(0.0, 0.0, 0.0); // Black
                    else if (grayscale < 0.5)
                        col.rgb = float3(0.0, 0.5, 0.5) * edge; // Dark Cyan with pencil effect
                    else if (grayscale < 0.75)
                        col.rgb = float3(0.2, 1.0, 1.0) * edge; // Bright Cyan with pencil effect
                    else
                        col.rgb = float3(0.6, 1.0, 1.0) * edge; // Light Cyan with pencil effect
                }

                return col;
            }
            ENDCG
        }
    }
}
