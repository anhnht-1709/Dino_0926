using Godot;

public partial class DinoPlayer : CharacterBody2D
{
    [Export] public float Gravity = 1200.0f;
    [Export] public float JumpForce = -775.0f; // Độ cao nhảy đạt chính xác ~250px (h = 775^2 / (2 * 1200) ≈ 250.2px)
    [Export] public float TargetJumpHeight = 250.0f; // Độ cao đỉnh nhảy mục tiêu ~250px
    [Export] public float DropSpeed = 1300.0f; // Tốc độ rơi nhanh khi ấn mũi tên xuống

    private float baseGravity;
    private float baseJumpForce;
    private float baseDropSpeed;
    private float currentSpeedMultiplier = 1.0f;

    // NODE
    private AnimatedSprite2D animatedSprite;
    private Sprite2D deadSprite;
    private Sprite2D duckSprite;

    [Export] private CollisionShape2D collisionShape;
    [Export] private CollisionShape2D collisionShape2;
    private RectangleShape2D defaultShape;
    private Vector2 defaultCollisionPos;
    private Vector2 duckCollisionPos;
    private Vector2 defaultCollisionSize;
    private Vector2 duckCollisionSize;
    private Vector2 defaultDuckPos;
    private bool isDucking = false;

    private Texture2D duckTex1;
    private Texture2D duckTex2;
    private float duckAnimTimer = 0f;
    private int duckFrame = 0;

    private bool isPlaying = false;
    private bool isDead = false;
    private Sprite2D clothesSprite;
    private Sprite2D hatSprite;

    private Vector2 defaultPlayerPos;
    private Vector2 clothesDefaultPos;
    private Vector2 hatDefaultPos;
    private Tween outfitTween;

    private int currentOutfit = 0;

    public override void _Ready()
    {
        baseGravity = Gravity;
        baseJumpForce = JumpForce;
        baseDropSpeed = DropSpeed;

        clothesSprite = GetNode<Sprite2D>("ClothesSprite");
        hatSprite = GetNode<Sprite2D>("HatSprite");
        deadSprite = GetNodeOrNull<Sprite2D>("DeadSprite");
        duckSprite = GetNodeOrNull<Sprite2D>("DuckSprite");

        duckTex1 = GD.Load<Texture2D>("res://dino_duck.png");
        duckTex2 = GD.Load<Texture2D>("res://dino_duck2.png");

        if (duckSprite != null)
        {
            defaultDuckPos = duckSprite.Position;
            duckSprite.Texture = duckTex1;
            duckSprite.Hide();
        }

        collisionShape = GetNodeOrNull<CollisionShape2D>("CollisionShape2D");
        collisionShape2 = GetNodeOrNull<CollisionShape2D>("CollisionShape2D2");

        if (collisionShape != null)
        {
            collisionShape.Disabled = false;
            defaultCollisionPos = collisionShape.Position;
            if (collisionShape.Shape is RectangleShape2D rect)
            {
                defaultShape = (RectangleShape2D)rect.Duplicate();
                collisionShape.Shape = defaultShape;
                defaultCollisionSize = defaultShape.Size;
                duckCollisionSize = new Vector2(130.0f, 75.0f);
                duckCollisionPos = new Vector2(defaultCollisionPos.X, 357.0f);
            }
        }
        if (collisionShape2 != null)
        {
            collisionShape2.Disabled = true;
        }

        clothesDefaultPos = clothesSprite.Position;
        hatDefaultPos = hatSprite.Position;
        defaultPlayerPos = Position;

        clothesSprite.Hide();
        hatSprite.Hide();
        if (deadSprite != null)
        {
            deadSprite.Hide();
        }

        animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        // Khi mới chạy game, KHỦNG LONG KHÔNG ĐƯỢC CHẠY
        isPlaying = false;
        isDead = false;
        isDucking = false;

        Velocity = Vector2.Zero;

        // Đứng yên
        if (animatedSprite != null)
        {
            animatedSprite.Stop();
        }

        SetPhysicsProcess(true);
    }

