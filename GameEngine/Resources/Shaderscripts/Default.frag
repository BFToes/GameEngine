#version 450 core


layout(location = 0) in vec2 FragUV;
layout(location = 1) in vec3 FragPos;
layout(location = 2) in vec3 FragNormal;

layout(location = 0) out vec3 gPosition;
layout(location = 1) out vec3 gNormal;
layout(location = 2) out vec4 gAlbedoSpec;

uniform sampler2D DiffuseTexture;
uniform sampler2D SpecularTexture;

void main(void)
{
	gPosition = FragPos;
	gNormal = normalize(FragNormal);
	gAlbedoSpec.rgb = texture(DiffuseTexture, FragUV).rgb;
	gAlbedoSpec.a = texture(SpecularTexture, FragUV).r;
}