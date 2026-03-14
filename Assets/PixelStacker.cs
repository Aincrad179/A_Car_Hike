using UnityEngine;

/// <summary>
/// 像素堆叠核心脚本：将切片平铺在 XZ 平面并沿 Y 轴向上堆叠
/// </summary>
public class PixelStacker : MonoBehaviour
{
    [Header("堆叠设置")]
    [Tooltip("按顺序拖入切片：从底盘到车顶")]
    public Sprite[] layers;
    
    [Tooltip("层级之间的高度偏移")]
    public float layerOffset = 0.05f;

    [Tooltip("建议使用 Unlit 材质防止像素色差")]
    public Material layerMaterial;

    void Start()
    {
        GenerateStack();
    }

    public void GenerateStack()
    {
        // 清理旧层级
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            if (child.name.StartsWith("Layer_"))
            {
                if (Application.isPlaying) Destroy(child.gameObject);
                else DestroyImmediate(child.gameObject);
            }
        }

        if (layers == null || layers.Length == 0) return;

        for (int i = 0; i < layers.Length; i++)
        {
            GameObject layerObj = new GameObject("Layer_" + i);
            layerObj.transform.SetParent(this.transform);
            
            // 关键：将 Sprite 旋转 90 度，使其平行于 XZ 地面
            layerObj.transform.localRotation = Quaternion.Euler(90, 0, 0);
            
            // 关键：沿着 Y 轴（高度方向）位移
            layerObj.transform.localPosition = new Vector3(0, i * layerOffset, 0);

            SpriteRenderer sr = layerObj.AddComponent<SpriteRenderer>();
            sr.sprite = layers[i];
            
            // 确保渲染层级正确（虽然在 3D 空间，但 SortingOrder 依然有效）
            sr.sortingOrder = i;

            if (layerMaterial != null)
            {
                sr.material = layerMaterial;
            }
        }
    }

    [ContextMenu("Force Refresh Stack")]
    void Refresh()
    {
        GenerateStack();
    }
}
