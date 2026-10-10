# 流体模拟学习计划（Godot 4.7 + C#）

> 目标：从零开始系统掌握实时流体模拟，覆盖**粒子法（SPH/PBF）**与**网格法（欧拉视角）**两大流派，并学会用 GPU（Compute Shader）加速。
> 前提：熟悉 C#，有线性代数/微积分基础。
> 资料详情见 [RESOURCES.md](RESOURCES.md)。

## 路线总览

```
阶段 00 ──► 阶段 01 ──► 阶段 02 ──► 阶段 03 ──► 阶段 04 ──► 阶段 05
C#/Shader   2D粒子水     2D SPH      GPU SPH     2D网格流体   3D网格+综合
工作流热身   (1周)       (2-3周)     (2-3周)     (3-4周)     (3-4周)
```

每个阶段对应 `lessons/` 下一个独立目录（场景 + C# 脚本 + README），完成后打 git tag（如 `lesson-02-done`），用版本历史记录学习进度。

---

## 阶段 00：Godot 4.7 C# 工作流 + GDShader 热身（约 1 周）

**目录**：`lessons/00_basics/`　**目标**：跑通"写 C# → 编译 → 场景挂载 → 运行"闭环，能看懂并修改 GDShader。

- [ ] 理解 `_Process()`（每帧渲染前）与 `_PhysicsProcess()`（固定物理步长）的区别——流体模拟里这个区分至关重要（模拟步进必须用固定 dt）
- [ ] 掌握 `Node2D._Draw()` 自定义绘制 + `QueueRedraw()`（阶段 01/02 渲染粒子的基础）
- [ ] 掌握 `Input`（鼠标位置、按键、拖拽）
- [ ] GDShader 入门：`canvas_item` 类型 shader、`uniform`、`TIME`、UV、颜色混合
- [ ] 了解 C# 与 GDScript 的 API 差异：`Godot.Vector2`、属性命名（`PascalCase`）、信号连接（`+=` 事件）
- [ ] 掌握 dotnet CLI：`dotnet build`，以及 Godot 编辑器里的构建报错排查

**里程碑验收**：`lessons/00_basics/Basics.tscn` 运行后——背景是波浪 shader，鼠标点击/拖拽生成受重力弹跳的粒子圆点。

**参考资料**：官方文档《C# API differences to GDScript》《Shading language》《Your first 2D shader》

---

## 阶段 01：2D 粒子水—— Verlet 积分与粒子间作用力（1-2 周）

**目录**：`lessons/01_particle_water/`　**目标**：不写任何"流体方程"，只用粒子+力做出"像水"的行为，建立物理直觉。

- [ ] **数值积分**：显式欧拉 vs Verlet（位置积分），理解 dt 稳定性问题（为什么固定小步长 + 子步 `substeps` 是实时模拟标配）
- [ ] 粒子受重力、与容器边界碰撞（简单反射/投影）
- [ ] **粒子间作用力**：双密度松弛（Clavet et al. 2005《Particle-based Viscoelastic Fluid Simulation》的简化版）或距离约束弹簧
- [ ] 粒子渲染：先画圆点，再尝试" metaball 感"（小圆 + 阈值/模糊后处理）
- [ ] 性能意识：1000 粒子 O(n²) 邻居查找为什么会卡（引出阶段 02 的空间哈希）

**里程碑验收**：鼠标搅动一个 500~1000 粒子的"水碗"，粒子有堆叠、晃荡、飞溅的行为。

**参考资料**：Ten Minute Physics《How to write an Einstein-style fluid simulation》之外的粒子篇；Clavet 2005 论文；Sebastian Lague《Coding Adventure: Simulating Fluids》（强烈推荐，思路与本阶段几乎一致）

---

## 阶段 02：完整 2D SPH（2-3 周）★ 拉格朗日流派核心

**目录**：`lessons/02_sph_2d/`　**目标**：实现教科书级 WCSPH 求解器，理解"密度→压力→力"链条。

- [ ] **核函数（kernel）**：Poly6、Spiky、Viscosity——为什么密度估计用偶函数、压力梯度用奇函数
- [ ] **密度估计**：ρᵢ = Σ mⱼ W(rᵢⱼ, h)，与静止密度 ρ₀ 比较
- [ ] **状态方程**：p = k(ρ - ρ₀)，压力力 = -Σ mⱼ (pᵢ+pⱼ)/(2ρⱼ) ∇W
- [ ] **粘滞力** 与人工粘滞
- [ ] **空间哈希（Spatial Hash）**：O(n) 邻域搜索，桶大小 = 核半径 h；这是 SPH 能跑 5k+ 粒子的关键
- [ ] 时间步进：CFL 条件直觉、substeps
- [ ] 边界处理：简单的位置钳制 / 粒子边界力
- [ ] 渲染升级：PointMesh 密度图 → SubViewport 模糊 → 阈值，或 Marching Squares 提取水面

**里程碑验收**：2D 大坝溃决（dam break）场景：初始一堵水墙，倒下、飞溅、归平，水面平滑；≥3000 粒子保持实时。

**参考资料**：Müller 2003 论文（本阶段的主教材，逐条对照实现）；Ten Minute Physics《Simulating fluids with SPH particles》

