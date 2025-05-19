Shader "Custom/BlendTwoTextures"
{
    Properties
    {
        _MainTex     ("Orijinal Texture", 2D) = "white" {}
        _BlendTex    ("Muz Texture",      2D) = "white" {}
        _BlendFactor ("Blend Factor",     Range(0,1)) = 0
        _Color       ("Tint Color",       Color) = (1,1,1,1)
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

            struct appdata {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };

            struct v2f {
                float2 uv      : TEXCOORD0;
                float4 vertex  : SV_POSITION;
            };

            sampler2D _MainTex, _BlendTex;
            float4   _MainTex_ST, _BlendTex_ST;
            float    _BlendFactor;
            float4   _Color;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv     = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 c1 = tex2D(_MainTex,  i.uv);
                fixed4 c2 = tex2D(_BlendTex, i.uv);
                return lerp(c1, c2, _BlendFactor) * _Color;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
