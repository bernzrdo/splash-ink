Shader "Hidden/PaintTangent"
{
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100
		CULL Off

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct v2f
			{
				float4 pos : SV_POSITION;
				float3 worldPos : TEXCOORD0;
			};
			
			v2f vert (appdata_full v)
			{
				v2f o;
				float3 uvWorldPos = float3( v.texcoord1.xy * 2.0 - 1.0, 0.5 );
				o.pos = mul( UNITY_MATRIX_VP, float4( uvWorldPos, 1.0 ) );
				o.worldPos = mul(unity_ObjectToWorld, v.vertex ).xyz;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float3 worldTangent = normalize( ddx( i.worldPos ) ) * 0.5 + 0.5;
				return float4(worldTangent, 1.0);
			}
			ENDCG
		}
	}
}
