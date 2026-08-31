# A Car Hike 项目核心规则 (Project Mandates)

本文档记录了项目的核心架构选择、物理规则以及未来路线图，Gemini CLI 在进行任何代码修改或新功能开发时必须严格遵守。

## 1. 坐标系统与层级结构 (Coordinate System & Hierarchy)
- **世界轴向**：遵循 Unity 3D 标准。Y 轴指向天空，XZ 平面为水平地面。
- **重力方向**：(0, -9.81, 0)，即垂直作用于 Y 轴负方向。
- **层级架构**：
    - **Parent (Player_Car)**：负责物理逻辑 (`Rigidbody`, `BoxCollider`, `PixelCarController`)。Rotation 始终保持在 (0,0,0) 作为基准。
    - **Child (Player_Visual)**：负责视觉表现 (`PixelStacker`, `PixelCarVisuals`, 粒子系统)。可根据切片方向进行初始旋转（如 X=90）。

## 2. 物理与移动 (Physics & Movement)
- **物理引擎**：使用 **3D 物理** (Rigidbody, BoxCollider)。禁止混用 2D 物理组件。
- **Rigidbody 配置**：
    - `useGravity = true`
    - `constraints`：锁定 **Freeze Rotation X** 和 **Freeze Rotation Z**，防止赛车倾翻。
- **漂移物理**：通过在 `FixedUpdate` 中削减侧向速度 (`KillOrthogonalVelocity`) 实现带惯性的转向感。

## 3. 视觉风格：像素堆叠 (Pixel Stacking)
- **层级生成**：使用 `PixelStacker.cs` 脚本。
- **堆叠逻辑**：切片绕 X 轴旋转 90 度以平铺在 XZ 平面，并沿 **局部 Y 轴** 向上位移生成厚度。
- **视觉反馈**：通过 `PixelCarVisuals.cs` 实现：
    - **Roll Tilt**：转向时车身绕局部 Z 轴侧倾（模拟离心力）。
    - **Visual Yaw**：转向时车头向弯心额外偏转（模拟过度转向/漂移感）。

## 4. 摄像头体感控制 (Pose Input)
- **方案**：使用 `jp.ikep.mediapipe.blazepose` (BlazePoseBarracuda)。
- **转向逻辑 (角度制)**：基于左右手腕连线的 **夹角 (Atan2)**。
    - `maxSteeringAngle` 默认为 45°。
    - 适配镜像摄像头：左手下压 = 向右转。
- **油门逻辑 (定值制)**：基于双手 **直线距离**。
    - 距离 < `centerDistance`：**合拢加速** (Throttle = 1)。
    - 距离 > `centerDistance`：**张开减速** (Throttle = -1)。

## 5. 渲染管线 (Rendering - URP)
- **管线类型**：Universal Render Pipeline (URP)。
- **像素化方案**：
    - 方案 A：使用 `Full Screen Pass` 结合 `Custom/OutlinePostProcess` Shader 实现全屏描边。
    - 方案 B：使用 `GameRenderCanvas` (Render Texture) 降分辨率渲染。
- **重要设置**：必须在 URP Asset 中开启 **Depth Texture** 和 **Opaque Texture**，否则后处理描边将失效。

## 6. 开发路线图 (Roadmap)
- **快速赛道生成工具**：使用 Spline 插值技术生成带碰撞的平滑路径。
- **环境交互**：实现雨滴粒子在车灯光柱中的 Additive 高亮效果。
- **同屏双人 (Local Split-Screen)**：多人体推理压力测试与视野适配。

---
*最后更新时间：2026年4月5日 (全系统集成版)*
