#version 450 core
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
void EmitEdge( vec3 a, vec3 b ) {
  vec3 LightDir = normalize(a - LightPosition.xyz); 
  vec3 Deviation = LightDir * Epsilon;
  gl_Position = Cam.Projection * Cam.View * vec4(a + Deviation, 1);
  EmitVertex();
  
  gl_Position = Cam.Projection * Cam.View * vec4(LightDir, 0);
  EmitVertex();

  LightDir = normalize(b - LightPosition.xyz); 
  Deviation = LightDir * Epsilon;
  gl_Position = Cam.Projection * Cam.View * vec4(b + Deviation, 1);
  EmitVertex();

  gl_Position = Cam.Projection * Cam.View * vec4(LightDir, 0);
  EmitVertex();
  EndPrimitive();
}
void main(void)
{
	vec3 e1 = VPos[2] - VPos[0];
    vec3 e2 = VPos[4] - VPos[0];
    vec3 e3 = VPos[1] - VPos[0];
    vec3 e4 = VPos[3] - VPos[2];
    vec3 e5 = VPos[4] - VPos[2];
    vec3 e6 = VPos[5] - VPos[0];

    vec3 Normal = normalize(cross(e1,e2));
    vec3 LightDir = normalize(LightPosition - VPos[0]);

    // Handle only light facing triangles
    if (dot(Normal, LightDir) > 0) {

        Normal = cross(e3,e1);

        if (dot(Normal, LightDir) <= 0) {
            vec3 StartVertex = VPos[0];
            vec3 EndVertex = VPos[2];
            EmitEdge(StartVertex, EndVertex);
        }

        Normal = cross(e4,e5);
        LightDir = LightPosition - VPos[2];

        if (dot(Normal, LightDir) <= 0) {
            vec3 StartVertex = VPos[2];
            vec3 EndVertex = VPos[4];
            EmitEdge(StartVertex, EndVertex);
        }

        Normal = cross(e2,e6);
        LightDir = LightPosition - VPos[4];

        if (dot(Normal, LightDir) <= 0) {
            vec3 StartVertex = VPos[4];
            vec3 EndVertex = VPos[0];
            EmitEdge(StartVertex, EndVertex);
        }

        // render the front cap
        vec3 LightDir = (normalize(VPos[0] - LightPosition));
        gl_Position = Cam.Projection * Cam.View * vec4((VPos[0] + LightDir * Epsilon), 1.0);
        EmitVertex();

        LightDir = (normalize(VPos[2] - LightPosition));
        gl_Position = Cam.Projection * Cam.View * vec4((VPos[2] + LightDir * Epsilon), 1.0);
        EmitVertex();

        LightDir = (normalize(VPos[4] - LightPosition));
        gl_Position = Cam.Projection * Cam.View * vec4((VPos[4] + LightDir * Epsilon), 1.0);
        EmitVertex();
        EndPrimitive();
 
        // render the back cap
        LightDir = VPos[0] - LightPosition;
        gl_Position = Cam.Projection * Cam.View * vec4(LightDir, 0.0);
        EmitVertex();

        LightDir = VPos[4] - LightPosition;
        gl_Position = Cam.Projection * Cam.View * vec4(LightDir, 0.0);
        EmitVertex();

        LightDir = VPos[2] - LightPosition;
        gl_Position = Cam.Projection * Cam.View * vec4(LightDir, 0.0);
        EmitVertex();
        EndPrimitive();
    }
}