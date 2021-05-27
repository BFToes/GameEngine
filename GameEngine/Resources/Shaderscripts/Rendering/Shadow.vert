#version 450 core

in vec3 Position;
out vec3 VPos;

uniform mat4 Model;

void main(void)
{
	VPos = (Model * vec4(Position, 1)).xyz;
}