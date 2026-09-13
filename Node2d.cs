using Godot;

public partial class Node2d : Godot.Node2D
{
    [Export] private TextureButton startButton;
    [Export] private TextureButton bigClothesButton;
    [Export] private TextureButton bigSettingButton;
    [Export] private TextureButton settingButtonSmall;
    private Label titleLabel;
    [Export] private TextureButton closeSettingButton;
    [Export] private BaseButton gameOverFrame;
    [Export] private BaseButton homeButton;
    [Export] private BaseButton replayButton;
    private Label gameOverScoreLabel;
    private Label gameOverHiLabel;
    private Label gameOverHintLabel;

    [Export] private Spawner spawner;
    [Export] private SkyManager skyManager;
    [Export] private GroundManager groundManager;
    [Export] private AnimationPlayer groundAnimation;
    [Export] private Label scoreLabel;
    [Export] private Control scoreFrame;
	private DinoPlayer dinoPlayer;
    [Export] private BaseButton changeClothesButton;
    [Export] private Control clothesPanel;

    [Export] private float initialSpeed = 1.0f;
    [Export] private float maxSpeed = 2.5f;
    [Export] private float speedAcceleration = 0.015f; // Mỗi giây tăng thêm 0.015 lần tốc độ gốc

    private float currentSpeedMultiplier = 1.0f;
    private float currentScore = 0f;
    private int highScore = 0;

    private bool isPlaying = false;
    private int spacePressCount = 0;
    private float spacePressTimer = 0f;

    public override void _Ready()
    {
        if (scoreFrame == null)
        {
            scoreFrame = GetNodeOrNull<Control>("HUD/ScoreFrame");
        }
        if (scoreLabel == null)
        {
            scoreLabel = GetNodeOrNull<Label>("HUD/ScoreFrame/ScoreLabel") ?? GetNodeOrNull<Label>("HUD/ScoreLabel");
        }
        UpdateScoreLayout();
        GetViewport().SizeChanged += UpdateScoreLayout;
        UpdateScoreDisplay();

        
        if (bigSettingButton == null)
            bigSettingButton = GetNodeOrNull<TextureButton>("HUD/BigSettingButton");
        if (bigSettingButton != null)
            bigSettingButton.Pressed += OnSettingButtonPressed;

        if (settingButtonSmall == null)
            settingButtonSmall = GetNodeOrNull<TextureButton>("HUD/SettingButtonSmall");
        if (settingButtonSmall != null)
            settingButtonSmall.Pressed += OnSettingButtonPressed;

        titleLabel = GetNodeOrNull<Label>("HUD/GameOverFrame/TitleLabel");

        if (closeSettingButton == null)
            closeSettingButton = GetNodeOrNull<TextureButton>("HUD/GameOverFrame/CloseSettingButton");
        if (closeSettingButton != null)
            closeSettingButton.Pressed += OnCloseSettingButtonPressed;


        if (bigClothesButton == null)
        {
            bigClothesButton = GetNodeOrNull<TextureButton>("HUD/BigClothesButton");
        }
        if (bigClothesButton != null)
        {
            bigClothesButton.Pressed += OpenClothesPanel;
        }

        if (homeButton == null)
        {
            homeButton = GetNodeOrNull<BaseButton>("HUD/GameOverFrame/HomeButton");
        }
        if (homeButton != null)
        {
            homeButton.Pressed += OnHomeButtonPressed;
        }

        if (replayButton == null)
        {
            replayButton = GetNodeOrNull<BaseButton>("HUD/GameOverFrame/ReplayButton");
        }
        if (replayButton != null)
        {
            replayButton.Pressed += OnReplayButtonPressed;
        }

        if (changeClothesButton == null)
        {
            changeClothesButton = GetNodeOrNull<BaseButton>("HUD/GameOverFrame/ChangeClothesButton") 
                               ?? GetNodeOrNull<BaseButton>("HUD/ChangeClothesButton");
        }
		if (changeClothesButton != null)
		{
			changeClothesButton.Pressed += OpenClothesPanel;
		}

        if (clothesPanel == null)
        {
            clothesPanel = GetNodeOrNull<Control>("HUD/ClothesPanel");
        }

		BaseButton closeClothesButton = GetNodeOrNull<BaseButton>("HUD/ClothesPanel/CloseClothesButton");
		if (closeClothesButton != null)
		{
			closeClothesButton.Pressed += CloseClothesPanel;
		}

		BaseButton outfitButton1 = GetNodeOrNull<BaseButton>("HUD/ClothesPanel/OutfitButton1");
		if (outfitButton1 != null)
		{
			outfitButton1.Pressed += TogglePrincessOutfit;
		}

		BaseButton outfitButton2 = GetNodeOrNull<BaseButton>("HUD/ClothesPanel/OutfitButton2");
		if (outfitButton2 != null)
		{
			outfitButton2.Pressed += ToggleSecondOutfit;
		}

        BaseButton outfitButton3 = GetNodeOrNull<BaseButton>("HUD/ClothesPanel/OutfitButton3");
        if (outfitButton3 != null)
        {
            outfitButton3.Pressed += ToggleThirdOutfit;
        }

		dinoPlayer = GetNodeOrNull<DinoPlayer>("DinoPlayer");
        CenterStartButton();
        GetViewport().SizeChanged += CenterStartButton;
        if (startButton != null)
        {
            startButton.Pressed += StartGame;
        }

        if (gameOverFrame == null)
        {
            gameOverFrame = GetNodeOrNull<BaseButton>("HUD/GameOverFrame");
        }
        if (gameOverFrame != null)
        {
            gameOverFrame.Pressed += OnGameOverFramePressed;
            CenterGameOverFrame();
            GetViewport().SizeChanged += CenterGameOverFrame;
            gameOverFrame.Hide();
        }

        gameOverScoreLabel = GetNodeOrNull<Label>("HUD/GameOverFrame/ScoreValLabel");
        gameOverHiLabel = GetNodeOrNull<Label>("HUD/GameOverFrame/HiValLabel");
        gameOverHintLabel = GetNodeOrNull<Label>("HUD/GameOverFrame/HintLabel");

        if (groundManager == null)
        {
            groundManager = GetNodeOrNull<GroundManager>("GroundManager");
        }
        if (skyManager == null)
        {
            skyManager = GetNodeOrNull<SkyManager>("SkyManager");
        }

        ResetGame();
    }

