import re

with open("car.tscn", "r") as f:
    content = f.read()

body_old = r'\[node name="Body" type="Sprite2D" parent="\." unique_id=[0-9]+\]\nposition = Vector2\([^\)]+\)\nscale = Vector2\([^\)]+\)\ntexture = ExtResource\("2_body"\)'
body_new = '[node name="Body" type="Sprite2D" parent="."]\ntexture = ExtResource("2_body")'
content = re.sub(body_old, body_new, content)

wheel1_old = r'\[node name="Wheel1" type="Sprite2D" parent="\."\]\nposition = Vector2\(-280, 180\)'
wheel1_new = '[node name="Wheel1" type="Sprite2D" parent="."]\nposition = Vector2(-280, 250)'
content = re.sub(wheel1_old, wheel1_new, content)

wheel2_old = r'\[node name="Wheel2" type="Sprite2D" parent="\."\]\nposition = Vector2\(250, 180\)'
wheel2_new = '[node name="Wheel2" type="Sprite2D" parent="."]\nposition = Vector2(250, 250)'
content = re.sub(wheel2_old, wheel2_new, content)

with open("car.tscn", "w") as f:
    f.write(content)
