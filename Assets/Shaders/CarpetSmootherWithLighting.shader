Shader "Unlit/CarpetSmootherWithLighting"
{
    Properties
    {
        _MainTex ("Regular Carpet", 2D) = "white" {}
        _SmoothTex ("Smoothed Carpet", 2D) = "white" {}
        _MaskTex ("Mask", 2D) = "white" {}
        _LightDirection ("Light Direction", Vector) = (0,1,0,0)
        _LightStrength ("Light Strength", Float) = 0.5
        _DepthStrength ("Depth Strength", Float) = 2.0
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

            sampler2D _MainTex;
            sampler2D _SmoothTex;
            sampler2D _MaskTex;
            float4 _LightDirection;
            float _LightStrength;
            float _DepthStrength;

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

            float4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;

                // Sample textures
                float4 regularCol = tex2D(_MainTex, uv);
                float4 smoothCol = tex2D(_SmoothTex, uv);
                float mask = tex2D(_MaskTex, uv).r;

                // Blend carpets by mask
                float4 baseColor = lerp(regularCol, smoothCol, mask);

                // Simulate heightmap lighting
                float2 texelSize = float2(1.0/_ScreenParams.x, 1.0/_ScreenParams.y);
                float maskRight = tex2D(_MaskTex, uv + float2(texelSize.x, 0)).r;
                float maskUp = tex2D(_MaskTex, uv + float2(0, texelSize.y)).r;
                float2 gradient = float2(maskRight - mask, maskUp - mask);

                float light = dot(normalize(float3(gradient * _DepthStrength, 1.0)), normalize(_LightDirection.xyz));

                // Make light stronger
                light = saturate(light) * _LightStrength;

                // Apply lighting to base color
                baseColor.rgb *= (1.0 - light);

                return baseColor;
            }
            ENDCG
        }
    }
}
