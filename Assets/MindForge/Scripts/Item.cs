using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Item : MonoBehaviour
{
    [Header(" Data")]
    [SerializeField] private EItemName itemName;
    public EItemName ItemName => itemName;

    private ItemSpot spot;
    public ItemSpot Spot => spot;

    [Header("Elements")]
    [SerializeField] private Renderer renderer;
    [SerializeField] private Collider collider;
    private Material baseMaterial;

    private void Awake()
    {
        baseMaterial = renderer.material;
    }

    public void AssignSpot(ItemSpot spot)
    {
        this.spot = spot; // Gán vị trí cho item để sau này có thể biết được item đang ở đâu
    }

    public void DisableShadows()
    {
        // Tắt đổ bóng
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    }
    public void DisablePhysics()
    {
        GetComponent<Rigidbody>().isKinematic = true;
        collider.enabled = false;
    }

    public void Select(Material outlineMaterial)
    {
        renderer.materials = new Material[] { baseMaterial, outlineMaterial };
    }

    public void Deselect()
    {
        renderer.materials = new Material[] { baseMaterial };
    }

}
