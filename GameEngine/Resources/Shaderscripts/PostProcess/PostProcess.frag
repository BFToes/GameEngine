#version 450 core

const float PI = 3.14;

in vec2 FragUV;
out vec4 Colour;

uniform sampler2D ColorTex;
uniform sampler2D DepthTex;

const float Blur = 0.0125;
const int Samples = 10;
void main(void)
{

	// Box Blur
	/*
	int n;
	vec4 C;
	for (float x = FragUV.x - Blur; x < FragUV.x + Blur; x += Blur * 2 / Samples)
	{
		for (float y = FragUV.y - Blur; y < FragUV.y + Blur; y += Blur * 2 / Samples)
		{
			C += texture(Texture, vec2(x, y));
			n ++;
		}
	}
	
	Colour = C / n;
	*/
	Colour = texture(ColorTex, FragUV);
}