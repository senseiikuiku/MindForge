using NaughtyAttributes;
using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;

public class PowerupManager : MonoBehaviour
{
    [Header("Elements")]
    [SerializeField] private Vaccum vaccum;
    [SerializeField] private Spring spring;
    [SerializeField] private Fan fan;
    [SerializeField] private FreezeGun freezeGun;
    [SerializeField] private Transform vaccumSuckPosition;

    [Header("Fan Settings")]
    [SerializeField] private float fanMagintude;

    [Header("Settings")]
    private bool isBusy;
    private int vaccumItemsToCollect; // Biến đếm số lượng item cần thu thập bởi máy hút
    private int vaccumCounter; // Biến đếm số lượng item đã được thu thập bởi máy hút

    [Header("Actions")]
    public static Action<Item> itemPickedUp;
    public static Action<Item> itemBackToGame;

    [Header("Data")]
    [SerializeField] private int inititalPUCount; // Số lượng powerup ban đầu khi bắt đầu game
    private int vaccumPUCount; // Biến đếm số lượng powerup máy hút hiện có của người chơi
    private int springPUCount; // Biến đếm số lượng powerup spring hiện có của người chơi
    private int fanPUCount; // Biến đếm số lượng powerup quạt hiện có của người chơi
    private int freezeGunPUCount; // Biến đếm số lượng powerup súng băng hiện có của người chơi

    private Powerup[] allPowerups;

    private void Awake()
    {
        allPowerups = new Powerup[] { vaccum, spring, fan, freezeGun };

        LoadData();

        Vaccum.started += OnVaccumStarted;
        Spring.started += OnSpringStarted;
        Fan.started += OnFanStarted;
        FreezeGun.started += OnFreezeGunStarted;

        Vaccum.completed += OnVaccumCompleted;
        Spring.completed += OnSpringCompleted;
        Fan.completed += OnFanCompleted;
        FreezeGun.completed += OnFreezeGunCompleted;

        InputManager.powerupClicked += OnPowerupClicked;
    }

    private void OnDestroy()
    {
        Vaccum.started -= OnVaccumStarted;
        Spring.started -= OnSpringStarted;
        Fan.started -= OnFanStarted;
        FreezeGun.started -= OnFreezeGunStarted;

        Vaccum.completed -= OnVaccumCompleted;
        Spring.completed -= OnSpringCompleted;
        Fan.completed -= OnFanCompleted;
        FreezeGun.completed -= OnFreezeGunCompleted;

        InputManager.powerupClicked -= OnPowerupClicked;
    }

    #region Event Handlers
    private void OnVaccumStarted()
    {
        VaccumPowerup();
    }

    private void OnSpringStarted()
    {
        SpringPowerup();
    }

    private void OnFanStarted()
    {
        FanPowerup();
    }

    private void OnFreezeGunStarted()
    {
        FreezeGunPowerup();
    }

    // Được gọi khi animation hoàn thành
    private void OnVaccumCompleted()
    {
        Debug.Log("🎬 Vaccum animation COMPLETED!");
        // Vaccum đã unlock trong ItemReachedVacuum callback
        // Chỉ cần đảm bảo unlock nếu chưa unlock
        if (isBusy && vaccumCounter >= vaccumItemsToCollect)
        {
            UnlockAllPowerups();
        }
    }

    private void OnSpringCompleted()
    {
        Debug.Log("🎬 Spring animation COMPLETED!");
        UnlockAllPowerups(); // ✅ Unlock khi animation xong
    }

    private void OnFanCompleted()
    {
        Debug.Log("🎬 Fan animation COMPLETED!");
        UnlockAllPowerups(); // ✅ Unlock khi animation xong
    }

    private void OnFreezeGunCompleted()
    {
        Debug.Log("🎬 Freeze Gun animation COMPLETED!");
        UnlockAllPowerups(); // ✅ Unlock khi animation xong
    }

    #endregion

    private void OnPowerupClicked(Powerup powerup)
    {
        if (powerup == null)
        {
            Debug.LogWarning("PowerupManager received NULL powerup!");
            return;
        }

        if (isBusy)
        {
            Debug.LogWarning($"IsBusy: {isBusy}, cannot use powerup {powerup.Type} right now!");
            return;
        }

        switch (powerup.Type)
        {
            case EPowerupType.Vaccum:
                HandleVaccumClicked();
                break;
            case EPowerupType.Spring:
                HandleSpringClicked();
                break;
            case EPowerupType.Fan:
                HandleFanClicked();
                break;
            case EPowerupType.Freeze:
                HandleFreezeGunClicked();
                break;
        }
    }

    private void HandleVaccumClicked()
    {
        if (vaccumPUCount <= 0)
        {
            vaccumPUCount = 3;
            UIManager.Instance.ShowInfo("Vaccum powerup refilled!");
            SaveData();
        }
        else
        {
            LockAllPowerups(); // Khóa tất cả các powerup khác để tránh người chơi sử dụng nhiều powerup cùng lúc

            vaccumPUCount--;
            SaveData();

            vaccum.Play();
        }

        UpdateVaccumVisuals();
    }

