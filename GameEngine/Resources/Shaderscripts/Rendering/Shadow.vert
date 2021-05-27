#version 450 core

in vec3 Position;
out vec3 VPos;

uniform mat4 Model;
layout(std140) uniform CameraBlock {
	mat4 Projection;
	mat4 View;
	vec3 Position;
    vec2 ScreenSize;
} Cam;
void main(void)
{
	VPos = (Cam.View * Model * vec4(Position, 1)).xyz;
}