#version 450 core

uniform mat4 View;
uniform mat4 Model;
uniform mat4 Projection;

layout(location = 0) in vec3 Position;
layout(location = 1) in vec3 Normal;
layout(location = 2) in vec2 UV;

layout(location = 0) out vec2 FragUV;
layout(location = 1) out vec3 FragPos;
layout(location = 2) out vec3 FragNormal;

void main(void)
{
	gl_Position = Projection * View * Model * vec4(Position, 1);
	FragUV = UV;
	FragPos = (Model * vec4(Position, 1)).rgb;
	FragNormal = (Model * vec4(Normal, 1)).rgb;
}