    public void SetSpeedMultiplier(float multiplier)
    {
        currentSpeedMultiplier = multiplier;

        // Khi game bắt đầu nhanh hơn (multiplier > 1.0), trọng lực kéo player xuống phải cao hơn
        // để khủng long rơi xuống nhanh hơn và dứt khoát hơn.
        float speedFactor = Mathf.Max(1.0f, multiplier);
        Gravity = baseGravity * (1.0f + 0.75f * (speedFactor - 1.0f));

        // Tự động điều chỉnh lực nhảy theo trọng lực mới để độ cao nhảy luôn duy trì chuẩn ~250px
        // (h = v^2 / (2g) => v = -sqrt(2 * g * h)) -> đảm bảo qua vật cản mà không bay khỏi màn hình
        JumpForce = -Mathf.Sqrt(2.0f * Gravity * TargetJumpHeight);

        DropSpeed = baseDropSpeed * multiplier;

        if (animatedSprite != null)
        {
            animatedSprite.SpeedScale = multiplier;
        }
    }
    public override void _PhysicsProcess(double delta)
    {


        if (!isPlaying)
        {
            Velocity = Vector2.Zero;

            if (animatedSprite != null && animatedSprite.IsPlaying())
            {
                animatedSprite.Stop();
            }

            return;
        }

        bool isDownPressed = Input.IsActionPressed("ui_down");
        bool isJumpPressed = Input.IsActionJustPressed("ui_accept") || Input.IsActionJustPressed("ui_up");

        // Trọng lực
        if (!IsOnFloor())
        {
            // Đang ở trên không thì không ở trạng thái cúi
            if (isDucking)
            {
                StopDucking();
            }

            float currentGravity = Gravity;
            // Khi đang rơi xuống (Velocity.Y > 0), kéo xuống mạnh hơn nữa (1.2x) để khủng long rớt xuống nhanh và dứt khoát
            if (Velocity.Y > 0)
            {
                currentGravity *= 1.2f;
            }

            Velocity = new Vector2(
                Velocity.X,
                Velocity.Y + currentGravity * (float)delta
            );

            // BẤM PHÍM MŨI TÊN XUỐNG KHI ĐANG TRÊN KHÔNG: Rơi nhanh xuống mặt đất
            if (isDownPressed)
            {
                // Nếu vận tốc rơi hiện tại nhỏ hơn DropSpeed thì tăng tốc rơi ngay lập tức
                if (Velocity.Y < DropSpeed)
                {
                    Velocity = new Vector2(Velocity.X, DropSpeed);
                }
            }
        }
        else
        {
            // Không cho rơi xuyên mặt đất
            Velocity = new Vector2(
                Velocity.X,
                0
            );

            // CÚI NGƯỜI (DUCK / CROUCH): Khi đang ở trên mặt đất và ấn giữ mũi tên xuống
            if (isDownPressed && !isJumpPressed)
            {
                StartDucking();

                // Luân phiên 2 frame chân chạy khi cúi người (12 FPS theo tốc độ game, đồng bộ hoàn hảo)
                duckAnimTimer += (float)delta * 12.0f * currentSpeedMultiplier;
                if (duckAnimTimer >= 1.0f)
                {
                    duckAnimTimer -= 1.0f;
                    duckFrame = (duckFrame + 1) % 2;
                    if (duckSprite != null)
                    {
                        duckSprite.Texture = duckFrame == 0 ? duckTex1 : duckTex2;
                    }
                }

                // Hiệu ứng nhấp nhô nhẹ theo nhịp chạy khi cúi người
                if (duckSprite != null)
                {
                    float bob = Mathf.Sin((float)Time.GetTicksMsec() * 0.025f * currentSpeedMultiplier) * 2.0f;
                    duckSprite.Position = new Vector2(defaultDuckPos.X, defaultDuckPos.Y + bob);

                    // Quần áo và phụ kiện cũng nhấp nhô đồng bộ theo thân khủng long khi cúi
                    if (currentOutfit > 0)
                    {
                        Vector2 baseClothesPos = GetDuckingClothesPos();
                        Vector2 baseHatPos = GetDuckingHatPos();
                        if (clothesSprite != null)
                        {
                            clothesSprite.Position = new Vector2(baseClothesPos.X, baseClothesPos.Y + bob);
                        }
                        if (hatSprite != null)
                        {
                            hatSprite.Position = new Vector2(baseHatPos.X, baseHatPos.Y + bob);
                        }
                    }
                }
            }
            else
            {
                if (isDucking)
                {
                    StopDucking();
                }
            }
        } 

        // NHẢY: Bấm phím Space (ui_accept) hoặc Mũi tên Lên (ui_up) khi đang ở trên mặt đất
        if (isJumpPressed && IsOnFloor())
        {
            if (isDucking)
            {
                StopDucking();
            }

            Velocity = new Vector2(
                Velocity.X,
                JumpForce
            );

            // Khi vừa bật nhảy: đứng yên ngay lập tức, dừng chuyển động hoạt ảnh
            if (animatedSprite != null)
            {
                animatedSprite.Stop();
                animatedSprite.Frame = 0;
            }
        }

        MoveAndSlide();
        Position = new Vector2(defaultPlayerPos.X, Position.Y);

        CheckCollision();


        if (!isDucking && animatedSprite != null)
        {
            if (IsOnFloor())
            {
                // Khi tiếp đất: tiếp tục chạy hoạt ảnh bước chân
                if (!animatedSprite.IsPlaying())
                {
                    animatedSprite.Play();
                }
            }
            else
            {
                // Khi đang ở trên không (nhảy/rơi): đứng yên, không chuyển động animation
                if (animatedSprite.IsPlaying() || animatedSprite.Frame != 0)
                {
                    animatedSprite.Stop();
                    animatedSprite.Frame = 0;
                }
            }
        }
    }

