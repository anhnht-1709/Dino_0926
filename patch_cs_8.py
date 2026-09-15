import re

with open("Node2d.cs", "r") as f:
    code = f.read()

# 1. Add private TextureButton gameOverSettingBtn; to declarations
if "private TextureButton gameOverSettingBtn;" not in code:
    code = code.replace("private TextureButton settingButtonSmall;", "private TextureButton settingButtonSmall;\n    private TextureButton gameOverSettingBtn;")

# 2. Replace the block from `if (settingButtonSmall != null && gameOverFrame != null)`
search_block = r"""        if \(settingButtonSmall != null && gameOverFrame != null\)
        \{
            var oldParent = settingButtonSmall\.GetParent\(\);
            if \(oldParent != null\) oldParent\.RemoveChild\(settingButtonSmall\);
            gameOverFrame\.AddChild\(settingButtonSmall\);
            
            settingButtonSmall\.SetAnchorsPreset\(Control\.LayoutPreset\.CenterTop\);
            settingButtonSmall\.OffsetTop = 230;
            settingButtonSmall\.OffsetBottom = 298;
            settingButtonSmall\.OffsetLeft = 98;
            settingButtonSmall\.OffsetRight = 166;"""

replace_block = """        if (gameOverFrame != null)
        {
            gameOverSettingBtn = new TextureButton();
            gameOverSettingBtn.TextureNormal = GD.Load<Texture2D>("res://assets/btn_setting_normal.png");
            gameOverSettingBtn.TextureHover = GD.Load<Texture2D>("res://assets/btn_setting_hover.png");
            gameOverSettingBtn.TexturePressed = GD.Load<Texture2D>("res://assets/btn_setting_pressed.png");
            gameOverSettingBtn.SetAnchorsPreset(Control.LayoutPreset.CenterTop);
            gameOverSettingBtn.OffsetTop = 230;
            gameOverSettingBtn.OffsetBottom = 298;
            gameOverSettingBtn.OffsetLeft = 98;
            gameOverSettingBtn.OffsetRight = 166;
            gameOverSettingBtn.Pressed += OnSettingButtonPressed;
            gameOverFrame.AddChild(gameOverSettingBtn);"""

code = re.sub(search_block, replace_block, code)

# 3. In StopGame(), change settingButtonSmall.Show() to Hide()
stopgame_search = r"""    if \(settingButtonSmall != null\)
        settingButtonSmall\.Show\(\);"""
stopgame_replace = """    if (settingButtonSmall != null)
        settingButtonSmall.Hide();"""

# There are multiple settingButtonSmall.Show() in the file, we specifically want the one in StopGame.
# So let's find the one that comes after bigSettingButton.Hide()
stopgame_full_search = r"""    if \(bigSettingButton != null\)
        bigSettingButton\.Hide\(\);

    if \(settingButtonSmall != null\)
        settingButtonSmall\.Show\(\);"""

stopgame_full_replace = """    if (bigSettingButton != null)
        bigSettingButton.Hide();

    if (settingButtonSmall != null)
        settingButtonSmall.Hide();"""

code = re.sub(stopgame_full_search, stopgame_full_replace, code)

with open("Node2d.cs", "w") as f:
    f.write(code)

print("Patch applied")
