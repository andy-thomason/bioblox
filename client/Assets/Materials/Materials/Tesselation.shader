Shader "Custom/Tesselation" {
	Properties {
		_EdgeLength ("Edge Length", Range(2,50))= 15
		_MainTex ("Base (RGB)", 2D) = "white" {}

		//_DispTex ("Disp Texture", 2D) = "gray" {}
		//_NormalMap("Normal Map", 2D) = "bump" {}

		//_Displacement("Displacement", Range(0,0.5)) = 0.05
		_Color("Color", color) = (1,1,1,0)
		
		//_RimColor("Rim Color", color) =(0.26,0.37, 0.87, 1)
		//_RimPower("Rim Power", Range(0.5,8.0)) = 0.5
		//_SpecularTex("Specular Level (R) Gloss (G) Rim Mask (B)", 2D) = "gray" {}
		//_RampTex("Toon Ramp (RGB)", 2D) = "white" {}
		//_Cutoff("Alphatest Cutoff", Range(0,1)) = 0.5
	
 
		
	}
	SubShader {
		
		Tags { "RenderType"="Opaque" "RenderType "="Transparent"}
		Cull Off
		ZWrite On
		
		LOD 500
		

		CGPROGRAM
		#pragma surface surf BlinnPhong addshadow fullforwardshadows vertex:disp tessellate:tessEdge nolightmap
		//#pragma surface surf TF2 alphatest:_Cutoff
		#pragma surface surf Lambert alpha
		//#pragma target Flash
	
        #pragma target 5.0 // compiler to DX11 shader model 5.0
        #include "Tessellation.cginc"
	

		struct appdata{
			float4 vertex : POSITION;
			float4 tangent : TANGENT;
			float3 normal : NORMAL;
			float2 texcoord : TEXCOORD0;
			
		};

		float _EdgeLength;

		float4 tessEdge (appdata v0, appdata v1, appdata v2)
		{
			return UnityEdgeLengthBasedTess (v0.vertex, v1.vertex, v2.vertex, _EdgeLength);
		}

		sampler2D _DispTex;
		sampler2D _MainTex;
		sampler2D _NormalMap;
		sampler2D _WaterTex;
		sampler2D _SpecularTex;
		sampler2D _RampTex;
		float _Cutoff;

		float4 _Color;
		float4 _RimColor;
		float _RimPower;
	
		float _Displacement;

		void disp(inout appdata v)
		{
			float2 offset = v.texcoord.xy*0.7;
	
			float d = tex2Dlod(_DispTex, float4(offset,0,0)).r *_Displacement;
			
			v.vertex.xyz += saturate(v.normal.xyz * d);	
			
		}

		struct Input {
			float4 color : COLOR;

			float2 uv_MainTex;
			float2 uv_WaterTex;

			float3 worldRefl;
			float3 viewDir;

			float3 totalLight;

			float3 worldNormal;

			INTERNAL_DATA
		};


		void surf (Input IN, inout SurfaceOutput o) {
			
			fixed4 c = tex2D( _MainTex, IN.uv_MainTex );
			o.Albedo = c.rgb;
			
			o.Alpha = c.a * _Cutoff;
			o.Normal = 0.7 - UnpackNormal(tex2D(_NormalMap, IN.uv_MainTex));
	
			float3 specGloss = tex2D(_SpecularTex, IN.uv_MainTex).rgb;
			o.Specular = specGloss.r;
			o.Gloss = specGloss.g;
	
			half4 rim = 1.0 - saturate(dot(normalize(1.0-IN.worldRefl)+normalize(IN.viewDir)/1.6, o.Normal));

			o.Emission = _RimColor.rgb * pow(rim, _RimPower);
		
		}
		 
		ENDCG
	} 
	
	FallBack "Custom/Tesselation"
}





	//inline fixed4 Lighting(SurfaceOutput s, fixed3 lightDir, fixed3 viewDir, fixed atten)
		//{
		//	fixed3 h = normalize(lightDir + viewDir);

		//	fixed2 NdotL = dot(s.Normal, lightDir) * 0.5 + 0.5;
		//	fixed3 ramp = tex2D(_RampTex, float2(NdotL * atten)).rgb;

		//	float nh = max(0,dot(s.Normal, h));
		//	float spec = pow(nh, s.Gloss * 256) *s.Specular;

		//	fixed4 c;
		//	c.rgb = ((s.Albedo * ramp * _LightColor0.rgb + _LightColor0.rgb * spec) * (atten * 2));
		//	c.a = s.Alpha;
		//	return c;
		
	//	}