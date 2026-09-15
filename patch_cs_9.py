import re

with open("Node2d.cs", "r") as f:
    code = f.read()

# 1. Add private Car backgroundCar; to declarations
if "private Car backgroundCar;" not in code:
    code = code.replace("private bool isPlaying = false;", "private bool isPlaying = false;\n    private Car backgroundCar;")

# 2. Add instantiation in _Ready
ready_insert = """
        PackedScene carScene = GD.Load<PackedScene>("res://car.tscn");
        if (carScene != null) {
            backgroundCar = carScene.Instantiate<Car>();
            backgroundCar.Position = new Vector2(70, 310);
            backgroundCar.Scale = new Vector2(0.15f, 0.15f);
            backgroundCar.ZIndex = 1;
            AddChild(backgroundCar);
            // Re-order so it is behind HUD but in front of sky
            MoveChild(backgroundCar, 3);
        }
"""
code = re.sub(r'public override void _Ready\(\)\s*\{', 'public override void _Ready()\n    {' + ready_insert, code, count=1)

# 3. In StartGame, add backgroundCar?.StartCar(); and SetSpeedMultiplier
startgame_insert = """
    if (backgroundCar != null) {
        backgroundCar.StartCar();
        backgroundCar.SetSpeedMultiplier(currentSpeedMultiplier);
    }
"""
code = re.sub(r'isPlaying = true;\s*spacePressCount = 0;', 'isPlaying = true;\n    spacePressCount = 0;' + startgame_insert, code, count=1)

# 4. In _Process, add backgroundCar?.SetSpeedMultiplier(currentSpeedMultiplier);
process_insert = """
            if (backgroundCar != null)
            {
                backgroundCar.SetSpeedMultiplier(currentSpeedMultiplier);
            }
"""
code = re.sub(r'if \(dinoPlayer != null\)\s*\{\s*dinoPlayer\.SetSpeedMultiplier\(currentSpeedMultiplier\);\s*\}', 'if (dinoPlayer != null)\n            {\n                dinoPlayer.SetSpeedMultiplier(currentSpeedMultiplier);\n            }' + process_insert, code, count=1)


# 5. In StopGame, add backgroundCar?.StopCar();
stopgame_insert = """
    if (backgroundCar != null) {
        backgroundCar.StopCar();
    }
"""
code = re.sub(r'public void StopGame\(\)\s*\{\s*isPlaying = false;', 'public void StopGame()\n{\n    isPlaying = false;' + stopgame_insert, code, count=1)

with open("Node2d.cs", "w") as f:
    f.write(code)

print("Patch applied")
