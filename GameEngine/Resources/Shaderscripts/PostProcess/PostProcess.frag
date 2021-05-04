#version 450 core

in vec2 FragUV;
out vec4 Colour;

layout(location = 0) uniform sampler2D PositionTexture;
layout(location = 1) uniform sampler2D NormalTexture;
layout(location = 2) uniform sampler2D ColourTexture;

struct Light {
	vec3 Position;
	vec3 Color;
	float Intensity;
	};
const int NR_LIGHTS = 1;

uniform Light Lights[NR_LIGHTS];
uniform vec3 ViewPos;

void main(void)
{
	vec3 FragPos = texture(PositionTexture, FragUV).rgb;
    vec3 Normal = texture(NormalTexture, FragUV).rgb;
    vec3 Albedo = texture(ColourTexture, FragUV).rgb;
    float Specular = texture(ColourTexture, FragUV).a;

	// then calculate lighting as usual
    vec3 lighting = Albedo * 1.0; // hard-coded ambient component
    vec3 viewDir = normalize(ViewPos - FragPos);
    
    for(int i = 0; i < NR_LIGHTS; i++)
    {
        // diffuse
        vec3 lightDir = normalize(vec3(0, 2, 0) - FragPos); // Lights[i].Position
        vec3 diffuse = max(dot(Normal, lightDir), 0.0) * Albedo * vec3(0, 1, 0); // Lights[i].Color
        lighting += diffuse * 200; // Lights[i].Intensity
    }
    
    Colour = vec4(lighting, 1.0);
    /*
	if (FragUV.x < 0.25) Colour = texture(PositionTexture, FragUV);
	else if (FragUV.x < 0.5) Colour = texture(NormalTexture, FragUV);
	else if (FragUV.x < 0.75) Colour = texture(ColourTexture, FragUV);
	else if (FragUV.x < 0.1) Colour = vec4(lighting, 1.0);
    */
}