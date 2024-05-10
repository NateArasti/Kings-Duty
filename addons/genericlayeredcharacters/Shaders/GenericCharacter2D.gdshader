shader_type canvas_item;
render_mode unshaded;

uniform sampler2D weapon : filter_nearest, source_color;
uniform sampler2D weapon_cardboard : filter_nearest, source_color;
uniform sampler2D weapon_cardboard_outline : filter_nearest, source_color;

uniform sampler2D hair : filter_nearest, source_color;
uniform sampler2D hair_cardboard : filter_nearest, source_color;
uniform sampler2D hair_cardboard_outline : filter_nearest, source_color;

uniform sampler2D eyes : filter_nearest, source_color;
uniform sampler2D nose : filter_nearest, source_color;
uniform sampler2D mouth : filter_nearest, source_color;

uniform sampler2D head : filter_nearest, source_color;
uniform sampler2D head_cardboard : filter_nearest, source_color;
uniform sampler2D head_cardboard_outline : filter_nearest, source_color;

uniform sampler2D clothes : filter_nearest, source_color;

uniform sampler2D body : filter_nearest, source_color;
uniform sampler2D body_cardboard: filter_nearest, source_color;
uniform sampler2D body_cardboard_outline : filter_nearest, source_color;

uniform sampler2D legs : filter_nearest, source_color;
uniform sampler2D legs_cardboard : filter_nearest, source_color;
uniform sampler2D legs_cardboard_outline : filter_nearest, source_color;

vec4 getOutlineColor(vec2 uv) {
	vec4 resultColor = vec4(0);
	
	vec4 layerColor = vec4(0);
	
	layerColor = texture(legs_cardboard_outline, uv);
	resultColor = mix(resultColor, layerColor, layerColor.a);
	layerColor = texture(body_cardboard_outline, uv);
	resultColor = mix(resultColor, layerColor, layerColor.a);
	layerColor = texture(head_cardboard_outline, uv);
	resultColor = mix(resultColor, layerColor, layerColor.a);
	layerColor = texture(hair_cardboard_outline, uv);
	resultColor = mix(resultColor, layerColor, layerColor.a);
	layerColor = texture(weapon_cardboard_outline, uv);
	resultColor = mix(resultColor, layerColor, layerColor.a);
	
	return resultColor;
}

vec4 getCardboardColor(vec2 uv) {
	vec4 resultColor = vec4(0);
	
	vec4 layerColor = vec4(0);
	
	layerColor = texture(legs_cardboard, uv);
	resultColor = mix(resultColor, layerColor, layerColor.a);
	layerColor = texture(body_cardboard, uv);
	resultColor = mix(resultColor, layerColor, layerColor.a);
	layerColor = texture(head_cardboard, uv);
	resultColor = mix(resultColor, layerColor, layerColor.a);
	layerColor = texture(hair_cardboard, uv);
	resultColor = mix(resultColor, layerColor, layerColor.a);
	layerColor = texture(weapon_cardboard, uv);
	resultColor = mix(resultColor, layerColor, layerColor.a);
	
	return resultColor;
}

vec4 getActualColor(vec2 uv) {
	vec4 resultColor = vec4(0);
	vec4 layerColor = vec4(0);
	
	layerColor = texture(legs, uv);
	resultColor = mix(resultColor, layerColor, layerColor.a);
	layerColor = texture(body, uv);
	resultColor = mix(resultColor, layerColor, layerColor.a);
	layerColor = texture(clothes, uv);
	resultColor = mix(resultColor, layerColor, layerColor.a);
	layerColor = texture(head, uv);
	resultColor = mix(resultColor, layerColor, layerColor.a);
	layerColor = texture(eyes, uv);
	resultColor = mix(resultColor, layerColor, layerColor.a);
	layerColor = texture(nose, uv);
	resultColor = mix(resultColor, layerColor, layerColor.a);
	layerColor = texture(mouth, uv);
	resultColor = mix(resultColor, layerColor, layerColor.a);
	layerColor = texture(hair, uv);
	resultColor = mix(resultColor, layerColor, layerColor.a);
	layerColor = texture(weapon, uv);
	resultColor = mix(resultColor, layerColor, layerColor.a);
	
	return resultColor;
}

vec4 getPixelColor(vec2 uv) {
	vec4 resultColor = vec4(0);
	
	vec4 outlineColor = getOutlineColor(uv);
	vec4 cardboardColor = getCardboardColor(uv);
	vec4 actualColor = getActualColor(uv);
	
	resultColor = mix(resultColor, outlineColor, outlineColor.a);
	resultColor = mix(resultColor, cardboardColor, cardboardColor.a);
	resultColor = mix(resultColor, actualColor, actualColor.a);
	
	return resultColor;
}

void fragment() {
	vec4 resultCol = getPixelColor(UV);
	COLOR = resultCol;
}
