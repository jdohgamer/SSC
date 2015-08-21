 Shader "Custom/RotateUVs" {
        Properties {
            _MainTex ("Base (RGB)", 2D) = "white" {}
            _Color("Hi Color",COLOR) = (0,0,0,0) 
            _Alpha("Hi Alpha", Float) = 1.0
            _RotationDegree("Rotation Degree", Float) = 0.0
        }
        SubShader {
            Tags { "RenderType"="Opaque" }
            LOD 200
           
            CGPROGRAM
            #pragma surface surf Lambert vertex:vert
     
            sampler2D _MainTex;
        	fixed4 _Color;
        	float _Alpha;
     
            struct Input {
                float2 uv_MainTex;
            };
     
            float _RotationDegree;
            void vert (inout appdata_full v) {
                v.texcoord.xy -=0.5;
                float s = sin ( _RotationDegree);
                float c = cos ( _RotationDegree);
                float2x2 rotationMatrix = float2x2( c, -s, s, c);
                rotationMatrix *=0.5;
                rotationMatrix +=0.5;
                rotationMatrix = rotationMatrix * 2-1;
                v.texcoord.xy = mul ( v.texcoord.xy, rotationMatrix );
                v.texcoord.xy += 0.5;
            }
     
            void surf (Input IN, inout SurfaceOutput o) {  
                half4 c = tex2D (_MainTex, IN.uv_MainTex);
                o.Albedo =_Color.rgb+ c.rgb;
                o.Alpha = _Alpha;
            }
            ENDCG
        }
        FallBack "Standard"
    }