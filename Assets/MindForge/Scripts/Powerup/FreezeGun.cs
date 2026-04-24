using UnityEngine;
using System;

public class FreezeGun : Powerup
{
    [Header("Elements")]
    [SerializeField] private Animator AnimatorSandClock;
    [SerializeField] private Animator AnimatorGlassHolder; // con của sand Clock

    [Header("Actions")]
    public static Action started;
    public static Action completed;

    protected override void Awake()
    {
        base.Awake();
    }

    // Được gọi từ Animation Event (giữa animation)
    private void TriggerPowerupStart()
    {
        started?.Invoke();
    }

    // Được gọi từ Animation Event (cuối animation)
    private void OnAnimationComplete()
    {
        completed?.Invoke();
    }

    // Được gọi từ Animation Event của SandBlock
    private void OnActivateComplete()
    {
        // CHỈ chạy GlassHolder KHI SandClock kết thúc
        AnimatorGlassHolder.Play("Activate");
    }

    public void Play()
    {
        // CHỈ chạy SandClock trước
        AnimatorSandClock.Play("Activate");
        // GlassHolder sẽ được trigger từ Animation Event
    }
}
