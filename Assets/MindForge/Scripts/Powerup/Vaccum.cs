using System;
using UnityEngine;

public class Vaccum : Powerup
{
    [Header("Elements")]
    [SerializeField] private Animator Animator;

    [Header("Actions")]
    public static Action started;

    private void TriggerPowerupStart()
    {
        started?.Invoke();
    }

    public void Play()
    {
        Animator.Play("Activate");
    }
}
