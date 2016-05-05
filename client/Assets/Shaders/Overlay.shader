Shader "Custom/Bioblox/Overlay" {
  Properties {
    _MainTex ("MainTex", 2D)= "white"{}
  }
  SubShader {
    Pass {
      Tags {"Queue" = "Overlay" }
      Tags {"LightMode" = "ForwardBase" }
      Blend SrcAlpha OneMinusSrcAlpha

      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag
      #pragma target 3.0

      #include "UnityCG.cginc"
      
      uniform sampler2D _MainTex;

      struct varying_t {
        float4 projection_pos : POSITION;
        float2 uv : TEXCOORD;
        fixed4 colour : COLOR;
      };
      
      varying_t vert(appdata_full v) {
        varying_t o;
        o.uv = v.texcoord1.xy;
        o.colour = v.color;
        o.projection_pos = mul (UNITY_MATRIX_MVP, v.vertex);
        return o;
      }

      fixed4 frag(varying_t i) : COLOR {
        fixed4 tex = tex2D(_MainTex, i.uv);
        return fixed4(tex.xyz * i.colour.xyz, tex.w);
      }

      ENDCG
    }
  }
}
