Shader "Custom/TetriminoShader"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (1, 1, 1, 1)
        _Albedo ("Albedo Texture", 2D) = "white" {} // Albedo texture
        _TintXPositive ("X+ Tint", Color) = (0.5, 0.45, 0.45, 1)
        _TintXNegative ("X- Tint", Color) = (0.45, 0.5, 0.45, 1)
        _TintYPositive ("Y+ Tint", Color) = (0.45, 0.45, 0.5, 1)
        _TintYNegative ("Y- Tint", Color) = (0.5, 0.45, 0.5, 1)
        _TintZPositive ("Z+ Tint", Color) = (0.45, 0.5, 0.5, 1)
        _TintZNegative ("Z- Tint", Color) = (0.5, 0.5, 0.45, 1)
        _FlashAmount ("Flash Amount", Range(0, 1)) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha // Enable transparency blending
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0; // For texture UV mapping
            };

            struct v2f
            {
                float4 pos : SV_POSITION;      // Screen-space position
                float3 worldNormal : TEXCOORD0;
                float2 uv : TEXCOORD1;        // Pass UV coordinates
            };

            sampler2D _Albedo; // Albedo texture sampler
            fixed4 _BaseColor;
            fixed4 _TintXPositive;
            fixed4 _TintXNegative;
            fixed4 _TintYPositive;
            fixed4 _TintYNegative;
            fixed4 _TintZPositive;
            fixed4 _TintZNegative;
            float _FlashAmount;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex); // Clip-space position
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.uv = v.uv; // Pass UV coordinates
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Get texel size for a 512x512 texture
                float2 texelSize = float2(1.0 / 512.0, 1.0 / 512.0);

                // Adjust UVs for orthographic projection
                float2 orthoUV = i.uv * float2(1.0, -1.0); // Flip Y for orthographic consistency

                // Snap UVs to texel centers for pixel-perfect alignment
                float2 snappedUV = floor(orthoUV / texelSize) * texelSize;

                // Sample the albedo texture using snapped UVs
                fixed4 albedo = tex2D(_Albedo, snappedUV);

                // Determine face tint based on world normal
                fixed4 tint = _BaseColor;

                if (i.worldNormal.x > 0.5)      tint = _TintXPositive;
                else if (i.worldNormal.x < -0.5) tint = _TintXNegative;
                else if (i.worldNormal.y > 0.5)  tint = _TintYPositive;
                else if (i.worldNormal.y < -0.5) tint = _TintYNegative;
                else if (i.worldNormal.z > 0.5)  tint = _TintZPositive;
                else if (i.worldNormal.z < -0.5) tint = _TintZNegative;

                // Handle transparency:
                // Opaque areas use albedo + tint
                // Transparent areas fall back to base color
                fixed4 finalColor = albedo.a > 0 ? (albedo * tint) : _BaseColor;

                // Blend with white based on _FlashAmount
                finalColor = lerp(finalColor, float4(1, 1, 1, 1), _FlashAmount);

                // Set alpha to fully opaque
                finalColor.a = 1.0;

                return finalColor;
            }
            ENDCG
        }
    }
}
