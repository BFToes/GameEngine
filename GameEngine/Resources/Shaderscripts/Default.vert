#version 450 core

uniform mat4 ObjMatrix;
uniform mat4 PrjMatrix;

layout(location = 0) in vec3 Position;
layout(location = 1) in vec3 Normal;
layout(location = 2) in vec2 UV;

out vec2 FragUV;
out vec3 FragNormal;

void main(void)
{
	gl_Position = PrjMatrix * ObjMatrix * vec4(Position, 1);
	FragUV = UV;
	FragNormal = Normal;
}