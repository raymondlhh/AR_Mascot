// Made with Amplify Shader Editor v1.9.3.3
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Vefects/SH_Vefects_VFX_Planet"
{
	Properties
	{
		_Specular("Specular", Float) = 0
		_Smoothness("Smoothness", Float) = 1
		[Space(33)][Header(Terrain)][Space(13)]_TerrainTexture("Terrain Texture", 2D) = "white" {}
		_TerrainLUT("Terrain LUT", 2D) = "white" {}
		_TerrainLUTOffset("Terrain LUT Offset", Float) = 0
		_TerrainLUTRange("Terrain LUT Range", Float) = 1
		[Space(33)][Header(Atmosphere)][Space(13)]_AtmosphereColor("Atmosphere Color", Color) = (0.553459,0.9314483,1,0)
		_AtmosphereDensity("Atmosphere Density", Float) = 0
		_AtmosphereFresnelScale("Atmosphere Fresnel Scale", Float) = 1
		_AtmosphereFresnelPower("Atmosphere Fresnel Power", Float) = 5
		_AtmosphereFresnelBias("Atmosphere Fresnel Bias", Float) = 0
		[Space(33)][Header(Clouds)][Space(13)]_CloudsTexture("Clouds Texture", 2D) = "white" {}
		_CloudsColor("Clouds Color", Color) = (0.8553458,1,0.9881043,0)
		_CloudsCoverage("Clouds Coverage", Float) = 1
		_CloudsSoftness("Clouds Softness", Float) = 1
		_CloudsPower("Clouds Power", Float) = 1
		_CloudsMultiply("Clouds Multiply", Float) = 1
		[Space(33)][Header(Emissive)][Space(13)]_EmissiveLUT("Emissive LUT", 2D) = "white" {}
		_EmissiveMinSmoothstep("Emissive Min Smoothstep", Float) = 0
		_EmissiveMaxSmoothstep("Emissive Max Smoothstep", Float) = 1
		_EmissiveStrength("Emissive Strength", Float) = 1
		[Space(33)][Header(Normal)][Space(13)]_NormalTexture("Normal Texture", 2D) = "bump" {}
		_NormalIntensity("Normal Intensity", Float) = 1
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGINCLUDE
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		#ifdef UNITY_PASS_SHADOWCASTER
			#undef INTERNAL_DATA
			#undef WorldReflectionVector
			#undef WorldNormalVector
			#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
			#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
			#define WorldNormalVector(data,normal) half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))
		#endif
		struct Input
		{
			float2 uv_texcoord;
			float3 worldPos;
			float3 worldNormal;
			INTERNAL_DATA
		};

		uniform sampler2D _NormalTexture;
		uniform float4 _NormalTexture_ST;
		uniform float _NormalIntensity;
		uniform sampler2D _TerrainLUT;
		uniform sampler2D _TerrainTexture;
		uniform float _TerrainLUTRange;
		uniform float _TerrainLUTOffset;
		uniform float4 _CloudsColor;
		uniform float _CloudsCoverage;
		uniform float _CloudsSoftness;
		uniform sampler2D _CloudsTexture;
		uniform float4 _CloudsTexture_ST;
		uniform float _CloudsPower;
		uniform float _CloudsMultiply;
		uniform float4 _AtmosphereColor;
		uniform float _AtmosphereFresnelBias;
		uniform float _AtmosphereFresnelScale;
		uniform float _AtmosphereFresnelPower;
		uniform float _AtmosphereDensity;
		uniform float _EmissiveMinSmoothstep;
		uniform float _EmissiveMaxSmoothstep;
		uniform sampler2D _EmissiveLUT;
		uniform float _EmissiveStrength;
		uniform float _Specular;
		uniform float _Smoothness;

		void surf( Input i , inout SurfaceOutputStandardSpecular o )
		{
			float2 uv_NormalTexture = i.uv_texcoord * _NormalTexture_ST.xy + _NormalTexture_ST.zw;
			float3 lerpResult62 = lerp( float3(0,0,1) , UnpackNormal( tex2D( _NormalTexture, uv_NormalTexture ) ) , _NormalIntensity);
			o.Normal = lerpResult62;
			float4 tex2DNode14 = tex2D( _TerrainTexture, i.uv_texcoord );
			float2 temp_cast_0 = (( ( tex2DNode14.r * _TerrainLUTRange ) + _TerrainLUTOffset )).xx;
			float temp_output_91_0 = ( 1.0 - _CloudsCoverage );
			float2 uv_CloudsTexture = i.uv_texcoord * _CloudsTexture_ST.xy + _CloudsTexture_ST.zw;
			float smoothstepResult90 = smoothstep( temp_output_91_0 , ( temp_output_91_0 + _CloudsSoftness ) , tex2D( _CloudsTexture, uv_CloudsTexture ).r);
			float4 lerpResult80 = lerp( tex2D( _TerrainLUT, temp_cast_0 ) , _CloudsColor , saturate( ( saturate( pow( smoothstepResult90 , _CloudsPower ) ) * _CloudsMultiply ) ));
			float3 ase_worldPos = i.worldPos;
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float3 ase_worldNormal = WorldNormalVector( i, float3( 0, 0, 1 ) );
			float3 ase_normWorldNormal = normalize( ase_worldNormal );
			float fresnelNdotV20 = dot( ase_normWorldNormal, ase_worldViewDir );
			float fresnelNode20 = ( _AtmosphereFresnelBias + _AtmosphereFresnelScale * pow( max( 1.0 - fresnelNdotV20 , 0.0001 ), _AtmosphereFresnelPower ) );
			float4 lerpResult72 = lerp( lerpResult80 , _AtmosphereColor , saturate( ( fresnelNode20 * _AtmosphereDensity ) ));
			float smoothstepResult95 = smoothstep( _EmissiveMinSmoothstep , _EmissiveMaxSmoothstep , tex2DNode14.r);
			float4 lerpResult98 = lerp( lerpResult72 , float4( 0,0,0,0 ) , smoothstepResult95);
			o.Albedo = lerpResult98.rgb;
			float2 temp_cast_2 = (smoothstepResult95).xx;
			o.Emission = ( ( tex2D( _EmissiveLUT, temp_cast_2 ) * _EmissiveStrength ) * smoothstepResult95 ).rgb;
			float3 temp_cast_4 = (_Specular).xxx;
			o.Specular = temp_cast_4;
			o.Smoothness = _Smoothness;
			o.Alpha = 1;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf StandardSpecular keepalpha fullforwardshadows 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float2 customPack1 : TEXCOORD1;
				float4 tSpace0 : TEXCOORD2;
				float4 tSpace1 : TEXCOORD3;
				float4 tSpace2 : TEXCOORD4;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				half3 worldTangent = UnityObjectToWorldDir( v.tangent.xyz );
				half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				half3 worldBinormal = cross( worldNormal, worldTangent ) * tangentSign;
				o.tSpace0 = float4( worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x );
				o.tSpace1 = float4( worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y );
				o.tSpace2 = float4( worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z );
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				return o;
			}
			half4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.uv_texcoord = IN.customPack1.xy;
				float3 worldPos = float3( IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w );
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = float3( IN.tSpace0.z, IN.tSpace1.z, IN.tSpace2.z );
				surfIN.internalSurfaceTtoW0 = IN.tSpace0.xyz;
				surfIN.internalSurfaceTtoW1 = IN.tSpace1.xyz;
				surfIN.internalSurfaceTtoW2 = IN.tSpace2.xyz;
				SurfaceOutputStandardSpecular o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandardSpecular, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=19303
Node;AmplifyShaderEditor.RangedFloatNode;92;-2432,384;Inherit;False;Property;_CloudsCoverage;Clouds Coverage;13;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;91;-2176,384;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;94;-2432,512;Inherit;False;Property;_CloudsSoftness;Clouds Softness;14;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;93;-1920,384;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;83;-2560,0;Inherit;True;Property;_CloudsTexture;Clouds Texture;11;0;Create;True;0;0;0;False;3;Space(33);Header(Clouds);Space(13);False;-1;bba7fd7d0923a3746b880c57e84963c8;bba7fd7d0923a3746b880c57e84963c8;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;25;-4096,256;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SmoothstepOpNode;90;-1920,0;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;88;-1664,128;Inherit;False;Property;_CloudsPower;Clouds Power;15;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;84;-1664,0;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;18;-3584,0;Inherit;False;Property;_TerrainLUTRange;Terrain LUT Range;5;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;14;-3840,256;Inherit;True;Property;_TerrainTexture;Terrain Texture;2;0;Create;True;0;0;0;False;3;Space(33);Header(Terrain);Space(13);False;-1;ec31af15634d9374a9dca49fade8e944;ec31af15634d9374a9dca49fade8e944;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;17;-3584,-128;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;89;-1408,128;Inherit;False;Property;_CloudsMultiply;Clouds Multiply;16;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;85;-1536,0;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;77;-1920,-768;Inherit;False;Property;_AtmosphereFresnelScale;Atmosphere Fresnel Scale;8;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;78;-1920,-640;Inherit;False;Property;_AtmosphereFresnelPower;Atmosphere Fresnel Power;9;0;Create;True;0;0;0;False;0;False;5;5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;79;-1920,-896;Inherit;False;Property;_AtmosphereFresnelBias;Atmosphere Fresnel Bias;10;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;16;-3328,0;Inherit;False;Property;_TerrainLUTOffset;Terrain LUT Offset;4;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;15;-3328,-128;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;86;-1408,0;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FresnelNode;20;-1536,-896;Inherit;False;Standard;WorldNormal;ViewDir;True;True;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;73;-1152,-768;Inherit;False;Property;_AtmosphereDensity;Atmosphere Density;7;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;96;-2432,768;Inherit;False;Property;_EmissiveMinSmoothstep;Emissive Min Smoothstep;18;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;97;-2432,896;Inherit;False;Property;_EmissiveMaxSmoothstep;Emissive Max Smoothstep;19;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;87;-1280,0;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;75;-1152,-896;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;95;-2048,768;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;13;-3072,-128;Inherit;True;Property;_TerrainLUT;Terrain LUT;3;0;Create;True;0;0;0;False;0;False;-1;eefdc5db1c4430941b5062bc7d88bb38;eefdc5db1c4430941b5062bc7d88bb38;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;82;-2560,-384;Inherit;False;Property;_CloudsColor;Clouds Color;12;0;Create;True;0;0;0;False;0;False;0.8553458,1,0.9881043,0;0.854902,1,0.9882353,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;80;-896,-128;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SaturateNode;76;-896,-896;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;99;-1792,768;Inherit;True;Property;_EmissiveLUT;Emissive LUT;17;0;Create;True;0;0;0;False;3;Space(33);Header(Emissive);Space(13);False;-1;8ad064702fd0b194c8a1efd6835ee789;8ad064702fd0b194c8a1efd6835ee789;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;71;-1024,-1152;Inherit;False;Property;_AtmosphereColor;Atmosphere Color;6;0;Create;True;0;0;0;False;3;Space(33);Header(Atmosphere);Space(13);False;0.553459,0.9314483,1,0;0.5529412,0.9333333,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;12;-1408,896;Inherit;False;Property;_EmissiveStrength;Emissive Strength;20;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector3Node;63;-768,768;Inherit;True;Constant;_Vector0;Vector 0;8;0;Create;True;0;0;0;False;0;False;0,0,1;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SamplerNode;66;-768,1152;Inherit;True;Property;_NormalTexture;Normal Texture;21;0;Create;True;0;0;0;False;3;Space(33);Header(Normal);Space(13);False;-1;2d3291a140aecdd4a9c8ddd35348ffb0;2d3291a140aecdd4a9c8ddd35348ffb0;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;64;-384,896;Inherit;False;Property;_NormalIntensity;Normal Intensity;22;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;72;-640,-1152;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;11;-1408,768;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;69;-256,512;Inherit;False;Property;_Smoothness;Smoothness;1;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;62;-384,768;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;98;-384,-512;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;68;-256,384;Inherit;False;Property;_Specular;Specular;0;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;101;-1152,768;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;102;0,0;Float;False;True;-1;2;ASEMaterialInspector;0;0;StandardSpecular;Vefects/SH_Vefects_VFX_Planet;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;17;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;16;FLOAT4;0,0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;91;0;92;0
WireConnection;93;0;91;0
WireConnection;93;1;94;0
WireConnection;90;0;83;1
WireConnection;90;1;91;0
WireConnection;90;2;93;0
WireConnection;84;0;90;0
WireConnection;84;1;88;0
WireConnection;14;1;25;0
WireConnection;17;0;14;1
WireConnection;17;1;18;0
WireConnection;85;0;84;0
WireConnection;15;0;17;0
WireConnection;15;1;16;0
WireConnection;86;0;85;0
WireConnection;86;1;89;0
WireConnection;20;1;79;0
WireConnection;20;2;77;0
WireConnection;20;3;78;0
WireConnection;87;0;86;0
WireConnection;75;0;20;0
WireConnection;75;1;73;0
WireConnection;95;0;14;1
WireConnection;95;1;96;0
WireConnection;95;2;97;0
WireConnection;13;1;15;0
WireConnection;80;0;13;0
WireConnection;80;1;82;0
WireConnection;80;2;87;0
WireConnection;76;0;75;0
WireConnection;99;1;95;0
WireConnection;72;0;80;0
WireConnection;72;1;71;0
WireConnection;72;2;76;0
WireConnection;11;0;99;0
WireConnection;11;1;12;0
WireConnection;62;0;63;0
WireConnection;62;1;66;0
WireConnection;62;2;64;0
WireConnection;98;0;72;0
WireConnection;98;2;95;0
WireConnection;101;0;11;0
WireConnection;101;1;95;0
WireConnection;102;0;98;0
WireConnection;102;1;62;0
WireConnection;102;2;101;0
WireConnection;102;3;68;0
WireConnection;102;4;69;0
ASEEND*/
//CHKSM=B14C0B821A539F592CCBA7DB53082901B6AAD0B1