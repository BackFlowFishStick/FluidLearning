using System;
using Godot;

namespace FluidLearing.Lessons.Sph;

/// <summary>
/// 阶段 02 前置样本：Poly6 核密度估计可视化。
/// 公式：ρᵢ = Σⱼ mⱼ·W(rᵢⱼ, h)（求和包含 j = i 自身）
/// 粒子按静止间距排布并用弹簧拴在初始格点上，按住鼠标左键推开粒子，
/// 实时观察密度场：蓝 ≈ 静止密度 ρ0，越红越密。白色圆圈 = 核半径 h。
/// 刻意用 O(n²) 暴力求和——256 粒子无压力，空间哈希留到正式实现再引入。
/// </summary>
public partial class DensitySample : Node2D
{
    private const int Side = 16;                    // 16×16 = 256 粒子
    private const float H = 40f;                    // 核半径（影响气泡的半径）
    private const float Spacing = 22f;              // 静止间距
    private const float Rho0 = 1f;                  // 静止密度（参考值）
    private const float Mass = Spacing * Spacing;   // 2D 质量：m = ρ0·spacing²，这样静止排布时 ρ ≈ ρ0

    // 2D Poly6 归一化常数 4/(π·h⁸)，保证 ∬W dA = 1
    private static readonly float Poly6Norm = 4f / (MathF.PI * MathF.Pow(H, 8f));
    private static readonly float H2 = H * H;

    // 弹簧把粒子拴回初始格点（否则推一下就满屏乱跑，看不出密度场）
    private const float SpringK = 24f;
    private const float Damping = 6f;
    private const float PushStrength = 2400f;
    private const float PushRadius = 120f;

    private readonly Vector2[] _positions = new Vector2[Side * Side];
    private readonly Vector2[] _homes = new Vector2[Side * Side];
    private readonly Vector2[] _velocities = new Vector2[Side * Side];
    private readonly float[] _densities = new float[Side * Side];

    private int _nearestToMouse;
    private Label _stats = null!;

    public override void _Ready()
    {
        Vector2 center = GetViewportRect().Size / 2f;
        Vector2 extent = new((Side - 1) * Spacing / 2f, (Side - 1) * Spacing / 2f);
        int idx = 0;
        for (int y = 0; y < Side; y++)
        {
            for (int x = 0; x < Side; x++)
            {
                _homes[idx] = center + new Vector2(x * Spacing, y * Spacing) - extent;
                _positions[idx] = _homes[idx];
                idx++;
            }
        }

        _stats = GetNode<Label>("StatsLabel");
    }

    public override void _PhysicsProcess(double delta)
    {
        float dt = (float)delta;
        TrackMouse();
        ApplyMousePush(dt);
        Integrate(dt);
        ComputeDensities();
        UpdateStats();
        QueueRedraw();
    }

    /// <summary>密度估计：对每个粒子，把核半径内所有粒子（含自己）的质量贡献求和。</summary>
    private void ComputeDensities()
    {
        for (int i = 0; i < _positions.Length; i++)
        {
            float rho = 0f;
            for (int j = 0; j < _positions.Length; j++)
            {
                // Poly6 只依赖 r²，全程不用开方；h 外贡献恒为 0，直接跳过
                float r2 = _positions[i].DistanceSquaredTo(_positions[j]);
                if (r2 < H2)
                    rho += Mass * Poly6(r2);
            }
            _densities[i] = rho;
        }
    }

    /// <summary>
    /// Poly6(r, h) = K·(h² − r²)³，K = 4/(π·h⁸)。注意参数是 r²（预先平方好的距离）。
    /// 逐项对应：diff = H2 − r2 就是括号内的 h² − r²（H2 = h²，r2 = r²），
    /// 随后 diff * diff * diff 把它立方，再乘归一化常数 K。
    /// 直接在 r² 上运算而非开方后算 (h − r)³：省一次开方，且 r = 0（粒子自身贡献）
    /// 及极小 r 处数值更稳定——Poly6 本就是只依赖 r² 的光滑核。
    /// </summary>
    private static float Poly6(float r2)
    {
        float diff = H2 - r2;
        return Poly6Norm * diff * diff * diff;
    }

    private void Integrate(float dt)
    {
        for (int i = 0; i < _positions.Length; i++)
        {
            // 弹簧回位 + 阻尼（单位质量的加速度，不除质量）
            Vector2 accel = -SpringK * (_positions[i] - _homes[i]) - Damping * _velocities[i];
            _velocities[i] += accel * dt;
            _positions[i] += _velocities[i] * dt;
        }
    }

    private void TrackMouse()
    {
        Vector2 mouse = GetGlobalMousePosition();
        float best = float.MaxValue;
        for (int i = 0; i < _positions.Length; i++)
        {
            float d2 = _positions[i].DistanceSquaredTo(mouse);
            if (d2 < best)
            {
                best = d2;
                _nearestToMouse = i;
            }
        }
    }

    private void ApplyMousePush(float dt)
    {
        if (!Input.IsMouseButtonPressed(MouseButton.Left))
            return;

        Vector2 mouse = GetGlobalMousePosition();
        for (int i = 0; i < _positions.Length; i++)
        {
            Vector2 away = _positions[i] - mouse;
            float dist = away.Length();
            if (dist < PushRadius && dist > 0.01f)
            {
                float falloff = 1f - dist / PushRadius;
                _velocities[i] += away / dist * PushStrength * falloff * dt;
            }
        }
    }

    /// <summary>
    /// 可视化两样东西：
    /// 1. 密度上色——把每个粒子的 ρ/ρ0 从 0.5（稀疏）到 1.5（拥挤）线性映射到
    ///    色相 0.66（蓝）→ 0（红）：蓝 ≈ 静止密度 ρ0，越红越密，越蓝越稀。
    ///    每个粒子画成一个半径 5px 的实心圆，256 个圆拼出连续的密度场。
    /// 2. 核半径圈——在离鼠标最近的粒子处画一个半径为 h 的半透明白圈，
    ///    圈内的粒子才会给中心粒子贡献密度（Poly6 在 r ≥ h 处恒为 0），
    ///    直观展示"密度气泡"的范围就是光滑长度 h。
    /// </summary>
    public override void _Draw()
    {
        // 密度上色：ρ/ρ0 从 0.5（稀）到 1.5（密）映射为 蓝 → 红
        for (int i = 0; i < _positions.Length; i++)
        {
            float ratio = _densities[i] / Rho0;
            float t = Mathf.Clamp((ratio - 0.5f) / 1.0f, 0f, 1f);
            float hue = Mathf.Lerp(0.66f, 0f, t);
            DrawCircle(_positions[i], 5f, Color.FromHsv(hue, 0.85f, 1f));
        }

        // 白色圆圈标出核半径 h：圈内粒子才会给中心粒子贡献密度
        DrawArc(_positions[_nearestToMouse], H, 0f, MathF.Tau, 96,
            new Color(1f, 1f, 1f, 0.35f), 1.5f);
    }

    private void UpdateStats()
    {
        float sum = 0f, max = 0f;
        for (int i = 0; i < _densities.Length; i++)
        {
            sum += _densities[i];
            max = MathF.Max(max, _densities[i]);
        }
        float avg = sum / _densities.Length;
        _stats.Text = $"ρ_avg = {avg:F3}（ρ0 = {Rho0}）   ρ_max = {max:F3}   按 h={H}px、间距={Spacing}px";
    }
}
