using System;
using UnityEngine;

public class Vaccum : Powerup
{
    [Header("Elements")]
    [SerializeField] private Animator Animator;

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

    public void Play()
    {
        Animator.Play("Activate");
    }
}
