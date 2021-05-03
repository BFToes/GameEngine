#version 450 core

uniform mat4 Transform;
uniform mat4 Projection;

layout(location = 0) in vec3 Position;

out vec2 FragUV;
out vec3 FragNormal;

void main(void)
{
	gl_Position = Projection * Transform * vec4(Position, 1);
}