    public override void _Process(double delta)
    {
        if (isPlaying)
        {
            // Tốc độ game tăng dần đều theo thời gian (giới hạn ở maxSpeed)
            currentSpeedMultiplier = Mathf.Min(maxSpeed, currentSpeedMultiplier + speedAcceleration * (float)delta);

            // Điểm tăng tỷ lệ với tốc độ game
            currentScore += (float)delta * 10f * currentSpeedMultiplier;
            UpdateScoreDisplay();

            // Đồng bộ tốc độ cho hoạt cảnh cuộn mặt đất và nền trời
            if (groundManager != null)
            {
                groundManager.SetSpeedMultiplier(currentSpeedMultiplier);
            }
            if (skyManager != null)
            {
                skyManager.SetSpeedMultiplier(currentSpeedMultiplier);
            }
            if (groundAnimation != null)
            {
                groundAnimation.SpeedScale = currentSpeedMultiplier;
            }

            // Đồng bộ trọng lực, lực nhảy và animation cho khủng long
            if (dinoPlayer != null)
            {
                dinoPlayer.SetSpeedMultiplier(currentSpeedMultiplier);
            }

            // Đồng bộ tốc độ di chuyển và khoảng cách spawn vật cản
            if (spawner != null)
            {
                spawner.SetSpeedMultiplier(currentSpeedMultiplier);
            }
        }
        else
        {
            if (spacePressCount > 0)
            {
                spacePressTimer += (float)delta;
                if (spacePressTimer > 0.65f)
                {
                    spacePressCount = 0;
                    spacePressTimer = 0f;
                    if (gameOverHintLabel != null)
                    {
                        gameOverHintLabel.Text = "NHẤN 2 LẦN ĐỂ CHƠI LẠI";
                    }
                }
            }

            bool startKeyPressed = Input.IsActionJustPressed("ui_accept") || Input.IsActionJustPressed("ui_up");
            if (startKeyPressed)
            {
                RegisterRestartPress();
            }
        }
    }

public void StartGame()
{
    foreach (Node obstacle in GetTree().GetNodesInGroup("obstacle"))
    {
        obstacle.QueueFree();
    }

    currentScore = 0f;
    currentSpeedMultiplier = initialSpeed;
    UpdateScoreDisplay();

    isPlaying = true;

    spacePressCount = 0;
    spacePressTimer = 0f;

    SetScoreVisible(true);

    if (startButton != null)
        startButton.Hide();

    if (bigClothesButton != null)
        bigClothesButton.Hide();

    if (bigSettingButton != null)
        bigSettingButton.Hide();

    if (settingButtonSmall != null)
        settingButtonSmall.Show();

    if (gameOverFrame != null)
        gameOverFrame.Hide();

    // Ẩn nút đổi đồ và đóng bảng đồ khi bắt đầu chơi
    if (changeClothesButton != null)
        changeClothesButton.Hide();

    if (clothesPanel != null)
        clothesPanel.Hide();

    // 🦖 Cho Dino chạy lại với tốc độ khởi điểm
    if (dinoPlayer != null)
    {
        dinoPlayer.StartGame();
        dinoPlayer.SetSpeedMultiplier(currentSpeedMultiplier);
    }

    // 🌱 Cho mặt đất và nền trời chạy lại
    if (groundManager != null)
    {
        groundManager.SetSpeedMultiplier(currentSpeedMultiplier);
        groundManager.StartGround();
    }
    if (skyManager != null)
    {
        skyManager.SetSpeedMultiplier(currentSpeedMultiplier);
        skyManager.StartSky();
    }
    if (groundAnimation != null)
    {
        groundAnimation.SpeedScale = currentSpeedMultiplier;
        groundAnimation.Play();
    }

    // 🚧 Bắt đầu Spawner lại
    if (spawner != null)
    {
        spawner.SetSpeedMultiplier(currentSpeedMultiplier);
        spawner.StartSpawning();
    }
}

