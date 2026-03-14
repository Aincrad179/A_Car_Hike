using UnityEngine;
using Mediapipe.BlazePose;

public class BlazePoseWristTracker : MonoBehaviour
{
    [Header("Input Source")]
    public RenderTexture inputRT; // Drag your Render Texture from WebCamToRT here

    [Header("Model Settings")]
    public BlazePoseModel modelType = BlazePoseModel.full;

    private BlazePoseDetecter _detecter;

    [Header("Extracted Wrist Coordinates (Normalized 0-1)")]
    public Vector3 leftWrist;
    public Vector3 rightWrist;
    [Range(0, 1)]
    public float leftWristScore;
    [Range(0, 1)]
    public float rightWristScore;

    void Start()
    {
        // Initialize the BlazePose detector
        _detecter = new BlazePoseDetecter(modelType);
    }

    void Update()
    {
        if (inputRT == null || _detecter == null) return;

        // Feed the Render Texture to BlazePose
        _detecter.ProcessImage(inputRT, modelType);

        // Extract landmarks (Index 15: Left Wrist, 16: Right Wrist)
        // x, y: Normalized [0, 1] relative to the input texture
        // z: Depth value (experimental)
        // w: Score/Confidence [0, 1]
        
        Vector4 leftLM = _detecter.GetPoseLandmark(15);
        Vector4 rightLM = _detecter.GetPoseLandmark(16);

        leftWrist = new Vector3(leftLM.x, leftLM.y, leftLM.z);
        leftWristScore = leftLM.w;

        rightWrist = new Vector3(rightLM.x, rightLM.y, rightLM.z);
        rightWristScore = rightLM.w;
    }

    void OnDestroy()
    {
        // Must dispose to release Compute Buffers and prevent memory leaks
        if (_detecter != null)
        {
            _detecter.Dispose();
        }
    }

    // Optional: Draw in scene view for debugging
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || inputRT == null) return;

        Gizmos.color = leftWristScore > 0.5f ? Color.green : Color.red;
        Gizmos.DrawSphere(new Vector3(leftWrist.x - 0.5f, 0.5f - leftWrist.y, 0) * 5, 0.1f);

        Gizmos.color = rightWristScore > 0.5f ? Color.green : Color.red;
        Gizmos.DrawSphere(new Vector3(rightWrist.x - 0.5f, 0.5f - rightWrist.y, 0) * 5, 0.1f);
    }
}
