Shader "Hidden/BlurConeTap" {
	SubShader {
		Pass {
			ZTest Always Cull Off ZWrite Off Fog { Mode Off }
			SetTexture [__RenderTex] {constantColor (0,0,0,0.25) combine texture * constant alpha}
			SetTexture [__RenderTex] {constantColor (0,0,0,0.25) combine texture * constant + previous}
			SetTexture [__RenderTex] {constantColor (0,0,0,0.25) combine texture * constant + previous}
			SetTexture [__RenderTex] {constantColor (0,0,0,0.25) combine texture * constant + previous}
		}
	}
	Fallback off
}