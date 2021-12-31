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