    private void HandleSpringClicked()
    {
        // Kiểm tra có item nào trên spots không
        ItemSpot spot = ItemSpotsManager.Instance.GetRandomOccupiedSpot();

        if (spot == null)
        {
            UIManager.Instance.ShowInfo("No items on spots to use Spring powerup!");
            return;
        }

        if (springPUCount <= 0)
        {
            springPUCount = 3;
            UIManager.Instance.ShowInfo("Spring powerup refilled!");
            SaveData();
        }
        else
        {
            LockAllPowerups();

            springPUCount--;
            SaveData();

            spring.Play();
        }

        UpdateSpringVisuals();
    }

    private void HandleFanClicked()
    {
        if (fanPUCount <= 0)
        {
            fanPUCount = 3;
            UIManager.Instance.ShowInfo("Fan powerup refilled!");
            SaveData();
        }
        else
        {
            LockAllPowerups();

            fanPUCount--;
            SaveData();

            fan.Play();
        }
        UpdateFanVisuals(); // Nếu có phần hiển thị số lượng quạt, hãy cập nhật nó ở đây
    }

    private void HandleFreezeGunClicked()
    {
        if (freezeGunPUCount <= 0)
        {
            freezeGunPUCount = 3;
            UIManager.Instance.ShowInfo("Freeze Gun powerup refilled!");
            SaveData();
        }
        else
        {
            LockAllPowerups();

            freezeGunPUCount--;
            SaveData();

            freezeGun.Play();
        }
        UpdateFreezeGunVisuals();
    }

    #region Vaccum Powerup Logic

    [Button]
    private void VaccumPowerup()
    {
        // Kiểm tra level đã được spawn chưa
        if (LevelManager.Instance == null || LevelManager.Instance.Items.Length == 0)
        {
            Debug.LogWarning("Cannot use vacuum powerup: Level not ready!");
            UnlockAllPowerups(); // Mở khóa tất cả các powerup
            return;
        }

        // Lấy tất cả các item trong level và các mục tiêu
        Item[] items = LevelManager.Instance.Items;
        ItemLevelData[] goals = GoalManager.Instance.Goals;

        // Tìm mục tiêu có số lượng lớn nhất
        ItemLevelData? greatesGoal = GetGreatesGoal(goals);

        if (greatesGoal == null)
        {
            UnlockAllPowerups();
            return;
        }

        // Lấy danh sách các item cần thu thập dựa trên mục tiêu lớn nhất
        ItemLevelData goal = (ItemLevelData)greatesGoal;

        vaccumCounter = 0;

        // Thu thập tối đa 3 item phù hợp với mục tiêu
        List<Item> itemsToCollect = new List<Item>();

        // Duyệt qua tất cả các item và thêm vào danh sách nếu chúng phù hợp với mục tiêu
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == null)
            {
                continue;
            }

