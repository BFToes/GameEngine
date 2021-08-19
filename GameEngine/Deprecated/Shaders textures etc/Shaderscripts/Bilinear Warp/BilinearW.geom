#version 450 core

layout(lines_adjacency, max_vertices = 4) in; // quad in
layout(triangle_strip, max_vertices = 6) out; // triangles out

in vec4 Vposition[4];
in vec3 Vnormal[4];
in vec2 Vuv[4];

out vec3 Gposition, Gnormal;
flat out vec4 gpos0, gpos1, gpos2, gpos3;
flat out vec2 guv0, guv1, guv2, guv3;

void EmitPoint(int i, bool provokeFlatOutput) {
    Gnormal = Vnormal[i];
    gl_Position = Vposition[i];
    if (provokeFlatOutput) {
        gpos0 = Vposition[0]; guv0 = Vuv[0]; 
        gpos1 = Vposition[1]; guv1 = Vuv[1]; 
        gpos2 = Vposition[2]; guv2 = Vuv[2]; 
        gpos3 = Vposition[3]; guv3 = Vuv[3];
        
        
    }
    EmitVertex();
}


void main() {
    // (-1, 1), ( 1, 1), ( 1,-1) -> ( 0, 1), ( 1, 1), (1, 0)
    EmitPoint(1, false); EmitPoint(2, false); EmitPoint(3, true);
    EndPrimitive();
    // (-1,-1), (-1, 1), ( 1,-1) -> ( 0, 0), ( 0, 1), ( 1, 0)
    EmitPoint(0, false); EmitPoint(1, false); EmitPoint(3, true);
    EndPrimitive();
    
    
}