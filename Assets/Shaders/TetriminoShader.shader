Shader "Custom/TetriminoShader"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (1, 1, 1, 1)
        _TintXPositive ("X+ Tint", Color) = (0.5, 0.45, 0.45, 1)
        _TintXNegative ("X- Tint", Color) = (0.45, 0.5, 0.45, 1)
        _TintYPositive ("Y+ Tint", Color) = (0.45, 0.45, 0.5, 1)
        _TintYNegative ("Y- Tint", Color) = (0.5, 0.45, 0.5, 1)
        _TintZPositive ("Z+ Tint", Color) = (0.45, 0.5, 0.5, 1)
        _TintZNegative ("Z- Tint", Color) = (0.5, 0.5, 0.45, 1)
        _FlashAmount ("Flash Amount", Range(0, 1)) = 0 // Add this property
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL; // For determining the face direction
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldNormal : TEXCOORD0; // To pass the world-space normal
            };

            fixed4 _BaseColor;
            fixed4 _TintXPositive;
            fixed4 _TintXNegative;
            fixed4 _TintYPositive;
            fixed4 _TintYNegative;
            fixed4 _TintZPositive;
            fixed4 _TintZNegative;
            float _FlashAmount; // Flash intensity

            v2f vert (appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);

                // Transform the normal to world space
                o.worldNormal = UnityObjectToWorldNormal(v.normal);

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Determine which face of the cube based on the normal
                fixed4 tint = _BaseColor;

                if (i.worldNormal.x > 0.5)      tint *= _TintXPositive;
                else if (i.worldNormal.x < -0.5) tint *= _TintXNegative;
                else if (i.worldNormal.y > 0.5)  tint *= _TintYPositive;
                else if (i.worldNormal.y < -0.5) tint *= _TintYNegative;
                else if (i.worldNormal.z > 0.5)  tint *= _TintZPositive;
                else if (i.worldNormal.z < -0.5) tint *= _TintZNegative;

                // Blend with white based on _FlashAmount
                tint = lerp(tint, float4(1, 1, 1, 1), _FlashAmount);

                return tint;
            }
            ENDCG
        }
    }
}