    private void StartDucking()
    {
        if (isDucking || isDead || !isPlaying)
            return;

        isDucking = true;

        if (animatedSprite != null)
        {
            animatedSprite.Hide();
        }

        if (duckSprite != null)
        {
            duckSprite.Texture = duckFrame == 0 ? duckTex1 : duckTex2;
            duckSprite.Position = defaultDuckPos;
            duckSprite.Show();
        }

        // Khi cúi người: nếu đang mặc đồ thì điều chỉnh quần áo nằm ngang theo thân dino
        ApplyDuckingOutfitTransform();

        // Chuyển vùng tương tác sang CollisionShape2D2 khi ở trạng thái cúi xuống
        if (collisionShape2 != null)
        {
            if (collisionShape != null)
            {
                collisionShape.SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
            }
            collisionShape2.SetDeferred(CollisionShape2D.PropertyName.Disabled, false);
        }
        else if (collisionShape != null && collisionShape.Shape is RectangleShape2D rect)
        {
            rect.Size = duckCollisionSize;
            collisionShape.Position = duckCollisionPos;
        }
    }

    private void StopDucking()
    {
        if (!isDucking)
            return;

        isDucking = false;
        duckAnimTimer = 0f;
        duckFrame = 0;

        if (duckSprite != null)
        {
            duckSprite.Texture = duckTex1;
            duckSprite.Hide();
        }

        if (!isDead)
        {
            if (animatedSprite != null)
            {
                animatedSprite.Show();
                if (isPlaying && IsOnFloor() && !animatedSprite.IsPlaying())
                {
                    animatedSprite.Play();
                }
                else if (!IsOnFloor())
                {
                    animatedSprite.Stop();
                    animatedSprite.Frame = 0;
                }
            }

            // Hiện lại trang phục nếu đang mặc
            RestoreOutfitVisibility();
        }

        // Khôi phục lại vùng tương tác là CollisionShape2D khi ở chế độ chạy thường
        if (collisionShape2 != null)
        {
            if (collisionShape != null)
            {
                collisionShape.SetDeferred(CollisionShape2D.PropertyName.Disabled, false);
            }
            collisionShape2.SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
        }
        else if (collisionShape != null && collisionShape.Shape is RectangleShape2D rect)
        {
            rect.Size = defaultCollisionSize;
            collisionShape.Position = defaultCollisionPos;
        }
    }


    private void CheckCollision()
    {
        if (isDead)
            return;

        int collisionCount = GetSlideCollisionCount();

        for (int i = 0; i < collisionCount; i++)
        {
            KinematicCollision2D collision = GetSlideCollision(i);

            if (collision == null)
                continue;

            Node collider = collision.GetCollider() as Node;

            if (collider == null)
                continue;


            // Nếu đụng vật cản
            if (collider.IsInGroup("obstacle"))
            {
                Die();
                return;
            }


            // Nếu đụng bất kỳ StaticBody2D / CharacterBody2D
            // bên cạnh mặt đất
            if (collider is StaticBody2D ||
                collider is CharacterBody2D)
            {
                // Nếu đang ở trên mặt đất thì bỏ qua
                if (!IsOnFloor())
                {
                    Die();
                    return;
                }
            }
        }
    }

