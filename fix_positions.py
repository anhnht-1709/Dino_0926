import re

with open("node_2d.tscn", "r") as f:
    content = f.read()

# 1. Reset AnimatedSprite2D position
old_anim_pos = r'\[node name="AnimatedSprite2D" parent="DinoPlayer"[^\]]*\]\nposition = Vector2\(437, 310\)'
new_anim_pos = '[node name="AnimatedSprite2D" parent="DinoPlayer" unique_id=1754628052]\nposition = Vector2(232.791, 316.279)'
content = re.sub(old_anim_pos, new_anim_pos, content)

# 2. Add position to DinoPlayer
old_dino = r'\[node name="DinoPlayer" type="CharacterBody2D" parent="\." unique_id=1852117108\]\nz_index = 5\nscript = ExtResource\("2_0e48y"\)'
new_dino = '[node name="DinoPlayer" type="CharacterBody2D" parent="." unique_id=1852117108]\nz_index = 5\nposition = Vector2(204, 0)\nscript = ExtResource("2_0e48y")'
content = re.sub(old_dino, new_dino, content)

with open("node_2d.tscn", "w") as f:
    f.write(content)

print("Fixed node_2d.tscn")
