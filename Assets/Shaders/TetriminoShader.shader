Shader "Custom/TetriminoShader"
{
    Properties
    {
        _Color ("Base Color", Color) = (1, 1, 1, 1) // Add this line if `_Color` is missing

        _BaseColor ("Base Color", Color) = (1, 1, 1, 1)
        _TintXPositive ("X+ Tint", Color) = (0.5, 0.45, 0.45, 1) // 50% darker red tint
        _TintXNegative ("X- Tint", Color) = (0.45, 0.5, 0.45, 1) // 50% darker green tint
        _TintYPositive ("Y+ Tint", Color) = (0.45, 0.45, 0.5, 1) // 50% darker blue tint
        _TintYNegative ("Y- Tint", Color) = (0.5, 0.45, 0.5, 1) // 50% darker magenta tint
        _TintZPositive ("Z+ Tint", Color) = (0.45, 0.5, 0.5, 1) // 50% darker cyan tint
        _TintZNegative ("Z- Tint", Color) = (0.5, 0.5, 0.45, 1) // 50% darker yellow tint

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

                return tint;
            }
            ENDCG
        }
    }
}
