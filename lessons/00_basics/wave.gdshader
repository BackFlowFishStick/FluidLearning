shader_type canvas_item;

// 阶段 00 shader 热身：动态波浪背景 + 淡淡的网格线。
// 网格线是给后续网格流体阶段（04）建立"计算域"的视觉直觉。

uniform vec4 base_color : source_color = vec4(0.04, 0.05, 0.10, 1.0);
uniform vec4 wave_color : source_color = vec4(0.15, 0.45, 0.75, 1.0);
uniform float wave_strength : hint_range(0.0, 1.0) = 0.35;

void fragment() {
    // 两组正弦波叠加，方向和频率不同，产生起伏感
    float w1 = sin(UV.x * 18.0 + TIME * 1.6) * 0.5 + 0.5;
    float w2 = sin(UV.y * 7.0 - TIME * 1.1) * 0.5 + 0.5;
    float wave = w1 * w2;

    vec4 col = mix(base_color, wave_color, wave * wave_strength);

    // 20x20 网格线：fract 把 UV 切成格子，越靠近格子边缘线越亮
    vec2 grid = abs(fract(UV * 20.0) - 0.5);
    float line = smoothstep(0.46, 0.5, max(grid.x, grid.y));
    col = mix(col, wave_color, line * 0.10);

    COLOR = col;
}
