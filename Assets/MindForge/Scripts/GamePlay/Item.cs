using System;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Rigidbody))]
public class Item : MonoBehaviour
{
    [Header(" Data")]
    [SerializeField] private EItemName itemName; // Loại item này là loại nào
    public EItemName ItemName => itemName;

    private ItemSpot spot; // Item này đang ở vị trí nào
    public ItemSpot Spot => spot;

    [SerializeField] private Sprite icon;
    public Sprite Icon => icon;

    [Header("Elements")]
    [SerializeField] private Renderer renderer;
    [SerializeField] private Collider collider;
    private Material baseMaterial;

    private void Awake()
    {
        baseMaterial = renderer.material;
    }

    // Hàm xử lý khi item được gán vào một vị trí nào đó
    public void AssignSpot(ItemSpot spot)
       => this.spot = spot; // Gán vị trí cho item để sau này có thể biết được item đang ở đâu

    public void UnassignSpot()
      => spot = null; // Hủy gán vị trí cho item khi nó không còn ở vị trí nào nữa

    public void DisableShadows()
    {
        // Tắt đổ bóng
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    }

    public void EnableShadows()
    {
        // Bật đổ bóng
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
    }

    public void EnablePhysics()
    {
        GetComponent<Rigidbody>().isKinematic = false;
        collider.enabled = true;
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

    public void ApplyRandomForce(float magnitude)
    {
        GetComponent<Rigidbody>().AddForce(Random.onUnitSphere * magnitude, ForceMode.VelocityChange); // Áp dụng lực ngẫu nhiên để item bay ra xa
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.02f);
    }
}
