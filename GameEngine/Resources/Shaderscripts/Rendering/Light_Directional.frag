#version 450 core

layout(std140) uniform CameraBlock {
	mat4 Projection;
	mat4 View;
	vec3 Position;
    vec2 ScreenSize;
} Cam;

layout(std140) uniform LightBlock {
    vec3 Colour;
    float AmbientIntensity;
    vec3 Direction;
    float DiffuseIntensity;
} Light;

// global
uniform float SpecularPower;
uniform float SpecularIntensity;

uniform sampler2D PositionTexture;
uniform sampler2D AlbedoTexture;
uniform sampler2D NormalTexture;

out vec4 Colour;

void main(void)
{
    vec2 TexCoord = gl_FragCoord.xy / Cam.ScreenSize;
    vec3 Position = texture(PositionTexture, TexCoord).xyz;
    vec4 Albedo = texture(AlbedoTexture, TexCoord);
    vec3 Normal = normalize(texture(NormalTexture, TexCoord).xyz);
    
    vec4 DiffuseColour;
    vec4 SpecularColour;
    vec4 AmbientColour = vec4(Light.Colour * Light.AmbientIntensity, 1);

    float DiffuseFactor = dot(Normal, -Light.Direction);
    
    if (DiffuseFactor > 0) {
        DiffuseColour = vec4(Light.Colour * Light.DiffuseIntensity * DiffuseFactor, 1);
        float SpecularFactor = dot(normalize(Cam.Position - Position), normalize(reflect(Light.Direction, Normal)));
        
        if (SpecularFactor > 0) {
            SpecularFactor = pow(SpecularFactor, SpecularPower);
            SpecularColour = vec4(Light.Colour * SpecularFactor * SpecularIntensity, 1);
        }
    }

    Colour = vec4(Albedo.xyz, 1) * (AmbientColour + DiffuseColour + SpecularColour);
}