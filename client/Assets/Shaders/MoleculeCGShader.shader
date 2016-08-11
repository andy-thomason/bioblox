Shader "Custom/MoleculeCGShader" {
    Properties{
        _Distance("Distance", Range(-400,400)) = 0
        _Color ("Color", Color) = (1,1,1,1) 
        _CutawayColor ("CutawayColor", Color) = (1,1,1,1) 
    }

    SubShader{
        Tags{ "RenderType" = "Opaque" }

        
	    // ------------------------------------------------------------
	    // Surface shader code generated out of a CGPROGRAM block:
	

	    // ---- forward rendering base pass:
	    Pass {
		    Name "Cutaway"
		    Tags { "LightMode" = "ForwardBase" }
        Cull Front
        Blend SrcAlpha OneMinusSrcAlpha

        CGPROGRAM
        // compile directives
        #pragma vertex vert_surf
        #pragma fragment frag_surf
        #pragma target 3.0
        #pragma multi_compile_fog
        #pragma multi_compile_fwdbase
        #include "HLSLSupport.cginc"
        #include "UnityShaderVariables.cginc"

        #define UNITY_PASS_FORWARDBASE
        #include "UnityCG.cginc"
        #include "Lighting.cginc"
        #include "UnityPBSLighting.cginc"
        #include "AutoLight.cginc"

        #define INTERNAL_DATA
        #define WorldReflectionVector(data,normal) data.worldRefl
        #define WorldNormalVector(data,normal) normal

        // Original surface shader snippet:
        #line 10 ""
        #ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
        #endif


            //#pragma surface surf Standard vertex:vert fullforwardshadows
            //#pragma target 3.0

            struct Input {
                float2 uv_MainTex;
                float3 vertexColor; // Vertex color stored here by vert() method
                float3 worldPos;
            };

            void vert(inout appdata_full v, out Input o)
            {
                UNITY_INITIALIZE_OUTPUT(Input,o);
                o.vertexColor = v.color; // Save the Vertex Color in the Input for the surf() method
                o.worldPos = mul(_Object2World, v.vertex).xyz;
            }

            float _Distance;
            float4 _Color;
            float4 _CutawayColor;

            void surf(Input i, inout SurfaceOutputStandard o)
            {
                o.Albedo = i.vertexColor * _Color;
                if (i.worldPos.z < _Distance) discard;
                o.Metallic = 0;
                o.Smoothness = 0.5;
                o.Alpha = _Color.a;
            }
        

        // vertex-to-fragment interpolation data
        // no lightmaps:
        #ifdef LIGHTMAP_OFF
        struct v2f_surf {
          float4 pos : SV_POSITION;
          half3 worldNormal : TEXCOORD0;
          float3 worldPos : TEXCOORD1;
          float3 custompack0 : TEXCOORD2; // vertexColor
          #if UNITY_SHOULD_SAMPLE_SH
          half3 sh : TEXCOORD3; // SH
          #endif
          SHADOW_COORDS(4)
          UNITY_FOG_COORDS(5)
          #if SHADER_TARGET >= 30
          float4 lmap : TEXCOORD6;
          #endif
        };
        #endif
        // with lightmaps:
        #ifndef LIGHTMAP_OFF
        struct v2f_surf {
          float4 pos : SV_POSITION;
          half3 worldNormal : TEXCOORD0;
          float3 worldPos : TEXCOORD1;
          float3 custompack0 : TEXCOORD2; // vertexColor
          float4 lmap : TEXCOORD3;
          SHADOW_COORDS(4)
          UNITY_FOG_COORDS(5)
          #ifdef DIRLIGHTMAP_COMBINED
          fixed3 tSpace0 : TEXCOORD6;
          fixed3 tSpace1 : TEXCOORD7;
          fixed3 tSpace2 : TEXCOORD8;
          #endif
        };
        #endif

        // vertex shader
        v2f_surf vert_surf (appdata_full v) {
          v2f_surf o;
          UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
          Input customInputData;
          vert (v, customInputData);
          o.custompack0.xyz = customInputData.vertexColor;
          o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
          float3 worldPos = mul(_Object2World, v.vertex).xyz;
          fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);
          #if !defined(LIGHTMAP_OFF) && defined(DIRLIGHTMAP_COMBINED)
          fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
          fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
          fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
          #endif
          #if !defined(LIGHTMAP_OFF) && defined(DIRLIGHTMAP_COMBINED)
          o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
          o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
          o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
          #endif
          o.worldPos = worldPos;
          o.worldNormal = worldNormal;
          #ifndef DYNAMICLIGHTMAP_OFF
          o.lmap.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
          #endif
          #ifndef LIGHTMAP_OFF
          o.lmap.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
          #endif

          // SH/ambient and vertex lights
          #ifdef LIGHTMAP_OFF
            #if UNITY_SHOULD_SAMPLE_SH
              o.sh = 0;
              // Approximated illumination from non-important point lights
              #ifdef VERTEXLIGHT_ON
                o.sh += Shade4PointLights (
                  unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
                  unity_LightColor[0].rgb, unity_LightColor[1].rgb, unity_LightColor[2].rgb, unity_LightColor[3].rgb,
                  unity_4LightAtten0, worldPos, worldNormal);
              #endif
              o.sh = ShadeSHPerVertex (worldNormal, o.sh);
            #endif
          #endif // LIGHTMAP_OFF

          TRANSFER_SHADOW(o); // pass shadow coordinates to pixel shader
          UNITY_TRANSFER_FOG(o,o.pos); // pass fog coordinates to pixel shader
          return o;
        }


        // fragment shader
        fixed4 frag_surf (v2f_surf IN) : SV_Target {
          // prepare and unpack data
          Input surfIN;
          UNITY_INITIALIZE_OUTPUT(Input,surfIN);
          surfIN.uv_MainTex.x = 1.0;
          surfIN.vertexColor.x = 1.0;
          surfIN.worldPos.x = 1.0;
          surfIN.vertexColor = IN.custompack0.xyz;
          float3 worldPos = IN.worldPos;
          #ifndef USING_DIRECTIONAL_LIGHT
            fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
          #else
            fixed3 lightDir = _WorldSpaceLightPos0.xyz;
          #endif
          fixed3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
          surfIN.worldPos = worldPos;
          #ifdef UNITY_COMPILER_HLSL
          SurfaceOutputStandard o = (SurfaceOutputStandard)0;
          #else
          SurfaceOutputStandard o;
          #endif
          o.Albedo = 0.0;
          o.Emission = 0.0;
          o.Alpha = 0.0;
          o.Occlusion = 1.0;
          fixed3 normalWorldVertex = fixed3(0,0,1);
          o.Normal = IN.worldNormal;
          normalWorldVertex = IN.worldNormal;

          // call surface function
          surf (surfIN, o);

          // compute lighting & shadowing factor
          UNITY_LIGHT_ATTENUATION(atten, IN, worldPos)
          fixed4 c = 0;

          // Setup lighting environment
          UnityGI gi;
          UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
          gi.indirect.diffuse = 0;
          gi.indirect.specular = 0;
          #if !defined(LIGHTMAP_ON)
              gi.light.color = _LightColor0.rgb;
              gi.light.dir = lightDir;
              gi.light.ndotl = LambertTerm (o.Normal, gi.light.dir);
          #endif
          // Call GI (lightmaps/SH/reflections) lighting function
          UnityGIInput giInput;
          UNITY_INITIALIZE_OUTPUT(UnityGIInput, giInput);
          giInput.light = gi.light;
          giInput.worldPos = worldPos;
          giInput.worldViewDir = worldViewDir;
          giInput.atten = atten;
          #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
            giInput.lightmapUV = IN.lmap;
          #else
            giInput.lightmapUV = 0.0;
          #endif
          #if UNITY_SHOULD_SAMPLE_SH
            giInput.ambient = IN.sh;
          #else
            giInput.ambient.rgb = 0.0;
          #endif
          giInput.probeHDR[0] = unity_SpecCube0_HDR;
          giInput.probeHDR[1] = unity_SpecCube1_HDR;
          #if UNITY_SPECCUBE_BLENDING || UNITY_SPECCUBE_BOX_PROJECTION
            giInput.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
          #endif
          #if UNITY_SPECCUBE_BOX_PROJECTION
            giInput.boxMax[0] = unity_SpecCube0_BoxMax;
            giInput.probePosition[0] = unity_SpecCube0_ProbePosition;
            giInput.boxMax[1] = unity_SpecCube1_BoxMax;
            giInput.boxMin[1] = unity_SpecCube1_BoxMin;
            giInput.probePosition[1] = unity_SpecCube1_ProbePosition;
          #endif
          LightingStandard_GI(o, giInput, gi);

          // realtime lighting: call lighting function
          c += LightingStandard (o, worldViewDir, gi);
          UNITY_APPLY_FOG(IN.fogCoord, c); // apply fog
          UNITY_OPAQUE_ALPHA(c.a);
          return _CutawayColor;
        }

        ENDCG

        }

        	    // ------------------------------------------------------------
	    // Surface shader code generated out of a CGPROGRAM block:
	

	    // ---- forward rendering base pass:
	    Pass {
		    Name "Molecule Skin"
		    Tags { "LightMode" = "ForwardBase" }
        Cull Back

        CGPROGRAM
        // compile directives
        #pragma vertex vert_surf
        #pragma fragment frag_surf
        #pragma target 3.0
        #pragma multi_compile_fog
        #pragma multi_compile_fwdbase
        #include "HLSLSupport.cginc"
        #include "UnityShaderVariables.cginc"

        #define UNITY_PASS_FORWARDBASE
        #include "UnityCG.cginc"
        #include "Lighting.cginc"
        #include "UnityPBSLighting.cginc"
        #include "AutoLight.cginc"

        #define INTERNAL_DATA
        #define WorldReflectionVector(data,normal) data.worldRefl
        #define WorldNormalVector(data,normal) normal

        // Original surface shader snippet:
        #line 10 ""
        #ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
        #endif


            //#pragma surface surf Standard vertex:vert fullforwardshadows
            //#pragma target 3.0

            struct Input {
                float2 uv_MainTex;
                float3 vertexColor; // Vertex color stored here by vert() method
                float3 worldPos;
            };

            void vert(inout appdata_full v, out Input o)
            {
                UNITY_INITIALIZE_OUTPUT(Input,o);
                o.vertexColor = v.color; // Save the Vertex Color in the Input for the surf() method
                o.worldPos = mul(_Object2World, v.vertex).xyz;
            }

            float _Distance;
            float4 _Color;

            void surf(Input i, inout SurfaceOutputStandard o)
            {
                o.Albedo = i.vertexColor * _Color;
                if (i.worldPos.z < _Distance) discard;
                o.Metallic = 0;
                o.Smoothness = 0.5;
                o.Alpha = _Color.a;
            }
        

        // vertex-to-fragment interpolation data
        // no lightmaps:
        #ifdef LIGHTMAP_OFF
        struct v2f_surf {
          float4 pos : SV_POSITION;
          half3 worldNormal : TEXCOORD0;
          float3 worldPos : TEXCOORD1;
          float3 custompack0 : TEXCOORD2; // vertexColor
          #if UNITY_SHOULD_SAMPLE_SH
          half3 sh : TEXCOORD3; // SH
          #endif
          SHADOW_COORDS(4)
          UNITY_FOG_COORDS(5)
          #if SHADER_TARGET >= 30
          float4 lmap : TEXCOORD6;
          #endif
        };
        #endif
        // with lightmaps:
        #ifndef LIGHTMAP_OFF
        struct v2f_surf {
          float4 pos : SV_POSITION;
          half3 worldNormal : TEXCOORD0;
          float3 worldPos : TEXCOORD1;
          float3 custompack0 : TEXCOORD2; // vertexColor
          float4 lmap : TEXCOORD3;
          SHADOW_COORDS(4)
          UNITY_FOG_COORDS(5)
          #ifdef DIRLIGHTMAP_COMBINED
          fixed3 tSpace0 : TEXCOORD6;
          fixed3 tSpace1 : TEXCOORD7;
          fixed3 tSpace2 : TEXCOORD8;
          #endif
        };
        #endif

        // vertex shader
        v2f_surf vert_surf (appdata_full v) {
          v2f_surf o;
          UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
          Input customInputData;
          vert (v, customInputData);
          o.custompack0.xyz = customInputData.vertexColor;
          o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
          float3 worldPos = mul(_Object2World, v.vertex).xyz;
          fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);
          #if !defined(LIGHTMAP_OFF) && defined(DIRLIGHTMAP_COMBINED)
          fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
          fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
          fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
          #endif
          #if !defined(LIGHTMAP_OFF) && defined(DIRLIGHTMAP_COMBINED)
          o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
          o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
          o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
          #endif
          o.worldPos = worldPos;
          o.worldNormal = worldNormal;
          #ifndef DYNAMICLIGHTMAP_OFF
          o.lmap.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
          #endif
          #ifndef LIGHTMAP_OFF
          o.lmap.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
          #endif

          // SH/ambient and vertex lights
          #ifdef LIGHTMAP_OFF
            #if UNITY_SHOULD_SAMPLE_SH
              o.sh = 0;
              // Approximated illumination from non-important point lights
              #ifdef VERTEXLIGHT_ON
                o.sh += Shade4PointLights (
                  unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
                  unity_LightColor[0].rgb, unity_LightColor[1].rgb, unity_LightColor[2].rgb, unity_LightColor[3].rgb,
                  unity_4LightAtten0, worldPos, worldNormal);
              #endif
              o.sh = ShadeSHPerVertex (worldNormal, o.sh);
            #endif
          #endif // LIGHTMAP_OFF

          TRANSFER_SHADOW(o); // pass shadow coordinates to pixel shader
          UNITY_TRANSFER_FOG(o,o.pos); // pass fog coordinates to pixel shader
          return o;
        }


        // fragment shader
        fixed4 frag_surf (v2f_surf IN) : SV_Target {
          // prepare and unpack data
          Input surfIN;
          UNITY_INITIALIZE_OUTPUT(Input,surfIN);
          surfIN.uv_MainTex.x = 1.0;
          surfIN.vertexColor.x = 1.0;
          surfIN.worldPos.x = 1.0;
          surfIN.vertexColor = IN.custompack0.xyz;
          float3 worldPos = IN.worldPos;
          #ifndef USING_DIRECTIONAL_LIGHT
            fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
          #else
            fixed3 lightDir = _WorldSpaceLightPos0.xyz;
          #endif
          fixed3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
          surfIN.worldPos = worldPos;
          #ifdef UNITY_COMPILER_HLSL
          SurfaceOutputStandard o = (SurfaceOutputStandard)0;
          #else
          SurfaceOutputStandard o;
          #endif
          o.Albedo = 0.0;
          o.Emission = 0.0;
          o.Alpha = 0.0;
          o.Occlusion = 1.0;
          fixed3 normalWorldVertex = fixed3(0,0,1);
          o.Normal = IN.worldNormal;
          normalWorldVertex = IN.worldNormal;

          // call surface function
          surf (surfIN, o);

          // compute lighting & shadowing factor
          UNITY_LIGHT_ATTENUATION(atten, IN, worldPos)
          fixed4 c = 0;

          // Setup lighting environment
          UnityGI gi;
          UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
          gi.indirect.diffuse = 0;
          gi.indirect.specular = 0;
          #if !defined(LIGHTMAP_ON)
              gi.light.color = _LightColor0.rgb;
              gi.light.dir = lightDir;
              gi.light.ndotl = LambertTerm (o.Normal, gi.light.dir);
          #endif
          // Call GI (lightmaps/SH/reflections) lighting function
          UnityGIInput giInput;
          UNITY_INITIALIZE_OUTPUT(UnityGIInput, giInput);
          giInput.light = gi.light;
          giInput.worldPos = worldPos;
          giInput.worldViewDir = worldViewDir;
          giInput.atten = atten;
          #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
            giInput.lightmapUV = IN.lmap;
          #else
            giInput.lightmapUV = 0.0;
          #endif
          #if UNITY_SHOULD_SAMPLE_SH
            giInput.ambient = IN.sh;
          #else
            giInput.ambient.rgb = 0.0;
          #endif
          giInput.probeHDR[0] = unity_SpecCube0_HDR;
          giInput.probeHDR[1] = unity_SpecCube1_HDR;
          #if UNITY_SPECCUBE_BLENDING || UNITY_SPECCUBE_BOX_PROJECTION
            giInput.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
          #endif
          #if UNITY_SPECCUBE_BOX_PROJECTION
            giInput.boxMax[0] = unity_SpecCube0_BoxMax;
            giInput.probePosition[0] = unity_SpecCube0_ProbePosition;
            giInput.boxMax[1] = unity_SpecCube1_BoxMax;
            giInput.boxMin[1] = unity_SpecCube1_BoxMin;
            giInput.probePosition[1] = unity_SpecCube1_ProbePosition;
          #endif
          LightingStandard_GI(o, giInput, gi);

          // realtime lighting: call lighting function
          c += LightingStandard (o, worldViewDir, gi);
          UNITY_APPLY_FOG(IN.fogCoord, c); // apply fog
          UNITY_OPAQUE_ALPHA(c.a);
          return c;
        }

        ENDCG

        }

    }
    FallBack "Diffuse"
}
