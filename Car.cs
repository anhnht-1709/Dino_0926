using Godot;
using System;

public partial class Car : Node2D
{
    private Sprite2D wheel1;
    private Sprite2D wheel2;
    private Sprite2D body;
    
    private bool isRunning = false;
    private float speedMultiplier = 1.0f;
    private float baseRotationSpeed = 10.0f; // radians per second
    
    private float timeElapsed = 0f;

    public override void _Ready()
    {
        wheel1 = GetNode<Sprite2D>("Wheel1");
        wheel2 = GetNode<Sprite2D>("Wheel2");
        body = GetNode<Sprite2D>("Body");
        
        // Initial setup
        body.Position = new Vector2(0, 0);
    }

    public override void _Process(double delta)
    {
        if (isRunning)
        {
            float rot = (float)delta * baseRotationSpeed * speedMultiplier;
            wheel1.Rotation += rot;
            wheel2.Rotation += rot;
            
            // Add a slight bumpy effect
            timeElapsed += (float)delta * 15f * speedMultiplier;
            float bump = (float)Math.Sin(timeElapsed) * 2f;
            body.Position = new Vector2(0, bump);
        }
    }

    public void StartCar()
    {
        isRunning = true;
    }

    public void StopCar()
    {
        isRunning = false;
    }
    
    public void SetSpeedMultiplier(float mult)
    {
        speedMultiplier = mult;
    }
}
