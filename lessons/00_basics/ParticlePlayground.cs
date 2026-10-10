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
    private const float Bounce = 0.6f;
    private const float ParticleRadius = 4f;

    // 预分配数组而不是 List<Particle>：阶段 03 搬到 GPU 时数据就是这个 SoA 形态
    private readonly Vector2[] _positions = new Vector2[MaxParticles];
    private readonly Vector2[] _velocities = new Vector2[MaxParticles];
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

            ReflectAt(ref _positions[i], ref _velocities[i], bounds);
        }

        QueueRedraw();
    }

    public override void _Draw()
    {
        float height = GetViewportRect().Size.Y;
        for (int i = 0; i < _count; i++)
        {
            // 按高度渐变色，顺便预演阶段 02"按密度/速度给粒子上色"的渲染习惯
            float hue = Mathf.Lerp(0.55f, 0.45f, Mathf.Clamp(_positions[i].Y / height, 0f, 1f));
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
}
