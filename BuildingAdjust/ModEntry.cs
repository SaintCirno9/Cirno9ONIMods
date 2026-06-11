using BuildingAdjust.STRINGS;
using HarmonyLib;
using KMod;
using UnityEngine;

namespace BuildingAdjust;

public class ModEntry : UserMod2
{
    public override void OnLoad(Harmony harmony)
    {
        base.OnLoad(harmony);
        LocString.CreateLocStringKeys(typeof(UI));

        // 已启用功能：
        // - PumpPatch：给泵添加吸收范围侧边栏控制、启停范围修正、管道速率同步和两倍管道容量缓存。
        // - StoragePatch：补充储物过滤标签，并密封默认储物。
        // - DoorPatch：让所有带 Door 组件的门以三倍速度开关，包含 mod 门。
        // - RonivansFridgePatch：把工业革命冷藏建筑的食物储存容量提高到一百倍。
        //
        // 已禁用功能清单：
        // - BatchBuildingPatch：批量关闭建筑淹没判定、提高过热温度、调整导热、允许部分建筑建在任意位置，并触发其他批量建筑调整。
        // - BatteryPatch：扩大电池和智能电池容量，并移除电量流失。
        // - BottleFillerAutoDropPatch：给气瓶装罐器和 Bottle Filler 添加自动掉落控制。
        // - ElectrolyzerPatch：取消电解器超压停机判定。
        // - ElementConverterPatch：提高元素转换建筑的输入输出速率，并额外增强空气过滤器、电解器和堆肥的产物。
        // - GeneratorPatch：调整发电机额定功率，并提高发电副产物生成速率。
        // - GeyserPatch：提高喷泉喷发量，并取消最大压力限制。
        // - MaterialPatch：给塑料追加装饰和过热温度属性。
        // - PipePatch：提高气液管道容量，并提高固体运输轨道输出速率。
        // - RotationPatch：允许空气过滤器、日光灯和变压器旋转或翻转。
        // - SimCellOccupierPatch：给门和梯子添加隔气隔水控制。
        // - TilePatch：提高砖块和梯子的移动速度倍率。
        // - VentPatch：给气体和液体排出口添加忽略压力控制。
        // - WirePatch：提高各级电线最大功率。
        Debug.Log((object)"[BuildingAdjust] Loaded with pump controls enabled.");
    }
}
