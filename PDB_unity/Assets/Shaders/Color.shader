Shader "Custom/Color" {
	Properties{
	  _Albedo("Albedo", Color) = (1,1,1,1)
	  _Metallic("Metallic", Range(0,1)) = 0.8
	  _Glossiness("Glossiness", Range(0,1)) = 0.6
      _VertexColors("Vertex Coloring", Range(0,1)) = 1
      _Brightness("Overall brightness", Range(0,10)) = 1

		//curv range min RGBA(-3.402, -10.734, -10.353, -4.928) max RGBA(2.687, 0.481, 18.382, 0.527)
		// but the extreme values are very rare (I still need to find them)
		_Range("Range", Range(-2,2)) = 1
		_LowR("Low R", Range(-2,2)) = 0
		_HighR("High R", Range(-2,2)) = 1
		_LowG("Low G", Range(-2,2)) = 0
		_HighG("High G", Range(-2,2)) = 1
		_LowB("Low B", Range(-1000,1000)) = 0
		_HighB("High B", Range(-1000,1000)) = 1
	}




   SubShader {
      Tags { "RenderType" = "Opaque" }
	  // Cull Off
      CGPROGRAM
      #pragma surface surf Standard
      struct Input {
          float4 color : COLOR;
      };

	  float4 _Albedo;
	  float _Glossiness;
	  float _Metallic;
	  float _VertexColors;
	  float _Brightness;
	  float _Range, _LowR, _HighR, _LowG, _HighG, _LowB, _HighB;

	  /**
	  struct SurfaceOutput   {
	  fixed3 Albedo;  // diffuse color
	  fixed3 Normal;  // tangent space normal, if written
	  fixed3 Emission;
	  half Specular;  // specular power in 0..1 range
	  fixed Gloss;    // specular intensity
	  fixed Alpha;    // alpha for transparencies
	  };
	  struct SurfaceOutputStandard  {
	  fixed3 Albedo;      // base (diffuse or specular) color
	  fixed3 Normal;      // tangent space normal, if written
	  half3 Emission;
	  half Metallic;      // 0=non-metal, 1=metal
	  half Smoothness;    // 0=rough, 1=smooth
	  half Occlusion;     // occlusion (default 1)
	  fixed Alpha;        // alpha for transparencies
	  };
	  struct SurfaceOutputStandardSpecular  {
	  fixed3 Albedo;      // diffuse color
	  fixed3 Specular;    // specular color
	  fixed3 Normal;      // tangent space normal, if written
	  half3 Emission;
	  half Smoothness;    // 0=rough, 1=smooth
	  half Occlusion;     // occlusion (default 1)
	  fixed Alpha;        // alpha for transparencies
	  };
	  **/

      
      // see http://docs.unity3d.com/Manual/SL-SurfaceShaders.html for SurfaceOuput
      void surf (Input IN, inout SurfaceOutputStandard o) {

		  //curv range min RGBA(-3.402, -10.734, -10.353, -4.928) max RGBA(2.687, 0.481, 18.382, 0.527)
		  // 0.4 typical for purish sphere, 0 for flat
		  // saved in IN.color
		  float3 rgb = float3(
			(IN.color.r - _LowR * _Range) / ((_HighR - _LowR) * _Range),
			(IN.color.g - _LowG * _Range) / ((_HighG - _LowG) * _Range),
			(IN.color.b - _LowB * _Range) / ((_HighB - _LowB) * _Range));
		  rgb = clamp(rgb, 0, 1);
		  rgb = rgb * _VertexColors + 0.5 * (1. - _VertexColors);
		  rgb *= _Brightness;
		  rgb *= rgb;		// perceptual range
		  /** /
		  if (abs(IN.color.r) < 0.02) o.Emission += float3(1, 0, 0);
		  if (abs(IN.color.g) < 0.01) o.Emission += float3(0, 1, 0);
		  if (abs(IN.color.b) < 0.01) o.Emission += float3(0, 0, 1);
		  if (abs(IN.color.b - 0.2) < 0.01) o.Emission += float3(0, 0, 1);
		  if (abs(IN.color.b + 0.1) < 0.005) o.Emission += float3(1, 1, 1);
		  if (abs(IN.color.b + 0.15) < 0.005) o.Emission += float3(1, 1, 0);
		  if (abs(IN.color.b + 0.2) < 0.01) o.Emission += float3(0, 0, 1);
		  if (abs(IN.color.b + 0.3) < 0.01) o.Emission += float3(0, 0, 1);
		  if (abs(IN.color.b + 0.4) < 0.01) o.Emission += float3(0, 0, 1);
		  if (abs(IN.color.b + 0.5) < 0.01) o.Emission += float3(0, 0, 1);
		  if (abs(IN.color.b + 0.6) < 0.01) o.Emission += float3(0, 0, 1);
		  if (abs(IN.color.b + 0.7) < 0.01) o.Emission += float3(0, 0, 1);
		  / **/

          o.Albedo = rgb * _Albedo.xyz;
          //o.Specular = _Specular;
          o.Smoothness = _Glossiness;
		  o.Metallic = _Metallic;

	  }
      ENDCG
     }  // subshader

    Fallback "Diffuse"
 }