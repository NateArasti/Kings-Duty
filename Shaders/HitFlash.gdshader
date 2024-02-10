shader_type spatial;
render_mode cull_disabled;

uniform float value: hint_range(0, 1);
uniform vec4 flash_color: source_color = vec4(1.0, 1.0, 1.0, 1.0);
uniform sampler2D source_texture: filter_nearest, source_color; // A copy of the texture for the Sprite3D or AnimatedSprite3D 

void fragment() {
	vec4 col = texture(source_texture, UV);
	
	if(value > 0.0) {
		vec4 new_col = mix(col, flash_color, value);
		vec3 rgb = vec3(new_col.r, new_col.g, new_col.b);
		ALBEDO = rgb;
		if(col.a > 0.0){
			ALPHA = new_col.a;
		}
		else{
			ALPHA = 0.0;
		}
	}
	else{
		ALPHA = 0.0;
	}
}