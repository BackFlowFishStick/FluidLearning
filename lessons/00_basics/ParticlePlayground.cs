using System;
using Godot;

namespace FluidLearing.Lessons.Basics;

/// <summary>
/// 阶段 00 热身：点击/拖拽鼠标生成粒子，粒子受重力下落并在屏幕边缘弹跳。
/// 演示三个后续阶段反复用到的机制：固定步长物理更新、数组化粒子存储、_Draw 自定义绘制。
/// </summary>
public partial class ParticlePlayground : Node2D
{
    private const int MaxParticles = 2000;
    private const float Gravity = 900f;
    private const float Bounce = 0.5f;
    private const float ParticleRadius = 4f;

    // 密度实验参数：核半径 h 取的比阶段 02 小，因为这里的粒子是稀疏飞散的
    private const float H = 24f;
    private const float H2 = H * H;
    private const float Mass = 1f;
    // 2D Poly6 归一化常数 4/(π·h⁸)
    private static readonly float Poly6Norm = 4f / (MathF.PI * MathF.Pow(H, 8f));

    // 预分配数组而不是 List<Particle>：阶段 03 搬到 GPU 时数据就是这个 SoA 形态
    private readonly Vector2[] _positions = new Vector2[MaxParticles];
    private readonly Vector2[] _velocities = new Vector2[MaxParticles];
    private readonly float[] _densities = new float[MaxParticles];
    private int _count;

    private readonly RandomNumberGenerator _rng = new();

    public override void _Ready()
    {
        _rng.Randomize();
    }

    // 物理更新放在固定步长的 _PhysicsProcess 而不是 _Process：
    // 流体求解器对 dt 敏感，dt 抖动会直接导致数值不稳定
    public override void _PhysicsProcess(double delta)
    {
        float dt = (float)delta;
        SpawnFromInput();

        Vector2 bounds = GetViewportRect().Size;
        for (int i = 0; i < _count; i++)
        {
            _velocities[i].Y += Gravity * dt;
            _positions[i] += _velocities[i] * dt;

            // 密度估计 ρᵢ = Σⱼ mⱼ·W(rᵢⱼ, h)：O(n²) 暴力版，只扫已激活的 _count 个粒子
            // （扫描 MaxParticles 会把还没用到的 (0,0) 槽位也算进去，密度全错）
            float density = 0f;
            for (int j = 0; j < _count; j++)   // j == i 也算：自身贡献最大
            {
                float r2 = _positions[i].DistanceSquaredTo(_positions[j]);
                if (r2 < H2)   // Poly6 在 h 外恒为 0
                    density += Mass * Poly6(r2);
            }
            _densities[i] = density;

            ReflectAt(ref _positions[i], ref _velocities[i], bounds);
        }
        
        QueueRedraw();
    }
        
    public override void _Draw()
    {
        // 密度上色：把本帧 [ρ_min, ρ_max] 线性映射到 蓝→红。
        // 用相对范围而不是固定 ρ0，因为这里的粒子是飞散的，绝对密度很小且随粒子数变化
        float min = float.MaxValue, max = float.MinValue;
        for (int i = 0; i < _count; i++)
        {
            min = MathF.Min(min, _densities[i]);
            max = MathF.Max(max, _densities[i]);
        }

        for (int i = 0; i < _count; i++)
        {
            float t = max - min > 1e-6f ? (_densities[i] - min) / (max - min) : 0f;
            float hue = Mathf.Lerp(0.66f, 0f, t);
            DrawCircle(_positions[i], ParticleRadius, Color.FromHsv(hue, 0.75f, 1f));
        }
    }

    private void SpawnFromInput()
    {
        if (!Input.IsMouseButtonPressed(MouseButton.Left))
            return;

        // 每个物理步最多补几个粒子，长按拖拽也不会瞬间塞满
        Vector2 mouse = GetGlobalMousePosition();
        int toSpawn = Mathf.Min(4, MaxParticles - _count);
        for (int i = 0; i < toSpawn; i++)
        {
            _positions[_count] = mouse + new Vector2(_rng.RandfRange(-6f, 6f), _rng.RandfRange(-6f, 6f));
            _velocities[_count] = new Vector2(_rng.RandfRange(-150f, 150f), _rng.RandfRange(-250f, 50f));
            _count++;
        }
    }

    private void ReflectAt(ref Vector2 pos, ref Vector2 vel, Vector2 bounds)
    {
        if (pos.X < ParticleRadius) { pos.X = ParticleRadius; vel.X = -vel.X * Bounce; }
        else if (pos.X > bounds.X - ParticleRadius) { pos.X = bounds.X - ParticleRadius; vel.X = -vel.X * Bounce; }

        if (pos.Y < ParticleRadius) { pos.Y = ParticleRadius; vel.Y = -vel.Y * Bounce; }
        else if (pos.Y > bounds.Y - ParticleRadius) { pos.Y = bounds.Y - ParticleRadius; vel.Y = -vel.Y * Bounce; }
    }

    // Poly6(r, h) = 4/(π·h⁸)·(h² − r²)³，参数收 r² 免开方，r ≥ h 时为 0
    private static float Poly6(float r2)
    {
        float diff = H2 - r2;
        return Poly6Norm * diff * diff * diff;
    }
}
