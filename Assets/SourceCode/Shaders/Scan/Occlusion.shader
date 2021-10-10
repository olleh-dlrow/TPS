Shader "Scanner/Occlusion"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" { }
        _DiffuseColor ("DiffuseColor", Color) = (0, 0, 0, 0)
        [HDR]_OcclusionColor ("OcclusionColor", Color) = (1, 1, 0, 0)
        _OcclusionPow ("OcclusionPow", Range(0, 2)) = 0.2
        _OcclusionStrength ("OcclusionStrength", Range(1, 4)) = 1
    }
    SubShader
    {
        Tags {
            //"RenderType"="Opaque"  "Queue"="Geometry"
            "Queue"="Transparent" "IgnoreProjector"="True" "RenderType" = "Transparent" 
        }
        LOD 100

        CGINCLUDE
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex: POSITION;
                float2 uv: TEXCOORD0;
                float3 normal: NORMAL;
            };
            
            struct v2f
            {
                float2 uv: TEXCOORD0;
                float4 vertex: SV_POSITION;
                float3 normal: NORMAL;
                float3 viewDir: NORMAL1;
            };
            
            
            float4 _OcclusionColor;
            float _OcclusionPow, _OcclusionStrength;
            
            v2f vert(appdata v)
            {
                v2f o;
                o.normal = UnityObjectToWorldNormal(v.normal);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.viewDir = normalize(WorldSpaceViewDir(o.vertex));;
                UNITY_TRANSFER_FOG(o, o.vertex);
                return o;
            }
            
            fixed4 frag(v2f i): SV_Target
            {
                i.normal = normalize(i.normal);
                float dotVal = pow(1 - saturate(dot(i.viewDir, i.normal)), _OcclusionPow) * _OcclusionStrength;
                return saturate(dotVal + 0.3) * _OcclusionColor;
                //return _OcclusionColor;
            }
        ENDCG
        
        Pass
        {
            //Tags { "Queue" = "Transparent" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZTest Always
            ZWrite Off
            Cull Off
            CGPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag
            

            ENDCG
            
        }

        // Pass
        // {
        //     ZTest Less
        //     CGPROGRAM
        //     // Tags { "RenderType"="Opaque" "Queue"="Geometry"}

        //     #pragma vertex vert
        //     #pragma fragment frag            

        //     ENDCG
        // }
        // Pass
        // {
        //     Tags { "RenderType" = "" }
        //     CGPROGRAM
            
        //     #pragma vertex vert
        //     #pragma fragment frag
            
        //     //#include "UnityCG.cginc"
        //     #include "Lighting.cginc"
            
        //     struct appdata
        //     {
        //         float4 vertex: POSITION;
        //         float2 uv: TEXCOORD0;
        //         float3 normal: NORMAL;
        //     };
            
        //     struct v2f
        //     {
        //         float2 uv: TEXCOORD0;
        //         UNITY_FOG_COORDS(1)
        //         float4 vertex: SV_POSITION;
        //         float3 normal: NORMAL;
        //     };
            
        //     sampler2D _MainTex;
        //     float4 _MainTex_ST;
        //     float4 _DiffuseColor;

            
        //     v2f vert(appdata v)
        //     {
        //         v2f o;
        //         o.normal = UnityObjectToWorldNormal(v.normal);
        //         o.vertex = UnityObjectToClipPos(v.vertex);
        //         o.uv = TRANSFORM_TEX(v.uv, _MainTex);
        //         UNITY_TRANSFER_FOG(o, o.vertex);
        //         return o;
        //     }
            
        //     fixed4 frag(v2f i): SV_Target
        //     {
        //         // sample the texture
        //         float3 lightDir = _WorldSpaceLightPos0.xyz;
        //         float3 lightCol = _LightColor0.xyz;
        //         float3 diffuse = dot(lightDir, i.normal) * lightCol + _DiffuseColor.rgb;
        //         fixed4 col = tex2D(_MainTex, i.uv);
        //         return float4(diffuse * col.rgb, 1);
        //         //return float4(col.rgb, 1);
        //     }
        //     ENDCG
            
        // }
    }

    // 这里居然会影响到屏幕后处理的深度纹理！！！
    Fallback "Transparent/Cutout/VertexLit"
    //Fallback "Diffuse"
}
