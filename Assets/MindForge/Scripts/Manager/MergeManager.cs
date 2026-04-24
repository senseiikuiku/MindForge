using System;
using System.Collections.Generic;
using UnityEngine;

public class MergeManager : MonoBehaviour
{
    [Header("Go Up Settings")]
    [SerializeField] private float goUpDistance = 0.15f;
    [SerializeField] private float goUpDuration = 0.15f;
    [SerializeField] private LeanTweenType goUpEasing;

    [Header("Smash Settings")]
    [SerializeField] private float smashDuration = 0.15f;
    [SerializeField] private LeanTweenType smashEasing;

    [Header("Effects")]
    [SerializeField] private ParticleSystem mergeParticles;

    [Header(" Actions ")]
    public static Action merged;

    private void Awake()
    {
        ItemSpotsManager.mergeStarted += OnMergeStarted;
    }

    private void OnDestroy()
    {
        ItemSpotsManager.mergeStarted -= OnMergeStarted;
    }

    private void OnMergeStarted(List<Item> items)
    {
        // Di chuyển tất cả items lên trên
        for (int i = 0; i < items.Count; i++)
        {
            // Vị trí đích: Vị trí hiện tại + hướng lên * khoảng cách
            Vector3 targetPos = items[i].transform.position
                              + items[i].transform.up * goUpDistance;

            Action callback = null;

            // Chỉ item đầu tiên mới có callback
            if (i == 0)
                callback = () => SmashItems(items);

            // Animation bay lên
            LeanTween.move(items[i].gameObject, targetPos, goUpDuration)
                .setEase(goUpEasing)
                .setOnComplete(callback);
        }
    }

    private void SmashItems(List<Item> items)
    {
        // 1. Sắp xếp items theo vị trí X (trái → phải)
        items.Sort((a, b) => a.transform.position.x.CompareTo(b.transform.position.x));

        // 2. Lấy vị trí X của item giữa (index 1)
        float targetX = items[1].transform.position.x;

        // 3. Item bên trái (index 0) lao sang phải
        LeanTween.moveX(items[0].gameObject, targetX, smashDuration)
            .setEase(smashEasing)
            .setOnComplete(() => FinalizeMerge(items));  // Callback sau khi xong

        // 4. Item bên phải (index 2) lao sang trái
        LeanTween.moveX(items[2].gameObject, targetX, smashDuration)
            .setEase(smashEasing);
        // Không cần callback, vì item[0] đã có rồi
    }

    private void FinalizeMerge(List<Item> items)
    {
        // 1. Xóa tất cả items
        for (int i = 0; i < items.Count; i++)
            Destroy(items[i].gameObject);

        // 2. Spawn particle effect tại vị trí item giữa
        ParticleSystem particles = Instantiate(
            mergeParticles,
            items[1].transform.position,  // Vị trí item giữa
            Quaternion.identity,
            transform
        );

        // 3. Play particle
        particles.Play();

        merged?.Invoke();
    }
}
