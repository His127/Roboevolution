shader_type canvas_item;

uniform vec2 start_position;
uniform vec2 end_position;

uniform float wobble_strength = 4.0;
uniform float wobble_speed = 2.0;
uniform float wobble_frequency = 0.015;

uniform float travel_time = 5.0;

uniform vec4 base_color : source_color = vec4(0.3f, 0.3f, 0.3f, 1.0);
uniform vec4 progress_color : source_color = vec4(1.0, 1.0, 1.0, 0.03);
uniform float animation_offset = 0.0;

varying float line_position;

void vertex() {
    vec2 lineDirection = end_position - start_position;
    float lineLengthSquared = dot(lineDirection, lineDirection);

    line_position = dot(VERTEX - start_position, lineDirection) / lineLengthSquared;

    line_position = clamp(line_position, 0.0, 1.0);

    float wave1 = sin(VERTEX.x * wobble_frequency + TIME * wobble_speed);
    float wave2 = sin(VERTEX.x * wobble_frequency * 2.3 - TIME * wobble_speed * 0.7);

    VERTEX.y += (wave1 * 0.7 + wave2 * 0.3) * wobble_strength;
}

void fragment() {
    float progress = fract((TIME + animation_offset) / travel_time);
	
	float boundary = progress * 1.2;
	float boundary1 = boundary - 0.2;

	float lower = min(boundary, boundary1);
	float upper = max(boundary, boundary1);

	float green = step(lower, line_position) * step(line_position, upper);

    COLOR = mix(base_color, progress_color, green);
}