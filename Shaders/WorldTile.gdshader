shader_type spatial;
render_mode diffuse_toon, blend_mix;

uniform sampler2D tile_textures[15];

varying flat int texture_index;

void vertex(){
	texture_index = int(INSTANCE_CUSTOM.x);
}

void fragment() {
	vec4 col = texture(tile_textures[texture_index], UV);
	
	ALBEDO = col.rgb;
	ALPHA = col.a;
}
