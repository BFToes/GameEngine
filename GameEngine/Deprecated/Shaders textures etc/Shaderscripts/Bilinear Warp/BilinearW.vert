#version 450 core

uniform mat4 Transform;
uniform mat4 Projection;
uniform float Time;

layout(location = 0) in vec3 Position;
layout(location = 1) in vec3 Normal;
layout(location = 2) in vec2 UV;

out vec4 Vposition;
out vec3 Vnormal;
out vec2 Vuv;

void main(void)
{
	Vposition = Projection * Transform * vec4(Position.x, Position.y, Position.z, 1);
	Vnormal = Normal;
	Vuv = UV;
}