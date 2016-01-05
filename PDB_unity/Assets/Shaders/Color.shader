Shader "Custom/Color" {
	Properties{
	  _Albedo("Albedo", Color) = (1,1,1,1)
	  _Metallic("Metallic", Float) = 0.8
	  _Glossiness("Glossiness", Float) = 0.6

		//curv range min RGBA(-3.402, -10.734, -10.353, -4.928) max RGBA(2.687, 0.481, 18.382, 0.527)
		// but the extreme values are very rare (I still need to find them)
		_Range("Range", Float) = 0.5
		_LowR("Low R", Float) = -1
		_HighR("High R", Float) = 1
		_LowG("Low G", Float) = -1
		_HighG("High G", Float) = 1
	}




   SubShader {
      Tags { "RenderType" = "Opaque" }
      CGPROGRAM
      #pragma surface surf Standard
      struct Input {
          float4 color : COLOR;
      };

	  float4 _Albedo;
	  float _Glossiness;
	  float _Metallic;
	  float _Range, _LowR, _HighR, _LowG, _HighG;

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
		  float r = (IN.color.r - _LowR * _Range) / ((_HighR - _LowR) * _Range);
		  float g = (IN.color.g - _LowG * _Range) / ((_HighG - _LowG) * _Range);
		  float b = 0.5;

          o.Albedo = float3(r*r, g*g, b*b) * _Albedo.xyz;
          //o.Specular = _Specular;
          o.Smoothness = _Glossiness;
		  o.Metallic = _Metallic;
      }
      ENDCG
     }  // subshader

    Fallback "Diffuse"
 }