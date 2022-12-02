Shader "BulletTimeShader"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _BTColor("Bullet Time Color", Color) = (1, 1, 1, 0)
    }
    SubShader
    {
        Cull Off
        ZWrite Off
        ZTest Always

        Pass
        {
            CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			#include "Lighting.cginc"

			float4 _CameraWS;//相机的世界坐标
			float4 _MainTex_TexelSize;

			sampler2D _MainTex;
			sampler2D _CameraDepthTexture;
			sampler2D _CameraDepthNormalsTexture;

            float4 _BTColor;

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

            v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv.xy;
                
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
				half4 col = tex2D(_MainTex, i.uv);
                return col * _BTColor;
            }

            ENDCG
        }
    }
    FallBack "Diffuse"
}
