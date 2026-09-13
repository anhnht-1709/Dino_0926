import re

with open("Node2d.cs", "r") as f:
    code = f.read()

# We want to insert the position updates right after newCloseBtn.Pressed += CloseClothesPanel; clothesPanel.AddChild(newCloseBtn);
insert_code = """
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
"""

code = code.replace("clothesPanel.AddChild(newCloseBtn);", "clothesPanel.AddChild(newCloseBtn);\n" + insert_code)

with open("Node2d.cs", "w") as f:
    f.write(code)

print("Patch applied")
