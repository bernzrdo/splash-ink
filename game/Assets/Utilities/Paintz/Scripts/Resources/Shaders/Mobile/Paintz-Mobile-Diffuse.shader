// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

// Simplified Diffuse shader. Differences from regular Diffuse one:
// - no Main Color
// - fully supports only 1 directional light. Other lights can affect it, but it will be per-vertex/SH.

Shader "Paint/Mobile/Diffuse" {
Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}

		_SplatColor1("Splat Color 1", Color) = (1,.5,0,1)
		_SplatColor2("Splat Color 2", Color) = (1,0,0,1)
		_SplatColor3("Splat Color 3", Color) = (0,1,0,1)
		_SplatColor4("Splat Color 4", Color) = (0,0,1,1)

		_SplatTex("Splat Texture", 2D) = "black" {}

		_SplatEdgeBump("Splat Edge Scale", Range(0.001,10)) = 1.0
		_SplatEdgeBumpWidth("Splat Edge Width", Range(0,10)) = 1.0

}

SubShader {
    Tags { "RenderType"="Opaque" }
    LOD 150

CGPROGRAM
#pragma surface surf Lambert noforwardadd

static const float _Clip = 0.5;

sampler2D _MainTex;
sampler2D _SplatTex;

float _SplatEdgeBump;
float _SplatEdgeBumpWidth;

fixed4 _SplatColor1;
fixed4 _SplatColor2;
fixed4 _SplatColor3;
fixed4 _SplatColor4;

float4 _SplatTex_TexelSize;


struct Input {
    float2 uv_MainTex;
	float2 uv2_SplatTex;
	float3 worldNormal;
	float3 worldTangent;
	float3 worldBinormal;
	float3 worldPos;
	INTERNAL_DATA
};

float3x3 cotangent_frame(float3 N, float3 p, float2 uv)
{
	// get edge vectors of the pixel triangle
	float3 dp1 = ddx(p);
	float3 dp2 = ddy(p);
	float2 duv1 = ddx(uv);
	float2 duv2 = ddy(uv);

	// solve the linear system
	float3 dp2perp = cross(dp2, N);
	float3 dp1perp = cross(N, dp1);
	float3 T = dp2perp * duv1.x + dp1perp * duv2.x;
	float3 B = dp2perp * duv1.y + dp1perp * duv2.y;

	// construct a scale-invariant frame 
	float invmax = rsqrt(max(dot(T, T), dot(B, B)));
	float3 TinvMax = normalize(T * invmax);
	float3 BinvMax = normalize(B * invmax);
	return float3x3(float3(TinvMax.x, BinvMax.x, N.x), float3(TinvMax.y, BinvMax.y, N.y), float3(TinvMax.z, BinvMax.z, N.z));
	//return float3x3( TinvMax, BinvMax, N );
}

half3 perturb_normal(float3 localNormal, float3 N, float3 V, float2 uv)
{
	// assume N, the interpolated vertex normal and 
	// V, the view vector (vertex to eye)
	float3x3 TBN = cotangent_frame(N, -V, uv);
	return normalize(mul(TBN, localNormal));
}

void surf (Input IN, inout SurfaceOutput o) {

	// Sample splat map texture with offsets
	float4 splatSDF = tex2D(_SplatTex, IN.uv2_SplatTex);
	float4 splatSDFx = tex2D(_SplatTex, IN.uv2_SplatTex + float2(_SplatTex_TexelSize.x, 0));
	float4 splatSDFy = tex2D(_SplatTex, IN.uv2_SplatTex + float2(0, _SplatTex_TexelSize.y));

	// Use ddx ddy to figure out a max clip amount to keep edge aliasing at bay when viewing from extreme angles or distances
	half splatDDX = length(ddx(IN.uv2_SplatTex * _SplatTex_TexelSize.zw));
	half splatDDY = length(ddy(IN.uv2_SplatTex * _SplatTex_TexelSize.zw));
	half clipDist = sqrt(splatDDX * splatDDX + splatDDY * splatDDY);
	half clipDistHard = max(clipDist * 0.01, 0.01);
	half clipDistSoft = 0.01 * _SplatEdgeBumpWidth;

	// Smoothstep to make a soft mask for the splats
	float4 splatMask = smoothstep((_Clip - 0.01) - clipDistHard, (_Clip - 0.01) + clipDistHard, splatSDF);
	float splatMaskTotal = max(max(splatMask.x, splatMask.y), max(splatMask.z, splatMask.w));

	// Smoothstep to make the edge bump mask for the splats
	float4 splatMaskInside = smoothstep(_Clip - clipDistSoft, _Clip + clipDistSoft, splatSDF);
	splatMaskInside = max(max(splatMaskInside.x, splatMaskInside.y), max(splatMaskInside.z, splatMaskInside.w));

	// Create normal offset for each splat channel
	float4 offsetSplatX = splatSDF - splatSDFx;
	float4 offsetSplatY = splatSDF - splatSDFy;

	// Combine all normal offsets into single offset
	float2 offsetSplat = lerp(float2(offsetSplatX.x, offsetSplatY.x), float2(offsetSplatX.y, offsetSplatY.y), splatMask.y);
	offsetSplat = lerp(offsetSplat, float2(offsetSplatX.z, offsetSplatY.z), splatMask.z);
	offsetSplat = lerp(offsetSplat, float2(offsetSplatX.w, offsetSplatY.w), splatMask.w);
	offsetSplat = normalize(float3(offsetSplat, 0.0001)).xy; // Normalize to ensure parity between texture sizes
	offsetSplat = offsetSplat * (1.0 - splatMaskInside) * _SplatEdgeBump;

	// Create the world normal of the splats
	
	// Use tangentless technique to get world normals
	float3 worldNormal = WorldNormalVector(IN, float3(0, 0, 1));
	float3 offsetSplatLocal2 = normalize(float3(offsetSplat, sqrt(1.0 - saturate(dot(offsetSplat, offsetSplat)))));
	float3 offsetSplatWorld = perturb_normal(offsetSplatLocal2, worldNormal, normalize(IN.worldPos - _WorldSpaceCameraPos), IN.uv2_SplatTex);

	// Get the tangent and binormal for the texcoord0 (this is just the actual tangent and binormal that comes in from the vertex shader)
	float3 worldTangent = WorldNormalVector(IN, float3(1, 0, 0));
	float3 worldBinormal = WorldNormalVector(IN, float3(0, 1, 0));

	// Convert the splat world normal to tangent normal for texcood0
	float2 offsetSplatLocal = 0;
	offsetSplatLocal.x = dot(worldTangent, offsetSplatWorld);
	offsetSplatLocal.y = dot(worldBinormal, offsetSplatWorld);

	// Albedo comes from a texture tinted by color
	fixed4 c = tex2D(_MainTex, IN.uv_MainTex);

	// Lerp the color with the splat colors based on the splat mask channels
	c.xyz = lerp(c.xyz, _SplatColor1.xyz, splatMask.x);
	c.xyz = lerp(c.xyz, _SplatColor2.xyz, splatMask.y);
	c.xyz = lerp(c.xyz, _SplatColor3.xyz, splatMask.z);
	c.xyz = lerp(c.xyz, _SplatColor4.xyz, splatMask.w);

	o.Albedo = c.rgb;
	o.Alpha = c.a;
}
ENDCG
}

Fallback "Mobile/VertexLit"
}
