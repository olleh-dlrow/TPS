Shader "Scanner/ScannerEffect"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
		_Diffuse("Diffuse", Color) = (1, 1, 1, 1)
		_LineOffset1("Line Offset1", float) = 0
		_LineOffset2("Line Offset2", float) = 0
		_DisplayRadio("Disappear Time Radio", float) = 1
        //_ScanDistance("Scan Distance", float) = 0
        //_ScanWidth("Scan Width", float) = 0
        [HDR]_ScanColor("Scan Color", Color) = (1, 1, 1, 0) 
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

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
                float4 srcPos : TEXCOORD1;
				float4 rayDir : TEXCOORD2;
			};

			float4 _CameraWS;//相机的世界坐标
			float4 _MainTex_TexelSize;

			sampler2D _MainTex;
			sampler2D _CameraDepthTexture;
			float _ScanDistance;
			float _ScanWidth;
			float4 _ScanColor;

			sampler2D _CameraDepthNormalsTexture;
			//fixed4 _Diffuse;
			float _LineOffset1;
			float _LineOffset2;
			float _DisplayRadio;

			float4x4 _FarClipRay;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv.xy;

                o.srcPos = ComputeScreenPos(o.vertex);

				// //用texcoord区分四个角，就四个点，if无所谓吧
				// int index = 0;
				// if (v.uv.x < 0.5 && v.uv.y > 0.5)
				// 	index = 0;
				// else if (v.uv.x > 0.5 && v.uv.y > 0.5)
				// 	index = 1;
				// else if (v.uv.x < 0.5 && v.uv.y < 0.5)
				// 	index = 2;
				// else
				// 	index = 3;
				
				// o.rayDir = _FarClipRay[index];


				return o;
			}

			half4 frag (v2f i) : SV_Target
			{
				half4 col = tex2D(_MainTex, i.uv);

				//获取深度信息
				//float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv_depth);
                float depth = tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.srcPos)).r;
				float linear01Depth = Linear01Depth(depth);
				//return half4(linear01Depth, linear01Depth, linear01Depth, 1);

				// 重建世界坐标
				//float3 worldPos = _WorldSpaceCameraPos + linear01Depth * i.rayDir.xyz;
				//float pixelDistance = distance(worldPos, float3(0, 0, 0));

				// 获得法线信息
				// fixed3 viewNormal=DecodeViewNormalStereo(tex2D(_CameraDepthNormalsTexture,i.uv));

				// fixed3 worldNormal = mul(viewNormal, (float3x3)UNITY_MATRIX_V);

				// 计算漫反射
				// fixed3 worldLightDir = normalize(_WorldSpaceLightPos0.xyz);

				// fixed3 diffuse = _LightColor0.rgb * _Diffuse.rgb * saturate(dot(worldNormal, worldLightDir));

				// fixed4 ambient = UNITY_LIGHTMODEL_AMBIENT;

// # start
				//_LineOffset1决定密度,_LineOffset2决定宽度
				float edgeCircleMask = saturate(round(sin(linear01Depth * _LineOffset1) + _LineOffset2));
				//edgeCircleMask = 1.0 - edgeCircleMask;

				if (linear01Depth < _ScanDistance && linear01Depth < 1)
				{
					// 计算扫描线的颜色
					fixed4 res = col;

					float t = saturate(1.0 - (_ScanDistance - linear01Depth) * _DisplayRadio);
					fixed4 lineColor = t * _ScanColor * edgeCircleMask;
					res += lineColor;
					
					// 计算扫描范围的颜色
					if(linear01Depth > _ScanDistance - _ScanWidth)
					{
						float diff = 1 - (_ScanDistance - linear01Depth) / (_ScanWidth);
						fixed4 color = _ScanColor;// + fixed4(diffuse + ambient, 1.0);
						color *= diff;	
						res += color;					
					}
					return res;
				}
				// if (linear01Depth < _ScanDistance && linear01Depth > _ScanDistance - _ScanWidth && linear01Depth < 1)
				// {
				// 	float diff = 1 - (_ScanDistance - linear01Depth) / (_ScanWidth);
				// 	fixed4 color = _ScanColor;// + fixed4(diffuse + ambient, 1.0);
				// 	color *= diff;
					
				// 	return col + color + edgeCircleMask * _ScanColor;
				// }

				return col;
// # end
			}

            ENDCG
        }
    }
}