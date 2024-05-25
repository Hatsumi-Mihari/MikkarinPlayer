#version 450
layout(binding = 0) uniform Uniforms
{
    mat4 view;
    mat4 proj;
};

layout(location = 0) in vec2 aTextureCoordinate;
layout(location = 1) in vec3 aPosition;
layout(location = 2) in vec4 aColor;
layout(location = 3) in vec4 aModelMlo;
layout(location = 4) in vec4 aModelMlt;
layout(location = 5) in vec4 aModelMlth;
layout(location = 6) in vec4 aModelMlf;

layout(location = 0) out vec4 vColor;
layout(location = 1) out vec2 fragTexCoord;


void main()
{
    mat4 MatrixModel = mat4(aModelMlo, aModelMlt, aModelMlth, aModelMlf);
    fragTexCoord = aTextureCoordinate;
    gl_Position = proj * view * MatrixModel * vec4(aPosition , 1.0f);
    vColor = aColor;
}