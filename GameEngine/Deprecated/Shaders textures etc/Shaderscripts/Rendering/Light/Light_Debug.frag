#version 450 core

layout(location = 0) in vec2 FragUV;
out vec4 Colour;

uniform vec3 DiffuseColor;

void main(void)
{
	Colour = vec4(DiffuseColor, 1);
}
