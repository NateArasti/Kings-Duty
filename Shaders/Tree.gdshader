shader_type spatial;
render_mode diffuse_toon, blend_mix;

uniform float alpha_distance = 0.2f;
uniform float alpha_border = 0.2f;
uniform float alpha_fade = 0.3f;
uniform sampler2D tile_textures[2]: filter_nearest;

varying flat int texture_index;

void vertex(){
	texture_index = int(INSTANCE_CUSTOM.x);
}

void fragment() {
	vec4 col = texture(tile_textures[texture_index], UV);
	
	ALBEDO = col.rgb;
	ALPHA = col.a;
	float distance_from_center = distance(SCREEN_UV, vec2(0.5f, 0.5f));
	float alpha_weight = distance_from_center / alpha_distance;
	if((1.0f - UV.y) > alpha_border && distance_from_center < alpha_distance){
		ALPHA = min(ALPHA, mix(ALPHA, alpha_fade, 1.0f - alpha_weight * alpha_weight * alpha_weight));
	}
}
