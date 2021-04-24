#version 450 core


in vec2 FragUV;
in vec3 FragNormal;

out vec4 Colour;

uniform sampler2D Texture;
void main(void)
{
    float Depth = (2.0 * 0.1 * 100) / (100 + 0.1 - (gl_FragCoord.z * 2.0 - 1.0) * (100 - 0.1));
	Colour = texture(Texture, FragUV); //vec4(vec3(Depth), 1); //
}