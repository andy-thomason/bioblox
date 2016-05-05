 Shader "Render Depth Official" {
 SubShader {
     Tags { "RenderType"="Opaque" }
     Pass {
         Fog { Mode Off }
         CGPROGRAM
         #pragma vertex vert
         #pragma fragment frag
         #include "UnityCG.cginc"
 
         struct v2f {
             float4 pos : SV_POSITION;
             float2 depth : TEXCOORD0;
         };
 
         v2f vert (appdata_base v) {
             v2f o;
             o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
             o.depth.x= o.pos.z/o.pos.w;
             return o;
         }
 
         float4 frag(v2f i) : COLOR {
             //UNITY_OUTPUT_DEPTH(i.depth);
        	 float d= i.depth.x;
             return float4(d,d,d,1);
         }
         ENDCG
     }
 }
 }