Shader "UI/CRTOverlay"
{
    Properties
    {
        _ScanlineIntensity ("Scanline Intensity", Range(0, 1)) = 0.2
        _ScanlineCount ("Scanline Count", Float) = 800
        _ScanlineSpeed ("Scanline Speed", Float) = 1
        _AberrationAmount ("Aberration Amount", Range(0, 0.02)) = 0.005
        _VignetteStrength ("Vignette Strength", Range(0, 1)) = 0.5
        _VignetteSmoothness ("Vignette Smoothness", Range(0, 1)) = 0.2
    }
    SubShader
    {
        Tags { "Queue"="Transparent+100" "RenderType"="Transparent" "IgnoreProjector"="True" }
        
        // Grab the screen behind this object
        GrabPass { }

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
                float4 vertex : SV_POSITION;
                float4 grabPos : TEXCOORD0; // Screen position for GrabPass
                float2 uv : TEXCOORD1;      // Normal UV for vignette/scanlines
            };

            sampler2D _GrabTexture;
            float _ScanlineIntensity;
            float _ScanlineCount;
            float _ScanlineSpeed;
            float _AberrationAmount;
            float _VignetteStrength;
            float _VignetteSmoothness;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.grabPos = ComputeGrabScreenPos(o.vertex);
                o.uv = v.uv; // Use mesh UVs for vignette/scanlines
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 1. Calculate base UV from GrabPosition
                float2 uv = i.grabPos.xy / i.grabPos.w;

                // 2. Chromatic Aberration (Split RGB)
                // Offset R and B channels slightly
                float2 rUV = uv + float2(_AberrationAmount, 0);
                float2 bUV = uv - float2(_AberrationAmount, 0);

                // Sample Background
                fixed4 rCol = tex2D(_GrabTexture, rUV);
                fixed4 gCol = tex2D(_GrabTexture, uv);
                fixed4 bCol = tex2D(_GrabTexture, bUV);

                fixed4 col = fixed4(rCol.r, gCol.g, bCol.b, 1.0);

                // 3. Scanlines
                // Use normal UVs (0..1) or Screen UVs. Let's use Screen UVs for consistent size.
                float scanVal = sin((uv.y * _ScanlineCount) + (_Time.y * _ScanlineSpeed));
                // -1..1 -> 0..1
                float lineVal = 0.5 + 0.5 * scanVal;
                
                // Darken
                col.rgb *= lerp(1.0, lineVal, _ScanlineIntensity);

                // 4. Vignette
                // Use mesh UVs (assuming full screen quad) or screen UVs centered.
                float2 center = i.uv * 2 - 1; // -1..1
                float dist = length(center);
                float vig = smoothstep(_VignetteStrength, _VignetteStrength + _VignetteSmoothness, dist);
                
                // Vignette darkens the result
                col.rgb *= (1.0 - vig);

                return col;
            }
            ENDCG
        }
    }
}
