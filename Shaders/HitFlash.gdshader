shader_type spatial;
render_mode cull_disabled;

uniform bool active = false; // Sets pixels to flash color when true
uniform vec3 flash_color: source_color = vec3(1.0,1.0,1.0); // The color to use when active
uniform sampler2D source_texture: filter_nearest, source_color; // A copy of the texture for the Sprite3D or AnimatedSprite3D 

void fragment() {
	vec4 col = texture(source_texture, UV);
	
	if(active == true) {
		ALBEDO = flash_color;
		if(col.a > 0.0){
			ALPHA = 1.0;
		}
		else{
			ALPHA = 0.0;
		}
	}
	else{
		ALPHA = 0.0;
	}
}