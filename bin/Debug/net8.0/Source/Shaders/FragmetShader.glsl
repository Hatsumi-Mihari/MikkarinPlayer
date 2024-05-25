#version 450

layout(location = 0) in vec4 vColor;
layout(location = 1) in vec2 fragTexCoord;

layout(location = 0) out vec4 fragColor;

layout(set = 0, binding = 1) uniform texture2D Texture;
layout(set = 0, binding = 2) uniform sampler SamplerT;
void main()
{
    fragColor = vec4(vColor);
    //fragColor =  texture(sampler2D(Texture, SamplerT), fragTexCoord);
}