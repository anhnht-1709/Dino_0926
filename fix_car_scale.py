import re

with open("car.tscn", "r") as f:
    content = f.read()

# Replace Wheel1
wheel1_old = r'\[node name="Wheel1" type="Sprite2D" parent="\."\]\nposition = Vector2\(-280, 150\)\ntexture = ExtResource\("3_wheel"\)'
wheel1_new = '[node name="Wheel1" type="Sprite2D" parent="."]\nposition = Vector2(-280, 250)\nscale = Vector2(0.5, 0.5)\ntexture = ExtResource("3_wheel")'
content = re.sub(wheel1_old, wheel1_new, content)

# Replace Wheel2
wheel2_old = r'\[node name="Wheel2" type="Sprite2D" parent="\."\]\nposition = Vector2\(180, 150\)\ntexture = ExtResource\("3_wheel"\)'
wheel2_new = '[node name="Wheel2" type="Sprite2D" parent="."]\nposition = Vector2(200, 250)\nscale = Vector2(0.5, 0.5)\ntexture = ExtResource("3_wheel")'
content = re.sub(wheel2_old, wheel2_new, content)

with open("car.tscn", "w") as f:
    f.write(content)
