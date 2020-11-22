Shader "LBoe/SpritesOutline"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		_Color("Tint", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap("Pixel snap", Float) = 0

		_OutlineColor("Outline Color", Color) = (1,1,1,1)
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
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile _ PIXELSNAP_ON
				#include "UnityCG.cginc"

				struct appdata_t
				{
					float4 vertex   : POSITION;
					float4 color    : COLOR;
					float2 texcoord : TEXCOORD0;
				};

				struct v2f
				{
					float4 vertex   : SV_POSITION;
					fixed4 color : COLOR;
					float2 texcoord  : TEXCOORD0;
				};

				fixed4 _Color;
				float _Outline;
				fixed4 _OutlineColor;

				v2f vert(appdata_t IN)
				{
					v2f OUT;
					OUT.vertex = UnityObjectToClipPos(IN.vertex);
					OUT.texcoord = IN.texcoord;
					OUT.color = IN.color * _Color;
					#ifdef PIXELSNAP_ON
					OUT.vertex = UnityPixelSnap(OUT.vertex);
					#endif

					return OUT;
				}

				sampler2D _MainTex;
				float4 _MainTex_TexelSize;
				sampler2D _AlphaTex;
				float _AlphaSplitEnabled;

				fixed4 SampleSpriteTexture(float2 uv)
				{
					fixed4 color = tex2D(_MainTex, uv);

				#if UNITY_TEXTURE_ALPHASPLIT_ALLOWED
					if (_AlphaSplitEnabled)
						color.a = tex2D(_AlphaTex, uv).r;
					#endif //UNITY_TEXTURE_ALPHASPLIT_ALLOWED

					return color;
				}

				float Random(float2 uv)
				{
					return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453123);
				}

				fixed4 frag(v2f IN) : SV_Target
				{
					fixed4 c = SampleSpriteTexture(IN.texcoord) * IN.color;

					float timeVal = floor(_Time.z/2);
					float xSize = (1 / _MainTex_TexelSize.x);
					float ySize = (1 / _MainTex_TexelSize.y);
					float x = floor(IN.texcoord.x * xSize) / xSize;
					float y = floor(IN.texcoord.y * ySize) / ySize;

					float rndVal = Random(fixed2(x,y) + timeVal);
					bool randomGlow =  rndVal > 0.97f;											

					if (c.a == 0) {

						// Get the neighbouring four pixels.
						fixed pixelUp = tex2D(_MainTex, IN.texcoord + fixed2(0, _MainTex_TexelSize.y)).a;
						fixed pixelDown = tex2D(_MainTex, IN.texcoord - fixed2(0, _MainTex_TexelSize.y)).a;
						fixed pixelRight = tex2D(_MainTex, IN.texcoord + fixed2(_MainTex_TexelSize.x, 0)).a;
						fixed pixelLeft = tex2D(_MainTex, IN.texcoord - fixed2(_MainTex_TexelSize.x, 0)).a;

						float surroundingAlpha = pixelUp + pixelDown + pixelRight + pixelLeft;

						if (surroundingAlpha != 0) {
							c.rgba = fixed4(1, 1, 1, 1) * _OutlineColor;
						}
					}
				
					c.rgb *= c.a;


					return c;
				}
			ENDCG
			}
		}
}