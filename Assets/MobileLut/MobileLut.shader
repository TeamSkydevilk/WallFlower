Shader "SupGames/Mobile/Lut"
{
	Properties
	{
		_MainTex("Base (RGB)", 2D) = "" {}
	}

	CGINCLUDE

	#include "UnityCG.cginc"

	struct v2f {
		fixed4 pos : POSITION;
		fixed2 uv  : TEXCOORD0;
	};

	sampler2D _MainTex;
	sampler2D _LutTex2D;
	sampler3D _LutTex3D;
	uniform fixed _LutAmount;
	uniform fixed4 _MainTex_TexelSize;

	v2f vert( appdata_img v ) 
	{
		v2f o;
		o.pos = UnityObjectToClipPos(v.vertex);
		o.uv = v.texcoord.xy;
		return o;
	} 

	fixed4 fragLut2D(v2f i) : COLOR 
	{
		fixed4 c = tex2D(_MainTex, i.uv);
		fixed b = floor(c.b * 256.0h);
		fixed by = floor(b * 0.0625h);
		fixed2 uv = c.rg * 0.05859375h + 0.001953125h + fixed2(floor(b - by * 16.0h), by) * 0.0625h;
		fixed4 lc= tex2D(_LutTex2D, uv);
		return lerp(c,lc, _LutAmount);
	}

	fixed4 fragLut3D(v2f i) : COLOR
	{
		fixed4 c = tex2D(_MainTex, i.uv);
		fixed3 lc = tex3D(_LutTex3D, c.rgb * 0.9375h + 0.03125h).rgb;
		return lerp(c,fixed4(lc,1.0h), _LutAmount);
	}

	fixed4 fragLut2DLin(v2f i) : COLOR
	{
		fixed4 c = tex2D(_MainTex, i.uv);
		c.rgb = sqrt(c.rgb);
		fixed b = floor(c.b * 256.0h);
		fixed by = floor(b * 0.0625h);
		fixed2 uv = c.rg * 0.05859375h + 0.001953125h + fixed2(floor(b - by * 16.0h), by) * 0.0625h;
		fixed4 lc = tex2D(_LutTex2D, uv); 
		c.rgb *= c.rgb;
		return lerp(c, lc, _LutAmount);
	}

	fixed4 fragLut3DLin(v2f i) : COLOR
	{
		fixed4 c = tex2D(_MainTex, i.uv);
		c.rgb = sqrt(c.rgb);
		fixed4 lc = tex3D(_LutTex3D, c.rgb * 0.9375h + 0.03125h);
		c = lerp(c,lc, _LutAmount);
		c.rgb *= c.rgb;
		return c;
	}

	ENDCG 
		
	Subshader 
	{
		Pass
		{
		  ZTest Always Cull Off ZWrite Off
		  Fog { Mode off }
		  CGPROGRAM
		  #pragma vertex vert
		  #pragma fragment fragLut2D
		  #pragma fragmentoption ARB_precision_hint_fastest
		  ENDCG
		}

		Pass
		{
		  ZTest Always Cull Off ZWrite Off
		  Fog { Mode off }
		  CGPROGRAM
		  #pragma vertex vert
		  #pragma fragment fragLut2DLin
		  #pragma fragmentoption ARB_precision_hint_fastest
		  ENDCG
		}

		Pass
		{
		  ZTest Always Cull Off ZWrite Off
		  Fog { Mode off }
		  CGPROGRAM
		  #pragma vertex vert
		  #pragma fragment fragLut3D
		  #pragma fragmentoption ARB_precision_hint_fastest
		  ENDCG
		}

		Pass
		{
		  ZTest Always Cull Off ZWrite Off
		  Fog { Mode off }
		  CGPROGRAM
		  #pragma vertex vert
		  #pragma fragment fragLut3DLin
		  #pragma fragmentoption ARB_precision_hint_fastest
		  ENDCG
		}
	}

	Fallback off
}