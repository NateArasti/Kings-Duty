shader_type spatial;
render_mode cull_back, unshaded;

uniform sampler2D SCREEN_TEXTURE : hint_screen_texture;

void fragment() {
	ALBEDO = textureLod(SCREEN_TEXTURE, SCREEN_UV, 0.0).rgb;
}