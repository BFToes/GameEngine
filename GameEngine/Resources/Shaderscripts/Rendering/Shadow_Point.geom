#version 450 core
const float Epsilon = 0.001953125; // 0.03125 // 0.015625 // 0.0078125 // 0.00390625

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

void EmitEdge(vec3 a, vec3 b) {
  vec3 LightDir = normalize(a - Light.Position.xyz); 
  gl_Position = Cam.Projection * Cam.View * vec4(a + LightDir * Epsilon, 1);
  EmitVertex();
  
  gl_Position = Cam.Projection * Cam.View * vec4(LightDir, 0);
  EmitVertex();

  LightDir = normalize(b - Light.Position.xyz); 
  gl_Position = Cam.Projection * Cam.View * vec4(b + LightDir * Epsilon, 1);
  EmitVertex();

  gl_Position = Cam.Projection * Cam.View * vec4(LightDir, 0);
  EmitVertex();
  EndPrimitive();
}

bool FacesLight(vec3 P1, vec3 P2, vec3 P3)
{
  vec3 N = cross(P2 - P1, P3 - P1);
  return dot(N, Light.Position.xyz - P1) > 0 || dot(N, Light.Position.xyz - P2) > 0 || dot(N, Light.Position.xyz - P3) > 0; 
}

void main(void)
{
    vec3 LightDir = normalize(Light.Position - VPos[0]);

    // Handle only light facing triangles
    if (FacesLight(VPos[0], VPos[2], VPos[4])) {

        if (!FacesLight(VPos[0], VPos[1], VPos[2]))
            EmitEdge(VPos[0], VPos[2]);
        
        if (!FacesLight(VPos[2], VPos[3], VPos[4]))
            EmitEdge(VPos[2], VPos[4]);
        
        if (!FacesLight(VPos[4], VPos[5], VPos[0]))
            EmitEdge(VPos[4], VPos[0]);
        

        // render the front cap
        gl_Position = Cam.Projection * Cam.View * vec4(VPos[0] + normalize(VPos[0] - Light.Position) * Epsilon, 1.0);
        EmitVertex();

        gl_Position = Cam.Projection * Cam.View * vec4(VPos[2] + normalize(VPos[2] - Light.Position) * Epsilon, 1.0);
        EmitVertex();

        gl_Position = Cam.Projection * Cam.View * vec4(VPos[4] + normalize(VPos[4] - Light.Position) * Epsilon, 1.0);
        EmitVertex();
        EndPrimitive();
 
        // render the back cap
        gl_Position = Cam.Projection * Cam.View * vec4(VPos[0] - Light.Position, 0);
        EmitVertex();

        gl_Position = Cam.Projection * Cam.View * vec4(VPos[4] - Light.Position, 0);
        EmitVertex();

        gl_Position = Cam.Projection * Cam.View * vec4(VPos[2] - Light.Position, 0);
        EmitVertex();
        EndPrimitive();
    }
}