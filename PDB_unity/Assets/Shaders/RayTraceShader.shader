Shader "Custom/Bioblox/RayTrace" {
  Properties {
    _Color ("Diffuse Material Color", Color) = (1,1,1,1) 
    _Specular ("Specular Material Color", Color) = (1,1,1,1) 
    _Shininess ("Shininess", Float) = 10
    _LightPos ("LightPos", Vector) = (50, 0, 0, 0)
    _CameraPos ("_CameraPos", Vector) = (0, 0, -50, 0)
    _CullPos ("CullPos", Vector) = (0,0,0,0)
    _K ("K transparrency", Float)=0
    //_AmbientOcclusion ("TexRanger", 3D)="white"{}
    _BVH ("BVH_texture", RECT) = "black"{}
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
        float4 col:COLOR;
        float4 sp :TEXCOORD1;
        float4 model_pos :TEXCOORD2;
      };
      
      uniform float4 _Color;
      uniform float4 _Specular;
      uniform float _Shininess;
      uniform float4 _CullPos;
      uniform float _K;
      
      //uniform sampler3D _AmbientOcclusion;
      uniform sampler2D _BVH;

      // note: _LightColor0, _WorldSpaceLightPos0 and _WorldSpaceCameraPos do not seem to work!
      uniform float3 _LightPos;
      uniform float3 _CameraPos;

	varying_t vert(appdata_full v) {
		varying_t o;
		o.projection_pos = mul (UNITY_MATRIX_MVP, v.vertex);
		o.normal = mul(_Object2World, float4(v.normal, 0)).xyz;
		o.model_pos = v.vertex;
		o.world_pos = mul (_Object2World, v.vertex);
		o.col = v.color;
		o.sp = ComputeScreenPos(o.projection_pos);
		return o;
	}
	
	float rgtof(fixed r, fixed g) {
		return (r * 256.0 + g) - 128.0;
	}

	// https://en.wikipedia.org/wiki/Line%E2%80%93sphere_intersection
	fixed4 frag(varying_t i) : COLOR {
		//return fixed4(1, 0, 0, 1);
		return fixed4(tex2Dlod(_BVH, float4(0, 0, 0, 0)));

		//return fixed4(1, 0, 0, 1);
		/*float4 cr0 = tex2D(_BVH, float2(0.25, 0.5));
		float4 cr1 = tex2D(_BVH, float2(0.75, 0.5));
		float3 ray_start = i.world_pos.xyz;
		float3 ray_dir = normalize(ray_start - _CameraPos);
		float3 centre = float3(rgtof(cr0.r, cr0.g), rgtof(cr0.b, cr0.a), rgtof(cr1.r, cr1.g));
		float radius = 10;
		float3 oc = ray_start - centre;
		float l_dot_oc = dot(ray_dir, oc);
		float oc2 = dot(oc, oc);
		
		float sq = l_dot_oc * l_dot_oc - oc2 + radius * radius;
		if (sq < 0) {
			clip(-1.0f);
		}
		float3 pos = ray_start + (-l_dot_oc - sqrt(sq)) * ray_dir;
		float3 normal = normalize(pos - centre);
		//return fixed4(normal, 1);

        float3 light_dir = normalize(_LightPos.xyz - pos);
        float3 view_dir = ray_dir;
        float3 reflect = view_dir - 2.0 * dot(normal, view_dir) * normal;
        float diffuse_factor = max(0.5f, dot(normal, light_dir));
        float specular_factor = pow(max(0.0, dot(reflect, light_dir)), _Shininess);
        float aoValue = 0;
		//return fixed4(diffuse_fact/or, diffuse_factor, diffuse_factor, 1);
        return fixed4(_Color.xyz * diffuse_factor + _Specular.xyz * specular_factor, _Color.w);*/
	}

      ENDCG
    }
  }
}
