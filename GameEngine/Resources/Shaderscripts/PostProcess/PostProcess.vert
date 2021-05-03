﻿#version 450 core

in vec2 Position;
in vec2 UV;

out vec2 FragUV;

void main(void)
{
	gl_Position = vec4(Position, 0, 1);
	FragUV = UV;
}