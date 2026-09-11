using Godot;
using System.Collections.Generic;

public partial class SkyManager : Node2D
{
    [Export] private PackedScene skyScene;
    [Export] private float baseSpeed = 400.0f; // Tốc độ di chuyển giống hệt mặt đất
    [Export] private float overlap = 2.0f;

    private float currentSpeedMultiplier = 1.0f;
    private bool isPlaying = false;
    private readonly List<SkyPiece> activeSkies = new();

    public override void _Ready()
    {
        if (skyScene == null)
        {
            skyScene = GD.Load<PackedScene>("res://sky_piece.tscn");
        }

        ResetSky();
    }

    public void StartSky()
    {
        isPlaying = true;
    }

    public void StopSky()
    {
        isPlaying = false;
    }

    public void SetSpeedMultiplier(float multiplier)
    {
        currentSpeedMultiplier = Mathf.Max(0.5f, multiplier);
    }

    public void ResetSky()
    {
        isPlaying = false;
        currentSpeedMultiplier = 1.0f;

        // Xoá tất cả các mảng nền trời cũ đang chạy
        foreach (var sky in activeSkies)
        {
            if (IsInstanceValid(sky))
            {
                sky.QueueFree();
            }
        }
        activeSkies.Clear();

        foreach (Node child in GetChildren())
        {
            if (child is SkyPiece sp)
            {
                sp.QueueFree();
            }
        }

        float viewportWidth = 1920f;
        try
        {
            float w = GetViewportRect().Size.X;
            if (w > 100f)
            {
                viewportWidth = w;
            }
        }
        catch
        {
            viewportWidth = 1920f;
        }

        // Khởi tạo các mảnh nền trời ban đầu phủ kín màn hình và kéo dài sang bên phải
        float currentX = 0f;
        while (currentX < viewportWidth + 2500f)
        {
            SkyPiece piece = SpawnPiece(currentX);
            if (piece == null)
                break;

            currentX += piece.Width - overlap;
        }
    }

    private SkyPiece SpawnPiece(float startX)
    {
        if (skyScene == null)
            return null;

        var instance = skyScene.Instantiate() as SkyPiece;
        if (instance == null)
            return null;

        instance.Position = new Vector2(startX, 0);
        AddChild(instance);
        activeSkies.Add(instance);
        return instance;
    }

    public override void _Process(double delta)
    {
        if (!isPlaying)
            return;

        // Tốc độ di chuyển khớp hoàn toàn với mặt đất
        float speed = baseSpeed * currentSpeedMultiplier;
        float moveAmount = speed * (float)delta;

        // 1. Di chuyển tất cả các mảnh nền trời sang trái
        for (int i = 0; i < activeSkies.Count; i++)
        {
            if (IsInstanceValid(activeSkies[i]))
            {
                activeSkies[i].Position -= new Vector2(moveAmount, 0);
            }
        }

        float viewportWidth = 1920f;
        try
        {
            float w = GetViewportRect().Size.X;
            if (w > 100f)
            {
                viewportWidth = w;
            }
        }
        catch
        {
            viewportWidth = 1920f;
        }

        // 2. Khi chạy hết nền trời thì tạo ra 1 nền trời mới nối liền phần cũ để chạy tiếp
        if (activeSkies.Count > 0)
        {
            var lastSky = activeSkies[activeSkies.Count - 1];
            if (IsInstanceValid(lastSky))
            {
                float lastRightEdge = lastSky.Position.X + lastSky.Width;

                // Nếu mép phải của mảng nền trời cuối cùng tiến gần tới màn hình, sinh ngay nền trời tiếp theo nối liền
                if (lastRightEdge < viewportWidth + 2000f)
                {
                    SpawnPiece(lastRightEdge - overlap);
                }
            }
        }

        // 3. Nền trời nào chuyển chạy hết rồi (đã trôi hoàn toàn ra ngoài mép trái màn hình) thì xoá đi
        for (int i = activeSkies.Count - 1; i >= 0; i--)
        {
            var sky = activeSkies[i];
            if (IsInstanceValid(sky))
            {
                float rightEdge = sky.Position.X + sky.Width;
                if (rightEdge < -100f) // Đã trôi hết qua bên trái màn hình
                {
                    sky.QueueFree();
                    activeSkies.RemoveAt(i);
                }
            }
            else
            {
                activeSkies.RemoveAt(i);
            }
        }
    }
}
