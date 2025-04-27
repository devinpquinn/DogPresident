Shader "Custom/CarpetBlend"
{
    Properties
    {
        _MainTex ("Regular Carpet", 2D) = "white" {}
        _SmoothTex ("Smoothed Carpet", 2D) = "white" {}
        _MaskTex ("Mask", 2D) = "black" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
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

            sampler2D _MainTex;
            sampler2D _SmoothTex;
            sampler2D _MaskTex;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float mask = tex2D(_MaskTex, i.uv).r;
                fixed4 regular = tex2D(_MainTex, i.uv);
                fixed4 smooth = tex2D(_SmoothTex, i.uv);
                return lerp(regular, smooth, mask);
            }
            ENDCG
        }
    }
}
