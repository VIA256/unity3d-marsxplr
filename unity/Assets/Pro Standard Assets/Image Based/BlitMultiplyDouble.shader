Shader "Hidden/BlitMultiplyDouble" {
	SubShader { Pass {
		Blend DstColor SrcColor
 		ZTest Always Cull Off ZWrite Off Fog { Mode Off }
		SetTexture [__RenderTex] { combine texture }
	}}
Fallback Off }