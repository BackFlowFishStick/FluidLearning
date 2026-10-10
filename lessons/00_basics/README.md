# 阶段 00：Godot 4.7 C# 工作流 + GDShader 热身

**目标**：跑通「写 C# → `dotnet build` → 场景挂载 → 运行调试」闭环，能独立修改本阶段的脚本与 shader。

## 运行

1. Godot 编辑器打开本项目的 `project.godot`
2. 打开 `lessons/00_basics/Basics.tscn`，按 F5（或 F6 运行当前场景）
3. **按住鼠标左键拖拽**：生成粒子，受重力下落、屏幕边缘弹跳

## 场景结构

```
Basics (Node2D)  ← ParticlePlayground.cs
├── Background (ColorRect)  ← wave.gdshader 波浪网格背景
└── Particles (Node2D)      ← 粒子实际由脚本 _Draw() 画在根节点上，此节点预留
```

## 知识点对照（改代码验证）

| 知识点 | 在哪里 | 动手实验 |
|---|---|---|
| `_PhysicsProcess` 固定步长 | `ParticlePlayground.cs` | 把物理逻辑挪进 `_Process`，观察不到差别？把 dt 手动放大 10 倍再挪——固定步长的意义在阶段 02 才会真正体现 |
| 数值积分 | `ReflectAt` 上的 `pos += vel * dt` | 把 `dt` 改成 `2 * dt`（显式欧拉错版），看粒子会不会越弹越高——这就是数值不稳定 |
| `_Draw` / `QueueRedraw` | `_Draw()` | 试着改 `ParticleRadius`、按速度上色而不是按高度 |
| 输入 API | `SpawnFromInput` | 加一个右键"清空粒子"（提示：`Input.IsMouseButtonPressed(MouseButton.Right)`） |
| GDShader | `wave.gdshader` | 改 `wave_strength`；把网格密度从 20 改成 64；加一个第三层正弦波 |

## 调参实验

1. `Gravity` 从 900 改成 1800：粒子行为如何变化？
2. `Bounce` 改成 1.0（完全弹性）和 0.0（完全非弹性）：分别像什么物理场景？
3. `MaxParticles` 拉满 + 长按拖拽 30 秒：帧率还稳吗？瓶颈在哪（绘制调用次数）？——这个卡顿就是后面所有"性能优化"话题的引子

## 完成后

- [ ] 所有动手实验做过一遍
- [ ] `git add -A && git commit -m "lesson00: 完成 C# 工作流与 shader 热身"`
- [ ] 进入 [阶段 01](../01_particle_water/)：给这些粒子加上"相互作用力"，让它们开始像水
