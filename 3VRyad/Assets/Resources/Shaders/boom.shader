Shader "Custom/boom"
{
	Properties{
		_MainTex("Base (RGB)", 2D) = "white" {}
	_DisplacementTex("Displacement rt", 2D) = "white" {}
	_DisplacementPower("Displacement power", Float) = 0.025
	}

		SubShader{
		Pass{
		CGPROGRAM
#pragma vertex vert_img
#pragma fragment frag

#include "UnityCG.cginc"

		uniform sampler2D _MainTex;
	uniform sampler2D _DisplacementTex;
	uniform float _DisplacementPower;

	float4 frag(v2f_img i) : COLOR
	{
		fixed4 displacementVector = tex2D(_DisplacementTex, i.uv);
	fixed2 uv_distorted = i.uv + _DisplacementPower * displacementVector.xy;

	return tex2D(_MainTex, uv_distorted);
	}
		ENDCG
	}
	}
}