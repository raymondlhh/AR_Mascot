// Made with Amplify Shader Editor v1.9.3.3
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Vefects/SH_Vefects_VFX_Space_Background_HDRI"
{
	Properties
	{
		_OverallEmissiveIntensity("Overall Emissive Intensity", Float) = 1
		[Space(33)][Header(Background)][Space(13)]_Background("Background", CUBE) = "white" {}
		_BGEmissiveIntensity("BG Emissive Intensity", Float) = 1
		_BGShiftHue("BG Shift Hue", Float) = 0
		_BGDesaturation("BG Desaturation", Range( -3 , 1)) = 0
		_BGRotateHorizontal("BG Rotate Horizontal", Float) = 0
		_BGRotateHorizontalSpeed("BG Rotate Horizontal Speed", Float) = 0
		_BGRotateVertical("BG Rotate Vertical", Float) = 0
		_BGRotateVerticalSpeed("BG Rotate Vertical Speed", Float) = 0
		_BGNoiseDistortionStrength("BG Noise Distortion Strength", Float) = 0.03
		_BGNoiseDistortionScale("BG Noise Distortion Scale", Float) = 1
		_BGNoiseDistortionSize("BG Noise Distortion Size", Float) = 1234
		_BGNoiseDistortionSpeed("BG Noise Distortion Speed", Float) = 0.01
		[Space(33)][Header(Stars)][Space(13)]_Stars("Stars", CUBE) = "white" {}
		_StarsColor("Stars Color", Color) = (1,1,1,0)
		_StarsEmissiveIntensity("Stars Emissive Intensity", Float) = 1
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf Unlit keepalpha addshadow fullforwardshadows vertex:vertexDataFunc 
		struct Input
		{
			float3 vertexToFrag27_g2;
			float3 worldPos;
		};

		uniform samplerCUBE _Background;
		uniform float _BGRotateHorizontal;
		uniform float _BGRotateHorizontalSpeed;
		uniform float _BGRotateVertical;
		uniform float _BGRotateVerticalSpeed;
		uniform float _BGNoiseDistortionSize;
		uniform float _BGNoiseDistortionSpeed;
		uniform float _BGNoiseDistortionScale;
		uniform float _BGNoiseDistortionStrength;
		uniform float _BGShiftHue;
		uniform float _BGDesaturation;
		uniform float _BGEmissiveIntensity;
		uniform samplerCUBE _Stars;
		uniform float4 _StarsColor;
		uniform float _StarsEmissiveIntensity;
		uniform float _OverallEmissiveIntensity;


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

		float3 mod3D289( float3 x ) { return x - floor( x / 289.0 ) * 289.0; }

		float4 mod3D289( float4 x ) { return x - floor( x / 289.0 ) * 289.0; }

		float4 permute( float4 x ) { return mod3D289( ( x * 34.0 + 1.0 ) * x ); }

		float4 taylorInvSqrt( float4 r ) { return 1.79284291400159 - r * 0.85373472095314; }

		float snoise( float3 v )
		{
			const float2 C = float2( 1.0 / 6.0, 1.0 / 3.0 );
			float3 i = floor( v + dot( v, C.yyy ) );
			float3 x0 = v - i + dot( i, C.xxx );
			float3 g = step( x0.yzx, x0.xyz );
			float3 l = 1.0 - g;
			float3 i1 = min( g.xyz, l.zxy );
			float3 i2 = max( g.xyz, l.zxy );
			float3 x1 = x0 - i1 + C.xxx;
			float3 x2 = x0 - i2 + C.yyy;
			float3 x3 = x0 - 0.5;
			i = mod3D289( i);
			float4 p = permute( permute( permute( i.z + float4( 0.0, i1.z, i2.z, 1.0 ) ) + i.y + float4( 0.0, i1.y, i2.y, 1.0 ) ) + i.x + float4( 0.0, i1.x, i2.x, 1.0 ) );
			float4 j = p - 49.0 * floor( p / 49.0 );  // mod(p,7*7)
			float4 x_ = floor( j / 7.0 );
			float4 y_ = floor( j - 7.0 * x_ );  // mod(j,N)
			float4 x = ( x_ * 2.0 + 0.5 ) / 7.0 - 1.0;
			float4 y = ( y_ * 2.0 + 0.5 ) / 7.0 - 1.0;
			float4 h = 1.0 - abs( x ) - abs( y );
			float4 b0 = float4( x.xy, y.xy );
			float4 b1 = float4( x.zw, y.zw );
			float4 s0 = floor( b0 ) * 2.0 + 1.0;
			float4 s1 = floor( b1 ) * 2.0 + 1.0;
			float4 sh = -step( h, 0.0 );
			float4 a0 = b0.xzyw + s0.xzyw * sh.xxyy;
			float4 a1 = b1.xzyw + s1.xzyw * sh.zzww;
			float3 g0 = float3( a0.xy, h.x );
			float3 g1 = float3( a0.zw, h.y );
			float3 g2 = float3( a1.xy, h.z );
			float3 g3 = float3( a1.zw, h.w );
			float4 norm = taylorInvSqrt( float4( dot( g0, g0 ), dot( g1, g1 ), dot( g2, g2 ), dot( g3, g3 ) ) );
			g0 *= norm.x;
			g1 *= norm.y;
			g2 *= norm.z;
			g3 *= norm.w;
			float4 m = max( 0.6 - float4( dot( x0, x0 ), dot( x1, x1 ), dot( x2, x2 ), dot( x3, x3 ) ), 0.0 );
			m = m* m;
			m = m* m;
			float4 px = float4( dot( x0, g0 ), dot( x1, g1 ), dot( x2, g2 ), dot( x3, g3 ) );
			return 42.0 * dot( m, px);
		}


		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float2 appendResult32_g2 = (float2(( _BGRotateHorizontal + ( _Time.y * _BGRotateHorizontalSpeed ) ) , ( _BGRotateVertical + ( _Time.y * _BGRotateVerticalSpeed ) )));
			float2 break8_g2 = radians( appendResult32_g2 );
			float temp_output_13_0_g2 = cos( break8_g2.x );
			float temp_output_9_0_g2 = sin( break8_g2.x );
			float3 appendResult16_g2 = (float3(temp_output_13_0_g2 , 0.0 , -temp_output_9_0_g2));
			float3 appendResult18_g2 = (float3(0.0 , 1.0 , 0.0));
			float3 appendResult19_g2 = (float3(temp_output_9_0_g2 , 0.0 , temp_output_13_0_g2));
			float3 appendResult15_g2 = (float3(1.0 , 0.0 , 0.0));
			float temp_output_12_0_g2 = cos( break8_g2.y );
			float temp_output_10_0_g2 = sin( break8_g2.y );
			float3 appendResult20_g2 = (float3(0.0 , temp_output_12_0_g2 , -temp_output_10_0_g2));
			float3 appendResult17_g2 = (float3(0.0 , temp_output_10_0_g2 , temp_output_12_0_g2));
			float3 ase_worldPos = mul( unity_ObjectToWorld, v.vertex );
			float3 normalizeResult25_g2 = normalize( ase_worldPos );
			o.vertexToFrag27_g2 = mul( mul( float3x3(appendResult16_g2, appendResult18_g2, appendResult19_g2), float3x3(appendResult15_g2, appendResult20_g2, appendResult17_g2) ), normalizeResult25_g2 );
		}

		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			float3 temp_output_133_0 = i.vertexToFrag27_g2;
			float3 ase_worldPos = i.worldPos;
			float simplePerlin3D102 = snoise( ( ( ( ase_worldPos / _BGNoiseDistortionSize ) + ( ( _BGNoiseDistortionSpeed * _Time.y ) * _BGNoiseDistortionScale ) ) / _BGNoiseDistortionScale ) );
			simplePerlin3D102 = simplePerlin3D102*0.5 + 0.5;
			float simplePerlin3D104 = snoise( ( ( ( ase_worldPos / ( _BGNoiseDistortionSize * 5.0 ) ) + ( ( ( _BGNoiseDistortionSpeed / 2.0 ) * _Time.y ) * _BGNoiseDistortionScale ) ) / _BGNoiseDistortionScale ) );
			simplePerlin3D104 = simplePerlin3D104*0.5 + 0.5;
			float temp_output_96_0 = ( ( simplePerlin3D102 * simplePerlin3D104 ) * _BGNoiseDistortionStrength );
			float3 hsvTorgb4_g1 = RGBToHSV( texCUBE( _Background, ( temp_output_133_0 + temp_output_96_0 ) ).rgb );
			float3 hsvTorgb8_g1 = HSVToRGB( float3(( hsvTorgb4_g1.x + _BGShiftHue ),( hsvTorgb4_g1.y + 0.0 ),( hsvTorgb4_g1.z + 0.0 )) );
			float3 desaturateInitialColor54 = saturate( hsvTorgb8_g1 );
			float desaturateDot54 = dot( desaturateInitialColor54, float3( 0.299, 0.587, 0.114 ));
			float3 desaturateVar54 = lerp( desaturateInitialColor54, desaturateDot54.xxx, _BGDesaturation );
			float3 normalizeResult95 = normalize( temp_output_133_0 );
			o.Emission = ( ( float4( ( desaturateVar54 * _BGEmissiveIntensity ) , 0.0 ) + ( ( texCUBE( _Stars, normalizeResult95 ) * _StarsColor ) * _StarsEmissiveIntensity ) ) * _OverallEmissiveIntensity ).rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=19303
Node;AmplifyShaderEditor.RangedFloatNode;115;-5248,1408;Inherit;False;Property;_BGNoiseDistortionSpeed;BG Noise Distortion Speed;18;0;Create;True;0;0;0;False;0;False;0.01;0.01;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;114;-5248,1536;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;120;-5248,2176;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;127;-4992,2048;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;109;-4992,1280;Inherit;False;Property;_BGNoiseDistortionSize;BG Noise Distortion Size;17;0;Create;True;0;0;0;False;0;False;1234;1234;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;108;-5248,1152;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;111;-4992,1408;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;117;-5248,1664;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;126;-4992,1792;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;118;-4992,2176;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;112;-4736,1408;Inherit;False;Property;_BGNoiseDistortionScale;BG Noise Distortion Scale;16;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;107;-4992,1152;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;110;-4736,1280;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;123;-4992,1664;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;124;-4736,1792;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;106;-4736,1152;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;122;-4736,1664;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;105;-4480,1152;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;116;-4480,1664;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleTimeNode;140;-3584,-128;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;143;-3584,0;Inherit;False;Property;_BGRotateHorizontalSpeed;BG Rotate Horizontal Speed;8;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;144;-3584,256;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;146;-3584,384;Inherit;False;Property;_BGRotateVerticalSpeed;BG Rotate Vertical Speed;10;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;102;-4096,1152;Inherit;True;Simplex3D;True;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;104;-4096,1664;Inherit;True;Simplex3D;True;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;135;-3584,128;Inherit;False;Property;_BGRotateVertical;BG Rotate Vertical;9;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;136;-3584,-256;Inherit;False;Property;_BGRotateHorizontal;BG Rotate Horizontal;7;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;139;-3328,-128;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;145;-3328,256;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;99;-3712,1152;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;100;-3328,1280;Inherit;False;Property;_BGNoiseDistortionStrength;BG Noise Distortion Strength;15;0;Create;True;0;0;0;False;0;False;0.03;0.03;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;141;-3200,-256;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;147;-3200,128;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;96;-3328,1152;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;133;-2944,-256;Inherit;False;RotateCubemap2D;-1;;2;395373cb78b7852418d091b9daed3a57;0;2;28;FLOAT;0;False;29;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;137;-2560,-256;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;59;-1664,-112;Inherit;False;Property;_BGShiftHue;BG Shift Hue;3;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.NormalizeNode;95;-2304,896;Inherit;False;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode;57;-2048,-256;Inherit;True;Property;_Background;Background;1;0;Create;True;0;0;0;False;3;Space(33);Header(Background);Space(13);False;-1;74203515ba73b4347b957c4be4981bbc;74203515ba73b4347b957c4be4981bbc;True;0;False;white;LockedToCube;False;Object;-1;Auto;Cube;8;0;SAMPLERCUBE;;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;58;-1664,-256;Inherit;False;HueShift;-1;;1;9f07e9ddd8ab81c47b3582f22189b65b;0;4;14;COLOR;0,0,0,0;False;15;FLOAT;0;False;16;FLOAT;0;False;17;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;56;-1408,-128;Inherit;False;Property;_BGDesaturation;BG Desaturation;4;0;Create;True;0;0;0;False;0;False;0;0;-3;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;93;-1664,1024;Inherit;False;Property;_StarsColor;Stars Color;20;0;Create;True;0;0;0;False;0;False;1,1,1,0;1,1,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;92;-2048,896;Inherit;True;Property;_Stars;Stars;19;0;Create;True;0;0;0;False;3;Space(33);Header(Stars);Space(13);False;-1;8fd059db5e069fa45b9b0a0843c52bd8;8fd059db5e069fa45b9b0a0843c52bd8;True;0;False;white;LockedToCube;False;Object;-1;Auto;Cube;8;0;SAMPLERCUBE;;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;90;-1024,-128;Inherit;False;Property;_BGEmissiveIntensity;BG Emissive Intensity;2;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DesaturateOpNode;54;-1408,-256;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;53;-1024,1024;Inherit;False;Property;_StarsEmissiveIntensity;Stars Emissive Intensity;21;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;91;-1664,896;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;89;-1024,-256;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;52;-1024,896;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;51;-640,0;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;12;-384,128;Inherit;False;Property;_OverallEmissiveIntensity;Overall Emissive Intensity;0;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;11;-256,0;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;60;-1664,0;Inherit;False;Property;_BGShiftSaturation;BG Shift Saturation;5;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;61;-1664,128;Inherit;False;Property;_BGShiftValue;BG Shift Value;6;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RotateAboutAxisNode;65;-5376,-640;Inherit;False;False;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.Vector3Node;66;-5632,-640;Inherit;False;Constant;_Vector0;Vector 0;14;0;Create;True;0;0;0;False;0;False;1,0,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;67;-6272,-384;Inherit;False;Property;_BGRotationAngleX;BG Rotation Angle X;11;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;69;-6016,-256;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;68;-6272,-256;Inherit;False;Property;_BGPanSpeedVertical;BG Pan Speed Vertical;13;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;71;-6272,-128;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;72;-5760,-384;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FractNode;73;-5632,-384;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RotateAboutAxisNode;75;-6016,-1280;Inherit;False;False;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;78;-6656,-896;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;80;-6912,-768;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;79;-6912,-896;Inherit;False;Property;_BGPanSpeedHorizontal;BG Pan Speed Horizontal;14;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;84;-6656,-1024;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;85;-6656,-1152;Inherit;False;Constant;_Float2;Float 2;18;0;Create;True;0;0;0;False;0;False;-1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;86;-6400,-1024;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;88;-5760,-1024;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.Vector3Node;76;-6272,-1280;Inherit;False;Constant;_Vector2;Vector 0;14;0;Create;True;0;0;0;False;0;False;0,0,1;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.FractNode;82;-6272,-1024;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector3Node;74;-5632,-256;Inherit;False;Constant;_Vector1;Vector 1;16;0;Create;True;0;0;0;False;0;False;0,0,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.Vector3Node;129;-6272,-896;Inherit;False;Constant;_Vector3;Vector 1;16;0;Create;True;0;0;0;False;0;False;0,0,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldPosInputsNode;87;-6272,-640;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleAddOpNode;64;-4992,-1024;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;97;-4480,-1024;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.NormalizeNode;63;-4736,-1024;Inherit;False;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.NormalizeNode;98;-4224,-1024;Inherit;False;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;134;-3968,-1024;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;77;-6912,-1024;Inherit;False;Property;_BGRotationAngleZ;BG Rotation Angle Z;12;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;148;0,0;Float;False;True;-1;2;ASEMaterialInspector;0;0;Unlit;Vefects/SH_Vefects_VFX_Space_Background_HDRI;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;16;FLOAT4;0,0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;127;0;115;0
WireConnection;111;0;115;0
WireConnection;111;1;114;0
WireConnection;126;0;109;0
WireConnection;118;0;127;0
WireConnection;118;1;120;0
WireConnection;107;0;108;0
WireConnection;107;1;109;0
WireConnection;110;0;111;0
WireConnection;110;1;112;0
WireConnection;123;0;117;0
WireConnection;123;1;126;0
WireConnection;124;0;118;0
WireConnection;124;1;112;0
WireConnection;106;0;107;0
WireConnection;106;1;110;0
WireConnection;122;0;123;0
WireConnection;122;1;124;0
WireConnection;105;0;106;0
WireConnection;105;1;112;0
WireConnection;116;0;122;0
WireConnection;116;1;112;0
WireConnection;102;0;105;0
WireConnection;104;0;116;0
WireConnection;139;0;140;0
WireConnection;139;1;143;0
WireConnection;145;0;144;0
WireConnection;145;1;146;0
WireConnection;99;0;102;0
WireConnection;99;1;104;0
WireConnection;141;0;136;0
WireConnection;141;1;139;0
WireConnection;147;0;135;0
WireConnection;147;1;145;0
WireConnection;96;0;99;0
WireConnection;96;1;100;0
WireConnection;133;28;141;0
WireConnection;133;29;147;0
WireConnection;137;0;133;0
WireConnection;137;1;96;0
WireConnection;95;0;133;0
WireConnection;57;1;137;0
WireConnection;58;14;57;0
WireConnection;58;15;59;0
WireConnection;92;1;95;0
WireConnection;54;0;58;0
WireConnection;54;1;56;0
WireConnection;91;0;92;0
WireConnection;91;1;93;0
WireConnection;89;0;54;0
WireConnection;89;1;90;0
WireConnection;52;0;91;0
WireConnection;52;1;53;0
WireConnection;51;0;89;0
WireConnection;51;1;52;0
WireConnection;11;0;51;0
WireConnection;11;1;12;0
WireConnection;65;0;66;0
WireConnection;65;1;73;0
WireConnection;65;2;74;0
WireConnection;65;3;88;0
WireConnection;69;0;68;0
WireConnection;69;1;71;0
WireConnection;72;0;67;0
WireConnection;72;1;69;0
WireConnection;73;0;72;0
WireConnection;75;0;76;0
WireConnection;75;1;82;0
WireConnection;75;2;129;0
WireConnection;75;3;87;0
WireConnection;78;0;79;0
WireConnection;78;1;80;0
WireConnection;84;0;77;0
WireConnection;84;1;85;0
WireConnection;86;0;84;0
WireConnection;86;1;78;0
WireConnection;88;0;75;0
WireConnection;88;1;87;0
WireConnection;82;0;86;0
WireConnection;64;0;88;0
WireConnection;64;1;65;0
WireConnection;97;0;63;0
WireConnection;97;1;96;0
WireConnection;63;0;64;0
WireConnection;98;0;97;0
WireConnection;134;0;98;0
WireConnection;148;2;11;0
ASEEND*/
//CHKSM=A683F45E2AF84B21F81E077B11BD88F985B9FD28