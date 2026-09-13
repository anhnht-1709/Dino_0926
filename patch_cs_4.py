import re

with open("Node2d.cs", "r") as f:
    code = f.read()

# 1. Add field
fields = """    private TextureRect settingFrameBase;
"""
code = re.sub(
    r'(private Control settingContent;)',
    fields + r'\1',
    code
)

# 2. Replace CreateSettingsUI completely
new_create = """
    private void CreateSettingsUI()
    {
        settingFrameBase = new TextureRect();
        settingFrameBase.Texture = GD.Load<Texture2D>("res://setting_board.png");
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
        
        GetNode("HUD").AddChild(settingFrameBase);
        settingFrameBase.Hide();

        settingContent = new Control();
        settingContent.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        settingContent.MouseFilter = Control.MouseFilterEnum.Ignore;
        settingFrameBase.AddChild(settingContent);
        
        TextureButton btnClose = new TextureButton();
        btnClose.TextureNormal = GD.Load<Texture2D>("res://btn_close_normal.png");
        btnClose.TextureHover = GD.Load<Texture2D>("res://btn_close_hover.png");
        btnClose.TexturePressed = GD.Load<Texture2D>("res://btn_close_pressed.png");
        btnClose.Position = new Vector2(-15, -15);
        btnClose.Pressed += OnCloseSettingButtonPressed;
        settingFrameBase.AddChild(btnClose);

        TextureButton btnHowToPlay = new TextureButton();
        btnHowToPlay.TextureNormal = GD.Load<Texture2D>("res://btn_tab1_norm.png");
        btnHowToPlay.TextureHover = GD.Load<Texture2D>("res://btn_tab1_hover.png");
        btnHowToPlay.TexturePressed = GD.Load<Texture2D>("res://btn_tab1_press.png");
        btnHowToPlay.Position = new Vector2(100, 35);
        btnHowToPlay.Pressed += () => SwitchSettingTab(0);
        settingContent.AddChild(btnHowToPlay);

        TextureButton btnVolume = new TextureButton();
        btnVolume.TextureNormal = GD.Load<Texture2D>("res://btn_tab2_norm.png");
        btnVolume.TextureHover = GD.Load<Texture2D>("res://btn_tab2_hover.png");
        btnVolume.TexturePressed = GD.Load<Texture2D>("res://btn_tab2_press.png");
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
"""
code = re.sub(
    r'\s+private void CreateSettingsUI\(\)\s*\{.*?(?=\s+private void SwitchSettingTab)',
    new_create,
    code,
    flags=re.DOTALL
)

# 3. Update OnSettingButtonPressed
# Remove gameOverFrame.Show() and add settingFrameBase.Show()
new_setting_press = """
    if (settingButtonSmall != null)
        settingButtonSmall.Hide();
        
    if (gameOverFrame != null)
        gameOverFrame.Hide();
        
    if (settingFrameBase != null)
    {
        settingFrameBase.Show();
        SwitchSettingTab(0);
    }
"""
code = re.sub(
    r'\s+if \(gameOverFrame != null\)\s*\{\s*CenterGameOverFrame\(\);\s*gameOverFrame\.Show\(\);\s*\}\s*if \(settingContent != null\) \{ settingContent\.Show\(\); SwitchSettingTab\(0\); \}',
    new_setting_press,
    code
)

# 4. Update OnCloseSettingButtonPressed
code = re.sub(
    r'if \(settingContent != null\) settingContent\.Hide\(\);',
    r'if (settingFrameBase != null) settingFrameBase.Hide();',
    code
)

# 5. Update StopGame
code = re.sub(
    r'if \(settingContent != null\) settingContent\.Hide\(\);',
    r'if (settingFrameBase != null) settingFrameBase.Hide();',
    code
)


with open("Node2d.cs", "w") as f:
    f.write(code)
print("Node2d.cs fully updated for new Settings board")
