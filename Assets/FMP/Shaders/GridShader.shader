Shader "Unlit/GridShader"
{
	Properties
	{
		_GridSize("Grid Size", Float) = 1
		_DrawVertically("Draw Grid Vertically", Int) = 1
	}
	SubShader
	{
		Tags { "RenderType" = "Overlay" }
		LOD 100
		ZTest Always

		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha
			//Offset -20, -20
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			float _GridSize;
			int _DrawVertically;

			struct appdata
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};


			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				if (_DrawVertically == 2)
					o.uv = mul(unity_ObjectToWorld, v.vertex).yx;
				else if(_DrawVertically == 1)
					o.uv = mul(unity_ObjectToWorld, v.vertex).yz;
				else if(_DrawVertically == 0)
					o.uv = mul(unity_ObjectToWorld, v.vertex).xz;
				return o;
			}

			float DrawGrid(float2 uv, float sz, float aa)
			{
				float aaThresh = aa;
				float aaMin = aa * 0.1;

				float2 gUV = uv / sz + aaThresh;

				float2 fl = floor(gUV);
				gUV = frac(gUV);
				gUV -= aaThresh;
				gUV = smoothstep(aaThresh, aaMin, abs(gUV));
				float d = max(gUV.x, gUV.y);

				return d;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				fixed grid = DrawGrid(i.uv + float2(0.5,0.5), _GridSize, 0.03);
				return float4(grid, grid, grid, grid);
			}
			ENDCG
		}
	}
}