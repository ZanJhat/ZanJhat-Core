#version 320 es

// <Semantic Name='POSITION' Attribute='a_position' />
// <Semantic Name='TEXCOORD' Attribute='a_texcoord' />
layout (location = 0) in vec3 a_position;
layout (location = 1) in vec2 a_texcoord;

uniform mat4 u_worldViewProjectionMatrix;
uniform vec2 u_offset;
uniform float u_zOffset;

// Biến truyền tọa độ ảnh sang Pixel Shader
out vec2 v_texcoord;

void main()
{
    vec4 clipPos = u_worldViewProjectionMatrix * vec4(a_position, 1.0);
    
    // TRICK XUYÊN TƯỜNG (Bức tường tàng hình)
    clipPos.z = u_zOffset * clipPos.w; 
    
    // GIẢI QUYẾT BÀI TOÁN KHOẢNG CÁCH CHỐNG GAI ĐỨT
    // Xa hơn 10.0, viền sẽ bắt đầu mỏng dần cùng với mô hình để không bị đứt rách
    float distanceFactor = min(clipPos.w, 10.0);
    
    clipPos.xy += u_offset * distanceFactor;
    
    gl_Position = clipPos;
    
    // Truyền tọa độ UV cho Pixel Shader đục lỗ Alpha
    v_texcoord = a_texcoord;
    
    OPENGL_POSITION_FIX;
}
