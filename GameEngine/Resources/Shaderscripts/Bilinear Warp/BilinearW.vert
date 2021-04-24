#version 450 core

uniform mat4 ObjMatrix;
uniform mat4 PrjMatrix;
uniform float Time;

layout(location = 0) in vec3 Position;
layout(location = 1) in vec3 Normal;
layout(location = 2) in vec2 UV;

out vec4 Vposition;
out vec3 Vnormal;
out vec2 Vuv;

void main(void)
{
	Vposition = vec4(vec3(ObjMatrix * vec4(Position.x, Position.y, Position.z, 1)), 1);
	Vnormal = Normal;
	Vuv = UV;
}