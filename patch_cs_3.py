import re

with open("Node2d.cs", "r") as f:
    code = f.read()

# Add fields
fields = """
    private Control settingContent;
    private Control howToPlayPanel;
    private Control volumePanel;
    private HSlider volumeSlider;
"""
code = re.sub(
    r'(private AudioStreamPlayer deadSoundPlayer;)',
    r'\1' + '\n' + fields,
    code
)

# Add CreateSettingsUI() call in _Ready
code = re.sub(
    r'(if \(duckSprite != null\))',
    r'CreateSettingsUI();\n\n        \1',
    code
)

# Add CreateSettingsUI() method and SwitchSettingTab, OnVolumeChanged
methods = """
    private void CreateSettingsUI()
    {
        if (gameOverFrame == null) return;

        settingContent = new Control();
        settingContent.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        gameOverFrame.AddChild(settingContent);
        settingContent.Hide();

        TextureButton btnHowToPlay = new TextureButton();
        btnHowToPlay.TextureNormal = GD.Load<Texture2D>("res://btn_tab1_norm.png");
        btnHowToPlay.TextureHover = GD.Load<Texture2D>("res://btn_tab1_hover.png");
        btnHowToPlay.TexturePressed = GD.Load<Texture2D>("res://btn_tab1_press.png");
        btnHowToPlay.Position = new Vector2(50, 50);
        btnHowToPlay.Pressed += () => SwitchSettingTab(0);
        settingContent.AddChild(btnHowToPlay);

        TextureButton btnVolume = new TextureButton();
        btnVolume.TextureNormal = GD.Load<Texture2D>("res://btn_tab2_norm.png");
        btnVolume.TextureHover = GD.Load<Texture2D>("res://btn_tab2_hover.png");
        btnVolume.TexturePressed = GD.Load<Texture2D>("res://btn_tab2_press.png");
        btnVolume.Position = new Vector2(276, 50);
        btnVolume.Pressed += () => SwitchSettingTab(1);
        settingContent.AddChild(btnVolume);

        howToPlayPanel = new Control();
        howToPlayPanel.Position = new Vector2(50, 120);
        howToPlayPanel.Size = new Vector2(426, 250);
        settingContent.AddChild(howToPlayPanel);

        Label lblInstructions = new Label();
        lblInstructions.Text = "- Nhấn SPACE hoặc Lên để nhảy.\n- Nhấn giữ Xuống để cúi người.\n- Tránh chướng ngại vật.\n- Máy bay bay cao: đi thẳng.\n- Máy bay bay thấp: CÚI để qua!";
        lblInstructions.AddThemeColorOverride("font_color", new Color(0.31f, 0.15f, 0.04f));
        lblInstructions.AddThemeFontSizeOverride("font_size", 22);
        lblInstructions.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        lblInstructions.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        howToPlayPanel.AddChild(lblInstructions);

        volumePanel = new Control();
        volumePanel.Position = new Vector2(50, 120);
        volumePanel.Size = new Vector2(426, 250);
        settingContent.AddChild(volumePanel);
        volumePanel.Hide();

        Label lblVolume = new Label();
        lblVolume.Text = "AM LUONG (VOLUME)";
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
"""
code = re.sub(
    r'(private void CenterGameOverFrame\(\))',
    methods + r'\n    \1',
    code
)

# Update OnSettingButtonPressed to show settingContent
code = re.sub(
    r'(if \(gameOverFrame != null\)\s*\{\s*CenterGameOverFrame\(\);\s*gameOverFrame\.Show\(\);\s*\})',
    r'\1\n\n    if (settingContent != null) { settingContent.Show(); SwitchSettingTab(0); }',
    code
)

# Update OnCloseSettingButtonPressed to hide settingContent
code = re.sub(
    r'(if \(gameOverFrame != null\)\s*\{\s*gameOverFrame\.Hide\(\);\s*\})',
    r'\1\n    if (settingContent != null) settingContent.Hide();',
    code
)

# Update StopGame to hide settingContent
code = re.sub(
    r'(if \(closeSettingButton != null\) closeSettingButton\.Hide\(\);)',
    r'\1\n    if (settingContent != null) settingContent.Hide();',
    code
)

with open("Node2d.cs", "w") as f:
    f.write(code)

print("Node2d.cs patched with tabs")
