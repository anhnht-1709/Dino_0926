with open("car.tscn", "w") as f:
    f.write("""[gd_scene format=3 uid="uid://26qp205l5p7a"]

[ext_resource type="Script" uid="uid://bef0ceceva7fx" path="res://Car.cs" id="1_car"]
[ext_resource type="Texture2D" uid="uid://tqugd6fcfadk" path="res://assets/car_body.png" id="2_body"]
[ext_resource type="Texture2D" uid="uid://bi50loqffi5uu" path="res://assets/wheel.png" id="3_wheel"]

[node name="Car" type="Node2D"]
script = ExtResource("1_car")

[node name="Body" type="Sprite2D" parent="."]
texture = ExtResource("2_body")

[node name="Wheel1" type="Sprite2D" parent="."]
position = Vector2(-280, 290)
scale = Vector2(0.5, 0.5)
texture = ExtResource("3_wheel")

[node name="Wheel2" type="Sprite2D" parent="."]
position = Vector2(200, 290)
scale = Vector2(0.5, 0.5)
texture = ExtResource("3_wheel")
""")
