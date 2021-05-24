#version 450 core

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
    vec3 Attenuation;
} Light;


in vec3 Position;


void main(void)
{
	gl_Position = Cam.Projection * Cam.View * Light.Model * vec4(Position, 1); 
}