   public void ResetGame()
{
    isPlaying = false;
    spacePressCount = 0;
    spacePressTimer = 0f;

    SetScoreVisible(false);

    if (gameOverFrame != null)
        gameOverFrame.Hide();

    if (startButton != null)
        startButton.Show();

    if (bigClothesButton != null)
        bigClothesButton.Show();

    if (bigSettingButton != null)
        bigSettingButton.Show();

    if (settingButtonSmall != null)
        settingButtonSmall.Hide();

    if (changeClothesButton != null)
        changeClothesButton.Hide();

    if (spawner != null)
        spawner.StopSpawning();

    if (groundManager != null)
        groundManager.ResetGround();

    if (skyManager != null)
        skyManager.ResetSky();

    if (groundAnimation != null)
        groundAnimation.Pause();

    if (dinoPlayer != null)
    {
        dinoPlayer.StopGame();
        dinoPlayer.SetStandingPose();
    }

    foreach (Node obstacle in GetTree().GetNodesInGroup("obstacle"))
    {
        obstacle.QueueFree();
    }
}

   public void StopGame()
{
    isPlaying = false;
    spacePressCount = 0;
    spacePressTimer = 0f;

    SetScoreVisible(false);

    if ((int)currentScore > highScore)
    {
        highScore = (int)currentScore;
    }
    UpdateScoreDisplay();

    // Khi thua: hiện khung Game Over hiển thị điểm số và kỷ lục, ẩn startButton
    if (startButton != null)
        startButton.Hide();

    if (bigClothesButton != null)
        bigClothesButton.Hide();

    if (bigSettingButton != null)
        bigSettingButton.Hide();

    if (settingButtonSmall != null)
        settingButtonSmall.Show();

    
    if (gameOverScoreLabel != null) gameOverScoreLabel.Show();
    if (gameOverHiLabel != null) gameOverHiLabel.Show();
    if (gameOverHintLabel != null) gameOverHintLabel.Show();
    if (titleLabel != null) titleLabel.Show();
    if (closeSettingButton != null) closeSettingButton.Hide();
    if (homeButton != null) homeButton.Show();
    if (replayButton != null) replayButton.Show();


    if (gameOverScoreLabel != null)
        gameOverScoreLabel.Text = $"SCORE: {(int)currentScore:D5}";

    if (gameOverHiLabel != null)
        gameOverHiLabel.Text = $"HI: {highScore:D5}";

    if (gameOverHintLabel != null)
        gameOverHintLabel.Text = "NHẤN SPACE 2 LẦN ĐỂ CHƠI LẠI";

    if (gameOverFrame != null)
    {
        CenterGameOverFrame();
        gameOverFrame.Show();
    }

    if (changeClothesButton != null)
        changeClothesButton.Show();

    if (settingButtonSmall != null)
        settingButtonSmall.Hide();

    if (spawner != null)
        spawner.StopSpawning();

    if (groundManager != null)
        groundManager.StopGround();

    if (skyManager != null)
        skyManager.StopSky();

    if (groundAnimation != null)
        groundAnimation.Pause();

	if (dinoPlayer != null)
    {
        dinoPlayer.Die();
    }

	// 🛑 DỪNG TẤT CẢ OBSTACLE ĐANG CÓ TRÊN MÀN HÌNH
    foreach (Node obstacle in GetTree().GetNodesInGroup("obstacle"))
    {
        obstacle.SetProcess(false);
        obstacle.SetPhysicsProcess(false);
    }
}

private void UpdateScoreDisplay()
{
    if (scoreLabel != null)
    {
        scoreLabel.Text = $"SCORE: {(int)currentScore:D5}\nHI: {highScore:D5}";
    }
}

private void SetScoreVisible(bool visible)
{
    Control frame = scoreFrame ?? GetNodeOrNull<Control>("HUD/ScoreFrame");
    if (frame != null)
    {
        frame.Visible = visible;
    }
    else if (scoreLabel != null)
    {
        scoreLabel.Visible = visible;
    }
}
private void OpenClothesPanel()
{
    if (clothesPanel != null)
    {
        clothesPanel.Show();
    }

    // Luôn đưa player về trạng thái đứng (nếu đang ở deadsprite thì chuyển về dáng đứng)
    if (dinoPlayer != null)
    {
        dinoPlayer.SetStandingPose();
    }
}
private void CloseClothesPanel()
{
    if (clothesPanel != null)
    {
        clothesPanel.Hide();
    }
}
private void TogglePrincessOutfit()
{

	
    if (dinoPlayer != null)
        dinoPlayer.TogglePrincessOutfit();
}
private void ToggleSecondOutfit()
{
    if (dinoPlayer != null)
        dinoPlayer.ToggleSecondOutfit();
}
private void ToggleThirdOutfit()
{
    if (dinoPlayer != null)
        dinoPlayer.ToggleThirdOutfit();
}



private void OnSettingButtonPressed()
{
    if (gameOverScoreLabel != null) gameOverScoreLabel.Hide();
    if (gameOverHiLabel != null) gameOverHiLabel.Hide();
    if (gameOverHintLabel != null) gameOverHintLabel.Hide();
    if (titleLabel != null) titleLabel.Hide();
    if (homeButton != null) homeButton.Hide();
    if (replayButton != null) replayButton.Hide();
    if (changeClothesButton != null) changeClothesButton.Hide();

    if (closeSettingButton != null) closeSettingButton.Show();

    if (gameOverFrame != null)
    {
        CenterGameOverFrame();
        gameOverFrame.Show();
    }
    
    if (isPlaying)
    {
        GetTree().Paused = true;
    }
}

private void OnCloseSettingButtonPressed()
{
    if (gameOverFrame != null)
    {
        gameOverFrame.Hide();
    }
    
    if (closeSettingButton != null) closeSettingButton.Hide();
    
    if (isPlaying)
    {
        GetTree().Paused = false;
    }
}
private void OnHomeButtonPressed()
{
    ResetGame();
}

private void OnReplayButtonPressed()
{
    StartGame();
}


private void CenterStartButton()
{
    Vector2 texSize = new Vector2(420f, 136f);
    if (startButton != null)
    {
        if (startButton.TextureNormal != null)
            texSize = startButton.TextureNormal.GetSize();
        else if (startButton.Size != Vector2.Zero)
            texSize = startButton.Size;
            
        float gap = 14f;
        float totalHeight = (texSize.Y * 3) + (gap * 2);
        float startY = -totalHeight / 2f;

        startButton.LayoutMode = 1;
        startButton.AnchorsPreset = (int)Control.LayoutPreset.Center;
        startButton.AnchorLeft = 0.5f;
        startButton.AnchorTop = 0.5f;
        startButton.AnchorRight = 0.5f;
        startButton.AnchorBottom = 0.5f;
        startButton.OffsetLeft = -texSize.X / 2f;
        startButton.OffsetRight = texSize.X / 2f;
        startButton.OffsetTop = startY + texSize.Y + gap;
        startButton.OffsetBottom = startY + texSize.Y + gap + texSize.Y;
        startButton.GrowHorizontal = Control.GrowDirection.Both;
        startButton.GrowVertical = Control.GrowDirection.Both;
        startButton.PivotOffset = texSize / 2f;

        if (bigClothesButton != null)
        {
            bigClothesButton.LayoutMode = 1;
            bigClothesButton.AnchorsPreset = (int)Control.LayoutPreset.Center;
            bigClothesButton.AnchorLeft = 0.5f;
            bigClothesButton.AnchorTop = 0.5f;
            bigClothesButton.AnchorRight = 0.5f;
            bigClothesButton.AnchorBottom = 0.5f;
            bigClothesButton.OffsetLeft = -texSize.X / 2f;
            bigClothesButton.OffsetRight = texSize.X / 2f;
            bigClothesButton.OffsetTop = startY + (texSize.Y + gap) * 2;
            bigClothesButton.OffsetBottom = startY + (texSize.Y + gap) * 2 + texSize.Y;
            bigClothesButton.GrowHorizontal = Control.GrowDirection.Both;
            bigClothesButton.GrowVertical = Control.GrowDirection.Both;
            bigClothesButton.PivotOffset = texSize / 2f;
        }

        if (bigSettingButton != null)
        {
            bigSettingButton.LayoutMode = 1;
            bigSettingButton.AnchorsPreset = (int)Control.LayoutPreset.Center;
            bigSettingButton.AnchorLeft = 0.5f;
            bigSettingButton.AnchorTop = 0.5f;
            bigSettingButton.AnchorRight = 0.5f;
            bigSettingButton.AnchorBottom = 0.5f;
            bigSettingButton.OffsetLeft = -texSize.X / 2f;
            bigSettingButton.OffsetRight = texSize.X / 2f;
            bigSettingButton.OffsetTop = startY;
            bigSettingButton.OffsetBottom = startY + texSize.Y;
            bigSettingButton.GrowHorizontal = Control.GrowDirection.Both;
            bigSettingButton.GrowVertical = Control.GrowDirection.Both;
            bigSettingButton.PivotOffset = texSize / 2f;
        }
    }
}


private void CenterGameOverFrame()
{
    if (gameOverFrame == null) return;

    Vector2 size = new Vector2(526f, 393f);
    gameOverFrame.LayoutMode = 1;
    gameOverFrame.AnchorsPreset = (int)Control.LayoutPreset.Center;
    gameOverFrame.AnchorLeft = 0.5f;
    gameOverFrame.AnchorTop = 0.5f;
    gameOverFrame.AnchorRight = 0.5f;
    gameOverFrame.AnchorBottom = 0.5f;
    gameOverFrame.OffsetLeft = -size.X / 2f;
    gameOverFrame.OffsetRight = size.X / 2f;
    gameOverFrame.OffsetTop = -size.Y / 2f;
    gameOverFrame.OffsetBottom = size.Y / 2f;
    gameOverFrame.GrowHorizontal = Control.GrowDirection.Both;
    gameOverFrame.GrowVertical = Control.GrowDirection.Both;
    gameOverFrame.PivotOffset = size / 2f;
}

private void RegisterRestartPress()
{
    spacePressCount++;
    spacePressTimer = 0f;

    if (spacePressCount == 1)
    {
        if (gameOverHintLabel != null)
        {
            gameOverHintLabel.Text = "NHẤN THÊM 1 LẦN NỮA...";
        }
    }
    else if (spacePressCount >= 2)
    {
        spacePressCount = 0;
        spacePressTimer = 0f;
        StartGame();
    }
}

private void OnGameOverFramePressed()
{
    if (!isPlaying)
    {
        RegisterRestartPress();
    }
}

private void UpdateScoreLayout()
{
    Control frame = scoreFrame ?? GetNodeOrNull<Control>("HUD/ScoreFrame");
    if (frame != null)
    {
        frame.LayoutMode = 1;
        frame.AnchorsPreset = (int)Control.LayoutPreset.TopRight;
        frame.AnchorLeft = 1.0f;
        frame.AnchorTop = 0.0f;
        frame.AnchorRight = 1.0f;
        frame.AnchorBottom = 0.0f;
        frame.OffsetLeft = -260.0f;
        frame.OffsetRight = -20.0f;
        frame.OffsetTop = 18.0f;
        frame.OffsetBottom = 117.0f;
        frame.GrowHorizontal = Control.GrowDirection.Begin;
        frame.GrowVertical = Control.GrowDirection.End;
    }
    else if (scoreLabel != null)
    {
        scoreLabel.LayoutMode = 1;
        scoreLabel.AnchorsPreset = (int)Control.LayoutPreset.TopRight;
        scoreLabel.AnchorLeft = 1.0f;
        scoreLabel.AnchorTop = 0.0f;
        scoreLabel.AnchorRight = 1.0f;
        scoreLabel.AnchorBottom = 0.0f;
        scoreLabel.OffsetLeft = -260.0f;
        scoreLabel.OffsetRight = -20.0f;
        scoreLabel.OffsetTop = 18.0f;
        scoreLabel.OffsetBottom = 95.0f;
        scoreLabel.GrowHorizontal = Control.GrowDirection.Begin;
        scoreLabel.GrowVertical = Control.GrowDirection.End;
        scoreLabel.HorizontalAlignment = HorizontalAlignment.Left;
    }
}
}