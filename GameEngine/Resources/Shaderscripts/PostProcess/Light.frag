#version 450 core

 layout(std140) struct LightData {
    vec3 Colour;
    float AmbientIntensity;
    vec3 Position;
    float DiffuseIntensity;
    vec3 Attenuation;
};


// per light
uniform LightData Light;
uniform vec3 CamPosition;

// global
uniform vec2 ScreenSize;
uniform float SpecularPower;
uniform float SpecularIntensity;
uniform sampler2D PositionTexture;
uniform sampler2D AlbedoTexture;
uniform sampler2D NormalTexture;

out vec4 Colour;

vec4 CalcPointLight(vec3 Position, vec3 Normal) {
    vec3 LightDir = Position - Light.Position;
    float Distance = length(LightDir);
    LightDir = normalize(LightDir);

    vec4 DiffuseColour;
    vec4 SpecularColour;
    vec4 AmbientColour = vec4(Light.Colour * Light.AmbientIntensity, 1);

    float DiffuseFactor = dot(Normal, -LightDir);
    
    if (DiffuseFactor > 0) {
        DiffuseColour = vec4(Light.Colour * Light.DiffuseIntensity * DiffuseFactor, 1);
        float SpecularFactor = dot(normalize(CamPosition - Position), normalize(reflect(LightDir, Normal)));
        
        if (SpecularFactor > 0) {
            SpecularFactor = pow(SpecularFactor, SpecularPower);
            SpecularColour = vec4(Light.Colour * SpecularFactor * SpecularIntensity, 1);
        }
    }
    vec4 BaseColour = AmbientColour + DiffuseColour + SpecularColour;
    float Attenuation = Light.Attenuation.x + Light.Attenuation.y * Distance + Light.Attenuation.z * Distance * Distance;
    Attenuation = max(Attenuation, 1);

    return BaseColour / Attenuation;
}

void main(void)
{
    vec2 TexCoord = gl_FragCoord.xy / ScreenSize;
    vec3 Position = texture(PositionTexture, TexCoord).xyz;
    vec4 Albedo = texture(AlbedoTexture, TexCoord);
    vec3 Normal = normalize(texture(NormalTexture, TexCoord).xyz);
    
    Colour = vec4(Albedo.xyz, 1) * CalcPointLight(Position, Normal);
}