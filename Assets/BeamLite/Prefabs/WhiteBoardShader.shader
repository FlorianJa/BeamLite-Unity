// Upgrade NOTE: upgraded instancing buffer 'Props' to new syntax.

Shader "Custom/WhiteBoardShader" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_CleanTex("CleanTex (RGBA32)", 2D) = "white" {}
		_RoughTex1("RoughTex (RGBA32)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
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

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_CBUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_CBUFFER_END

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed4 c0 = tex2D(_CleanTex, IN.uv_CleanTex);
			fixed4 c1 = tex2D(_RoughTex, IN.uv_CleanTex);
			//fixed4 c;//=c0*c1;
			float a=max(max(c0.a,c1.a),_Color.a);
            fixed4 c=c0*_Color;
			if (c1.r!=_Color.r|| c1.g!=_Color.g||c1.b!=_Color.b) {
				c = c1*_Color;
			}
			//c.rgb=c.rgb*c.a+((1-c.a)*c1.a)*c1.rgb;
			//c.a=c.a+(1-c.a)*c1.a;
			//c.rgb=c.rgb*c.a+((1-c.a)*c2.a)*c2.rgb;
			//c.a=c.a+(1-c.a)*c2.a;
			//c.rgb=c.rgb*c.a+((1-c.a)*c3.a)*c3.rgb;
			//c.a=c.a+(1-c.a)*c3.a;
			o.Albedo=c.rgb;
			o.Alpha=a;
			//o.Albedo = (c+(c.a)) * (c1 + (c1.a))*(c2 + (c2.a))*(c3 + (c3.a))*_Color;
//				* tex2D(_CleanTex, IN.uv_CleanTex).rgb 
//				* tex2D(_RoughTex1, IN.uv_RoughTex1).rgb
//				* tex2D(_RoughTex2, IN.uv_RoughTex2).rgb 
//				* tex2D(_RoughTex3, IN.uv_RoughTex3).rgb;
			//c.a = _Color.a;
			//c.a += (1 - c.a)*tex2D(_CleanTex, IN.uv_CleanTex).a;
			//o.Alpha = c.a;
			//o.Alpha += (1 - o.Alpha)*c1;
			//o.Alpha += (1 - o.Alpha)*c2;
			//o.Alpha += (1 - o.Alpha)*c3;
			//o.Alpha = 1;
			//o.Albedo = c.rgb;
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
