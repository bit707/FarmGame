# FarmGame - Unity 种田游戏

一款参考星露谷物语玩法的 3D 种田模拟游戏，使用 Unity 2022+ (URP) 开发。

## 系统架构

### 核心系统
- **GameManager** - 全局单例，管理所有子系统
- **TimeSystem** - 游戏内时间流逝、日夜循环、季节变化
- **WeatherSystem** - 天气生成（晴/阴/雨/暴风雨/雪），影响农作物浇水
- **SaveSystem** - JSON 序列化存档/读档

### 玩家系统
- **PlayerController** - 第三人称角色控制（WASD移动，Shift跑步）
- **ThirdPersonCamera** - 可旋转缩放的跟随相机（右键旋转，滚轮缩放）
- **PlayerInteraction** - 鼠标指向农田格子，左键使用工具
- **PlayerStamina** - 体力系统，使用工具消耗体力，每天恢复

### 农场系统
- **FarmGrid** - 网格化农田管理（翻地/浇水/种植/收获）
- **CropData** - ScriptableObject 定义作物属性（生长天数、季节、阶段模型）
- **CropDatabase** - 作物数据库

### 物品与经济
- **InventorySystem** - 36格背包 + 9格快捷栏
- **ItemData** - ScriptableObject 定义物品属性
- **ShopSystem** - 商店买卖，季节限定商品
- **NPCShopkeeper** - 靠近NPC按F打开商店

### UI
- **HUDManager** - 时间、日期、金币、天气、快捷栏
- **InventoryUI** - 背包界面（E键开关）
- **ShopUI** - 商店界面
- **PauseMenu** - 暂停菜单（ESC），存档/读档/退出
- **StaminaBarUI** - 体力条

## 操作方式

| 按键 | 功能 |
|------|------|
| WASD | 移动 |
| Shift | 跑步 |
| 鼠标左键 | 使用当前工具/种子 |
| 鼠标右键 | 旋转相机 |
| 滚轮 | 缩放相机 |
| 1-9 | 切换快捷栏 |
| E | 打开/关闭背包 |
| F | 与NPC交互 |
| ESC | 暂停菜单 |

## 开发环境要求

- Unity 2022.3 LTS 或更高版本
- Universal Render Pipeline (URP)
- Input System Package
- TextMeshPro

## 快速开始

1. 用 Unity Hub 打开 `D:\FarmGame` 文件夹
2. Unity 会自动导入 Packages
3. 打开 `Assets/Scenes/` 下的场景（或新建场景）
4. 在场景中创建空物体，挂载 `SceneBootstrap` 脚本用于快速测试
5. 创建 GameManager 空物体，挂载 `GameManager`，子物体分别挂载 `TimeSystem`、`WeatherSystem`、`InventorySystem`、`ShopSystem`、`SaveSystem`

## 添加作物

1. 右键 Assets → Create → Farm → Crop Data
2. 填写作物名称、生长天数、可种植季节
3. 指定各生长阶段的 Prefab（种子→幼苗→成长→成熟）
4. 创建对应的 Item Data 作为收获物
5. 将作物添加到 CropDatabase

## 美术资源

项目代码已完成，需要导入以下美术资源：
- 角色模型 + 动画（idle, walk, run, tool_use）
- 作物各阶段模型（建议每种作物4个阶段）
- 地形材质（草地、翻耕土、浇水土）
- UI 图标（工具、种子、作物、天气）
- 粒子特效（雨、雪、收获）
- 环境音效（BGM、雨声、工具声）

推荐资源：
- Unity Asset Store 搜索 "Low Poly Farm" 或 "Stylized Farm"
- 使用 ProBuilder 快速搭建建筑原型
