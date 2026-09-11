using Godot;
using System.Collections.Generic;

public partial class GroundManager : Node2D
{
    [Export] private PackedScene ground1Scene;
    [Export] private PackedScene ground2Scene;
    [Export] private float baseSpeed = 400.0f;
    [Export] private float overlap = 4.0f;

    private float currentSpeedMultiplier = 1.0f;
    private bool isPlaying = false;
    private readonly List<GroundPiece> activeGrounds = new();
    private int nextType = 0;

    public override void _Ready()
    {
        if (ground1Scene == null)
        {
            ground1Scene = GD.Load<PackedScene>("res://ground1.tscn");
        }
        if (ground2Scene == null)
        {
            ground2Scene = GD.Load<PackedScene>("res://ground2.tscn");
        }

        ResetGround();
    }

    public void StartGround()
    {
        isPlaying = true;
    }

    public void StopGround()
    {
        isPlaying = false;
    }

    public void SetSpeedMultiplier(float multiplier)
    {
        currentSpeedMultiplier = Mathf.Max(0.5f, multiplier);
    }

    public void ResetGround()
    {
        isPlaying = false;
        currentSpeedMultiplier = 1.0f;
        nextType = 0;

        // Xoá tất cả các mảnh đất cũ đang hoạt động
        foreach (var ground in activeGrounds)
        {
            if (IsInstanceValid(ground))
            {
                ground.QueueFree();
            }
        }
        activeGrounds.Clear();

        // Đồng thời dọn dẹp các node con cũ nếu có
        foreach (Node child in GetChildren())
        {
            if (child is GroundPiece gp)
            {
                gp.QueueFree();
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

        // Khởi tạo các mảnh đất ban đầu nối tiếp nhau phủ kín màn hình và kéo dài ra phía trước
        float currentX = 0f;
        while (currentX < viewportWidth + 2500f)
        {
            GroundPiece piece = SpawnPiece(currentX);
            if (piece == null)
                break;

            currentX += piece.Width - overlap;
        }
    }

    private GroundPiece SpawnPiece(float startX)
    {
        PackedScene sceneToSpawn = (nextType == 0) ? ground1Scene : ground2Scene;
        nextType = (nextType + 1) % 2; // Luân phiên giữa Ground1 và Ground2

        if (sceneToSpawn == null)
            return null;

        var instance = sceneToSpawn.Instantiate() as GroundPiece;
        if (instance == null)
            return null;

        instance.Position = new Vector2(startX, 0);
        AddChild(instance);
        activeGrounds.Add(instance);
        return instance;
    }

    public override void _Process(double delta)
    {
        if (!isPlaying)
            return;

        float speed = baseSpeed * currentSpeedMultiplier;
        float moveAmount = speed * (float)delta;

        // 1. Di chuyển tất cả các mảnh đất đang chạy sang trái
        for (int i = 0; i < activeGrounds.Count; i++)
        {
            if (IsInstanceValid(activeGrounds[i]))
            {
                activeGrounds[i].Position -= new Vector2(moveAmount, 0);
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

        // 2. Khi chạy hết mặt đất thì tạo ra 1 mặt đất nối liền phần đất cũ để chạy tiếp
        if (activeGrounds.Count > 0)
        {
            var lastGround = activeGrounds[activeGrounds.Count - 1];
            if (IsInstanceValid(lastGround))
            {
                float lastRightEdge = lastGround.Position.X + lastGround.Width;

                // Nếu mép phải của mảnh đất cuối cùng tiến gần tới màn hình, sinh ngay mảnh đất tiếp theo nối liền
                if (lastRightEdge < viewportWidth + 2000f)
                {
                    SpawnPiece(lastRightEdge - overlap);
                }
            }
        }

        // 3. Mặt đất nào chuyển chạy hết rồi (đã trôi hoàn toàn ra ngoài mép trái màn hình) thì xoá đi
        for (int i = activeGrounds.Count - 1; i >= 0; i--)
        {
            var ground = activeGrounds[i];
            if (IsInstanceValid(ground))
            {
                float rightEdge = ground.Position.X + ground.Width;
                if (rightEdge < -100f) // Đã trôi hết qua bên trái màn hình
                {
                    ground.QueueFree();
                    activeGrounds.RemoveAt(i);
                }
            }
            else
            {
                activeGrounds.RemoveAt(i);
            }
        }
    }
}
