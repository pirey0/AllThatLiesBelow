// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Sprites/CustomColorPicker"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		_Color("Tint", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap("Pixel snap", Float) = 0
		[HideInInspector] _RendererColor("RendererColor", Color) = (1,1,1,1)
		[HideInInspector] _Flip("Flip", Vector) = (1,1,1,1)
		[PerRendererData] _AlphaTex("External Alpha", 2D) = "white" {}
		[PerRendererData] _EnableExternalAlpha("Enable External Alpha", Float) = 0

		_Sensitivity("Sensitivity", Range(0,1)) = 0.1

		_Color1("Color 1 (0,0,0)", Color) = (1,1,1,1)
		_Color2("Color 2 (1,0,0)", Color) = (1,1,1,1)
		_Color3("Color 3 (0,1,0)", Color) = (1,1,1,1)
		_Color4("Color 4 (0,0,1)", Color) = (1,1,1,1)
		_Color5("Color 5 (1,1,0)", Color) = (1,1,1,1)
		_Color6("Color 6 (1,0,1)", Color) = (1,1,1,1)
		_Color7("Color 7 (0,1,1)", Color) = (1,1,1,1)
		_Color8("Color 8 (1,1,1)", Color) = (1,1,1,1)

		_SourceColor1("Source Color 1", Color) = (0,0,0,0)
		_SourceColor2("Source Color 2", Color) = (0,0,0,0)
		_SourceColor3("Source Color 3", Color) = (0,0,0,0)
		_SourceColor4("Source Color 4", Color) = (0,0,0,0)
		_SourceColor5("Source Color 5", Color) = (0,0,0,0)
		_SourceColor6("Source Color 6", Color) = (0,0,0,0)
		_SourceColor7("Source Color 7", Color) = (0,0,0,0)
		_SourceColor8("Source Color 8", Color) = (0,0,0,0)

	}

		SubShader
		{
			Tags
			{
				"Queue" = "Transparent"
				"IgnoreProjector" = "True"
				"RenderType" = "Transparent"
				"PreviewType" = "Plane"
				"CanUseSpriteAtlas" = "True"
			}

			Cull Off
			Lighting Off
			ZWrite Off
			Blend One OneMinusSrcAlpha

			Pass
			{
			CGPROGRAM
				#pragma vertex SpriteVert
				#pragma fragment CustomSpriteFrag
				#pragma target 2.0
				#pragma multi_compile_instancing
				#pragma multi_compile_local _ PIXELSNAP_ON
				#pragma multi_compile _ ETC1_EXTERNAL_ALPHA
				#include "UnitySprites.cginc"


			fixed4 _Color1;
			fixed4 _Color2;
			fixed4 _Color3;
			fixed4 _Color4;
			fixed4 _Color5;
			fixed4 _Color6;
			fixed4 _Color7;
			fixed4 _Color8;

			fixed4 _SourceColor1;
			fixed4 _SourceColor2;
			fixed4 _SourceColor3;
			fixed4 _SourceColor4;
			fixed4 _SourceColor5;
			fixed4 _SourceColor6;
			fixed4 _SourceColor7;
			fixed4 _SourceColor8;
			fixed _Sensitivity;

			fixed4 CustomSpriteFrag(v2f IN) : SV_Target
			{
				fixed4 c = SampleSpriteTexture(IN.texcoord) * IN.color;
				c.rgb *= c.a;


				if (length(abs(c.rgb - _SourceColor1.rgb))< _Sensitivity)
					c.rgb = _Color1;
				else if (length(abs(c.rgb - _SourceColor2.rgb)) < _Sensitivity)
					c.rgb = _Color2;
				else if (length(abs(c.rgb - _SourceColor3.rgb)) < _Sensitivity)
					c.rgb = _Color3;
				else if (length(abs(c.rgb - _SourceColor4.rgb)) < _Sensitivity)
					c.rgb = _Color4;
				else if (length(abs(c.rgb - _SourceColor5.rgb)) < _Sensitivity)
					c.rgb = _Color5;
				else if (length(abs(c.rgb - _SourceColor6.rgb)) < _Sensitivity)
					c.rgb = _Color6;
				else if (length(abs(c.rgb - _SourceColor7.rgb)) < _Sensitivity)
					c.rgb = _Color7;
				else if (length(abs(c.rgb - _SourceColor8.rgb)) < _Sensitivity)
					c.rgb = _Color8;

				return c;
			}

			ENDCG
			}
		}
}
