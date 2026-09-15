import re

with open("car.tscn", "r") as f:
    content = f.read()

# Modify Wheel1 position
content = re.sub(r'\[node name="Wheel1".*?position = Vector2\([^\)]+\)', '[node name="Wheel1" type="Sprite2D" parent="."]\nposition = Vector2(-280, 180)', content, flags=re.DOTALL)

# Modify Wheel2 position
content = re.sub(r'\[node name="Wheel2".*?position = Vector2\([^\)]+\)', '[node name="Wheel2" type="Sprite2D" parent="."]\nposition = Vector2(250, 180)', content, flags=re.DOTALL)

with open("car.tscn", "w") as f:
    f.write(content)
print("Updated car.tscn")
