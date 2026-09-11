using Godot;

public partial class Spawner : Node
{
    [Export] private PackedScene obstacleScene;
    [Export] private PackedScene planeScene;
    [Export] private float baseSpeed = 400.0f;
    [Export] private float baseMinInterval = 1.6f;
    [Export] private float baseMaxInterval = 2.8f;

    // 2 độ cao máy bay:
    // - planeLowY (218.0f): Độ cao thấp (bằng lần chỉnh thứ 2), khủng long đứng chạy sẽ đụng trúng đầu, bắt buộc phải CÚI XUỐNG để né
    // - planeHighY (150.0f): Độ cao cao hơn trên bầu trời, khủng long chạy thẳng KHÔNG CẦN CÚI vẫn đi qua được (nhảy lên sẽ bị đụng) để đánh lừa người chơi
    [Export] private float planeLowY = 218.0f;
    [Export] private float planeHighY = 150.0f;
    [Export] private float groundSpawnY = 350.0f;

    private float timer = 0f;
    private float nextSpawnInterval = 2.0f;
    private bool isPlaying = false;
    private float currentSpeedMultiplier = 1.0f;
    private int consecutiveGroundCount = 0;
    private int consecutivePlaneCount = 0;

    public override void _Ready()
    {
        SetProcess(true);
        if (planeScene == null)
        {
            planeScene = GD.Load<PackedScene>("res://plane_obstacle.tscn");
        }
        if (obstacleScene == null)
        {
            obstacleScene = GD.Load<PackedScene>("res://obstacle.tscn");
        }
        PickNextInterval();
    }

    public void SetSpeedMultiplier(float multiplier)
    {
        currentSpeedMultiplier = Mathf.Max(0.5f, multiplier);
    }

    private void PickNextInterval()
    {
        // Khi game chạy nhanh hơn, khoảng thời gian chờ (giây) giảm tương ứng
        // để duy trì khoảng cách tối thiểu an toàn giữa các vật cản
        float minInterval = baseMinInterval / currentSpeedMultiplier;
        float maxInterval = baseMaxInterval / currentSpeedMultiplier;
        nextSpawnInterval = (float)GD.RandRange(minInterval, maxInterval);
    }

    public override void _Process(double delta)
    {
        if (!isPlaying)
            return;

        timer += (float)delta;

        if (timer >= nextSpawnInterval)
        {
            SpawnObstacle();
            timer = 0f;
            PickNextInterval();
        }
    }

    private void SpawnObstacle()
    {
        // Quyết định loại chướng ngại vật xuất hiện:
        // ĐẢM BẢO: Máy bay và vật cản mặt đất (bịch rác) KHÔNG BAO GIỜ XUẤT HIỆN CÙNG LÚC.
        bool spawnPlane = false;

        // Máy bay xuất hiện theo thuật toán độ khó tăng dần:
        // - Khi tốc độ game tăng (currentSpeedMultiplier >= 1.15): bắt đầu có xác suất xuất hiện máy bay
        // - Tốc độ càng tăng, tỷ lệ gặp máy bay càng cao (từ 25% lên tới 45%)
        if (planeScene != null && currentSpeedMultiplier >= 1.15f)
        {
            float planeChance = Mathf.Clamp(0.25f + (currentSpeedMultiplier - 1.15f) * 0.25f, 0.25f, 0.45f);

            // Không để xuất hiện quá 2 máy bay liên tiếp để đảm bảo nhịp chơi đa dạng
            if (consecutivePlaneCount >= 2)
            {
                spawnPlane = false;
            }
            // Nếu đã qua 3 lần liên tiếp toàn rác mặt đất, ưu tiên xuất hiện máy bay
            else if (consecutiveGroundCount >= 3)
            {
                spawnPlane = true;
            }
            else
            {
                spawnPlane = GD.Randf() < planeChance;
            }
        }

        if (spawnPlane)
        {
            consecutivePlaneCount++;
            consecutiveGroundCount = 0;
            SpawnPlane();
        }
        else
        {
            consecutiveGroundCount++;
            consecutivePlaneCount = 0;
            SpawnGroundObstacle();
        }
    }

    private void SpawnPlane()
    {
        if (planeScene == null)
            return;

        var plane = planeScene.Instantiate() as Node2D;
        if (plane == null)
            return;

        if (plane is Obstacle obs)
        {
            obs.SetSpeed(baseSpeed * currentSpeedMultiplier);
        }

        // Thuật toán 2 độ cao máy bay:
        // 1. Bay thấp (planeLowY = 218.0f): Bắt buộc Dino phải cúi người xuống để né qua
        // 2. Bay cao hơn (planeHighY = 150.0f): Bay cao hơn, dino chạy thẳng không cần cúi vẫn qua được để đánh lừa phản xạ (nhảy lên sẽ bị đụng)
        bool isLow = GD.Randf() < 0.55f;
        float chosenY = isLow ? planeLowY : planeHighY;

        plane.Position = new Vector2(1300, chosenY);
        GetTree().CurrentScene.AddChild(plane);
    }

    private void SpawnGroundObstacle()
    {
        if (obstacleScene == null)
            return;

        // Khi tốc độ game tăng đến một mức (multiplier >= 1.25):
        // Bắt đầu có xác suất xuất hiện cụm 2 chướng ngại vật xếp liền kề nhau
        int count = 1;
        if (currentSpeedMultiplier >= 1.25f)
        {
            float doubleChance = Mathf.Clamp((currentSpeedMultiplier - 1.25f) * 0.6f, 0.25f, 0.6f);
            if (GD.Randf() < doubleChance)
            {
                count = 2;
            }
        }

        float obstacleGap = 60.0f; // Đặt dính liền kề nhau như 1 cụm để nhảy qua trong 1 cú nhảy duy nhất

        for (int i = 0; i < count; i++)
        {
            var obstacle = obstacleScene.Instantiate() as Node2D;
            if (obstacle == null)
                continue;

            if (obstacle is Obstacle obs)
            {
                obs.SetSpeed(baseSpeed * currentSpeedMultiplier);
            }

            // Đặt vật cản thứ 2 ngay sau vật cản thứ 1
            obstacle.Position = new Vector2(1300 + (i * obstacleGap), groundSpawnY);
            GetTree().CurrentScene.AddChild(obstacle);
        }
    }

    public void StartSpawning()
    {
        isPlaying = true;
        timer = 0f;
        currentSpeedMultiplier = 1.0f;
        consecutiveGroundCount = 0;
        consecutivePlaneCount = 0;
        PickNextInterval();
    }

    public void StopSpawning()
    {
        isPlaying = false;
        timer = 0f;
    }
}