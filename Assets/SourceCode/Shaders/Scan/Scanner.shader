Shader "ScannerEffect"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_DetailTex("Texture", 2D) = "white" {}
		_ScanDistance("Scan Distance", float) = 0
		_ScanWidth("Scan Width", float) = 10
		_LeadSharp("Leading Edge Sharpness", float) = 10
		_LeadColor("Leading Edge Color", Color) = (1, 1, 1, 0)
		_MidColor("Mid Color", Color) = (1, 1, 1, 0)
		_TrailColor("Trail Color", Color) = (1, 1, 1, 0)
		_HBarColor("Horizontal Bar Color", Color) = (0.5, 0.5, 0.5, 0)
		// edge
		_EdgeOnly ("Edge Only", Float) = 1.0
		[HDR]_EdgeColor ("Edge Color", Color) = (0, 0, 0, 1)
		_BackgroundColor ("Background Color", Color) = (1, 1, 1, 1)
		_SampleDistance ("Sample Distance", Float) = 1.0
		_Sensitivity ("Sensitivity", Vector) = (1, 1, 1, 1)
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

			struct VertIn
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float4 ray : TEXCOORD1;
			};

			struct VertOut
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float2 uv_depth : TEXCOORD1;
				float4 interpolatedRay : TEXCOORD2;


				half2 uvs[5] : TEXCOORD3;
			};

			float4 _MainTex_TexelSize;
			float4 _CameraWS;

			// edge
			fixed _EdgeOnly;
			fixed4 _EdgeColor;
			fixed4 _BackgroundColor;
			// 控制对深度+法线纹理采样时，使用的采样距离，sampleDistance越大，描边越宽
			float _SampleDistance;
			// x分量存放法线阈值，y分量存放深度阈值
			half4 _Sensitivity;
			sampler2D _CameraDepthNormalsTexture;

			VertOut vert(VertIn v)
			{
				VertOut o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv.xy;
				o.uv_depth = v.uv.xy;

				#if UNITY_UV_STARTS_AT_TOP
				if (_MainTex_TexelSize.y < 0)
					o.uv.y = 1 - o.uv.y;
				#endif				

				o.interpolatedRay = v.ray;

				// edge
				half2 uv = v.uv;
				o.uvs[0] = uv;
			
				#if UNITY_UV_STARTS_AT_TOP
				if (_MainTex_TexelSize.y < 0)
					uv.y = 1 - uv.y;
				#endif
				
				o.uvs[1] = uv + _MainTex_TexelSize.xy * half2(1,1) * _SampleDistance;
				o.uvs[2] = uv + _MainTex_TexelSize.xy * half2(-1,-1) * _SampleDistance;
				o.uvs[3] = uv + _MainTex_TexelSize.xy * half2(-1,1) * _SampleDistance;
				o.uvs[4] = uv + _MainTex_TexelSize.xy * half2(1,-1) * _SampleDistance;
				
				return o;
			}

			sampler2D _MainTex;
			sampler2D _DetailTex;
			sampler2D_float _CameraDepthTexture;
			float4 _WorldSpaceScannerPos;
			float _ScanDistance;
			float _ScanWidth;
			float _LeadSharp;
			float4 _LeadColor;
			float4 _MidColor;
			float4 _TrailColor;
			float4 _HBarColor;			

			float4 horizBars(float2 p)
			{
				float arg = (p.y) * 50;
				return 1 - saturate(round(abs(frac(arg) * 2)));
			}

			float4 horizTex(float2 p)
			{
				return tex2D(_DetailTex, float2(p.x * 30, p.y * 40));
			}

			half CheckSame(half4 center, half4 sample) {
				// 直接使用了xy分量，因为只需要比较两个值之间的差异度
				half2 centerNormal = center.xy;
				float centerDepth = DecodeFloatRG(center.zw);
				half2 sampleNormal = sample.xy;
				float sampleDepth = DecodeFloatRG(sample.zw);
				
				// difference in normals
				// do not bother decoding normals - there's no need here
				half2 diffNormal = abs(centerNormal - sampleNormal) * _Sensitivity.x;
				int isSameNormal = (diffNormal.x + diffNormal.y) < 0.1;
				// difference in depth
				float diffDepth = abs(centerDepth - sampleDepth) * _Sensitivity.y;
				// scale the required threshold by the distance
				int isSameDepth = diffDepth < 0.1 * centerDepth;
				
				// return:
				// 1 - if normals and depth are similar enough
				// 0 - otherwise
				return isSameNormal * isSameDepth ? 1.0 : 0.0;
			}

			half4 frag (VertOut i) : SV_Target
			{
				half4 col = tex2D(_MainTex, i.uv);

				float rawDepth = DecodeFloatRG(tex2D(_CameraDepthTexture, i.uv_depth));
				float linearDepth = Linear01Depth(rawDepth);
				float4 wsDir = linearDepth * i.interpolatedRay;
				float3 wsPos = _WorldSpaceCameraPos + wsDir;
				half4 scannerCol = half4(0, 0, 0, 0);

				float dist = distance(wsPos, _WorldSpaceScannerPos);

				if (dist < _ScanDistance && dist > _ScanDistance - _ScanWidth && linearDepth < 1)
				{
					float diff = 1 - (_ScanDistance - dist) / (_ScanWidth);
					half4 edge = lerp(_MidColor, _LeadColor, pow(diff, _LeadSharp));
					scannerCol = lerp(_TrailColor, edge, diff) + horizBars(i.uv) * _HBarColor;
					scannerCol *= diff;
				}

				// before edge color
				half4 bfEdgeCol = col + scannerCol;

				// edge
				half edge = 1.0;
				if (dist < _ScanDistance && linearDepth < 1)
				{
					half4 sample1 = tex2D(_CameraDepthNormalsTexture, i.uvs[1]);
					half4 sample2 = tex2D(_CameraDepthNormalsTexture, i.uvs[2]);
					half4 sample3 = tex2D(_CameraDepthNormalsTexture, i.uvs[3]);
					half4 sample4 = tex2D(_CameraDepthNormalsTexture, i.uvs[4]);
					
					edge *= CheckSame(sample1, sample2);
					edge *= CheckSame(sample3, sample4);
					
					half percent = dist / _ScanDistance;
					edge = lerp(1, edge, percent);
					if (edge > 0.95f)
					{
						edge = 1;
					}
					// fixed4 withEdgeColor = lerp(_EdgeColor, tex2D(_MainTex, i.uv[0]), edge);
					// fixed4 onlyEdgeColor = lerp(_EdgeColor, _BackgroundColor, edge);
				}

				fixed4 withEdgeColor = lerp(_EdgeColor, bfEdgeCol, edge);

				return withEdgeColor;
			}
			ENDCG
		}
	}
}
