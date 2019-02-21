// Upgrade NOTE: upgraded instancing buffer 'Props' to new syntax.

Shader "Custom/WhiteBoardShader" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_CleanTex("CleanTex (RGBA32)", 2D) = "white" {}
		_RoughTex1("RoughTex (RGBA32)", 2D) = "white" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0
		
		//sampler2D _MainTex;
		sampler2D _CleanTex;
		sampler2D _RoughTex;

		struct Input {
			float2 uv_CleanTex;
		};

		fixed4 _Color;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_CBUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_CBUFFER_END

		void surf (Input IN, inout SurfaceOutputStandard o) {
			fixed4 c0 = tex2D(_CleanTex, IN.uv_CleanTex);
			fixed4 c1 = tex2D(_RoughTex, IN.uv_CleanTex);
			o.Alpha = c0.a + c1.a + _Color.a;//max(max(c0.a,c1.a),_Color.a);
            o.Albedo = _Color.rgb*c0.rgb;
			//if (c1.r>0||c1.g>0||c1.b>0) {
			if(c1.a>0){
				o.Albedo = _Color.rgb*c1.rgb;
			}
		}
		ENDCG
	}
	FallBack "Diffuse"
}
