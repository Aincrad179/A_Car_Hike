using UnityEngine;

public class WebCamToRT : MonoBehaviour
{
    [Header("把你的 vedio RT 拖到这里")]
    public RenderTexture targetRT;

    private WebCamTexture webCamTexture;

    void Start()
    {
        // 获取电脑上所有的摄像头硬件
        WebCamDevice[] devices = WebCamTexture.devices;

        if (devices.Length > 0)
        {
            // 默认调用第一个摄像头（通常就是笔记本自带的摄像头）
            webCamTexture = new WebCamTexture(devices[0].name);
            webCamTexture.Play();
        }
        else
        {
            Debug.LogError("未能检测到笔记本摄像头！");
        }
    }

    void Update()
    {
        // 确保摄像头正在运行，并且目标 RT 不为空
        if (webCamTexture != null && webCamTexture.isPlaying && targetRT != null)
        {
            // 只有当摄像头画面在这一帧真正更新了，才进行绘制，节省性能
            if (webCamTexture.didUpdateThisFrame)
            {
                // 核心魔法：把摄像头的画面直接“印”到 Render Texture 上
                Graphics.Blit(webCamTexture, targetRT);
            }
        }
    }

    void OnDestroy()
    {
        // 养成好习惯：游戏停止时关闭摄像头，防止摄像头指示灯一直亮着
        if (webCamTexture != null && webCamTexture.isPlaying)
        {
            webCamTexture.Stop();
        }
    }
}