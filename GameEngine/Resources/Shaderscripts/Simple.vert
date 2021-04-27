#version 450 core

uniform mat4 ObjMatrix;

in vec2 Position;

void main(void)
{
	gl_Position = vec4(Position, 0, 1);
}