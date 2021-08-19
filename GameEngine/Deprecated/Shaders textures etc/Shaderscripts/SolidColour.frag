#version 450 core


layout(location = 0) in vec2 FragUV;
layout(location = 1) in vec3 FragPos;
layout(location = 2) in vec3 FragNormal;
//geometry textures
layout(location = 0) out vec3 gPosition;
layout(location = 1) out vec3 gNormal;
layout(location = 2) out vec4 gAlbedoSpec;

uniform sampler2D SpecularTexture;
uniform vec3 DiffuseColor;

void main(void)
{
	gPosition = FragPos;
	gNormal = normalize(FragNormal);
	gAlbedoSpec = vec4(DiffuseColor, texture(SpecularTexture, FragUV).r);
}