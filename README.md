# FluidLearning — Godot 4.7 C# 流体模拟学习项目

从零学习实时流体模拟：**粒子法（SPH / PBF）→ GPU Compute Shader → 欧拉网格法（Stable Fluids）→ 3D 体积烟雾**，全部在 Godot 4.7 + C# (.NET 8) 中实现。

## 快速开始

1. 安装 [Godot 4.7 .NET 版](https://godotengine.org/download) 与 [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
2. 用 Godot 打开本项目根目录的 `project.godot`
3. 命令行构建（或交给编辑器自动构建）：
   ```bash
   dotnet build
   ```
4. 从 `lessons/` 选择当前学习阶段的主场景运行

## 学习路线

详见 **[docs/LEARNING_PLAN.md](docs/LEARNING_PLAN.md)**（含每个阶段的知识点、验收标准与分步指引），资料索引见 **[docs/RESOURCES.md](docs/RESOURCES.md)**。

| 阶段 | 目录 | 内容 |
|---|---|---|
| 00 | `lessons/00_basics` | Godot C# 工作流 + GDShader 热身（粒子 playground） |
| 01 | `lessons/01_particle_water` | 2D 粒子水：Verlet 积分、粒子间作用力 |
| 02 | `lessons/02_sph_2d` | 完整 2D SPH：核函数、密度/压力、空间哈希 |
| 03 | `lessons/03_sph_gpu` | Compute Shader 化 SPH：RenderingDevice、SSBO |
| 04 | `lessons/04_grid_fluid_2d` | 2D 欧拉网格：半拉格朗日平流、压力投影、涡量约束 |
| 05 | `lessons/05_grid_fluid_3d` | 3D 网格烟雾 + 综合项目 |

## 工程约定

- **技术栈**：Godot 4.7 / Forward Plus (D3D12) / Jolt Physics / C# .NET 8
- **代码组织**：每个阶段独立场景 + 脚本 + README，互不依赖
- **版本控制**：每完成一个里程碑打 tag `lesson-XX-done`；调参实验开 `exp/*` 分支
- 学习笔记（中文）写在各阶段 README 与 `docs/` 下，随代码一起提交
