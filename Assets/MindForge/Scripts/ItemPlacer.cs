using UnityEngine;
using System.Collections.Generic;
using NaughtyAttributes;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ItemPlacer : MonoBehaviour
{
    [Header("Elements")]
    [SerializeField] private List<ItemLevelData> itemDatas;

    [Header("Settings")]
    [SerializeField] private BoxCollider spawnZone;
    [SerializeField] private int seed;

    public ItemLevelData[] GetGoals()
    {
        List<ItemLevelData> goals = new List<ItemLevelData>();

        foreach (ItemLevelData data in itemDatas)
        {
            if (data.isGoal)
            {
                goals.Add(data);
            }
        }
        return goals.ToArray();
    }

#if UNITY_EDITOR
    [Button] // Nút để gọi hàm GenerateItems trong Inspector
    private void GenerateItems()
    {
        // Xóa tất cả các item con hiện có trước khi tạo mới
        while (transform.childCount > 0)
        {
            Transform t = transform.GetChild(0);
            t.SetParent(null);
            DestroyImmediate(t.gameObject); // Sử dụng DestroyImmediate để xóa ngay lập tức trong Editor
        }

        Random.InitState(seed); // Thiết lập seed cho Random để đảm bảo tính nhất quán

        // Tạo mới các item dựa trên itemDatas
        for (int i = 0; i < itemDatas.Count; i++)
        {
            ItemLevelData data = itemDatas[i];
            for (int j = 0; j < data.amount; j++)
            {
                Vector3 spawnPosition = GetSpawnPosition();

                Item itemInstance = PrefabUtility.InstantiatePrefab(data.itemPrefab, transform) as Item;
                itemInstance.transform.position = spawnPosition;
                itemInstance.transform.rotation = Quaternion.Euler(Random.onUnitSphere * 360);
            }
        }
    }

    private Vector3 GetSpawnPosition()
    {
        // Random vị trí trong spawnZone
        float x = Random.Range(-spawnZone.size.x / 2, spawnZone.size.x / 2);
        float y = Random.Range(-spawnZone.size.y / 2, spawnZone.size.y / 2);
        float z = Random.Range(-spawnZone.size.z / 2, spawnZone.size.z / 2);

        // Tính toán vị trí local dựa trên center của spawnZone và random offset
        Vector3 localPosition = spawnZone.center + new Vector3(x, y, z);
        // Chuyển đổi vị trí local sang world position
        Vector3 spawnPosition = transform.TransformPoint(localPosition);

        return spawnPosition;
    }
#endif
}
