#version 450 core


layout(location = 0) uniform mat4 Model;
layout(location = 1) uniform mat4 Projection;
layout(location = 2) uniform mat4 View;

layout(location = 0) in vec3 Position;
layout(location = 1) in vec3 Normal;
layout(location = 2) in vec2 UV;

layout(location = 0) out vec2 FragUV;
layout(location = 1) out vec3 FragPos;
layout(location = 2) out vec3 FragNormal;

void main(void)
{
	gl_Position = Projection * View * Model * vec4(Position, 1); // view space position
	FragUV = UV;
	FragPos = (Model * vec4(Position, 1)).rgb; // world space position
	FragNormal = mat3(Model) * Normal; // world space normal
}