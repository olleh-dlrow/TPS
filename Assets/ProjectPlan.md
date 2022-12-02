# Sneak-项目管理规划

## 资源管理

eg: 

资源分类？

第三方插件/外部资源/

文件夹划分？

按照类型划分：3D资源，纹理，音效，...

脚本管理？

暂时不去管理，但是要写注释

## 测试

需求：将每个功能划分出模块（组件），能够做到即插即拔并且不会影响到主体

## 场景

TestEnvTemplate: 测试环境的模板，包含已经搭建好的测试场景

## 玩家/角色

PlayerSpawner：负责生成玩家和相机



依赖关系与必要属性：

PlayerRoot <- ModelPrefab(venti)

ModelPrefab: WeaponHolder, Spine

ModelPrefab <- Model



组件：

ThirdPersonController：必要

<- Camera

---

ShooterController：非必要，提供射击功能

<- ThirdPersonController

<- AimCamera

可选：

CrossHair

---

CharacterIKController：非必要，控制角色IK

<- ShooterController



---

Character:

代表玩家的角色，用于和其它的类通信

static characters

static GetCharacter（index）



---

关于雾的深度：

受到Globalfog和Lighting设置中fog的双重影响



## 输入控制

使用InputSystem控制输入

使用Response信号记录当前能够响应的操作



## 各种后处理效果

PostProcess: 

功能性组件

需要注意处理的顺序

1. Fog
2. Scanner
3. BulletTime



## 扫描效果

ScannableObject：

可以被扫描的物体，扫描后显示不同的颜色或者不同的效果

预先设定好可以使用的材质，在开启扫描后控制材质的透明度



ScanCamera:

扫描摄像机，在扫描后可以整体被渲染成不同的风格

能够控制渲染的速度和时间，可以通过按键的长短控制



Scanner：

全局扫描效果，给物体添加描边



## 计时器

Timer：

用于各种需要计时的场景

在第一次被使用时生成

能够一次性触发计时和间隔触发计时

## 机关

MoveableRoad：

控制平台的升降和旋转

MoveableRoadController：

可移动平台控制器，当玩家靠近该机关时，显示可控制UI；当玩家点击确认后，玩家进入**控制视角**，右下角UI提示操作方法

---



JumpChallange：

跳跃挑战（跳跳乐）



电梯：

帮助人物上升和下降

电梯停下的条件是触碰到（或者高度超过）提前放置好的标记

电梯运动的方向受当前位置和上一个运动方向的共同影响，如果没有到达边界，则顺着上个方向走，否则转向

电梯的下一个目标标记受到当前标记和运动方向影响



## 战斗

玩家遭遇敌人时，需要击破敌人身上的弱点和周围的火力点，同时，如果无法在限定时间内击破所有弱点，则同样失败

敌人的部分弱点击破之后能够延长战斗时间，火力点必须在限定时间内击破



## 奖励

收集品：

可完成全收集成就



子弹：

补充弹药，但是有上限



钥匙：

打开大门
