#version 450 core
const float Epsilon = 0.001953125; // 0.03125 // 0.015625 // 0.0078125 // 0.00390625 // 0.001953125

layout (triangles_adjacency) in; 
layout (triangle_strip, max_vertices = 18) out; 

in vec3[] VPos; 

uniform vec3 LightPosition;
layout(std140) uniform CameraBlock {
	mat4 Projection;
	mat4 View;
	vec3 Position;
    vec2 ScreenSize;
} Cam;

layout(std140) uniform LightBlock {
    vec3 Colour;
    float AmbientIntensity;
    vec3 Direction;
    float DiffuseIntensity;
} Light;


void EmitEdge( vec3 a, vec3 b, vec3 Dir, vec3 Dev) {
  gl_Position = Cam.Projection * Cam.View * vec4(a + Dev, 1);
  EmitVertex();
  
  gl_Position = Cam.Projection * Cam.View * vec4(Dir, 0);
  EmitVertex();

  gl_Position = Cam.Projection * Cam.View * vec4(b + Dev, 1);
  EmitVertex();

  gl_Position = Cam.Projection * Cam.View * vec4(Dir, 0);
  EmitVertex();
  EndPrimitive();
}

bool FacesLight(vec3 P1, vec3 P2, vec3 P3, vec3 LightDir)
{
  return dot(cross(P2 - P1, P3 - P1), LightDir) > 0; 
}

void main(void)
{
    vec3 LightDir = normalize(Light.Direction);
    vec3 Deviation = LightDir * Epsilon;

    // Handle only light facing triangles
    if (FacesLight(VPos[0], VPos[2], VPos[4], LightDir)) {

        if (!FacesLight(VPos[0], VPos[1], VPos[2], LightDir))
            EmitEdge(VPos[0], VPos[2], LightDir, Deviation);
        
        if (!FacesLight(VPos[2], VPos[3], VPos[4], LightDir))
            EmitEdge(VPos[2], VPos[4], LightDir, Deviation);
        
        if (!FacesLight(VPos[4], VPos[5], VPos[0], LightDir))
            EmitEdge(VPos[4], VPos[0], LightDir, Deviation);
        

        // render the front cap
        gl_Position = Cam.Projection * Cam.View * vec4((VPos[0] + Deviation), 1.0);
        EmitVertex();

        gl_Position = Cam.Projection * Cam.View * vec4((VPos[2] + Deviation), 1.0);
        EmitVertex();

        gl_Position = Cam.Projection * Cam.View * vec4((VPos[4] + Deviation), 1.0);
        EmitVertex();
        EndPrimitive();
 
        // render the back cap
        gl_Position = Cam.Projection * Cam.View * vec4(LightDir, 0.0);
        EmitVertex();

        gl_Position = Cam.Projection * Cam.View * vec4(LightDir, 0.0);
        EmitVertex();

        gl_Position = Cam.Projection * Cam.View * vec4(LightDir, 0.0);
        EmitVertex();
        EndPrimitive();
    }
}
