﻿#version 450 core
const float Epsilon = 0.0078125; // 0.015625 // 0.0078125 // 0.00390625

layout (triangles_adjacency) in; 
layout (triangle_strip, max_vertices = 18) out; 

// world space position of face in a triangle adjacency matrix
in vec3[] VPos; 

uniform vec3 LightPosition;
layout(std140) uniform CameraBlock {
	mat4 Projection;
	mat4 View;
	vec3 Position;
    vec2 ScreenSize;
} Cam;

bool FacesLight(vec3 a, vec3 b, vec3 c) 
{
    vec3 n = cross(b - a, c - a);
    vec3 da = LightPosition - a;
    vec3 db = LightPosition - b;
    vec3 dc = LightPosition - c;

    return dot(n, da) > 0 || dot(n, db) > 0 || dot(n, dc) > 0; 
}
void EmitEdge(vec3 a, vec3 b) 
{
    vec3 LightDir = normalize(a - LightPosition);
    vec3 Deviation = LightDir * Epsilon;
    gl_Position = Cam.Projection * vec4(a + Deviation, 1);
    EmitVertex();

    gl_Position = Cam.Projection * vec4(LightPosition, 0);
    EmitVertex();

    LightDir = normalize(b - LightPosition);
    Deviation = LightDir * Epsilon;
    gl_Position = Cam.Projection * vec4(b + Deviation, 1);
    EmitVertex();

    gl_Position = Cam.Projection * vec4(LightDir, 0);
    EmitVertex();

    EndPrimitive();

}
void main(void)
{
	if (FacesLight(VPos[0], VPos[1], VPos[4])) {
        if (!FacesLight(VPos[0], VPos[1], VPos[2]))
            EmitEdge(VPos[0], VPos[2]);
        if (!FacesLight(VPos[2], VPos[3], VPos[4]))
            EmitEdge(VPos[2], VPos[4]);
        if (!FacesLight(VPos[4], VPos[5], VPos[0]))
            EmitEdge(VPos[4], VPos[0]);

        // front cap
        vec3 LightDir = normalize(VPos[0] - LightPosition);
        vec3 Deviation = LightDir * Epsilon;
        gl_Position = Cam.Projection * vec4(VPos[0] + Deviation, 1);
        EmitVertex();

        LightDir = normalize(VPos[2] - LightPosition); 
		Deviation = LightDir * Epsilon;
		gl_Position =  Cam.Projection * vec4(VPos[2] + Deviation, 1);
		EmitVertex();

		LightDir = normalize(VPos[4] - LightPosition); 
		Deviation = LightDir * Epsilon;
		gl_Position =  Cam.Projection * vec4(VPos[4] + Deviation, 1);
		EmitVertex();
	    EndPrimitive();

        //BACK CAP
		LightDir = normalize(VPos[0] - LightPosition); 
		gl_Position = Cam.Projection * vec4(LightDir, 0);
		EmitVertex();

		LightDir = normalize(VPos[4] - LightPosition); 
		gl_Position =  Cam.Projection * vec4(LightDir, 0);
		EmitVertex();

		LightDir = normalize(VPos[2] - LightPosition); 
		gl_Position =  Cam.Projection * vec4(LightDir, 0);
		EmitVertex();
        EndPrimitive();
    }
}