#version 450 core

in vec3 Gposition, Gnormal;
flat in vec4 gpos0, gpos1, gpos2, gpos3;
flat in vec2 guv0, guv1, guv2, guv3;

out vec4 Colour;

uniform sampler2D Texture;
uniform vec3 Light;
uniform ivec4 VP;

float cross2D(vec2 a, vec2 b) { return (a.x * b.y) - (a.y * b.x); }

vec2 UV() {
	// from quad rendering
	// get barycentric weights do perspective div, set uv weighted-sum
    vec2 p = vec2(2 * gl_FragCoord.x / VP.w - 1, 2 * gl_FragCoord.y / VP.x - 1);
    
    float w[4] = { gpos0.w, gpos1.w, gpos2.w, gpos3.w }; 
    vec2 v[4] = {
        vec2(gpos0) / w[0], 
        vec2(gpos1) / w[1], 
        vec2(gpos2) / w[2], 
        vec2(gpos3) / w[3] 
        };
    
    float r[4];
    vec2  s[4];
    for (int i = 0; i < 4; i++) {
        s[i] = v[i] - p;
        r[i] = length(s[i]) * sign(w[i]);
    }

    float t[4];
    for (int i = 0; i < 4; i++) 
    {
        float A = cross2D(s[i], s[(i + 1) % 4]);
        float D = dot(s[i], s[(i + 1) % 4]);
        t[i] = (r[i] * r[(i + 1) % 4] - D) / A;
    }

    float u[4]; 
    for (int i = 0; i < 4; i++) {
        u[i] = (t[(i + 3) % 4] + t[i]) / r[i];
    }

    vec4 wt = vec4(u[0], u[1], u[2], u[3]) / (u[0] + u[1] + u[2] + u[3]);
    
    float f[4] = { 
        wt[0] / gpos0.w, 
        wt[1] / gpos1.w, 
        wt[2] / gpos2.w, 
        wt[3] / gpos3.w 
        };
    
    return (f[0]*guv0 + f[1]*guv1 + f[2]*guv2 + f[3]*guv3) / (f[0] + f[1] + f[2] + f[3]);
}


void main() 
{
    vec2 UV = UV();
    Colour = texture(Texture, UV);
}