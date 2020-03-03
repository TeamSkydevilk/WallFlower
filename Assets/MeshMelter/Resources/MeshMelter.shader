// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

//
// Mesh Melter v1.0 - Sep, 2017 - by Pixeldust @ Cloverbit srl (pixeldust@cloverbit.com)
// 
// THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES WITH REGARD
// TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS.
// IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL
// DAMAGES OR ANY DAMAGES WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN
// AN ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF OR IN
// CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
//

Shader "Cloverbit/MeshMelter" {

	Properties {
		// inherit parameters from the standard shader
		[HideInInspector] _Color ("Color", Color) = (1,1,1,1)
		[HideInInspector] _MainTex ("Texture", 2D) = "white" {}
		[HideInInspector] _BumpMap("Normal", 2D) = "bump" {}
		[HideInInspector] _BumpScale("Normal", Float) = 1
		[HideInInspector] _MetallicGlossMap ("Metallic", 2D) = "white" {}
		[HideInInspector] _Metallic("Metallic", Range(0.0, 1.0)) = 0
		[HideInInspector] _Glossiness("Smoothness", Range(0.0, 1.0)) = 0.5
		[HideInInspector] _OcclusionMap("Occlusion", 2D) = "white" {}
		[HideInInspector] _OcclusionStrength("Strength", Range(0.0, 1.0)) = 1.0
		[HideInInspector] _EmissionMap("Emission", 2D) = "white" {}
		[HideInInspector] _EmissionColor("Emission Color", Color) = (0,0,0)

		_Melt ("Melting Level", Range(0, 1)) = 0
		_Center ("Center Offset (from Pivot)", Vector) = (0, 0, 0)
		_Gravity ("Gravity Direction (world space)", Vector) = (0, -1, 0)
		_Offset ("Falling Offset (world distance)", Float) = 0
		// _Offset is not enough if we want to calculate Hstack based on actual mesh height
		_BasePoint ("Offset to Base", Float) = 0
		_MeltTint ("Melting Color", Color) = (1,1,1,1)

		_RHz ("Radial Waves/m", Range(0,10)) = 1
		_MeltAmplify ("Peripheral Melt Amplify", Range(0,1)) = 0.2

		_HStack ("Stack Height (%)", Range(0,0.5)) = 0.2
		_RStack ("Stack Radius", Float) = 1
		_Smooth ("Stack Smoothness", Range(0,1)) = 0.5
		_Spread ("Spread Level", Range(0,1)) = 0.5
		
		_NorthReference("Radial Noise Reference", Vector) = (0, 0, 1, 0)		

	}
	
	SubShader {
		Tags {
			"Queue"="Geometry"
			"RenderType"="Opaque"
			"PerformanceChecks"="False"
			// avoid batching, otherwise sliding may not work correctly
			"DisableBatching" = "True"
		}
		LOD 300
				
		CGPROGRAM

		#pragma surface surf Standard fullforwardshadows addshadow
		#pragma vertex vert			
		#pragma target 3.0

		#include "UnityCG.cginc"
		#include "UnityPBSLighting.cginc"
				
		#define HALFPI 1.57079632679
		#define PI 3.14159265359
		#define TWOPI 6.28318530718
		
		half _Melt;
		half _RHz;
		half _MeltAmplify;
		half _HStack;
		half _RStack;
		half _Spread;
		half _Smooth;
		half4 _MeltTint;

		float4 _Gravity;
		float4 _Center;
		half4 _NorthReference;
		half _Offset;
		half _BaseOffset;
		
		struct Input {
			float2 uv_MainTex;
			float2 uv_EmissionMap;
		};
				
		void vert (inout appdata_full v) {
		
			// prevent divisions by zero
			half nzRStack = max (_RStack, 0.0001);
			half nzHStack = max (_HStack, 0.0001);
			half nzSpread = max (_Spread, 0.0001);
			
			// we must computer vertex displacement in world space
			// otherwise, mesh scale and rotation will produce additional unwanted deformations
					
			// make sure gravity is not zero (defaults to -y) and normalized
			float3 gravityAxis = normalize(_Gravity + (step(length(_Gravity.xyz), 0) * float4(0, -1, 0, 0))).xyz;
			
			// in world position of the center
			float3 geometricCenter = mul(unity_ObjectToWorld, float4(_Center.xyz, 0)).xyz;
			
			// in world coordinates are relative to current position of the geometric center
			float3 vertexToCenter = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 0)).xyz - geometricCenter;
			
			// take out two components from the coordinates using the gravity axis passing by the center
			float alongGravity = dot(gravityAxis, vertexToCenter);
			float3 normalToGravity = vertexToCenter - (alongGravity * gravityAxis);
			
			// distance to the gravity axis; this will be used for radial deformation
			float normalLength = length(normalToGravity);
			
			// uneven melting level
						
			// apply a cosine to the melting level to see deformation in waves
			// _RHz is the number of peaks per meter and is set by the component to have higher melt values on external vertices
			float noisedMelt = saturate(_Melt * (1 + (((-cos(normalLength * _RHz * PI) + 1) / 2) * (_MeltAmplify * 5))));
			
			// now on, we are going to use noisedMelt for all interpolations
			
			// horizontal deformation

			// from the center we push vertices outward while after a certain limit we shrink inward
			// the center will look like spashing away while arms and extrusions will look like shrinking
			// the spread limit is set to the radius of the final stack plus the spread factor from there
			
			float spreadLimit = _RStack * (1 + _Spread);
			
			// outside the spread limit we recalculate normalLength moving it up to half way to the spread limit 

			normalLength -= step(spreadLimit, normalLength) * (normalLength - spreadLimit) * 0.5 * noisedMelt;
			
			// when we are closer than _HStack to the collapsing plane we must also start spreading away
			// but if the vertex is already below the stack profile we should start spread immediately using noisedMelt and considering how much we are below the profile
			// spread level goes from 0 to 1 

			float stackProfile = _HStack * _BaseOffset * 2 * ((cos(saturate(normalLength / nzRStack) * PI) + 1) / 2); // hill shape
				// large hill	(1 - pow(saturate(normalLength / nzRStack), 2));
				// cone 		pow(1 - saturate(normalLength / nzRStack), 2);
				// cylinder		step(normalLength, nzRStack);
				// mesa 		(step(normalLength, nzRStack * 0.5) + step(nzRStack * 0.5, normalLength) * saturate((nzRStack - normalLength) / nzRStack * 0.5));
			
			half belowProfile = step(_Offset -  alongGravity, stackProfile);
			float shareBelowStack =  1 - saturate((_Offset -  alongGravity) /  (max(_BaseOffset, 0.01) * 2 * nzHStack));			

			float spreadLevel = (1 - belowProfile) * saturate((noisedMelt - ( 1 - _HStack)) / nzHStack) + belowProfile * noisedMelt * shareBelowStack;								
															
			// spreading logic
			// at the center, no spread is applied
			// spread increase gradually to _Spread up to _RStack
			// from _RStack to _RStack * (1 + Spread) we decrease back to no spreading
			// to avoid having a circular stack we put radial noise on top of the spread factor (this is why we use _NorthReference)
			// finally, the spread factor must be applied using spreadlevel
		
			float teta = acos(dot(normalize(normalToGravity), normalize(_NorthReference)));
			float spreadFactor = spreadLevel * _Spread * (
					(saturate(normalLength / nzRStack) - saturate((normalLength - _RStack) / (nzRStack * nzSpread)))	// base spread
						+ 
					((cos(teta * 3) + sin(teta * 5)) / 4)																// noise spread
				);

			// vertical deformation			
												
			// we push vertices along gravity depending on noisedMelt
			// the destination is not the collapsing plane but the stack profile calculated above
			// unless, once again, the vertex al already below the stack profile, otherwse they would fly up
			// to reduce vertex overlap we introduce a small vertex pushup based on distance to _BaseOffset
			// this is going to introduce small gaps in the stack, but should also improve the visual result
	
			float stackLevel = (1 - belowProfile) * (stackProfile + ((_BaseOffset - alongGravity) / 100));
						
			// we lerp vertical to destination taking offset into consideration
			float vertexHeight = lerp(alongGravity, _Offset - stackLevel, noisedMelt);
			
			// note: it is possible to collapse everything in the lerp and save two variable declarations ... if you are ready to kill code readability
			
			// and the final vertex position is ...
			
			v.vertex.xyz = 
				mul( unity_WorldToObject, float4((
					   gravityAxis * vertexHeight				// in-world position along gravity
				     + normalize(normalToGravity) * normalLength * ( 1 + spreadFactor )   // in-world position normal to gravity
				     + geometricCenter ) .xyz, 0)				// offset back the the geometric center
				    ).xyz; 										// bring everything again in object coordinates 

			// recalculating normals
			
			// model normals are lerped to the stack normal up to a fraction given by _Smooth
			// when _Smooth is 1 we lerp all the way to obtain a polished surface
			// when _Smooth is 0 we stick to the original normals giving the impression of an irregular surface
			
			float slope = atan ( - _HStack * _BaseOffset * sin(saturate(normalLength / nzRStack) * PI));
			float3 normalToStack = normalize((-gravityAxis * cos(slope)) - (normalize(normalToGravity) * sin(slope)));
			// we lerp in world space for the same reasons mentioned above					
			float3 newNormal = lerp(normalize(mul(unity_ObjectToWorld, float4(v.normal.xyz, 0))), normalToStack, noisedMelt * _Smooth).xyz;
			v.normal.xyz = mul(unity_WorldToObject, float4(newNormal.xyz, 0));      
		}

		sampler2D _MainTex;
		fixed4 _Color;
		sampler2D _BumpMap;
		half _BumpScale;
		sampler2D _MetallicGlossMap;
		half _Metallic;
		sampler2D _OcclusionMap;
		half _OcclusionStrength;
		sampler2D _EmissionMap;
		fixed4 _EmissionColor;		
		half _Glossiness;

		void surf (Input IN, inout SurfaceOutputStandard o) {		
		
			// this is the usual color application from standard shader
			// if I missed something, you can drop me a line
	
			fixed4 col = tex2D (_MainTex, IN.uv_MainTex);
			o.Albedo = (col.rgb * _Color.rgb) * lerp(float3(1, 1, 1), _MeltTint.rgb, _Melt);
			o.Alpha = (col.a * _Color.a) * lerp(1, _MeltTint.a, _Melt);;			
			fixed4 metal = tex2D (_MetallicGlossMap, IN.uv_MainTex);
			o.Metallic = metal.r * _Metallic; 
			o.Smoothness = metal.a * _Glossiness;
			o.Normal = UnpackScaleNormal (tex2D (_BumpMap, IN.uv_MainTex), _BumpScale).rgb;
			o.Emission = tex2D(_EmissionMap, IN.uv_EmissionMap).rgb * _EmissionColor.rgb;	
			o.Occlusion = lerp(1, tex2D(_OcclusionMap, IN.uv_MainTex).g, _OcclusionStrength);			
		}		

		ENDCG
	}
	
	FallBack "Diffuse"
}