    public void StartGame()
    {
        if (isDucking) StopDucking();
        if (duckSprite != null) duckSprite.Hide();

        isPlaying = true;
        isDead = false;

        Velocity = Vector2.Zero;
        SetSpeedMultiplier(1.0f);

        // Ẩn sprite thua, hiện lại sprite chạy
        if (deadSprite != null)
        {
            deadSprite.Hide();
        }

        if (animatedSprite != null)
        {
            animatedSprite.Show();
            animatedSprite.Play();
        }

        // Khôi phục lại trang phục nếu đang mặc
        RestoreOutfitVisibility();

        GD.Print("🦖 Dino bắt đầu chạy!");
    }

    public void StopGame()
    {
        if (isDucking) StopDucking();
        if (duckSprite != null) duckSprite.Hide();

        isPlaying = false;
        Velocity = Vector2.Zero;

        if (animatedSprite != null)
        {
            animatedSprite.Stop();
        }

        GD.Print("🛑 Dino dừng lại!");
    }

    // Đưa Player về tư thế đứng (chuẩn bị thay đồ)
    public void SetStandingPose()
    {
        if (isDucking) StopDucking();
        if (duckSprite != null) duckSprite.Hide();

        isDead = false;
        isPlaying = false;
        Velocity = Vector2.Zero;

        // Ẩn sprite thua
        if (deadSprite != null)
        {
            deadSprite.Hide();
        }

        // Hiện lại sprite bình thường và dừng lại ở dáng đứng
        if (animatedSprite != null)
        {
            animatedSprite.Show();
            animatedSprite.Stop();
            animatedSprite.Frame = 0; // Frame đứng yên ban đầu
        }

        // Khôi phục lại trang phục về đúng vị trí chuẩn
        RestoreOutfitVisibility();

        GD.Print("🦖 Dino chuyển về tư thế đứng!");
    }

    public void Die()
    {
        if (isDead)
            return;

        if (isDucking) StopDucking();
        if (duckSprite != null) duckSprite.Hide();

        isDead = true;
        isPlaying = false;

        Velocity = Vector2.Zero;

        // Đảm bảo khủng long luôn nằm bẹp ngay trên mặt đất (ground) thay vì lơ lửng trên không
        Position = new Vector2(Position.X, defaultPlayerPos.Y);

        // Ẩn sprite hoạt hình chạy bình thường
        if (animatedSprite != null)
        {
            animatedSprite.Stop();
            animatedSprite.Hide();
        }

        // Hiện sprite thua (nằm gục x_x)
        if (deadSprite != null)
        {
            deadSprite.Show();
        }

        // Kích hoạt hiệu ứng quần áo rớt ra và văng xuống đất
        DropClothesEffect();

        GD.Print("💥 DINO ĐÃ VA CHẠM, HIỆN SPRITE THUA VÀ RỚT ĐỒ!");

        // Gọi StopGame() của Node2D
        Node parent = GetParent();

        if (parent != null)
        {
            if (parent.HasMethod("StopGame"))
            {
                parent.Call("StopGame");
            }
        }
    }

    private Vector2 GetDuckingClothesPos()
    {
        return currentOutfit switch
        {
            1 => new Vector2(218.0f, 326.0f),
            2 => new Vector2(236.0f, 326.0f),
            3 => new Vector2(234.0f, 326.0f),
            _ => clothesDefaultPos
        };
    }

    private float GetDuckingClothesRotation()
    {
        return currentOutfit switch
        {
            1 => Mathf.DegToRad(34.0f),
            2 => Mathf.DegToRad(-22.0f),
            3 => Mathf.DegToRad(-18.0f),
            _ => 0f
        };
    }

    private Vector2 GetDuckingHatPos()
    {
        return currentOutfit switch
        {
            1 => new Vector2(322.0f, 256.0f),
            2 => new Vector2(320.0f, 280.0f),
            3 => new Vector2(335.0f, 248.0f),
            _ => hatDefaultPos
        };
    }

    private float GetDuckingHatRotation()
    {
        return 0f;
    }

