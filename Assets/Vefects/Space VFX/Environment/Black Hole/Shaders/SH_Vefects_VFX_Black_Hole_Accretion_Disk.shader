// Made with Amplify Shader Editor v1.9.3.3
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Vefects/SH_Vefects_VFX_Black_Hole_Accretion_Disk"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,0)
		_EmissiveStrength("EmissiveStrength", Float) = 50
		_VMaskPower("V Mask Power", Float) = 1
		[Space(33)][Header(Black Hole Noise)][Space(13)]_BlackHoleNoise("Black Hole Noise", 2D) = "white" {}
		_BlackHoleNoise01Selector("Black Hole Noise 01 Selector", Vector) = (1,0,0,0)
		_BlackHoleNoise01Scale("Black Hole Noise 01 Scale", Vector) = (1,1,0,0)
		_BlackHoleNoise01Pan("Black Hole Noise 01 Pan", Vector) = (-0.001,0.001,0,0)
		_BlackHoleNoise02Selector("Black Hole Noise 02 Selector", Vector) = (1,0,0,0)
		_BlackHoleNoise02Scale("Black Hole Noise 02 Scale", Vector) = (1,1,0,0)
		_BlackHoleNoise02Pan("Black Hole Noise 02 Pan", Vector) = (-0.002,0.002,0,0)
		_NoiseSSEro("Noise SS Ero", Float) = 0
		_NoiseSSEroSmooth("Noise SS Ero Smooth", Float) = 1
		[Space(33)][Header(WPO)][Space(13)]_WPOStrength("WPO Strength", Float) = 1
		[Space(33)][Header(AR)][Space(13)]_Cull("Cull", Float) = 0
		_Src("Src", Float) = 5
		_Dst("Dst", Float) = 10
		_ZWrite("ZWrite", Float) = 0
		_ZTest("ZTest", Float) = 2
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IsEmissive" = "true"  }
		Cull [_Cull]
		ZWrite [_ZWrite]
		ZTest [_ZTest]
		Blend [_Src] [_Dst]
		CGINCLUDE
		#include "UnityShaderVariables.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		struct Input
		{
			float2 uv_texcoord;
			float4 vertexColor : COLOR;
		};

		uniform float _Src;
		uniform float _Dst;
		uniform float _ZWrite;
		uniform float _ZTest;
		uniform float _Cull;
		uniform float _NoiseSSEro;
		uniform float _NoiseSSEroSmooth;
		uniform sampler2D _BlackHoleNoise;
		uniform float2 _BlackHoleNoise01Pan;
		uniform float2 _BlackHoleNoise01Scale;
		uniform float4 _BlackHoleNoise01Selector;
		uniform float2 _BlackHoleNoise02Pan;
		uniform float2 _BlackHoleNoise02Scale;
		uniform float4 _BlackHoleNoise02Selector;
		uniform float _WPOStrength;
		uniform float4 _Color;
		uniform float _VMaskPower;
		uniform float _EmissiveStrength;

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float2 panner104 = ( 1.0 * _Time.y * _BlackHoleNoise01Pan + ( v.texcoord.xy * _BlackHoleNoise01Scale ));
			float dotResult98 = dot( tex2Dlod( _BlackHoleNoise, float4( panner104, 0, 0.0) ) , _BlackHoleNoise01Selector );
			float2 panner106 = ( 1.0 * _Time.y * _BlackHoleNoise02Pan + ( v.texcoord.xy * _BlackHoleNoise02Scale ));
			float dotResult99 = dot( tex2Dlod( _BlackHoleNoise, float4( panner106, 0, 0.0) ) , _BlackHoleNoise02Selector );
			float smoothstepResult89 = smoothstep( _NoiseSSEro , ( _NoiseSSEro + _NoiseSSEroSmooth ) , saturate( ( saturate( dotResult98 ) * saturate( dotResult99 ) ) ));
			float temp_output_85_0 = saturate( smoothstepResult89 );
			float3 ase_objectScale = float3( length( unity_ObjectToWorld[ 0 ].xyz ), length( unity_ObjectToWorld[ 1 ].xyz ), length( unity_ObjectToWorld[ 2 ].xyz ) );
			float3 ase_vertexNormal = v.normal.xyz;
			v.vertex.xyz += ( ( ( temp_output_85_0 * length( ase_objectScale ) ) * ase_vertexNormal ) * ( _WPOStrength / 1000000.0 ) );
			v.vertex.w = 1;
		}

		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			o.Emission = ( ( ( _Color * pow( i.uv_texcoord.y , _VMaskPower ) ) * i.vertexColor.r ) * _EmissiveStrength ).rgb;
			float2 panner104 = ( 1.0 * _Time.y * _BlackHoleNoise01Pan + ( i.uv_texcoord * _BlackHoleNoise01Scale ));
			float dotResult98 = dot( tex2D( _BlackHoleNoise, panner104 ) , _BlackHoleNoise01Selector );
			float2 panner106 = ( 1.0 * _Time.y * _BlackHoleNoise02Pan + ( i.uv_texcoord * _BlackHoleNoise02Scale ));
			float dotResult99 = dot( tex2D( _BlackHoleNoise, panner106 ) , _BlackHoleNoise02Selector );
			float smoothstepResult89 = smoothstep( _NoiseSSEro , ( _NoiseSSEro + _NoiseSSEroSmooth ) , saturate( ( saturate( dotResult98 ) * saturate( dotResult99 ) ) ));
			float temp_output_85_0 = saturate( smoothstepResult89 );
			o.Alpha = saturate( ( i.vertexColor.r * saturate( pow( saturate( ( i.uv_texcoord.y * temp_output_85_0 ) ) , 2.0 ) ) ) );
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Unlit keepalpha fullforwardshadows vertex:vertexDataFunc 

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
			sampler3D _DitherMaskLOD;
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float2 customPack1 : TEXCOORD1;
				float3 worldPos : TEXCOORD2;
				half4 color : COLOR0;
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
				vertexDataFunc( v, customInputData );
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				o.worldPos = worldPos;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				o.color = v.color;
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
				float3 worldPos = IN.worldPos;
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.vertexColor = IN.color;
				SurfaceOutput o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutput, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				half alphaRef = tex3D( _DitherMaskLOD, float3( vpos.xy * 0.25, o.Alpha * 0.9375 ) ).a;
				clip( alphaRef - 0.01 );
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
Node;AmplifyShaderEditor.TextureCoordinatesNode;103;-4752,1664;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;105;-4752,2048;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector2Node;111;-4480,1792;Inherit;False;Property;_BlackHoleNoise01Scale;Black Hole Noise 01 Scale;6;0;Create;True;0;0;0;False;0;False;1,1;1,1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.Vector2Node;112;-4480,2176;Inherit;False;Property;_BlackHoleNoise02Scale;Black Hole Noise 02 Scale;9;0;Create;True;0;0;0;False;0;False;1,1;1,1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.Vector2Node;109;-4096,2176;Inherit;False;Property;_BlackHoleNoise02Pan;Black Hole Noise 02 Pan;10;0;Create;True;0;0;0;False;0;False;-0.002,0.002;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.Vector2Node;108;-4096,1792;Inherit;False;Property;_BlackHoleNoise01Pan;Black Hole Noise 01 Pan;7;0;Create;True;0;0;0;False;0;False;-0.001,0.001;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;113;-4480,2048;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;114;-4480,1664;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PannerNode;104;-4096,1664;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PannerNode;106;-4096,2048;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TexturePropertyNode;94;-4224,1408;Inherit;True;Property;_BlackHoleNoise;Black Hole Noise;4;0;Create;True;0;0;0;False;3;Space(33);Header(Black Hole Noise);Space(13);False;d55e5b65bce22ab4da9a14a757120624;d55e5b65bce22ab4da9a14a757120624;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.SamplerNode;95;-3712,1664;Inherit;True;Property;_TextureSample0;Texture Sample 0;11;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Instance;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;96;-3712,2048;Inherit;True;Property;_TextureSample1;Texture Sample 0;11;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Instance;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector4Node;101;-3328,2176;Inherit;False;Property;_BlackHoleNoise02Selector;Black Hole Noise 02 Selector;8;0;Create;True;0;0;0;False;0;False;1,0,0,0;1,0,0,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector4Node;100;-3328,1792;Inherit;False;Property;_BlackHoleNoise01Selector;Black Hole Noise 01 Selector;5;0;Create;True;0;0;0;False;0;False;1,0,0,0;1,0,0,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DotProductOpNode;98;-3072,1664;Inherit;False;2;0;COLOR;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;99;-3072,2048;Inherit;False;2;0;COLOR;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;91;-2688,1664;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;92;-2688,2048;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;86;-2048,1920;Inherit;False;Property;_NoiseSSEro;Noise SS Ero;11;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;87;-2048,2048;Inherit;False;Property;_NoiseSSEroSmooth;Noise SS Ero Smooth;12;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;93;-2432,1792;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;88;-1792,1920;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;90;-2048,1792;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;89;-1792,1792;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;66;-1792,384;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SaturateNode;85;-1408,1792;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;74;-1536,640;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;77;-1280,640;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ObjectScaleNode;78;-1408,2176;Inherit;False;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;115;-1152,512;Inherit;False;Property;_VMaskPower;V Mask Power;3;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;75;-1024,640;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.LengthOpNode;79;-1152,2176;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;64;-1792,0;Inherit;False;Property;_Color;Color;1;0;Create;True;0;0;0;False;0;False;1,1,1,0;1,1,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PowerNode;69;-1152,384;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;20;False;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode;65;-896,128;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SaturateNode;76;-768,640;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;80;-896,2048;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NormalVertexDataNode;82;-640,2176;Inherit;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;84;-256,2304;Inherit;False;Property;_WPOStrength;WPO Strength;13;0;Create;True;0;0;0;False;3;Space(33);Header(WPO);Space(13);False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;70;-1152,0;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;57;334,-50;Inherit;False;1252;162.6667;AR;5;53;54;55;56;51;AR;0,0,0,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;72;-512,256;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;81;-640,2048;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;110;-256,2176;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;1000000;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;12;-272,-128;Inherit;False;Property;_EmissiveStrength;EmissiveStrength;2;0;Create;True;0;0;0;False;0;False;50;50;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;71;-512,0;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.OneMinusNode;67;-1536,384;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;53;640,0;Inherit;False;Property;_Src;Src;15;0;Create;True;0;0;0;True;0;False;5;5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;54;896,0;Inherit;False;Property;_Dst;Dst;16;0;Create;True;0;0;0;True;0;False;10;10;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;55;1152,0;Inherit;False;Property;_ZWrite;ZWrite;17;0;Create;True;0;0;0;True;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;56;1408,0;Inherit;False;Property;_ZTest;ZTest;18;0;Create;True;0;0;0;True;0;False;2;2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;51;384,0;Inherit;False;Property;_Cull;Cull;14;0;Create;True;0;0;0;True;3;Space(33);Header(AR);Space(13);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;11;-256,0;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SaturateNode;73;-256,256;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;83;-256,2048;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;116;0,0;Float;False;True;-1;2;ASEMaterialInspector;0;0;Unlit;Vefects/SH_Vefects_VFX_Black_Hole_Accretion_Disk;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;True;_ZWrite;0;True;_ZTest;False;0;False;;0;False;;False;0;Custom;0.5;True;True;0;False;Transparent;;Transparent;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;1;5;True;_Src;10;True;_Dst;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;0;-1;-1;-1;0;False;0;0;True;_Cull;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;16;FLOAT4;0,0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;113;0;105;0
WireConnection;113;1;112;0
WireConnection;114;0;103;0
WireConnection;114;1;111;0
WireConnection;104;0;114;0
WireConnection;104;2;108;0
WireConnection;106;0;113;0
WireConnection;106;2;109;0
WireConnection;95;0;94;0
WireConnection;95;1;104;0
WireConnection;96;0;94;0
WireConnection;96;1;106;0
WireConnection;98;0;95;0
WireConnection;98;1;100;0
WireConnection;99;0;96;0
WireConnection;99;1;101;0
WireConnection;91;0;98;0
WireConnection;92;0;99;0
WireConnection;93;0;91;0
WireConnection;93;1;92;0
WireConnection;88;0;86;0
WireConnection;88;1;87;0
WireConnection;90;0;93;0
WireConnection;89;0;90;0
WireConnection;89;1;86;0
WireConnection;89;2;88;0
WireConnection;85;0;89;0
WireConnection;74;0;66;2
WireConnection;74;1;85;0
WireConnection;77;0;74;0
WireConnection;75;0;77;0
WireConnection;79;0;78;0
WireConnection;69;0;66;2
WireConnection;69;1;115;0
WireConnection;76;0;75;0
WireConnection;80;0;85;0
WireConnection;80;1;79;0
WireConnection;70;0;64;0
WireConnection;70;1;69;0
WireConnection;72;0;65;1
WireConnection;72;1;76;0
WireConnection;81;0;80;0
WireConnection;81;1;82;0
WireConnection;110;0;84;0
WireConnection;71;0;70;0
WireConnection;71;1;65;1
WireConnection;67;0;66;2
WireConnection;11;0;71;0
WireConnection;11;1;12;0
WireConnection;73;0;72;0
WireConnection;83;0;81;0
WireConnection;83;1;110;0
WireConnection;116;2;11;0
WireConnection;116;9;73;0
WireConnection;116;11;83;0
ASEEND*/
//CHKSM=2E00BBA8FB36183B74AF2E582917F57DAF8D196E