            if (items[i].ItemName == goal.itemPrefab.ItemName)
            {
                itemsToCollect.Add(items[i]);

                if (itemsToCollect.Count >= 3)
                    break;
            }
        }

        if (itemsToCollect.Count == 0)
        {
            Debug.LogWarning($"No items of type {goal.itemPrefab.ItemName} found to collect!");
            UnlockAllPowerups();
            return;
        }

        // Cập nhật số lượng item cần thu thập
        vaccumItemsToCollect = itemsToCollect.Count;

        // Di chuyển các item được thu thập về vị trí của máy hút và sau đó hủy bỏ chúng khỏi scene
        for (int i = 0; i < itemsToCollect.Count; i++)
        {
            Item itemToCollect = itemsToCollect[i];  // Lưu item vào biến local để tránh closure issue

            // Vô hiệu hóa vật lý của item để tránh va chạm trong quá trình di chuyển
            itemToCollect.DisablePhysics();

            // Tạo một đường cong từ vị trí của item đến vị trí của máy hút
            List<Vector3> points = new List<Vector3>();

            points.Add(itemToCollect.transform.position);
            points.Add(itemToCollect.transform.position);

            points.Add(itemToCollect.transform.position + Vector3.up * 2f); // Thêm một điểm trung gian để tạo đường cong

            points.Add(vaccumSuckPosition.position + Vector3.zero * 2); // Thêm một điểm trung gian khác để tạo đường cong
            points.Add(vaccumSuckPosition.position);
            points.Add(vaccumSuckPosition.position);

            LeanTween.moveSpline(itemToCollect.gameObject, points.ToArray(), .5f)
                .setOnComplete(() => ItemReachedVacuum(itemToCollect));

            LeanTween.scale(itemToCollect.gameObject, Vector3.zero, 0.75f);
        }

        // Kích hoạt sự kiện itemPickedUp cho mỗi item được thu thập
        for (int i = itemsToCollect.Count - 1; i >= 0; i--)
        {
            itemPickedUp?.Invoke(itemsToCollect[i]);
        }
    }

    // Hàm xử lý khi một item đã đến vị trí của máy hút, tăng biến đếm và kiểm tra nếu đã thu thập đủ số lượng item cần thiết
    private void ItemReachedVacuum(Item item)
    {
        vaccumCounter++;

        Destroy(item.gameObject);
    }

    private ItemLevelData? GetGreatesGoal(ItemLevelData[] goals)
    {
        int max = 0;
        int goalIndex = -1;

        for (int i = 0; i < goals.Length; i++)
        {
            if (goals[i].amount >= max)
            {
                max = goals[i].amount;
                goalIndex = i;
            }
        }

        if (goalIndex <= -1)
            return null;

        return goals[goalIndex];

        //return goals.OrderByDescending(g => g.amount).FirstOrDefault();
    }

    private void UpdateVaccumVisuals()
    {
        vaccum.UpdateVisuals(vaccumPUCount);
    }

    #endregion

    #region Spring Powerup Logic

    [Button]
    public void SpringPowerup()
    {
        ItemSpot spot = ItemSpotsManager.Instance.GetRandomOccupiedSpot();

        if (spot == null)
        {
            UIManager.Instance.ShowInfo("No items on spots to use Spring powerup!");
            UnlockAllPowerups();
            return;
        }

        Item itemToRelease = spot.Item;

        spot.Clear(); // Xóa item khỏi vị trí để nó có thể di chuyển tự do

        itemToRelease.UnassignSpot(); // Hủy liên kết item với vị trí để tránh các vấn đề về tham chiếu

        itemToRelease.transform.parent = LevelManager.Instance.ItemParent;

        Vector3 jumpTarget = Vector3.up * 3f;

        LeanTween.moveLocal(itemToRelease.gameObject, jumpTarget, 0.75f)
            .setEase(LeanTweenType.easeOutBack) // Hiệu ứng bật lên có độ nẩy
            .setOnComplete(() =>
            {
                // Sau khi bay lên xong thì mới bật lại Vật lý để nó rơi tự do xuống
                itemToRelease.EnablePhysics();
            });

        itemToRelease.transform.localScale = Vector3.zero;
        LeanTween.scale(itemToRelease.gameObject, Vector3.one, 0.4f)
            .setEase(LeanTweenType.easeOutElastic);

        itemBackToGame?.Invoke(itemToRelease); // Kích hoạt sự kiện để thông báo rằng item đã trở lại game

    }

    private void UpdateSpringVisuals()
    {
        spring.UpdateVisuals(springPUCount);
    }

    #endregion

    #region Fan Powerup Logic

    [Button]
    public void FanPowerup()
    {
        Item[] items = LevelManager.Instance.Items;

        foreach (Item item in items)
        {
            if (item == null)
                continue;

            item.ApplyRandomForce(fanMagintude);
        }
    }

    private void UpdateFanVisuals()
    {
        fan.UpdateVisuals(fanPUCount);
    }

    #endregion

    #region Freeze Gun Powerup Logic

    [Button]
    public void FreezeGunPowerup()
    {
        if (TimerManager.Instance == null)
        {
            Debug.LogWarning("TimerManager not found!");
            UnlockAllPowerups();
            return;
        }
        UIManager.Instance.ShowInfo("Freeze time: 10s!");
        TimerManager.Instance.FreezeTimer();
    }

    private void UpdateFreezeGunVisuals()
    {
        freezeGun.UpdateVisuals(freezeGunPUCount);
    }

    #endregion

    #region Lock/Unlock System
    private void LockAllPowerups()
    {
        isBusy = true;

        foreach (Powerup powerup in allPowerups)
        {
            if (powerup != null)
                powerup.SetInteractable(false);
        }
    }

    private void UnlockAllPowerups()
    {
        isBusy = false;
        foreach (Powerup powerup in allPowerups)
        {
            if (powerup != null)
                powerup.SetInteractable(true);
        }
    }
    #endregion

    private void LoadData()
    {
        vaccumPUCount = PlayerPrefs.GetInt("VaccumCount", inititalPUCount);
        springPUCount = PlayerPrefs.GetInt("SpringCount", inititalPUCount);
        fanPUCount = PlayerPrefs.GetInt("FanCount", inititalPUCount);
        freezeGunPUCount = PlayerPrefs.GetInt("FreezeGunCount", inititalPUCount);

        UpdateVaccumVisuals();
        UpdateSpringVisuals();
        UpdateFanVisuals();
        UpdateFreezeGunVisuals();
    }

    private void SaveData()
    {
        PlayerPrefs.SetInt("VaccumCount", vaccumPUCount);
        PlayerPrefs.SetInt("SpringCount", springPUCount);
        PlayerPrefs.SetInt("FanCount", fanPUCount);
        PlayerPrefs.SetInt("FreezeGunCount", freezeGunPUCount);
    }
}
