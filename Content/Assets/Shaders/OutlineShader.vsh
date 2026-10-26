#ifdef GLSL

// <Semantic Name='POSITION' Attribute='a_position' />
// <Semantic Name='TEXCOORD' Attribute='a_texcoord' />

attribute vec3 a_position;
attribute vec2 a_texcoord;

uniform mat4 u_worldViewProjectionMatrix;
uniform vec2 u_offset;
uniform float u_zOffset;

// Đổi 'out' thành 'varying' để tương thích GLES 2.0
varying vec2 v_texcoord;

void main()
{
    vec4 clipPos = u_worldViewProjectionMatrix * vec4(a_position, 1.0);

    // TRICK XUYÊN TƯỜNG (Bức tường tàng hình)
    clipPos.z = u_zOffset * clipPos.w;

    // GIẢI QUYẾT BÀI TOÁN KHOẢNG CÁCH CHỐNG GAI ĐỨT
    float distanceFactor = min(clipPos.w, 10.0);
    clipPos.xy += u_offset * distanceFactor;
    
    gl_Position = clipPos;
    
    v_texcoord = a_texcoord;

    OPENGL_POSITION_FIX;
}

#endif
