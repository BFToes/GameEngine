#version 450 core
const float Epsilon = 0.03125; // 0.03125 // 0.015625 // 0.0078125 // 0.00390625

layout (triangles_adjacency) in; 
layout (triangle_strip, max_vertices = 18) out; 

// world space position of face in a triangle adjacency matrix
in vec3[] VPos; 


layout(std140) uniform CameraBlock {
	mat4 Projection;
	mat4 View;
	vec3 Position;
    vec2 ScreenSize;
} Cam;

layout(std140) uniform LightBlock {
    mat4 Model;
    vec3 Colour;
    float AmbientIntensity;
    vec3 Position;
    float DiffuseIntensity;
} Light;

void EmitEdge( vec3 a, vec3 b ) {
  vec3 LightDir = normalize(a - Light.Position.xyz); 
  vec3 Deviation = LightDir * Epsilon;
  gl_Position = Cam.Projection * Cam.View * vec4(a + Deviation, 1);
  EmitVertex();
  
  gl_Position = Cam.Projection * Cam.View * vec4(LightDir, 0);
  EmitVertex();

  LightDir = normalize(b - Light.Position.xyz); 
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

    vec3 LightDir = normalize(Light.Position - VPos[0]);

    // Handle only light facing triangles
    if (dot(normalize(cross(e1,e2)), LightDir) > 0) {

        if (dot(cross(e3,e1), LightDir) <= 0) {
            EmitEdge(VPos[0], VPos[2]);
        }

        if (dot(cross(e4,e5), Light.Position - VPos[2]) <= 0) {
            EmitEdge(VPos[2], VPos[4]);
        }

        if (dot(cross(e2,e6), Light.Position - VPos[4]) <= 0) {
            EmitEdge(VPos[4], VPos[0]);
        }

        // render the front cap
        gl_Position = Cam.Projection * Cam.View * vec4((VPos[0] + (normalize(VPos[0] - Light.Position)) * Epsilon), 1.0);
        EmitVertex();

        gl_Position = Cam.Projection * Cam.View * vec4((VPos[2] + (normalize(VPos[2] - Light.Position)) * Epsilon), 1.0);
        EmitVertex();

        gl_Position = Cam.Projection * Cam.View * vec4((VPos[4] + (normalize(VPos[4] - Light.Position)) * Epsilon), 1.0);
        EmitVertex();
        EndPrimitive();
 
        // render the back cap
        gl_Position = Cam.Projection * Cam.View * vec4(VPos[0] - Light.Position, 0.0);
        EmitVertex();

        gl_Position = Cam.Projection * Cam.View * vec4(VPos[4] - Light.Position, 0.0);
        EmitVertex();

        gl_Position = Cam.Projection * Cam.View * vec4(VPos[2] - Light.Position, 0.0);
        EmitVertex();
        EndPrimitive();
    }
}