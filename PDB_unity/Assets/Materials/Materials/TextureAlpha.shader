Shader "Custom/Texture" {
	Properties {
		
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_NormalMap ("Normal Map", 2D) = "bump" {}
		//_MainTint ("Color", Color) = (1,1,1,1)
		_tintAmount ("Tint Amount", Range(0,1)) = 0.5
		_ColorA ("Color A", Color) = (1,1,1,1)
		_ColorB ("Color B", Color) = (1,1,1,1)
		_Speed ("Wave Speed", Range(0.005,80)) = 5
		_Frequency ("Wave Frequency", Range(0,5)) = 2
		_Amplitude ("Wave Amplitude", Range(-1,1)) = 1
		_RimColor ("Rim Color", Color) = (0.26,0.19,0.16,0.0)
		_RimPower ("Rim Power", Range(0.5 , 8.0)) = 3.0
		_Transparency ("Transpareny", Range(0,1.0)) = 0.5
		//_Glossiness ("Smoothness", Range(0,1)) = 0.5
		//_Metallic ("Metallic", Range(0,1)) = 0.0
		//_ScrollXSpeed ("X Scroll Speed", Range(0,10)) = 2
		//_ScrollYSpeed ("X Scroll Speed", Range(0,10)) = 2
		
	}
	
	SubShader {
		Tags { "RenderType"="Opaque" "RenderType" = "Transparent"}
		LOD 300
		Cull Off
		Zwrite On
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		//#pragma surface surf Standard fullforwardshadows
		#pragma surface surf Lambert vertex:vert
		#pragma surface surf BlinnPhong
		#pragma surface Transparent
		//#pragma surface surf Standard fullforwardshadows
		//#pragma surface surf fragment:frag
		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 5.0
		
		struct Input {
			float2 uv_MainTex;
			float2 uv_NormalMap;
			//float3 tan1;
			//float3 tan2;
			
			float3 vertColor;
			float3 viewDir;
		//	INTERNAL_DATA
			//float2 scrolledUV;
		};
		
		sampler2D _MainTex;
		sampler2D _NormalMap;
		float4 _ColorA;
		float4 _ColorB;
		float _tintAmount;
		float _Speed;
		float _Frequency;
		float _Amplitude;
		float _OffsetVal;
		float4 _RimColor;
		float _RimPower;
		float _Transparency;
		//half _Glossiness;
		//half _Metallic;
		//fixed4 _MainTint;
		//fixed _ScrollXSpeed;
		//fixed _ScrollYSpeed;
		
			
		//Vertex shader changes to tangent rotation matrix
		void vert (inout appdata_full v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input,o);
			float time = _Time * _Speed;
			float waveValueA = sin(time + v.vertex.x * _Frequency) * _Amplitude;
			
			v.vertex.xyz = float3(v.vertex.x, v.vertex.y + waveValueA, v.vertex.z);
			
			v.normal = normalize(float3(v.normal.x + waveValueA, v.normal.y, v.normal.z));
			
			o.vertColor = float3(waveValueA,waveValueA,waveValueA);
			//TANGENT_SPACE_ROTATION;
			//o.tan1 = mul(rotation, UNITY_MATRIX_IT_MV[0].xyz);
			//o.tan2 = mul(rotation, UNITY_MATRIX_IT_MV[1].xyz);	
		}
		
		void surf (Input IN, inout SurfaceOutput o) {
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
			float3 tintColor = lerp(_ColorA, _ColorB, IN.vertColor).rgb;
			float3 normals = UnpackNormal(tex2D(_NormalMap, IN.uv_NormalMap));
			o.Albedo = c.rgb * (tintColor * _tintAmount);
			half rim = 0.9 - saturate(dot (normalize(IN.viewDir), o.Normal));
          	o.Emission = _RimColor.rgb * pow (rim, _RimPower);
			// Metallic and smoothness come from slider variables
			//o.Metallic = _Metallic;
			//o.Smoothness = _Glossiness;
			o.Normal = normals;
			o.Alpha = c.a * _Transparency;

			//float2 litSphereUV;
			//litSphereUV.x = dot(IN.tan1, o.Normal);
			//litSphereUV.y = dot(IN.tan2, o.Normal);
			
			//fixed2 scrolledUV = IN.uv_MainTex;
			//fixed xScrollValue = _ScrollXSpeed * _Time;
			//fixed yScrollValue = _ScrollYSpeed * _Time;
			//scrolledUV += fixed2(xScrollValue, yScrollValue);
			// Albedo comes from a texture tinted by color
			
			// + tex2D (_MainTex, scrolledUV) + tex2D(_MainTex, litSphereUV * 0.5 + 0.5)
			
			
			
		}
		ENDCG
	} 
	FallBack "Transparent"
}
