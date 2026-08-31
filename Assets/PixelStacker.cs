using UnityEngine;

public class PixelStacker : MonoBehaviour
{
    [Header("堆叠设置")]
    public Sprite[] layers;
    public float layerOffset = 0.05f;
    public Material layerMaterial;

    void Start()
    {
        GenerateStack();
    }

    public void GenerateStack()
    {
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
            layerObj.transform.localRotation = Quaternion.Euler(90, 0, 0);
            layerObj.transform.localPosition = new Vector3(0, i * layerOffset, 0);

            SpriteRenderer sr = layerObj.AddComponent<SpriteRenderer>();
            sr.sprite = layers[i];
            sr.sortingOrder = i;

            if (layerMaterial != null)
            {
                sr.material = layerMaterial;
            }
        }
    }

    [ContextMenu("Force Refresh Stack")]
    void Refresh() { GenerateStack(); }
}