    private Vector2 GetStandingClothesScale()
    {
        return currentOutfit switch
        {
            1 => new Vector2(0.316f, 0.278f),
            2 => new Vector2(0.468f, 0.413f),
            3 => new Vector2(0.425f, 0.442f),
            _ => Vector2.One
        };
    }

    private Vector2 GetStandingHatScale()
    {
        return currentOutfit switch
        {
            1 => new Vector2(0.212f, 0.228f),
            2 => new Vector2(0.541f, 0.468f),
            3 => new Vector2(0.396f, 0.408f),
            _ => Vector2.One
        };
    }

    private void ApplyDuckingOutfitTransform()
    {
        if (currentOutfit > 0)
        {
            if (clothesSprite != null)
            {
                clothesSprite.Position = GetDuckingClothesPos();
                clothesSprite.Rotation = GetDuckingClothesRotation();
                clothesSprite.Scale = GetStandingClothesScale();
                clothesSprite.ZIndex = 1;
                clothesSprite.Modulate = new Color(1, 1, 1, 1);
                clothesSprite.Show();
            }
            if (hatSprite != null)
            {
                hatSprite.Position = GetDuckingHatPos();
                hatSprite.Rotation = GetDuckingHatRotation();
                hatSprite.Scale = GetStandingHatScale();
                hatSprite.ZIndex = 1;
                hatSprite.Modulate = new Color(1, 1, 1, 1);
                hatSprite.Show();
            }
        }
        else
        {
            if (clothesSprite != null) clothesSprite.Hide();
            if (hatSprite != null) hatSprite.Hide();
        }
    }

    private void RestoreOutfitVisibility()
    {
        if (outfitTween != null && outfitTween.IsValid())
        {
            outfitTween.Kill();
        }

        if (currentOutfit > 0)
        {
            if (clothesSprite != null)
            {
                clothesSprite.Position = clothesDefaultPos;
                clothesSprite.Rotation = 0f;
                clothesSprite.Scale = GetStandingClothesScale();
                clothesSprite.ZIndex = 1;
                clothesSprite.Modulate = new Color(1, 1, 1, 1);
                clothesSprite.Show();
            }
            if (hatSprite != null)
            {
                hatSprite.Position = hatDefaultPos;
                hatSprite.Rotation = 0f;
                hatSprite.Scale = GetStandingHatScale();
                hatSprite.ZIndex = 1;
                hatSprite.Modulate = new Color(1, 1, 1, 1);
                hatSprite.Show();
            }
        }
        else
        {
            if (clothesSprite != null) clothesSprite.Hide();
            if (hatSprite != null) hatSprite.Hide();
        }
    }

