shader_type canvas_item;

uniform float radius : hint_range(0.0, 1.0, 0.1);

void fragment() {
	// Called for every pixel the material is visible on.
	vec2 middle = vec2(0.5, 0.5);
	if(distance(middle, UV) > radius){
		COLOR = vec4(0.0, 0.0, 0.0, 0.0);
	}
}
