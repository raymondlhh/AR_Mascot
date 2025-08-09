Shader "Imagine/ChromaKeyCutout" {
Properties {
    _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
    _MaskCol ("Mask Color", Color)  = (1.0, 0.0, 0.0, 1.0)
    _Sensitivity ("Threshold Sensitivity", Range(0,1)) = 0.5
    _Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
    _Feather ("Feathering", Range(0,1)) = 1
}
SubShader {
    Tags {"Queue"="AlphaTest" "IgnoreProjector"="True" "RenderType"="TransparentCutout"}
    LOD 100

    Lighting Off

    Pass {
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata_t {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float2 texcoord : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed _Cutoff;
            fixed _Feather;


            float4 _MaskCol;
            float _Sensitivity;

            v2f vert (appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, i.texcoord);

                // float maskY = 0.2989 * _MaskCol.r + 0.5866 * _MaskCol.g + 0.1145 * _MaskCol.b;
		        // float maskCr = 0.7132 * (_MaskCol.r - maskY);
 		        // float maskCb = 0.5647 * (_MaskCol.b - maskY);
 
		        // float Y = 0.2989 * c.r + 0.5866 * c.g + 0.1145 * c.b;
 		        // float Cr = 0.7132 * (c.r - Y);
 		        // float Cb = 0.5647 * (c.b - Y);

                float MY = 0.2989*_MaskCol.r + 0.5866*_MaskCol.g + 0.1145*_MaskCol.b;
                float MCr = 0.5 + 0.5*_MaskCol.r - 0.418688*_MaskCol.g - 0.081312*_MaskCol.b;
                float MCb = 0.5 + -0.168736*_MaskCol.r - 0.331264*_MaskCol.g + 0.5*_MaskCol.b;
  
                float Y = 0.2989 * c.r + 0.5866 * c.g + 0.1145 * c.b;
                float Cr = 0.5 + 0.5*c.r - 0.418688*c.g - 0.081312*c.b;
                float Cb = 0.5 + -0.168736*c.r - 0.331264*c.g + 0.5*c.b;

                 // float dist = distance(float2(Cr, Cb), float2(MCr, MCb));
                 float sqDist = (Cr - MCr)*(Cr - MCr) + (Cb - MCb)*(Cb - MCb);


                float S2 = _Sensitivity * _Sensitivity;
                float F2 = _Feather * _Feather;
                float d = 1;
                if(sqDist < S2) d = 0;
                else if(sqDist < F2) d = (sqDist-S2)/(F2-S2);
                 
                // float blendValue = smoothstep(_Sensitivity, _Sensitivity, dist);

                clip(d - _Cutoff);
                return c;//float4(sqDist, sqDist, sqDist, 1);

            }
        ENDCG
    }
}

}