---

## 阶段 03：GPU 化—— Compute Shader SPH（2-3 周）★ 性能飞跃

**目录**：`lessons/03_sph_gpu/`　**目标**：把阶段 02 求解器搬到 GPU，粒子数从几千涨到几十万。

- [ ] Godot 4 渲染架构：`RenderingServer`、`RenderingDevice`、`RD::StorageBuffer`、`RD::ComputeList`
- [ ] 用 `.glsl`（`#[compute]`）写 compute shader，C# 侧管理 Buffer 绑定与 dispatch
- [ ] 数据布局：SoA（position[]/velocity[] 分开存）vs AoS，GPU 友好的内存排布
- [ ] **GPU 邻域搜索**：均匀网格 + 计数排序（counting sort）两遍 pass（clear → count → prefix sum → scatter）
- [ ] 双缓冲：读上一帧 buffer、写新 buffer
- [ ] 渲染：MultiMeshInstance2D 批量画粒子（C# 只更新 buffer，不逐粒子走场景树）

**里程碑验收**：同一段 dam break，GPU 版 ≥50,000 粒子实时（对比阶段 02 的 3000）。

**参考资料**：官方文档《Using compute shaders》《Optimized cell grid fluid simulation》示例仓库；OmbarGT / community compute shader 教程

---

## 阶段 04：2D 欧拉网格流体（3-4 周）★ 拉格朗日 → 欧拉视角切换

**目录**：`lessons/04_grid_fluid_2d/`　**目标**：实现 Jos Stam 风格的 Stable Fluids——烟雾、涡旋、染料；这是 3D 网格流体的全部基础。

- [ ] **Navier-Stokes 动量方程**各项含义：∂u/∂t = -(u·∇)u - ∇p/ρ + ν∇²u + f
- [ ] **MAC 网格（staggered grid）**：速度存格点边上、压力/染料存格心——为什么交错网格能消除棋盘格压力
- [ ] **半拉格朗日平流（semi-Lagrangian advection）**：无条件稳定的回溯采样，理解数值耗散（烟雾为什么越滚越糊）
- [ ] **压力投影**：泊松方程 ∇²p = ∇·u*，用 Jacobi/Gauss-Seidel 迭代解，然后 u -= ∇p → 无散度速度场（不可压缩性的本质）
- [ ] **涡量约束（vorticity confinement）**：补偿数值耗散，找回小涡旋
- [ ] 边界条件：封闭盒（法向速度为零）
- [ ] 实现方式二选一：先 C# CPU 网格（64×64，便于调试）→ 再移植到 compute shader（512×512，用 RD::Texture 采样）
- [ ] 染料（dye）平流渲染 + 鼠标交互注入速度

**里程碑验收**：512×512 GPU 网格，鼠标拖拽出持久的彩色烟涡，像 PavelDoGreat WebGL-Fluid-Simulation 的效果。

**参考资料**：Jos Stam《Real-Time Fluid Dynamics for Games》(GDC 2003) + 《Stable Fluids》论文；**PavelDoGreat/WebGL-Fluid-Simulation 源码（本阶段最佳对照实现，逐 shader 注释）**；Ten Minute Physics《How to write an Einstein-style fluid simulation》（MIT 6.838 课程风格讲解）

---

## 阶段 05：3D 体积烟雾 + 综合项目（3-4 周）

**目录**：`lessons/05_grid_fluid_3d/`　**目标**：把阶段 04 推广到 3D，并完成一个综合 demo。

- [ ] 3D MAC 网格 + 3D 半拉格朗日（三线性插值）
- [ ] 3D 压力求解（Jacobi 多次迭代 / 可选 Red-Black Gauss-Seidel / 多层网格概念了解即可）
- [ ] 3D compute shader：`imageStore` 写 3D 纹理，数据量意识（128³ = 2M 格点）
- [ ] 体积渲染：光线步进（ray marching）逐像素采样密度/染料，雾/发光材质
- [ ] **综合项目**（选一，作为学习收官）：
  - A. 交互式 3D 烟雾游乐场（注入速度/染料/障碍物）
  - B. 3D PBF 液体（Macklin 2013，把阶段 02/03 经验升级到约束求解路线）
  - C. 2D FLIP/PIC 水面（粒子+网格混合法，游戏液体水坑的工业方案）

**参考资料**：Bridson《Fluid Simulation for Computer Graphics》第 2 版（3D 部分主教材，本阶段开始通读）；Acerola 体积云视频；UPenn CIS 563 物理模拟课程讲义

---

## 学习方法约定

1. **每个理论概念先手推/手画一遍再写代码**——流体模拟的坑几乎都出在"公式没理解就抄实现"
2. **调参是一门课**：dt、核半径 h、刚度 k、粘滞系数 ν 各自对稳定性的影响，每个阶段 README 留了调参实验
3. **用 git 记录学习**：每完成一个里程碑 `git tag lesson-XX-done`；调参实验用分支（`exp/sph-stiffness`）
4. **数值爆炸（NaN/乱飞）时排查顺序**：dt 太大 → 边界条件 → 密度估计除零 → 邻居查找越界
5. 每阶段 README 里写清"分步实现指引"，**严格按步提交**，每步可运行
