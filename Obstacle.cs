using Godot;

public partial class Obstacle : Area2D
{
    [Export] private float speed = 400.0f;

    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }

    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
    }

    public override void _Process(double delta)
    {
        Position -= new Vector2(speed * (float)delta, 0);

        if (Position.X < -3000)
        {
            QueueFree();
        }
    }

    private void OnBodyEntered(Node body)
    {
        if (body.Name == "DinoPlayer")
        {
            // Gọi hàm Die() trên DinoPlayer để chuyển sang sprite thua (deadSprite)
            if (body.HasMethod("Die"))
            {
                body.Call("Die");
            }
            else
            {
                var mainScene = GetTree().CurrentScene;
                if (mainScene != null && mainScene.HasMethod("StopGame"))
                {
                    mainScene.Call("StopGame");
                }
            }
            QueueFree();
        }
    }
}