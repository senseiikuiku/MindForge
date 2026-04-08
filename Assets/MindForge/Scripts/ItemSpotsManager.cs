using System;
using UnityEngine;

public class ItemSpotsManager : MonoBehaviour
{
    [Header("Elements")]
    [SerializeField] private Transform itemSpot;

    [Header("Settings")]
    [SerializeField] private Vector3 itemLocalPositionOnSpot;
    [SerializeField] private Vector3 itemLocalScaleOnSpot;

    private void Awake()
    {
        InputManager.itemCliked += OnItemClicked;
    }

    private void OnDestroy()
    {
        InputManager.itemCliked -= OnItemClicked;
    }

    private void OnItemClicked(Item item)
    {
        // Set item as child of item spot
        item.transform.SetParent(itemSpot);

        // Reset local position and scale
        item.transform.localPosition = itemLocalPositionOnSpot;
        item.transform.localScale = itemLocalScaleOnSpot;
        item.transform.localRotation = Quaternion.identity;

        // Disable shadows 
        item.DisableShadows();

        // Disable physics
        item.DisablePhysics();
    }


}
