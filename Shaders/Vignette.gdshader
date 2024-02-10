shader_type canvas_item;

uniform float value: hint_range(0, 1);
uniform vec3 accent_color: source_color = vec3(1.0, 1.0, 1.0);

uniform float alpha = 1.0;
uniform float inner_radius = 0.0;
uniform float outer_radius = 1.0;

uniform sampler2D NOISE_PATTERN;

void fragment() {
	float x = abs(UV.r - 0.5) * 2.0;
	float y = abs(UV.g - 0.5) * 2.0;
	float q = 1.0 - (1.0 - sqrt(x * x + y * y) / outer_radius) / (1.0 - inner_radius);
	
    float noise = texture(NOISE_PATTERN, UV).x;
	vec3 rgb = mix(vec3(0, 0, 0), noise *  accent_color, value);
	COLOR = vec4(rgb.r, rgb.g, rgb.b, q * alpha);
}