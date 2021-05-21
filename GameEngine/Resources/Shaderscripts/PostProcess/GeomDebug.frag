#version 450 core

in vec2 FragUV;
out vec4 Colour;

uniform sampler2D PositionTexture;
uniform sampler2D NormalTexture;
uniform sampler2D ColourTexture;

void main(void)
{
	vec3 FragPos = texture(PositionTexture, FragUV).rgb;
    vec3 Normal = texture(NormalTexture, FragUV).rgb;
    vec4 AlbedoSpec = texture(ColourTexture, FragUV);

    if (FragUV.x < 0.5 && FragUV.y > 0.5) Colour = vec4(mod(FragPos.x, 1), mod(FragPos.y, 1), mod(FragPos.z, 1), 1.0); // top left
    if (FragUV.x > 0.5 && FragUV.y > 0.5) Colour = vec4(Normal, 1.0); // top right
    if (FragUV.x > 0.5 && FragUV.y < 0.5) Colour = vec4(AlbedoSpec.rgb, 1.0); // bottom right
    if (FragUV.x < 0.5 && FragUV.y < 0.5) Colour = vec4(AlbedoSpec.rgb, 1.0); // bottom left

    vec4 GridCol = vec4(1.0);
    vec2 Grid = abs(abs(FragUV * 2 - 1) * 2 - 1);
    if (max(Grid.x, Grid.y) > 0.98) Colour = GridCol;
}