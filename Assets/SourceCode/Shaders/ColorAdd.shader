Shader "Custom/ColorAdd"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex("Texture", 2D) = "white" {}
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

            fixed4 _Color;
            sampler2D _MainTex;

            struct a2v {
                float4 vertex : POSITION;
                float4 uv : TEXCOORD0;
            };

            struct v2f {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(a2v v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv.xy;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 tex = tex2D(_MainTex, i.uv);
                return tex * _Color;
            }

            ENDCG
        }
    }
}
