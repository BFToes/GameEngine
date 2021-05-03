#version 450 core

in vec2 FragUV;
out vec4 Colour;

uniform sampler2D PositionTexture;
uniform sampler2D NormalTexture;
uniform sampler2D ColourTexture;

const float Blur = 0.0125;
const int Samples = 10;
void main(void)
{
	Colour = texture(ColourTexture, FragUV);
}