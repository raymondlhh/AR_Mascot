// Made with Amplify Shader Editor v1.9.3.3
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Vefects/SH_Vefects_VFX_Pyro_No_Blend"
{
	Properties
	{
		_Specular("Specular", Float) = 0
		_Smoothness("Smoothness", Float) = 0
		_SmokeTint("Smoke Tint", Color) = (1,1,1,0)
		_DFEroSS("DF Ero SS", Float) = 0
		_DFEroSSS("DF Ero SSS", Float) = 1
		[Space(33)][Header(Flipbook)][Space(13)]_Flipbook("Flipbook", 2D) = "white" {}
		_FlipbookColumns("Flipbook Columns", Float) = 8
		_FlipbookRows("Flipbook Rows", Float) = 8
		_FlipbookSpeed("Flipbook Speed", Float) = 64
		_FlipbookStartFrame("Flipbook Start Frame", Float) = 0
		[Space(33)][Header(Emissive)][Space(13)]_EmissiveLUT("Emissive LUT", 2D) = "white" {}
		_EmissiveMult("Emissive Mult", Float) = 1
		_EmissiveTint("Emissive Tint", Color) = (1,1,1,0)
		_EmissiveHueShift("Emissive Hue Shift", Float) = 0
		[Space(13)][Header(AR)][Space(13)]_Cull1("Cull", Float) = 2
		_Src1("Src", Float) = 5
		_Dst1("Dst", Float) = 10
		_ZWrite1("ZWrite", Float) = 0
		_ZTest1("ZTest", Float) = 2
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IsEmissive" = "true"  }
		Cull [_Cull1]
		ZWrite [_ZWrite1]
		ZTest [_ZTest1]
		Blend [_Src1] [_Dst1]
		CGINCLUDE
		#include "UnityCG.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		#undef TRANSFORM_TEX
		#define TRANSFORM_TEX(tex,name) float4(tex.xy * name##_ST.xy + name##_ST.zw, tex.z, tex.w)
		struct Input
		{
			float4 uv_texcoord;
			float4 screenPos;
			float4 vertexColor : COLOR;
		};

		uniform float _Cull1;
		uniform float _Src1;
		uniform float _Dst1;
		uniform float _ZTest1;
		uniform float _ZWrite1;
		uniform float4 _SmokeTint;
		uniform sampler2D _Flipbook;
		uniform float _FlipbookColumns;
		uniform float _FlipbookRows;
		uniform float _FlipbookSpeed;
		uniform float _FlipbookStartFrame;
		UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
		uniform float4 _CameraDepthTexture_TexelSize;
		uniform float _DFEroSS;
		uniform float _DFEroSSS;
		uniform sampler2D _EmissiveLUT;
		uniform float4 _EmissiveTint;
		uniform float _EmissiveHueShift;
		uniform float _EmissiveMult;
		uniform float _Specular;
		uniform float _Smoothness;


		float3 HSVToRGB( float3 c )
		{
			float4 K = float4( 1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0 );
			float3 p = abs( frac( c.xxx + K.xyz ) * 6.0 - K.www );
			return c.z * lerp( K.xxx, saturate( p - K.xxx ), c.y );
		}


		float3 RGBToHSV(float3 c)
		{
			float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
			float4 p = lerp( float4( c.bg, K.wz ), float4( c.gb, K.xy ), step( c.b, c.g ) );
			float4 q = lerp( float4( p.xyw, c.r ), float4( c.r, p.yzx ), step( p.x, c.r ) );
			float d = q.x - min( q.w, q.y );
			float e = 1.0e-10;
			return float3( abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
		}

		void surf( Input i , inout SurfaceOutputStandardSpecular o )
		{
			// *** BEGIN Flipbook UV Animation vars ***
			// Total tiles of Flipbook Texture
			float fbtotaltiles82 = _FlipbookColumns * _FlipbookRows;
			// Offsets for cols and rows of Flipbook Texture
			float fbcolsoffset82 = 1.0f / _FlipbookColumns;
			float fbrowsoffset82 = 1.0f / _FlipbookRows;
			// Speed of animation
			float fbspeed82 = i.uv_texcoord.z * _FlipbookSpeed;
			// UV Tiling (col and row offset)
			float2 fbtiling82 = float2(fbcolsoffset82, fbrowsoffset82);
			// UV Offset - calculate current tile linear index, and convert it to (X * coloffset, Y * rowoffset)
			// Calculate current tile linear index
			float fbcurrenttileindex82 = round( fmod( fbspeed82 + _FlipbookStartFrame, fbtotaltiles82) );
			fbcurrenttileindex82 += ( fbcurrenttileindex82 < 0) ? fbtotaltiles82 : 0;
			// Obtain Offset X coordinate from current tile linear index
			float fblinearindextox82 = round ( fmod ( fbcurrenttileindex82, _FlipbookColumns ) );
			// Multiply Offset X by coloffset
			float fboffsetx82 = fblinearindextox82 * fbcolsoffset82;
			// Obtain Offset Y coordinate from current tile linear index
			float fblinearindextoy82 = round( fmod( ( fbcurrenttileindex82 - fblinearindextox82 ) / _FlipbookColumns, _FlipbookRows ) );
			// Reverse Y to get tiles from Top to Bottom
			fblinearindextoy82 = (int)(_FlipbookRows-1) - fblinearindextoy82;
			// Multiply Offset Y by rowoffset
			float fboffsety82 = fblinearindextoy82 * fbrowsoffset82;
			// UV Offset
			float2 fboffset82 = float2(fboffsetx82, fboffsety82);
			// Flipbook UV
			half2 fbuv82 = i.uv_texcoord.xy * fbtiling82 + fboffset82;
			// *** END Flipbook UV Animation vars ***
			float4 tex2DNode83 = tex2D( _Flipbook, fbuv82 );
			o.Albedo = ( _SmokeTint * saturate( tex2DNode83.r ) ).rgb;
			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
			float4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
			ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
			float smoothstepResult123 = smoothstep( _DFEroSS , ( _DFEroSS + _DFEroSSS ) , tex2DNode83.r);
			float screenDepth121 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy ));
			float distanceDepth121 = saturate( ( screenDepth121 - LinearEyeDepth( ase_screenPosNorm.z ) ) / ( smoothstepResult123 ) );
			float temp_output_127_0 = saturate( ( saturate( tex2DNode83.a ) * distanceDepth121 ) );
			float2 temp_cast_1 = (saturate( tex2DNode83.g )).xx;
			float3 hsvTorgb4_g5 = RGBToHSV( ( ( i.vertexColor * tex2D( _EmissiveLUT, temp_cast_1 ) ) * _EmissiveTint ).rgb );
			float3 hsvTorgb8_g5 = HSVToRGB( float3(( hsvTorgb4_g5.x + _EmissiveHueShift ),( hsvTorgb4_g5.y + 0.0 ),( hsvTorgb4_g5.z + 0.0 )) );
			o.Emission = ( temp_output_127_0 * ( ( saturate( hsvTorgb8_g5 ) * i.uv_texcoord.w ) * _EmissiveMult ) );
			float3 temp_cast_3 = (_Specular).xxx;
			o.Specular = temp_cast_3;
			o.Smoothness = _Smoothness;
			o.Alpha = saturate( ( temp_output_127_0 * i.vertexColor.a ) );
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
			sampler3D _DitherMaskLOD;
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float4 customPack1 : TEXCOORD1;
				float3 worldPos : TEXCOORD2;
				float4 screenPos : TEXCOORD3;
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
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				o.customPack1.xyzw = customInputData.uv_texcoord;
				o.customPack1.xyzw = v.texcoord;
				o.worldPos = worldPos;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				o.screenPos = ComputeScreenPos( o.pos );
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
				surfIN.uv_texcoord = IN.customPack1.xyzw;
				float3 worldPos = IN.worldPos;
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.screenPos = IN.screenPos;
				surfIN.vertexColor = IN.color;
				SurfaceOutputStandardSpecular o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandardSpecular, o )
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
Node;AmplifyShaderEditor.TextureCoordinatesNode;78;-3200,0;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;77;-3200,256;Inherit;False;Property;_FlipbookColumns;Flipbook Columns;7;0;Create;True;0;0;0;False;0;False;8;4;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;76;-3200,384;Inherit;False;Property;_FlipbookRows;Flipbook Rows;8;0;Create;True;0;0;0;False;0;False;8;4;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;80;-3200,128;Inherit;False;Property;_FlipbookStartFrame;Flipbook Start Frame;10;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;79;-3200,512;Inherit;False;Property;_FlipbookSpeed;Flipbook Speed;9;0;Create;True;0;0;0;False;0;False;64;64;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TexCoordVertexDataNode;106;-3200,896;Inherit;False;0;4;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TFHCFlipBookUVAnimation;82;-2560,256;Inherit;False;0;0;6;0;FLOAT2;0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.TexturePropertyNode;72;-2560,-256;Inherit;True;Property;_Flipbook;Flipbook;6;0;Create;True;0;0;0;False;3;Space(33);Header(Flipbook);Space(13);False;54fa908065a6dc6428a7405ad5dedcf9;54fa908065a6dc6428a7405ad5dedcf9;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.SamplerNode;83;-2176,0;Inherit;True;Property;_TextureSample0;Texture Sample 0;12;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Instance;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;124;-1664,640;Inherit;False;Property;_DFEroSS;DF Ero SS;4;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;125;-1664,736;Inherit;False;Property;_DFEroSSS;DF Ero SSS;5;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;112;-1792,896;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;126;-1504,640;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode;37;-1536,-384;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;113;-1536,1024;Inherit;True;Property;_EmissiveLUT;Emissive LUT;11;0;Create;True;0;0;0;False;3;Space(33);Header(Emissive);Space(13);False;-1;f57767c513f8ca64dbef45a46c4349a2;f57767c513f8ca64dbef45a46c4349a2;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SmoothstepOpNode;123;-1280,512;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;118;-896,1152;Inherit;False;Property;_EmissiveTint;Emissive Tint;13;0;Create;True;0;0;0;False;0;False;1,1,1,0;1,1,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;89;-1152,1024;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.DepthFade;121;-1152,384;Inherit;False;True;True;False;2;1;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;105;-1536,128;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;115;-896,1024;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;117;-640,1168;Inherit;False;Property;_EmissiveHueShift;Emissive Hue Shift;14;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;122;-1152,256;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;116;-640,1024;Inherit;False;HueShift;-1;;5;9f07e9ddd8ab81c47b3582f22189b65b;0;4;14;COLOR;0,0,0,0;False;15;FLOAT;0;False;16;FLOAT;0;False;17;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SaturateNode;127;-896,256;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;114;-256,1152;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;62;-256,896;Inherit;False;Property;_EmissiveMult;Emissive Mult;12;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;49;459.26,31.68577;Inherit;False;1238;166;Auto Register Variables;5;54;53;52;51;50;Lush was here! <3;0.4872068,0.2971698,1,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;104;-640,256;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;90;-1792,0;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;119;-1536,-768;Inherit;False;Property;_SmokeTint;Smoke Tint;3;0;Create;True;0;0;0;False;0;False;1,1,1,0;1,1,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;61;-256,1024;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;50;507.2599,79.68589;Inherit;False;Property;_Cull1;Cull;15;0;Create;True;0;0;0;True;3;Space(13);Header(AR);Space(13);False;2;2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;51;763.2599,79.68589;Inherit;False;Property;_Src1;Src;16;0;Create;True;0;0;0;True;0;False;5;5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;52;1019.26,79.68589;Inherit;False;Property;_Dst1;Dst;17;0;Create;True;0;0;0;True;0;False;10;10;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;53;1531.26,79.68589;Inherit;False;Property;_ZTest1;ZTest;19;0;Create;True;0;0;0;True;0;False;2;2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;54;1275.26,79.68589;Inherit;False;Property;_ZWrite1;ZWrite;18;0;Create;True;0;0;0;True;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;107;-640,0;Inherit;False;Property;_Specular;Specular;1;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;108;-640,128;Inherit;False;Property;_Smoothness;Smoothness;2;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;120;-768,-384;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;130;-256,768;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SaturateNode;67;-256,256;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;131;0,0;Float;False;True;-1;2;ASEMaterialInspector;0;0;StandardSpecular;Vefects/SH_Vefects_VFX_Pyro_No_Blend;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;True;_ZWrite1;0;True;_ZTest1;False;0;False;;0;False;;False;0;Custom;0.5;True;True;0;False;Transparent;;Transparent;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;2;5;True;_Src1;10;True;_Dst1;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;0;-1;-1;-1;0;False;0;0;True;_Cull1;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;17;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;16;FLOAT4;0,0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;82;0;78;0
WireConnection;82;1;77;0
WireConnection;82;2;76;0
WireConnection;82;3;79;0
WireConnection;82;4;80;0
WireConnection;82;5;106;3
WireConnection;83;0;72;0
WireConnection;83;1;82;0
WireConnection;112;0;83;2
WireConnection;126;0;124;0
WireConnection;126;1;125;0
WireConnection;113;1;112;0
WireConnection;123;0;83;1
WireConnection;123;1;124;0
WireConnection;123;2;126;0
WireConnection;89;0;37;0
WireConnection;89;1;113;0
WireConnection;121;0;123;0
WireConnection;105;0;83;4
WireConnection;115;0;89;0
WireConnection;115;1;118;0
WireConnection;122;0;105;0
WireConnection;122;1;121;0
WireConnection;116;14;115;0
WireConnection;116;15;117;0
WireConnection;127;0;122;0
WireConnection;114;0;116;0
WireConnection;114;1;106;4
WireConnection;104;0;127;0
WireConnection;104;1;37;4
WireConnection;90;0;83;1
WireConnection;61;0;114;0
WireConnection;61;1;62;0
WireConnection;120;0;119;0
WireConnection;120;1;90;0
WireConnection;130;0;127;0
WireConnection;130;1;61;0
WireConnection;67;0;104;0
WireConnection;131;0;120;0
WireConnection;131;2;130;0
WireConnection;131;3;107;0
WireConnection;131;4;108;0
WireConnection;131;9;67;0
ASEEND*/
//CHKSM=AC1066244A3E24964DCE6349CA8E52D44526EA49