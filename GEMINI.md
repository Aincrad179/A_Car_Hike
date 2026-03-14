# A Car Hike 项目核心规则 (Project Mandates)

本文档记录了项目的核心架构选择和物理规则，Gemini CLI 在进行任何代码修改或新功能开发时必须严格遵守。

## 1. 坐标系统 (Coordinate System)
- **世界轴向**：遵循 Unity 3D 标准。Y 轴指向天空，XZ 平面为水平地面。
- **重力方向**：(0, -9.81, 0)，即垂直作用于 Y 轴负方向。
- **赛车朝向**：Forward (蓝色箭头) 为行驶方向，Up (绿色箭头) 为像素堆叠方向。

## 2. 物理与碰撞 (Physics)
- **物理引擎**：使用 **3D 物理** (Rigidbody, BoxCollider)。禁止混用 2D 物理组件。
- **Rigidbody 配置**：
    - `useGravity = true`
    - `constraints` 必须锁定 **Freeze Rotation X** 和 **Freeze Rotation Z**，防止赛车倾翻。
- **碰撞检测**：赛道边缘检测优先使用 Raycast (向下探测地面颜色或 Tag)。

## 3. 视觉风格：像素堆叠 (Pixel Stacking)
- **层级生成**：使用 `PixelStacker.cs` 脚本。
- **堆叠轴向**：切片必须绕 X 轴旋转 90 度以平铺在 XZ 平面，并沿 **局部 Y 轴** 向上位移。
- **渲染设置**：优先使用 `Unlit` 材质以保持像素颜色纯正。

## 4. 摄像头体感控制 (Pose Input)
- **方案**：使用 `jp.ikep.mediapipe.blazepose` (BlazePoseBarracuda)。
- **转向逻辑**：基于左/右手腕的高度差 (`leftWrist.y - rightWrist.y`)。
- **镜像适配**：已在 `PoseSteeringManager` 中适配镜像摄像头，确保“左手下压 = 向右转”的体感直觉。

## 5. 相机系统 (Camera System)
- **插件**：使用 **Cinemachine**。
- **投影模式**：**Orthographic (正交)**。
- **视角配置**：
    - 旋转 X 轴约为 **60°** (俯视视角)。
    - 使用 `3rd Person Follow` 或 `Framing Transposer` 实现追尾平滑跟随。
- **阻尼设置**：保持一定的 Damping (1~2) 以模拟驾驶惯性。

---
*最后更新时间：2026年3月14日*
