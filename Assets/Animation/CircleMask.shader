Shader "Unlit/CircleMask"
{
    Properties
    {
        _MainTex("Texture", 2D) = "black" {}
        _Cutoff("Cutoff", Range(0,1)) = 0.5
        _Center("Center", Vector) = (0.5, 0.5, 0, 0)
    }
    SubShader
    {
        Tags { "Queue" = "Overlay" }
        Pass
        {
            ZTest Always Cull Off ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 texcoord : TEXCOORD0;
            };

            sampler2D _MainTex;
            float _Cutoff;
            float4 _Center;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.texcoord;
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                half4 color = half4(0, 0, 0, 1);
                float2 uv = i.texcoord - _Center.xy;
                float dist = length(uv);
                color.a = step(_Cutoff, dist);
                return color;
            }
            ENDCG
        }
    }
        FallBack "Diffuse"
}
