# 学习资料库（中文注释）

按阶段组织。标注 ⭐ 的是该阶段的核心资料，优先吃透。

## 教材与论文（阶段 01/02/05）

| 资料 | 说明 | 阶段 |
|---|---|---|
| ⭐ Müller et al. 2003《Particle-Based Fluid Simulation for Interactive Applications》 | SPH 入门第一论文，公式编号与本仓库阶段 02 实现逐条对应。搜索标题可免费获取 PDF | 02 |
| Clavet et al. 2005《Particle-based Viscoelastic Fluid Simulation》 | 双密度松弛法，阶段 01 的理论原型，也是游戏里粘液/史莱姆常用方案 | 01 |
| Macklin & Müller 2013《Position Based Fluids》 | PBF，位置约束路线的 SPH，比 WCSPH 稳定得多，阶段 05 综合项目 B 的基础 | 02/05 |
| ⭐ Bridson《Fluid Simulation for Computer Graphics, 2nd Edition》 | 网格法圣经。3D 部分（平流、压力投影、边界）是阶段 04/05 的主教材。前 3 章免费试读，学有余力建议购入 | 04/05 |
| Jos Stam《Real-Time Fluid Dynamics for Games》(GDC 2003) + 《Stable Fluids》(SIGGRAPH 1999) | 网格流体鼻祖，半拉格朗日+压力投影框架的出处，8 页读完即可动手 | 04 |

## 视频课程

| 资料 | 说明 | 阶段 |
|---|---|---|
| ⭐ GAMES201：胡渊鸣《深入浅出物理模拟》（B站有搬运/字幕版） | **中文**课程，Taichi 语言演示，但把 SPH、欧拉网格、FLIP 的思想讲得最清楚，配合本计划各阶段观看 | 全程 |
| ⭐ Matthias Müller《Ten Minute Physics》(YouTube) | 每集 10-20 分钟实现一个物理模拟，作者就是 Müller 2003 论文作者本人。重点看 fluids / SPH 相关几集 | 01/02/04 |
| Sebastian Lague《Coding Adventure: Simulating Fluids》(YouTube) | 粒子流体最佳入门视频，思路与阶段 01/02 一致，配套 GitHub 仓库（Unity，但可迁移） | 01/02 |
| Acerola (YouTube) | 体积渲染、云雾类视觉算法，阶段 05 光线步进的直观讲解 | 05 |
| The Art of Code (YouTube) | Shader 教学频道，补 GDShader 之外的通用 shader 直觉 | 00 |
| Simondev (YouTube) | 图形学工程向杂谈，含流体/粒子专题 | 04/05 |

## 开源参考实现（对照着抄，理解后自己写）

| 仓库 | 说明 | 阶段 |
|---|---|---|
| ⭐ [PavelDoGreat/WebGL-Fluid-Simulation](https://github.com/PavelDoGreat/WebGL-Fluid-Simulation) | 2D 欧拉网格流体最著名的开源实现（在线演示效果惊艳）。代码组织即阶段 04 的求解器结构：advection / divergence / pressure / gradientSubtract / vorticity 各一个 shader。**注意 License 为 GPLv3，只作学习参考，不要复制代码进本仓库** | 04 |
| [Ten Minute Physics 官网 ten-minute-physics.net](https://ten-minute-physics.net) | Müller 的配套讲义与 JS demo | 01/02/04 |
| [Godot Compute Shader 示例（官方 demo 仓库 gui/compute 及相关）](https://github.com/godotengine/godot-demo-projects) | 官方 compute shader 用法参考 | 03 |
| Sebastian Lague Fluid 仓库（视频简介里有链接） | Unity C# 粒子流体，空间哈希+渲染管线参考 | 01/02 |

## Godot 4.7 官方文档（按需查阅）

- **C# 基础**：《C# basics》《C# API differences to GDScript》——命名规则、信号、`Vector2` 等差异总表，C# 开发者必读
- **自定义绘制**：`Node2D` 类参考中的 `_Draw()`、`RenderingServer`（阶段 01/02 渲染粒子）
- **Shading**：《Shading language》《Your first 2D shader》《CanvasItem shaders》《Spatial shaders》
- **Compute（阶段 03 核心）**：《Using compute shaders》《Storage buffers》《Low-level rendering》——`RenderingDevice`、`RD::storage_buffer_create`、`compute_list_dispatch`
- **批量渲染（阶段 03）**：`MultiMeshInstance2D`、`MultiMesh`
- **3D 体积（阶段 05）**：《Volumetric fog》、3D texture、`FogVolume`

## 数学补丁包（用到时查）

- 矢量微积分三件套：梯度 ∇f、散度 ∇·u、旋度 ∇×u —— GAMES201 第 1 课或 Khan Academy《Multivariable calculus》，理解"不可压缩 = 散度为零"是全计划的钥匙
- 泊松方程与迭代求解：Jacobi / Gauss-Seidel 直觉，看 Bridson 第 5 章或 Wikipedia 条目即可
- 核函数（kernel）概念：光滑近似 δ 函数，Müller 2003 论文第 4 节

## 调试与性能工具

- Godot 编辑器内置：**Debugger → Monitors**（FPS、显存）、`print_orphan_nodes()`
- `RenderDoc` 集成（Godot 支持抓帧）——阶段 03/04 排查 compute/渲染问题的利器
- RenderDoc + D3D12（本项目 Windows 默认后端）抓 compute dispatch 查看数据
