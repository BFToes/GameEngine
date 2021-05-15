#version 450 core

in vec2 FragUV;
out vec4 Colour;

uniform sampler2D PositionTexture;
uniform sampler2D NormalTexture;
uniform sampler2D ColourTexture;
uniform int LightCount;

struct LightData { 
    vec4 Position;
    vec4 Color;
};

layout (std140) uniform LightBlock {
    LightData Lights[32];
};


void main(void)
{
	vec3 FragPos = texture(PositionTexture, FragUV).rgb;
    vec3 Normal = texture(NormalTexture, FragUV).rgb;
    vec4 AlbedoSpec = texture(ColourTexture, FragUV);
    
    vec3 lighting = vec3(0.1);
    
    for( int i = 0; i < LightCount; i++) {
        vec3 lightDir = normalize(Lights[i].Position.rgb - FragPos);
        vec3 diffuse = max(dot(Normal, lightDir), 0) * Lights[i].Color.rgb;
        lighting += diffuse;
    }
    
    if (FragUV.x < 0.5 && FragUV.y > 0.5) Colour = vec4(mod(FragPos.x, 1), mod(FragPos.y, 1), mod(FragPos.z, 1), 1.0); // top left
    if (FragUV.x > 0.5 && FragUV.y > 0.5) Colour = vec4(Normal, 1.0); // top right
    if (FragUV.x > 0.5 && FragUV.y < 0.5) Colour = vec4(AlbedoSpec.rgb, 1.0); // bottom right
    if (FragUV.x < 0.5 && FragUV.y < 0.5) Colour = vec4(lighting * AlbedoSpec.rgb, 1.0); // bottom left

    
}