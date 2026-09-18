using Godot;

public partial class Node2d : Godot.Node2D
{
    [Export] private TextureButton startButton;
    [Export] private TextureButton bigClothesButton;
    [Export] private TextureButton bigSettingButton;
    [Export] private TextureButton settingButtonSmall;
    private TextureButton gameOverSettingBtn;
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
    private Car backgroundCar;
    private int spacePressCount = 0;
    private float spacePressTimer = 0f;

        private TextureRect settingFrameBase;
private Control settingContent;
    private Control howToPlayPanel;
    private Control volumePanel;
    private HSlider volumeSlider;

    public override void _Ready()
    {
        PackedScene carScene = GD.Load<PackedScene>("res://car.tscn");
        if (carScene != null) {
            backgroundCar = carScene.Instantiate<Car>();
            backgroundCar.Position = new Vector2(100, 350);
            backgroundCar.Scale = new Vector2(0.45f, 0.45f);
            backgroundCar.ZIndex = 1;
            AddChild(backgroundCar);
            // Re-order so it is behind HUD but in front of sky
            MoveChild(backgroundCar, 3);
        }

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

		if (clothesPanel != null)
        {
            clothesPanel.SelfModulate = new Color(1, 1, 1, 0); 
            clothesPanel.SetAnchorsPreset(Control.LayoutPreset.Center);
            clothesPanel.AnchorLeft = 0.5f;
            clothesPanel.AnchorTop = 0.5f;
            clothesPanel.AnchorRight = 0.5f;
            clothesPanel.AnchorBottom = 0.5f;
            clothesPanel.OffsetLeft = -560f / 2f;
            clothesPanel.OffsetTop = -358f / 2f;
            clothesPanel.OffsetRight = 560f / 2f;
            clothesPanel.OffsetBottom = 358f / 2f;
            clothesPanel.PivotOffset = new Vector2(280f, 179f);
            clothesPanel.Scale = new Vector2(1.2f, 1.2f);
            
            TextureRect bg = new TextureRect();
            bg.Texture = GD.Load<Texture2D>("res://assets/setting_board.png");
            bg.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            clothesPanel.AddChild(bg);
            clothesPanel.MoveChild(bg, 0);

            BaseButton oldCloseBtn = GetNodeOrNull<BaseButton>("HUD/ClothesPanel/CloseClothesButton");
            if (oldCloseBtn != null) oldCloseBtn.QueueFree();

            TextureButton newCloseBtn = new TextureButton();
            newCloseBtn.TextureNormal = GD.Load<Texture2D>("res://assets/btn_close_normal.png");
            newCloseBtn.TextureHover = GD.Load<Texture2D>("res://assets/btn_close_hover.png");
            newCloseBtn.TexturePressed = GD.Load<Texture2D>("res://assets/btn_close_pressed.png");
            newCloseBtn.Position = new Vector2(-15, -15);
            newCloseBtn.Pressed += CloseClothesPanel;
            clothesPanel.AddChild(newCloseBtn);

            BaseButton btn1 = clothesPanel.GetNodeOrNull<BaseButton>("OutfitButton1");
            BaseButton btn2 = clothesPanel.GetNodeOrNull<BaseButton>("OutfitButton2");
            BaseButton btn3 = clothesPanel.GetNodeOrNull<BaseButton>("OutfitButton3");

            if (btn1 != null) {
                btn1.SetAnchorsPreset(Control.LayoutPreset.TopLeft);
                btn1.Position = new Vector2(40, 100);
            }
            if (btn2 != null) {
                btn2.SetAnchorsPreset(Control.LayoutPreset.TopLeft);
                btn2.Position = new Vector2(220, 100);
            }
            if (btn3 != null) {
                btn3.SetAnchorsPreset(Control.LayoutPreset.TopLeft);
                btn3.Position = new Vector2(400, 100);
            }

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
            gameOverSettingBtn = new TextureButton();
            gameOverSettingBtn.TextureNormal = GD.Load<Texture2D>("res://assets/btn_setting_normal.png");
            gameOverSettingBtn.TextureHover = GD.Load<Texture2D>("res://assets/btn_setting_hover.png");
            gameOverSettingBtn.TexturePressed = GD.Load<Texture2D>("res://assets/btn_setting_pressed.png");
            gameOverSettingBtn.IgnoreTextureSize = true;
            gameOverSettingBtn.StretchMode = TextureButton.StretchModeEnum.KeepAspectCentered;
            gameOverSettingBtn.SetAnchorsPreset(Control.LayoutPreset.CenterTop);
            gameOverSettingBtn.OffsetTop = 230;
            gameOverSettingBtn.OffsetBottom = 298;
            gameOverSettingBtn.OffsetLeft = 98;
            gameOverSettingBtn.OffsetRight = 166;
            gameOverSettingBtn.Pressed += OnSettingButtonPressed;
            gameOverFrame.AddChild(gameOverSettingBtn);
            
            if (homeButton != null) {
                homeButton.OffsetLeft = -166;
                homeButton.OffsetRight = -98;
            }
            if (replayButton != null) {
                replayButton.OffsetLeft = -78;
                replayButton.OffsetRight = -10;
            }
            if (changeClothesButton != null) {
                changeClothesButton.OffsetLeft = 10;
                changeClothesButton.OffsetRight = 78;
            }
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

        CreateSettingsUI();
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
            if (backgroundCar != null)
            {
                backgroundCar.SetSpeedMultiplier(currentSpeedMultiplier);
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
    if (backgroundCar != null) {
        backgroundCar.StartCar();
        backgroundCar.SetSpeedMultiplier(currentSpeedMultiplier);
    }

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
    if (backgroundCar != null) {
        backgroundCar.StopCar();
    }

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
        settingButtonSmall.Hide();

    
    if (gameOverScoreLabel != null) gameOverScoreLabel.Show();
    if (gameOverHiLabel != null) gameOverHiLabel.Show();
    if (gameOverHintLabel != null) gameOverHintLabel.Show();
    if (titleLabel != null) titleLabel.Show();
    if (closeSettingButton != null) closeSettingButton.Hide();
    if (settingFrameBase != null) settingFrameBase.Hide();
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
    if (startButton != null) startButton.Hide();
    if (bigClothesButton != null) bigClothesButton.Hide();
    if (bigSettingButton != null) bigSettingButton.Hide();

    if (closeSettingButton != null) closeSettingButton.Show();
    if (settingButtonSmall != null)
        settingButtonSmall.Hide();
        
    if (gameOverFrame != null)
        gameOverFrame.Hide();
        
    if (settingFrameBase != null)
    {
        settingFrameBase.Show();
        SwitchSettingTab(0);
    }

    
    if (isPlaying)
    {
        GetTree().Paused = true;
    }
}

private void OnCloseSettingButtonPressed()
{
    if (settingFrameBase != null) settingFrameBase.Hide();
    
    // Only show HUD setting button if the game is actively playing
    if (isPlaying && settingButtonSmall != null)
    {
        settingButtonSmall.Show();
    }
    
    if (isPlaying)
    {
        GetTree().Paused = false;
    }
    else if (spacePressCount == 0)
    {
        // Restore Main Menu UI
        ResetGame();
    }
    else 
    {
        // Restore Game Over UI if we were in Game Over state
        StopGame();
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
    private void CreateSettingsUI()
    {
        settingFrameBase = new TextureRect();
        settingFrameBase.Texture = GD.Load<Texture2D>("res://assets/setting_board.png");
        settingFrameBase.SetAnchorsPreset(Control.LayoutPreset.Center);
        settingFrameBase.AnchorLeft = 0.5f;
        settingFrameBase.AnchorTop = 0.5f;
        settingFrameBase.AnchorRight = 0.5f;
        settingFrameBase.AnchorBottom = 0.5f;
        settingFrameBase.OffsetLeft = -560f / 2f;
        settingFrameBase.OffsetTop = -358f / 2f;
        settingFrameBase.OffsetRight = 560f / 2f;
        settingFrameBase.OffsetBottom = 358f / 2f;
        settingFrameBase.PivotOffset = new Vector2(560f / 2f, 358f / 2f);
        settingFrameBase.Scale = new Vector2(1.2f, 1.2f);
        settingFrameBase.ProcessMode = Node.ProcessModeEnum.Always;
        
        GetNode("HUD").AddChild(settingFrameBase);
        settingFrameBase.Hide();

        settingContent = new Control();
        settingContent.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        settingContent.MouseFilter = Control.MouseFilterEnum.Ignore;
        settingFrameBase.AddChild(settingContent);
        
        TextureButton btnClose = new TextureButton();
        btnClose.TextureNormal = GD.Load<Texture2D>("res://assets/btn_close_normal.png");
        btnClose.TextureHover = GD.Load<Texture2D>("res://assets/btn_close_hover.png");
        btnClose.TexturePressed = GD.Load<Texture2D>("res://assets/btn_close_pressed.png");
        btnClose.Position = new Vector2(-15, -15);
        btnClose.Pressed += OnCloseSettingButtonPressed;
        settingFrameBase.AddChild(btnClose);

        TextureButton btnHowToPlay = new TextureButton();
        btnHowToPlay.TextureNormal = GD.Load<Texture2D>("res://assets/btn_tab1_norm.png");
        btnHowToPlay.TextureHover = GD.Load<Texture2D>("res://assets/btn_tab1_hover.png");
        btnHowToPlay.TexturePressed = GD.Load<Texture2D>("res://assets/btn_tab1_press.png");
        btnHowToPlay.Position = new Vector2(100, 35);
        btnHowToPlay.Pressed += () => SwitchSettingTab(0);
        settingContent.AddChild(btnHowToPlay);

        TextureButton btnVolume = new TextureButton();
        btnVolume.TextureNormal = GD.Load<Texture2D>("res://assets/btn_tab2_norm.png");
        btnVolume.TextureHover = GD.Load<Texture2D>("res://assets/btn_tab2_hover.png");
        btnVolume.TexturePressed = GD.Load<Texture2D>("res://assets/btn_tab2_press.png");
        btnVolume.Position = new Vector2(290, 35);
        btnVolume.Pressed += () => SwitchSettingTab(1);
        settingContent.AddChild(btnVolume);

        howToPlayPanel = new Control();
        howToPlayPanel.Position = new Vector2(60, 100);
        howToPlayPanel.Size = new Vector2(440, 240);
        howToPlayPanel.MouseFilter = Control.MouseFilterEnum.Ignore;
        settingContent.AddChild(howToPlayPanel);

        Label lblInstructions = new Label();
        lblInstructions.Text = @"- Nhấn SPACE hoặc Lên để nhảy.
- Nhấn giữ Xuống để cúi người.
- Tránh chướng ngại vật.
- Máy bay bay cao: đi thẳng.
- Máy bay bay thấp: CÚI để qua!";
        lblInstructions.AddThemeColorOverride("font_color", new Color(0.31f, 0.15f, 0.04f));
        lblInstructions.AddThemeFontSizeOverride("font_size", 22);
        lblInstructions.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        lblInstructions.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        howToPlayPanel.AddChild(lblInstructions);

        volumePanel = new Control();
        volumePanel.Position = new Vector2(60, 100);
        volumePanel.Size = new Vector2(440, 240);
        volumePanel.MouseFilter = Control.MouseFilterEnum.Ignore;
        settingContent.AddChild(volumePanel);
        volumePanel.Hide();

        Label lblVolume = new Label();
        lblVolume.Text = "ÂM LƯỢNG (VOLUME)";
        lblVolume.AddThemeColorOverride("font_color", new Color(0.31f, 0.15f, 0.04f));
        lblVolume.AddThemeFontSizeOverride("font_size", 24);
        lblVolume.Position = new Vector2(0, 20);
        volumePanel.AddChild(lblVolume);

        volumeSlider = new HSlider();
        volumeSlider.Position = new Vector2(0, 80);
        volumeSlider.Size = new Vector2(300, 30);
        volumeSlider.MinValue = 0;
        volumeSlider.MaxValue = 100;
        volumeSlider.Value = 100;
        volumeSlider.ValueChanged += OnVolumeChanged;
        volumePanel.AddChild(volumeSlider);
    }


    private void SwitchSettingTab(int tabIndex)
    {
        if (howToPlayPanel == null || volumePanel == null) return;
        if (tabIndex == 0)
        {
            howToPlayPanel.Show();
            volumePanel.Hide();
        }
        else
        {
            howToPlayPanel.Hide();
            volumePanel.Show();
        }
    }

    private void OnVolumeChanged(double value)
    {
        int masterBusIndex = AudioServer.GetBusIndex("Master");
        if (value <= 0)
        {
            AudioServer.SetBusMute(masterBusIndex, true);
        }
        else
        {
            AudioServer.SetBusMute(masterBusIndex, false);
            float db = (float)Mathf.LinearToDb(value / 100.0);
            AudioServer.SetBusVolumeDb(masterBusIndex, db);
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
    gameOverFrame.Scale = new Vector2(1.3f, 1.3f);
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