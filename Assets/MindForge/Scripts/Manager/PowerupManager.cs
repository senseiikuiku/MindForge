using NaughtyAttributes;
using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;

public class PowerupManager : MonoBehaviour
{
    [Header("Elements")]
    [SerializeField] private Vaccum vaccum;
    [SerializeField] private Transform vaccumSuckPosition;

    [Header("Settings")]
    private bool isBusy;
    private int vaccumItemsToCollect; // Biến đếm số lượng item cần thu thập bởi máy hút
    private int vaccumCounter; // Biến đếm số lượng item đã được thu thập bởi máy hút

    [Header("Actions")]
    public static Action<Item> itemPickedUp;

    [Header("Data")]
    [SerializeField] private int inititalPUCount; // Số lượng powerup ban đầu khi bắt đầu game
    private int vaccumPUCount; // Biến đếm số lượng powerup máy hút hiện có của người chơi

    private void Awake()
    {
        LoadData();

        Vaccum.started += OnVaccumStarted;
        InputManager.powerupClicked += OnPowerupClicked;
    }

    private void OnDestroy()
    {
        Vaccum.started -= OnVaccumStarted;
        InputManager.powerupClicked -= OnPowerupClicked;
    }

    private void OnVaccumStarted()
    {
        vaccumPowerup();
    }

    private void OnPowerupClicked(Powerup powerup)
    {
        if (isBusy)
            return;

        switch (powerup.Type)
        {
            case EPowerupType.Vaccum:
                HandleVaccumClicked();
                UpdateVaccumVisuals();
                break;
            case EPowerupType.Spring:
                break;
            case EPowerupType.Fan:
                break;
            case EPowerupType.Freeze:
                break;
        }
    }

    private void HandleVaccumClicked()
    {
        if (vaccumPUCount <= 0)
        {
            vaccumPUCount = 3;
            SaveData();
        }
        else
        {
            isBusy = true;

            vaccumPUCount--;
            SaveData();

            vaccum.Play();
        }

    }

    [Button]
    private void vaccumPowerup()
    {
        // Kiểm tra level đã được spawn chưa
        if (LevelManager.Instance == null || LevelManager.Instance.Items.Length == 0)
        {
            Debug.LogWarning("Cannot use vacuum powerup: Level not ready!");
            return;
        }

        // Lấy tất cả các item trong level và các mục tiêu
        Item[] items = LevelManager.Instance.Items;
        ItemLevelData[] goals = GoalManager.Instance.Goals;

        // Tìm mục tiêu có số lượng lớn nhất
        ItemLevelData? greatesGoal = GetGreatesGoal(goals);

        if (greatesGoal == null)
            return;

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
                Debug.LogWarning($"Không thể thu thập item tại index {i} vì nó đã bị hủy hoặc chưa được khởi tạo.");
                continue;
            }

            if (items[i].ItemName == goal.itemPrefab.ItemName)
            {
                itemsToCollect.Add(items[i]);

                if (itemsToCollect.Count >= 3)
                    break;
            }
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

        if (vaccumCounter >= vaccumItemsToCollect)
        {
            isBusy = false;
        }

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

    private void LoadData()
    {
        vaccumPUCount = PlayerPrefs.GetInt("VaccumCount", inititalPUCount);
        UpdateVaccumVisuals();
    }

    private void SaveData()
    {
        PlayerPrefs.SetInt("VaccumCount", vaccumPUCount);
    }
}
