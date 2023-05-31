Shader "Custom/TerrainShader" {
	Properties{
		_Tess("Tessellation", Range(1,32)) = 4
		_MainTex("Heightmap", 2D) = "white" {}
		_Color1("Color1", Color) = (1,1,1,1)
		_Color2("Color2", Color) = (1,1,1,1)
		_GridSize("Grid size", Float) = 8.0
	}
		SubShader{
			Tags { "RenderType" = "Opaque" }
			LOD 200

			CGPROGRAM
			// Physically based Standard lighting model, and enable shadows on all light types
			#pragma surface surf Standard addshadow tessellate:tessFixed vertex:vert 

			// Use shader model 3.0 target, to get nicer looking lighting
			#pragma target 5.0

			sampler2D _MainTex;

			struct appdata {
				float4 vertex : POSITION;
				float4 tangent : TANGENT;
				float3 normal : NORMAL;
				float2 texcoord : TEXCOORD0;
				float2 texcoord1 : TEXCOORD1;
				float2 texcoord2 : TEXCOORD2;
			};

			struct Input {
				float2 uv_MainTex;
				float3 normal;
				float3 viewDir;
			};

			int _Tess;
			fixed4 _Color1;
			fixed4 _Color2;
			float _GridSize;

			// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
			// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
			// #pragma instancing_options assumeuniformscaling
			UNITY_INSTANCING_BUFFER_START(Props)
				// put more per-instance properties here
			UNITY_INSTANCING_BUFFER_END(Props)

			// Gradients for 3D noise
			static float gradient[48] = {
				1, 1, 0,    -1,  1, 0,     1, -1,  0,    -1, -1,  0,
				1, 0, 1,    -1,  0, 1,     1,  0, -1,    -1,  0, -1,
				0, 1, 1,     0, -1, 1,     0,  1, -1,     0, -1, -1,
				1, 1, 0,     0, -1, 1,    -1,  1,  0,     0, -1, -1
			};

			float permute(float x) {
				// This makes more sense to me
				float index = fmod(x, 289.0);
				return (index * 34.0 + 1.0) * index;
				//return fmod((x*34.0 + 1.0)*x, 289.0);
			}

			float3 fade(float3 t) {

				return t * t * t * (t * (t * 6 - 15) + 10); // new curve
				//  return t * t * (3 - 2 * t); // old curve
			}

			float grad(float x, float3 p) {
				int index = fmod(x, 16.0);
				float3 g = float3(gradient[index * 3], gradient[index * 3 + 1], gradient[index * 3 + 2]);
				return dot(g, p);
			}

			float mlerp(float a, float b, float t) {
				return a + t * (b - a);
			}

			// 3D version of noise function
			float inoise(float3 p) {

				float3 P = fmod(floor(p), 256.0);
				p -= floor(p);
				float3 f = fade(p);

				// HASH COORDINATES FOR 6 OF THE 8 CUBE CORNERS
				float A = permute(P.x) + P.y;
				float AA = permute(A) + P.z;
				float AB = permute(A + 1) + P.z;
				float B = permute(P.x + 1) + P.y;
				float BA = permute(B) + P.z;
				float BB = permute(B + 1) + P.z;

				// AND ADD BLENDED RESULTS FROM 8 CORNERS OF CUBE
				return mlerp(
				mlerp(mlerp(grad(permute(AA), p),
							grad(permute(BA), p + float3(-1, 0, 0)), f.x),
						mlerp(grad(permute(AB), p + float3(0, -1, 0)),
							grad(permute(BB), p + float3(-1, -1, 0)), f.x), f.y),
				mlerp(mlerp(grad(permute(AA + 1), p + float3(0, 0, -1)),
							grad(permute(BA + 1), p + float3(-1, 0, -1)), f.x),
						mlerp(grad(permute(AB + 1), p + float3(0, -1, -1)),
							grad(permute(BB + 1), p + float3(-1, -1, -1)), f.x), f.y), f.z);
			}

			float4 tessFixed()
			{
				return _Tess;
			}

			void vert(inout appdata v) {
				v.vertex.y += (1 - tex2Dlod(_MainTex, float4(v.texcoord.xy, 0, 0)).r);
			}

			void surf(Input IN, inout SurfaceOutputStandard o) {
				//discard transparent fragmanets
				if (tex2D(_MainTex, IN.uv_MainTex).r == 1) {
					discard;
				} else {
					o.Alpha = 1;
				}



				//use perlin noise to generate albedo
				float nse = 0.0;
				nse = inoise(float3(IN.uv_MainTex * _GridSize, 0.0)) - 0.5 + tex2D(_MainTex, IN.uv_MainTex).r;
				o.Albedo = tex2D(_MainTex, IN.uv_MainTex) * float3(
					mlerp(_Color1.r, _Color2.r, nse),
					mlerp(_Color1.g, _Color2.g, nse),
					mlerp(_Color1.b, _Color2.b, nse)
				);



				//calculate the new normal for this fragment
				//code from https://polycount.com/discussion/117185/creating-normals-from-alpha-heightmap-inside-a-shader
				float e = (1 - tex2Dlod(_MainTex, float4(IN.uv_MainTex.x - 1.0 / 512.0, IN.uv_MainTex.y, 0, 0)).r);
				float w = (1 - tex2Dlod(_MainTex, float4(IN.uv_MainTex.x + 1.0 / 512.0, IN.uv_MainTex.y, 0, 0)).r);
				float s = (1 - tex2Dlod(_MainTex, float4(IN.uv_MainTex.x, IN.uv_MainTex.y - 1.0 / 512.0, 0, 0)).r);
				float n = (1 - tex2Dlod(_MainTex, float4(IN.uv_MainTex.x, IN.uv_MainTex.y + 1.0 / 512.0, 0, 0)).r);

				float3 norm = o.Normal;
				float3 temp = o.Normal;
				if (norm.x == 1) { temp.y += 0.5; }
				else { temp.x += 0.5; }

				float3 perp1 = normalize(cross(norm, temp));
				float3 perp2 = normalize(cross(norm, perp1));

				float3 normOffset = -float3(2 * (n-s), 2 * (e-w), -4);
				o.Normal = normalize(norm + normOffset);
			}
			ENDCG
		}
			FallBack "Diffuse"
}
