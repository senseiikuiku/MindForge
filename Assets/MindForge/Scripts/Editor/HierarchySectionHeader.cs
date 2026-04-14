#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

[InitializeOnLoad]  // Auto chạy khi Unity load
public static class HierarchySectionHeader
{
    static HierarchySectionHeader()
    {
        // Đăng ký callback cho mỗi item trong Hierarchy
        EditorApplication.hierarchyWindowItemOnGUI += HierarchyWindowItemOnGUI;
    }

    static void HierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
    {
        // Lấy GameObject từ instance ID
        var gameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;

        // Kiểm tra tên có bắt đầu bằng "//" không
        if (gameObject != null && gameObject.name.StartsWith("//"))
        {
            // Vẽ nền đen
            EditorGUI.DrawRect(selectionRect, Color.black);

            // Vẽ text trắng, in hoa, căn giữa
            EditorGUI.LabelField(selectionRect,
                gameObject.name.Replace("/", "").ToUpperInvariant(),  // "// MANAGERS" → "MANAGERS"
                new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize = 12,
                    alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = Color.white }
                });
        }
    }
}
#endif