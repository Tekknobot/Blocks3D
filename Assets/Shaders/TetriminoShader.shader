Shader "Custom/TetriminoShader"
{
    Properties
    {
        _Color ("Base Color", Color) = (1, 1, 1, 1) // Add this line if `_Color` is missing
        _BaseColor ("Base Color", Color) = (1, 1, 1, 1)
        _GlowColor ("Glow Color", Color) = (1, 1, 1, 1)
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
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            fixed4 _BaseColor;
            fixed4 _GlowColor;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return _BaseColor; // You can blend _BaseColor and _GlowColor here if needed
            }
            ENDCG
        }
    }
}
