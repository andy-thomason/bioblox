/**** //~ 
note on transparency:
transparency experiment 9 Jan 2016.  commented out with //~
There were lots of steps needed to get transparency to work, so I left the code commented out in case we want it back.
Problem was that even with alpha=1 (or alpha large) there was still some transparency left;  to revisit some time.
***/

Shader "Custom/Color" {
    Properties{
        _COPY("Copy", Range(0,1)) = 1

      _Albedo("Albedo", Color) = (1,1,1,1)
      _Metallic("Metallic", Range(0,1)) = 0.8
      _Glossiness("Glossiness", Range(0,1)) = 0.6
      _VertexColors("Vertex Coloring", Range(0,1)) = 1
        _IDColors("ID Coloring", Range(0,1)) = 0
        _Brightness("Overall brightness", Range(0,10)) = 1
 //~     _Alpha("Alpha", Range(0,1)) = 1

        //curv range min RGBA(-3.402, -10.734, -10.353, -4.928) max RGBA(2.687, 0.481, 18.382, 0.527)
        // but the extreme values are very rare (I still need to find them)
        _Range("Range", Range(-2,2)) = 1
        _LowR("Low R", Range(-2,2)) = 0
        _HighR("High R", Range(-2,2)) = 1
        _LowG("Low G", Range(-2,2)) = 0
        _HighG("High G", Range(-2,2)) = 1
        _LowB("Low B", Range(-1000,1000)) = 0
        _HighB("High B", Range(-1000,1000)) = 1

        _P1Color("P1 colour", Color) = (1,0,0,0)  // colour for first principal curvature
        _P1Step("P1 step", Range(0,1)) = 0.5 // step for p1 contours
        _P1Width("P1 width", Range(0,0.1)) = 0 // width of P1

        _P2Color("P2 colour", Color) = (0,1,0,0)  // colour for second principal curvature
        _P2Step("P2 step", Range(0,1)) = 0.5 // step for p1 contours
        _P2Width("P2 width", Range(0,0.1)) = 0 // width of P1

        _PGColor("PGauss colour", Color) = (1,1,0,0)  // colour for gaussian curvature
        _PGStep("PGauss step", Range(0,1)) = 0.5 // step for p1 contours
        _PGWidth("PGauss width", Range(0,0.1)) = 0 // width of P1

        _PMColor("PMean colour", Color) = (0,0,1,0)  // colour for mean curvature
        _PMStep("PMean step", Range(0,1)) = 0.5 // step for p1 contours
        _PMWidth("PMean width", Range(0,0.1)) = 0 // width of P1

        _Normtest("Normal test value", Range(0,1.1)) = 1.1 // normal length below this bright yellow, helps find grid

        _Tex("Base (RGB)", 2D) = "white" {}  // seem to be needed to get the uv to be used?
    }




   SubShader {
      //~ Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
      Tags{ "RenderType" = "Opaque" }
      //~  Blend SrcAlpha OneMinusSrcAlpha
        // Cull Off
        // AlphaToMask On
      //  ColorMask RGBA
      //~ ZWrite On
      CGPROGRAM
      #pragma surface surf Standard //~ alpha:fade
      struct Input {
          float4 color : COLOR;
          float2 uv_Tex : TEXCOORD0;
      };

      float4 _Albedo;
      float _Glossiness;
      float _Metallic;
      float _VertexColors;
      float _IDColors;
      float _Brightness;
      //~ float _Alpha;
      float _Range, _LowR, _HighR, _LowG, _HighG, _LowB, _HighB;
      float4 _P1Color; float _P1Step, _P1Width;
      float4 _P2Color; float _P2Step, _P2Width;
      float4 _PGColor; float _PGStep, _PGWidth;
      float4 _PMColor; float _PMStep, _PMWidth;
      float _Normtest;


      /**
      struct SurfaceOutputStandard  {
      fixed3 Albedo;      // base (diffuse or specular) color
      fixed3 Normal;      // tangent space normal, if written
      half3 Emission;
      half Metallic;      // 0=non-metal, 1=metal
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

          // at vertices the uv.x is an integer nearest sphere number
          if (_IDColors != 0) {
              float id = IN.uv_Tex.x;
              if (frac(id) == 0) {  // don't try to colour ambiguous in-between regions
                  float3 idcol = float3(frac(id * 0.161), frac(id * 0.197), frac(id * 0.097));
                  rgb = lerp(rgb, idcol, _IDColors);
              }
          }

          rgb *= _Brightness;
          rgb *= rgb;		// perceptual range

          // create contours.  contour at 0 has double width
          // todo, use multipass with special antialias (like in organci art shader) for better contours
#define contour(q, id) \
          if (_P##id##Width != 0) { \
          float ps = IN.color.q / _P##id##Step; \
          float p = frac(ps); \
          if (p < _P##id##Width * ( floor(p) == 0 ? 2 : 1)) o.Emission += _P##id##Color; \
          }

          contour(r, 1);
          contour(g, 2);
          contour(b, G);
          contour(a, M);

          if (length(o.Normal) > _Normtest)
              o.Emission += float3(1, 1, 0);

          o.Albedo = rgb * _Albedo.xyz;
          o.Smoothness = _Glossiness;
          o.Metallic = _Metallic;
          //~ o.Alpha = _Alpha;
      }
      ENDCG
     }  // subshader

    Fallback "Diffuse"
 }