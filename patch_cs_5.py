import re

with open("Node2d.cs", "r") as f:
    code = f.read()

new_clothes = """        if (clothesPanel != null)
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
        }

"""

# Replace the block from `BaseButton closeClothesButton...` up to `closeClothesButton.Pressed += CloseClothesPanel;`
# But actually we can just insert this block after line 119 and remove the old CloseClothesButton logic.

search_pattern = r'(\s*)BaseButton closeClothesButton = GetNodeOrNull<BaseButton>\("HUD/ClothesPanel/CloseClothesButton"\);\s*if \(closeClothesButton != null\)\s*\{\s*closeClothesButton\.Pressed \+= CloseClothesPanel;\s*\}'
code = re.sub(search_pattern, r'\1' + new_clothes.strip() + '\n', code)

with open("Node2d.cs", "w") as f:
    f.write(code)

print("Patch applied")
