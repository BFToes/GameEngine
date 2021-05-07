#version 450 core

//const int NR_LIGHTS = 3; // for testing


in vec2 FragUV;
out vec4 Colour;

//uniform sampler2D PositionTexture;
//uniform sampler2D NormalTexture;
uniform sampler2D ColourTexture;

//uniform vec3 ViewPos;
/*
layout(std140) uniform Light {
	vec3 Position;
	vec3 Color;
	float Intensity;
	} Lights[NR_LIGHTS];
*/





void main(void)
{
    /*
	vec3 FragPos = texture(PositionTexture, FragUV).rgb;
    vec3 Normal = texture(NormalTexture, FragUV).rgb;
    vec3 Albedo = texture(ColourTexture, FragUV).rgb;
    float Specular = texture(ColourTexture, FragUV).a;
    
	// then calculate lighting as usual
    vec3 lighting = Albedo * 0.1;
    
    if (FragUV.x < 0.5 && FragUV.y < 0.5) {
        Colour = vec4(FragPos, 1.0); // bottom left
    }
    if (FragUV.x > 0.5 && FragUV.y < 0.5) Colour = vec4(Normal, 1.0); // bottom right
    if (FragUV.x < 0.5 && FragUV.y > 0.5) Colour = vec4(Albedo, 1.0); // top left
    if (FragUV.x > 0.5 && FragUV.y > 0.5) Colour = vec4(lighting, 1.0); // top right
    */
    Colour = vec4(texture(ColourTexture, FragUV).rgb, 1);
    //Colour = vec4(FragUV, 0, 1);
}