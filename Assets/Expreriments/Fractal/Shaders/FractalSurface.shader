Shader "Custom/FractalSurface" {
	Properties {
		_Color1 ("Color", Color) = (1,1,1,1)
		_Color2 ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		_I ("-", Float) = 0
		_MAX ("-", Float) = 1
		 _PointSize("PointSize", Float) = 1
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;
		float _I;
		float _MAX;

		struct Input {
			float2 uv_MainTex;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color1;
		fixed4 _Color2;

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color

			float lp =  _I / _MAX;
			o.Albedo = _Color1.rgb * lp + (1 - lp) * _Color2.rgb;

			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = _Color1.a;
		}
		ENDCG
	}





	FallBack "Diffuse"
}
