Shader "Custom/Bioblox/PDB" {
  Properties {
    _Color ("Diffuse Material Color", Color) = (1,1,1,1) 
    _Specular ("Specular Material Color", Color) = (1,1,1,1) 
    _Ambient ("Ambient Material Color", Color) = (0.3,0.3,0.3,1) 
    _Shininess ("Shininess", Float) = 10
    _LightPos ("LightPos", Vector) = (50, 0, 0, 0)
    _CameraPos ("_CameraPos", Vector) = (0, 0, -50, 0)
    _CullPos ("CullPos", Vector) = (0,0,0,0)
    _K ("K transparrency", Float)=0
    _AmbientOcclusion ("TexRanger", 3D)="white"{}
    _GlowPoint ("GlowPointLocal" , Vector) = (0,0,0,0)
    _GlowRadius("GlowRadius" , Float) = 0
    _DarkPoint ("DarkPointLocal" , Vector) = (0,0,0,0)
    _DarkK("DarkK" , Float) = -0.01
  }
  SubShader {
    Pass {
      Tags {"Queue" = "Geometry" }
      Tags {"LightMode" = "ShadowCaster" }
      Blend One Zero
      Fog { Mode Off }

      CGPROGRAM
      // this pass is used when rendering shadows

      #pragma vertex vert
      #pragma fragment frag
      #pragma target 3.0

      #include "UnityCG.cginc"

      float4 vert(appdata_base v) : POSITION {
        return mul (UNITY_MATRIX_MVP, v.vertex);
      }

      fixed4 frag() : COLOR {
        return fixed4(1, 1, 1, 1);
      }

      ENDCG
    }
//    Pass {
//      Tags {"Queue" = "Geometry" }
//      Tags {"LightMode" = "ForwardBase" }
//      Blend Zero One
//      Fog { Mode Off }
//
//      CGPROGRAM
//      // this first pass just writes the Z buffer
//      // this ensures that the transparency is only drawn once per screen pixel.
//
//      #pragma vertex vert
//      #pragma fragment frag
//      #pragma target 3.0
//
//      #include "UnityCG.cginc"
//
//      float4 vert(appdata_base v) : POSITION {
//        return mul (UNITY_MATRIX_MVP, v.vertex);
//      }
//
//      fixed4 frag() : COLOR {
//        return fixed4(0, 0, 0, 0);
//      }
//
//      ENDCG
//    }
//     Pass {
//      Tags {"Queue" = "Opaque" }
//      Tags {"LightMode" = "ForwardBase" }
//      Cull Front
//      Blend SrcAlpha OneMinusSrcAlpha
//      Fog { Mode Off }
//
//      CGPROGRAM
//      #pragma vertex vert
//      #pragma fragment frag
//      #pragma target 3.0
//
//      #include "UnityCG.cginc"
//      #include "Lighting.cginc"
//      
//      struct varying_t {
//        float4 projection_pos : POSITION;
//        float3 normal : NORMAL;
//        float4 world_pos : TEXCOORD;
//        float4 col:COLOR;
//      };
//      
//      uniform float4 _Color;
//      uniform float4 _Specular;
//      uniform float _Shininess;
//      uniform float4 _CullPos;
//      uniform float _Cull;
//      uniform float _K;
//
//      // note: _LightColor0, _WorldSpaceLightPos0 and _WorldSpaceCameraPos do not seem to work!
//      uniform float3 _LightPos;
//      uniform float3 _CameraPos;
//
//      varying_t vert(appdata_full v) {
//        varying_t o;
//        o.projection_pos = mul (UNITY_MATRIX_MVP, v.vertex);
//        o.normal = mul(_Object2World, float4(v.normal, 0)).xyz;
//        o.world_pos = mul (_Object2World, v.vertex);
//        o.col=v.color;
//        return o;
//      }
//
//      fixed4 frag(varying_t i) : COLOR {
//        float half_tone = 0.5f;
//      	float alpha = exp(_K * dot(i.world_pos.xyz-_CullPos,i.world_pos.xyz-_CullPos)) * 4;
//      	if (half_tone > alpha)
//      	{
//      		clip(-1.0f);
//      	}
//        return fixed4(_Color.xyz*0.6f,1.0f);
//      }
//
//      ENDCG
//    }
    Pass {
      Tags {"Queue" = "Opaque" }
      Tags {"LightMode" = "ForwardBase" }
      Cull Back
      Blend SrcAlpha OneMinusSrcAlpha
      Fog { Mode Off }

      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag
      #pragma target 3.0

      #include "UnityCG.cginc"
      #include "Lighting.cginc"
      
      struct varying_t {
        float4 projection_pos : POSITION;
        float3 normal : NORMAL;
        float4 world_pos : TEXCOORD;
        float4 color : COLOR;
        float4 sp : TEXCOORD1;
        float4 model_pos : TEXCOORD2;
      };
      
      uniform float4 _Color;
      uniform float4 _Ambient;
      uniform float4 _Specular;
      uniform float _Shininess;
      uniform float4 _CullPos;
      uniform float _K;
      
      uniform float4 _GlowPoint;
      uniform float _GlowRadius;

      uniform float4 _DarkPoint;
      uniform float _DarkK;
      
      uniform sampler3D _AmbientOcclusion;

      // note: _LightColor0, _WorldSpaceLightPos0 and _WorldSpaceCameraPos do not seem to work!
      uniform float3 _LightPos;
      uniform float3 _CameraPos;

      varying_t vert(appdata_full v) {
        varying_t o;
        o.projection_pos = mul (UNITY_MATRIX_MVP, v.vertex);
        o.normal = mul(_Object2World, float4(v.normal, 0)).xyz;
        o.model_pos = v.vertex;
        o.world_pos = mul (_Object2World, v.vertex);
        o.color = v.color;
        o.sp= ComputeScreenPos(o.projection_pos);
        return o;
      }

      fixed4 frag(varying_t i) : COLOR {
        float3 normal = normalize(i.normal);
        float2 screen_pos = i.sp.xy/i.sp.w*_ScreenParams.xy;

        float3 light_dir = normalize(_LightPos.xyz - i.world_pos.xyz);
        float3 view_dir = normalize(i.world_pos.xyz - _CameraPos.xyz);
        float3 reflect = view_dir - 2.0 * dot(normal, view_dir) * normal;
        float diffuse_factor = min(1.0f, dot(normal, light_dir));
        float specular_factor = pow(max(0.0, dot(reflect, light_dir)), _Shininess);

//		float2 dxy = frac(screen_pos * 0.25) - 0.5;
//		float2 dxy2 = dxy * dxy;
//        float half_tone = (dxy2.x + dxy2.y) * 4;
//        
//      	float alpha = exp(_K * dot(i.model_pos.xyz-_CullPos,i.model_pos.xyz-_CullPos)) * 4;
//      	//alpha = 0.5;
//      	if (half_tone > alpha)
//      	{
//      		clip(-1.0f);
//      	}

      	float4 glowColor = float4(1, 1, 0, 1);
      	float glowVal = 0;
      	float dist = distance(i.model_pos, _GlowPoint);

      	if(dist < _GlowRadius)
      	{
     		glowVal = _GlowRadius - dist;
     		glowVal *= abs(_SinTime.w);
      	}

      	float3 rpos = i.model_pos - _DarkPoint.xyz;
      	float d2 = dot(rpos, rpos);
      	float dark = 1.0 - exp(_DarkK * d2);
      	
      	return fixed4(_Ambient.xyz * dark + _Color.xyz * i.color.xyz * diffuse_factor * dark + glowColor * glowVal * 0.25 + _Specular.xyz * specular_factor * dark, _Color.w);
      }

      ENDCG
    }
  }
}
