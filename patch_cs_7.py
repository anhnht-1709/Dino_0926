import re

with open("Node2d.cs", "r") as f:
    code = f.read()

insert_code = """
        if (settingButtonSmall != null && gameOverFrame != null)
        {
            var oldParent = settingButtonSmall.GetParent();
            if (oldParent != null) oldParent.RemoveChild(settingButtonSmall);
            gameOverFrame.AddChild(settingButtonSmall);
            
            settingButtonSmall.SetAnchorsPreset(Control.LayoutPreset.CenterTop);
            settingButtonSmall.OffsetTop = 230;
            settingButtonSmall.OffsetBottom = 298;
            settingButtonSmall.OffsetLeft = 98;
            settingButtonSmall.OffsetRight = 166;
            
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
"""

# We'll inject this in _Ready, right after gameOverFrame = GetNodeOrNull<BaseButton>("HUD/GameOverFrame");
search = r'gameOverFrame = GetNodeOrNull<BaseButton>\("HUD/GameOverFrame"\);\s*\}'
code = re.sub(search, search.replace(r'\)', ')') + insert_code, code, count=1)

with open("Node2d.cs", "w") as f:
    f.write(code)

print("Patch 7 applied")
