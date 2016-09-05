// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

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
    _GlowTexture ("GlowTexture", 2D)= "white"{}
    _GlowPoint1 ("GlowPointLocal1" , Vector) = (0,0,0,0)
    _GlowRadius1("GlowRadius1" , Float) = 0
    _GlowPoint2 ("GlowPointLocal2" , Vector) = (0,0,0,0)
    _GlowRadius2("GlowRadius2" , Float) = 0
    _GlowPoint3 ("GlowPointLocal3" , Vector) = (0,0,0,0)
    _GlowRadius3("GlowRadius3" , Float) = 0
    _DarkPoint ("DarkPointLoca3" , Vector) = (0,0,0,0)
    _DarkK("DarkK" , Float) = -0.01
    _Thickness("Thickness" , Float) = 0
    _CutawayPlane("CutawayPlane", Vector) = (0,0,1,0)
    _CutawayColor("CutawayColor", Color) = (1,1,1,1)
    _Atom0 ("Atom0" , Vector) = (0,0,0,0)
    _Atom1 ("Atom1" , Vector) = (0,0,0,0)
    _Atom2 ("Atom2" , Vector) = (0,0,0,0)
    _Atom3 ("Atom3" , Vector) = (0,0,0,0)
    _Atom4 ("Atom4" , Vector) = (0,0,0,0)
    _Atom5 ("Atom5" , Vector) = (0,0,0,0)
    _Atom6 ("Atom6" , Vector) = (0,0,0,0)
    _Atom7 ("Atom7" , Vector) = (0,0,0,0)
    _Atom8 ("Atom8" , Vector) = (0,0,0,0)
    _Atom9 ("Atom9" , Vector) = (0,0,0,0)
    _Atom10 ("Atom10" , Vector) = (0,0,0,0)
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
     Pass {
      Tags {"Queue" = "Opaque" }
      Tags {"LightMode" = "ForwardBase" }
      Cull Front
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
        float4 world_pos : TEXCOORD;
      };
      
      uniform float4 _CutawayPlane;
      uniform float4 _CutawayColor;
      uniform float _Thickness;

      varying_t vert(appdata_full v) {
        varying_t o;
        float4 vertex = float4(v.vertex + v.normal * _Thickness, 1);
        o.projection_pos = mul (UNITY_MATRIX_MVP, vertex);
        o.world_pos = mul (unity_ObjectToWorld, vertex);
        return o;
      }

      fixed4 frag(varying_t i) : COLOR {
      	if (dot(float4(i.world_pos.xyz, 1), _CutawayPlane) < 0) {
      		clip(-1);
      	}
        return fixed4(_CutawayColor.xyz, 1.0f);
      }

      ENDCG
    }
    Pass {
      Tags {"Queue" = "Transparent" }
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
        float4 model_pos : TEXCOORD2;
        float4 scrPos : TEXCOORD1;
      };
      
      uniform float4 _Color;
      uniform float4 _Ambient;
      uniform float4 _Specular;
      uniform float _Shininess;
      uniform float4 _CullPos;
      uniform float _K;
      
      uniform float4 _GlowPoint1;
      uniform float _GlowRadius1;
      uniform float4 _GlowPoint2;
      uniform float _GlowRadius2;
      uniform float4 _GlowPoint3;
      uniform float _GlowRadius3;

      uniform float4 _DarkPoint;
      uniform float _DarkK;
      
      uniform float4 _Atom0;
      uniform float4 _Atom1;
      uniform float4 _Atom2;
      uniform float4 _Atom3;
      uniform float4 _Atom4;
      uniform float4 _Atom5;
      uniform float4 _Atom6;
      uniform float4 _Atom7;
      uniform float4 _Atom8;
      uniform float4 _Atom9;
      uniform float4 _Atom10;
      uniform sampler2D _GlowTexture;

      // note: _LightColor0, _WorldSpaceLightPos0 and _WorldSpaceCameraPos do not seem to work!
      uniform float3 _LightPos;
      uniform float3 _CameraPos;
      uniform float4 _CutawayPlane;
      uniform float _CutawayDepth;
      uniform float _Thickness;

      varying_t vert(appdata_full v) {
        varying_t o;
        float4 vertex = float4(v.vertex + v.normal * _Thickness, 1);
        o.projection_pos = mul (UNITY_MATRIX_MVP, vertex);
        o.normal = mul(unity_ObjectToWorld, float4(v.normal, 0)).xyz;
        o.model_pos = vertex;
        o.world_pos = mul (unity_ObjectToWorld, vertex);
        o.color = v.color;
        o.scrPos = ComputeScreenPos(o.projection_pos);
        return o;
      }

      fixed4 frag(varying_t i) : COLOR {
        float3 normal = normalize(i.normal);
 		
 		float2 screen_pos = (i.scrPos.xy/i.scrPos.w);
        
        float4 glowRead = tex2D(_GlowTexture,screen_pos);

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
      	float dist = distance(i.model_pos, _GlowPoint1);
      	if(dist < _GlowRadius1)
      	{
     		glowVal = _GlowRadius1 - dist;
      	}
      	
      	dist = distance(i.model_pos, _GlowPoint2);
      	if(dist < _GlowRadius2)
      	{
     		glowVal = _GlowRadius2 - dist;
      	}
      	
      	dist = distance(i.model_pos, _GlowPoint3);
      	if(dist < _GlowRadius3)
      	{
     		glowVal = _GlowRadius3 - dist;
      	}

  		glowVal *= _SinTime.w * _SinTime.w * 0.05;

      	float3 rpos = i.model_pos - _DarkPoint.xyz;
      	float dp2 = dot(rpos, rpos);
      	float dark = 1.0 - exp(_DarkK * dp2);
      	
      	float2 d0 = (screen_pos - _Atom0.xy) * _Atom0.zw;
      	float dot0 = dot(d0, d0);
      	float2 d1 = (screen_pos - _Atom1.xy) * _Atom1.zw;
      	float dot1 = dot(d1, d1);
      	float2 d2 = (screen_pos - _Atom2.xy) * _Atom2.zw;
      	float dot2 = dot(d2, d2);
      	float2 d3 = (screen_pos - _Atom3.xy) * _Atom3.zw;
      	float dot3 = dot(d3, d3);
      	float2 d4 = (screen_pos - _Atom4.xy) * _Atom4.zw;
      	float dot4 = dot(d4, d4);
      	float2 d5 = (screen_pos - _Atom5.xy) * _Atom5.zw;
      	float dot5 = dot(d5, d5);
      	float2 d6 = (screen_pos - _Atom6.xy) * _Atom6.zw;
      	float dot6 = dot(d6, d6);
      	float2 d7 = (screen_pos - _Atom7.xy) * _Atom7.zw;
      	float dot7 = dot(d7, d7);
      	float2 d8 = (screen_pos - _Atom8.xy) * _Atom8.zw;
      	float dot8 = dot(d8, d8);
      	float2 d9 = (screen_pos - _Atom9.xy) * _Atom9.zw;
      	float dot9 = dot(d9, d9);
      	float2 d10 = (screen_pos - _Atom10.xy) * _Atom10.zw;
      	float dot10 = dot(d10, d10);
      	
      	float min01 = min(dot0, dot1);
      	float min23 = min(dot2, dot3);
      	float min45 = min(dot4, dot5);
      	float min67 = min(dot6, dot7);
      	float min89 = min(dot8, dot9);
      	float min0123 = min(min01, min23);
      	float min4567 = min(min45, min67);
      	float dmin = min(min(min0123, min4567), min(min89, dot10));
      	float dmin_mid = 0.8f;
      	float dmin_k = -10.0f;
      	glowVal += exp((dmin - dmin_mid) * (dmin - dmin_mid) * dmin_k) * 0.5f;
      	//glowVal += dmin >= 0.7 && dmin <= 1 ? 1 : 0;
      	
      	/*glowVal +=
      		(dot0 >= 0.7 && dot1 >= 0.7 && dot2 >= 0.7 && dot3 >= 0.7 && dot4 >= 0.7) && 
      		(dot0 <= 1 || dot1 <= 1 || dot2 <= 1 || dot3 <= 1 || dot4 <= 1) ? 1 : 0
      	;*/
      	//glowVal += dot0 <= 1 || dot1 <= 1 || dot2 <= 1 || dot3 <= 1 || dot4 <= 1 ? 1 : 0;
      	
       	float3 colour = _Ambient.xyz * dark;
      	colour += _Color.xyz * i.color.xyz * diffuse_factor * dark;
      	colour += glowColor * glowVal;
      	colour += _Specular.xyz * specular_factor * dark;
   
      	if (dot(float4(i.world_pos.xyz, 1), _CutawayPlane) < 0) {
      		clip(-1);
      	}

      	return fixed4(colour, _Color.w);
      	//return fixed4(_Ambient.xyz * dark + _Color.xyz * i.color.xyz * diffuse_factor * dark + glowColor * glowVal + _Specular.xyz * specular_factor * dark, _Color.w);
      }

      ENDCG
    }
  }
}
