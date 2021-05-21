#version 450 core

in vec3 Position;
// for directional light this should be indentity
uniform mat4 WVP; // world view projection

void main(void)
{
	gl_Position = WVP * vec4(Position, 1);
}