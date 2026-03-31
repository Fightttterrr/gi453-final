Shader "Custom/URP_CRT_Distortion"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Curvature ("Curvature", Range(0, 1)) = 0.1
        _VignetteWidth ("Vignette Width", Range(0, 1)) = 0.1
        _VignetteSmoothness ("Vignette Smoothness", Range(0, 1)) = 0.1
        _BorderColor ("Border Color", Color) = (0,0,0,1)
        _Resolution ("Resolution (XY)", Vector) = (320, 240, 0, 0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        LOD 100

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
            float4 _MainTex_ST;
            float4 _Resolution; // xy = target resolution

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float2 CurveUV(float2 uv)
            {
                float2 centered = uv * 2.0 - 1.0;
                float2 offset = centered.yx / 2.0; 
                float r = length(offset);
                
                // Barrel distortion
                float2 distorted = centered * (1.0 + (dot(offset, offset) * _Curvature));
                
                return distorted * 0.5 + 0.5;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = CurveUV(i.uv);

                // Black Border (Hard Crop)
                if (uv.x < 0.0 || uv.x > 1.0 || uv.y < 0.0 || uv.y > 1.0)
                {
                    return _BorderColor;
                }

                // Pixelation Effect: Quantize UVs based on target resolution
                // If Resolution is set (non-zero), snap UVs to that grid
                if (_Resolution.x > 0 && _Resolution.y > 0)
                {
                    uv = floor(uv * _Resolution.xy) / _Resolution.xy;
                }

                fixed4 col = tex2D(_MainTex, uv);

                // Vignette 
                float2 center = uv * 2.0 - 1.0;
                float dist = length(center);
                float vig = smoothstep(1.0 - _VignetteWidth - _VignetteSmoothness, 1.0 - _VignetteWidth, dist);
                col = lerp(col, _BorderColor, vig);

                return col;
            }
            ENDCG
        }
    }
}
