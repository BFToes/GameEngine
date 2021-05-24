#version 450 core

in vec2 FragUV;
out vec4 Colour;

uniform sampler2D ColourTexture;

uniform float  Ambient;

void main(void)
{
    vec4 AlbedoSpec = texture(ColourTexture, FragUV);
    Colour = vec4(Ambient * AlbedoSpec.rgb, 1.0);
}