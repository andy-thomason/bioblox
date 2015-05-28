Shader "Custom/Bioblox/PDB" {
  Properties {
    _Color ("Diffuse Material Color", Color) = (1,1,1,1) 
    _Specular ("Specular Material Color", Color) = (1,1,1,1) 
    _Shininess ("Shininess", Float) = 10
    _LightPos ("LightPos", Vector) = (50, 0, 0, 0)
    _CameraPos ("_CameraPos", Vector) = (0, 0, -50, 0)
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
    Pass {
      Tags {"Queue" = "Geometry" }
      Tags {"LightMode" = "ForwardBase" }
      Blend Zero One
      Fog { Mode Off }

      CGPROGRAM
      // this first pass just writes the Z buffer
      // this ensures that the transparency is only drawn once per screen pixel.

      #pragma vertex vert
      #pragma fragment frag
      #pragma target 3.0

      #include "UnityCG.cginc"

      float4 vert(appdata_base v) : POSITION {
        return mul (UNITY_MATRIX_MVP, v.vertex);
      }

      fixed4 frag() : COLOR {
        return fixed4(0, 0, 0, 0);
      }

      ENDCG
    }
    Pass {
      Tags {"Queue" = "Transparent" }
      Tags {"LightMode" = "ForwardBase" }
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
        float4 col:COLOR;
      };
      
      uniform float4 _Color;
      uniform float4 _Specular;
      uniform float _Shininess;

      // note: _LightColor0, _WorldSpaceLightPos0 and _WorldSpaceCameraPos do not seem to work!
      uniform float3 _LightPos;
      uniform float3 _CameraPos;

      varying_t vert(appdata_full v) {
        varying_t o;
        o.projection_pos = mul (UNITY_MATRIX_MVP, v.vertex);
        o.normal = mul(_Object2World, float4(v.normal, 0)).xyz;
        o.world_pos = mul (_Object2World, v.vertex);
        o.col=v.color;
        return o;
      }

      fixed4 frag(varying_t i) : COLOR {
        float3 normal = normalize(i.normal);
        float aoValue=i.col.w;
        i.col.w=1;
        //float3 light_dir = normalize(_WorldSpaceLightPos0.xyz - i.world_pos.xyz);
        //float3 view_dir = normalize(i.world_pos.xyz - _WorldSpaceCameraPos.xyz);
        float3 light_dir = normalize(_LightPos.xyz - i.world_pos.xyz);
        float3 view_dir = normalize(i.world_pos.xyz - _CameraPos.xyz);
        float3 reflect = view_dir - 2.0 * dot(normal, view_dir) * normal;
        float diffuse_factor = max(0.5f, dot(normal, light_dir));
        float specular_factor = pow(max(0.0, dot(reflect, light_dir)), _Shininess);
      	
        return fixed4(_Color.xyz * aoValue + i.col * diffuse_factor + _Specular.xyz * specular_factor, _Color.w);
        //return fixed4(colr);
        //return fixed4(normalize(_WorldSpaceLightPos0.xyz), 1);
        //return fixed4(normalize(_WorldSpaceCameraPos.xyz), 1);
        //return fixed4(light_dir,1);
      }

      ENDCG
    }
  }
}
