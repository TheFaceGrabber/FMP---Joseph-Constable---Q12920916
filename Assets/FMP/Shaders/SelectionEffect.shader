Shader "Hidden/FMP/SelectionEffect"
{
    Properties
    {
        _Selection ("Texture", 2D) = "white" {}
        _Scene("Texture", 2D) = "white" {}
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _Scene;
            sampler2D _Selection;

            fixed4 frag (v2f i) : SV_Target
            {
               // fixed4 selCol = tex2D(_Selection, i.uv);
                fixed4 col = tex2D(_Scene, i.uv);
                col = fixed4(1, 1, 0, 1);

                //col = saturate(col + saturate(selCol));
                return col;
            }
            ENDCG
        }
    }
}