    private void DropClothesEffect()
    {
        if (currentOutfit <= 0)
            return;

        if (outfitTween != null && outfitTween.IsValid())
        {
            outfitTween.Kill();
        }

        // Đưa quần áo và mũ lên lớp hiển thị đằng trước player (ZIndex cao hơn)
        if (clothesSprite != null) clothesSprite.ZIndex = 10;
        if (hatSprite != null) hatSprite.ZIndex = 11;

        outfitTween = CreateTween();
        outfitTween.SetParallel(true);

        // Áo rơi văng ra xa hơn phía sau (-130px) và rớt xuống đất (+80px)
        if (clothesSprite != null && clothesSprite.Visible)
        {
            Vector2 dropTarget = clothesDefaultPos + new Vector2(-130f, 80f);
            outfitTween.TweenProperty(clothesSprite, "position", dropTarget, 0.50)
                .SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
            outfitTween.TweenProperty(clothesSprite, "rotation", -0.6f, 0.50);
        }

        // Mũ bay bổng lên cao rồi rơi văng ra xa hơn hẳn (-170px) xuống đất (+130px)
        if (hatSprite != null && hatSprite.Visible)
        {
            Vector2 hatApex = hatDefaultPos + new Vector2(-70f, -60f);
            Vector2 hatTarget = hatDefaultPos + new Vector2(-170f, 130f);

            Tween hatSubTween = CreateTween();
            hatSubTween.TweenProperty(hatSprite, "position", hatApex, 0.20)
                .SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
            hatSubTween.TweenProperty(hatSprite, "position", hatTarget, 0.32)
                .SetTrans(Tween.TransitionType.Bounce).SetEase(Tween.EaseType.Out);

            outfitTween.TweenProperty(hatSprite, "rotation", -1.2f, 0.52);
        }
    }
    
// 0 = không mặc
// 1 = bộ công chúa
// 2 = bộ 2

public void TogglePrincessOutfit()
{
    if (currentOutfit == 1)
    {
        // Đang mặc Bộ 1 → cởi đồ
        clothesSprite.Hide();
        hatSprite.Hide();

        currentOutfit = 0;

        GD.Print("👗 Đã cởi đồ công chúa!");
    }
    else
    {
        // ÁO BỘ 1
        clothesSprite.Texture = GD.Load<Texture2D>(
            "res://Clothes/aocongchua-removebg-preview.png"
        );

        clothesDefaultPos = new Vector2(212.782f, 340.0f);
        clothesSprite.Scale = new Vector2(0.316f, 0.278f);

        // MŨ BỘ 1
        hatSprite.Texture = GD.Load<Texture2D>(
            "res://Clothes/mucongchua-removebg-preview.png"
        );

        hatDefaultPos = new Vector2(277.0f, 201.0f);
        hatSprite.Scale = new Vector2(0.212f, 0.228f);

        currentOutfit = 1;

        if (isDucking)
        {
            ApplyDuckingOutfitTransform();
        }
        else
        {
            clothesSprite.Position = clothesDefaultPos;
            clothesSprite.Rotation = 0f;
            hatSprite.Position = hatDefaultPos;
            hatSprite.Rotation = 0f;
            clothesSprite.Show();
            hatSprite.Show();
        }

        GD.Print("👗👑 Đã mặc đồ công chúa!");
    }
}


public void ToggleSecondOutfit()
{
    if (currentOutfit == 2)
    {
        // Đang mặc Bộ 2 → cởi đồ
        clothesSprite.Hide();
        hatSprite.Hide();

        currentOutfit = 0;

        GD.Print("👕 Đã cởi bộ đồ 2!");
    }
    else
    {
        // ÁO BỘ 2
        clothesSprite.Texture = GD.Load<Texture2D>(
            "res://Clothes/aobodo2.png"
        );

        clothesDefaultPos = new Vector2(263.0f, 328.938f);
        clothesSprite.Scale = new Vector2(0.468f, 0.413f);

        // MŨ BỘ 2
        hatSprite.Texture = GD.Load<Texture2D>(
            "res://Clothes/mubodo2.png"
        );

        hatDefaultPos = new Vector2(279.0f, 249.5f);
        hatSprite.Scale = new Vector2(0.541f, 0.468f);

        currentOutfit = 2;

        if (isDucking)
        {
            ApplyDuckingOutfitTransform();
        }
        else
        {
            clothesSprite.Position = clothesDefaultPos;
            clothesSprite.Rotation = 0f;
            hatSprite.Position = hatDefaultPos;
            hatSprite.Rotation = 0f;
            clothesSprite.Show();
            hatSprite.Show();
        }

        GD.Print("👕🎩 Đã mặc bộ đồ 2!");
    }
}
public void ToggleThirdOutfit()
{
    GD.Print("ĐÃ BẤM BỘ 3!");
    if (currentOutfit == 3)
    {
        clothesSprite.Hide();
        hatSprite.Hide();
        currentOutfit = 0;
        GD.Print("👚 Đã cởi bộ đồ 3!");
    }
    else
    {
        clothesSprite.Texture = GD.Load<Texture2D>(
            "res://Clothes/aobodo3.png"
        );
        clothesDefaultPos = new Vector2(239.75f, 329.5f);
        clothesSprite.Scale = new Vector2(0.425f, 0.442f);

        hatSprite.Texture = GD.Load<Texture2D>(
            "res://Clothes/mubodo3.png"
        );
        hatDefaultPos = new Vector2(264.0f, 229.5f);
        hatSprite.Scale = new Vector2(0.396f, 0.408f);

        currentOutfit = 3;

        if (isDucking)
        {
            ApplyDuckingOutfitTransform();
        }
        else
        {
            clothesSprite.Position = clothesDefaultPos;
            clothesSprite.Rotation = 0f;
            hatSprite.Position = hatDefaultPos;
            hatSprite.Rotation = 0f;
            clothesSprite.Show();
            hatSprite.Show();
        }

        GD.Print("👚🎩 Đã mặc bộ đồ 3!");
    }
}

}