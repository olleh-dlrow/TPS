Shader "Outline"
{
    Properties
    {
        _OutLine ("Outline", float) = 0.5
        _OutLineColor ("OutLineColor", Color) = (1,1,1,1)
    }
    SubShader
    {
		Pass{
			Name "OutLine"
			cull Front //关闭模型正面（不画模型正面）

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"


			float _OutLine;
			fixed4 _OutLineColor;

			struct v2f{
				float4 vertex:SV_POSITION;

			};
			
			v2f vert(appdata_base v)
			{
				v2f o;
				//=============物体空间法线外拓========
				//v.vertex.xyz+=v.normal*_OutLine;
				//o.vertex=UnityObjectToClipPos(v.vertex);

				//=================视角空间法线外拓=================
				//float4 pos=mul(UNITY_MATRIX_V,mul(unity_ObjectToWorld,v.vertex)); //UnityObjectToViewPos(v.vertex);//顶点转到视角
				////使用物体坐标到视角坐标的转置矩阵 
				//float3 normal=normalize(mul((float3x3)UNITY_MATRIX_IT_MV,v.normal));
				//pos=pos+float4(normal,0)*_OutLine;//外拓顶点
				//o.vertex=mul(UNITY_MATRIX_P,pos);//转到裁剪空间

				//=============裁剪空间法线外拓===================
				o.vertex=UnityObjectToClipPos(v.vertex);
				float3 normal=normalize(mul((float3x3)UNITY_MATRIX_IT_MV,v.normal));//转到视角坐标 z值变成了纯粹的深度
				float2 viewNormal=TransformViewToProjection(normal.xy);//转到三角空间
				o.vertex.xy+=viewNormal*_OutLine;

				return o;
			}


			fixed4 frag(v2f i):SV_Target
			{
				return _OutLineColor;
			}

			ENDCG
		
		}
    }
    // FallBack "Diffuse"
}
