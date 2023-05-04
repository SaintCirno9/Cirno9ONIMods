// using PeterHan.PLib.Detours;
// using PeterHan.PLib.UI;
// using UnityEngine;
// using UnityEngine.UI;
//
// namespace AdjustableSolidTransferArm;
//
// // Currently just a copy of the SingleCheckboxSideScreen class
// // TODO: Make this contains two checkboxes
// public class SolidTransferArmControlSideScreen : SideScreenContent 
// {
//     private SolidTransferArmControl target;
//     
//     // public KToggle crossWallToggle;
//     // public KImage crossWallCheckMark;
//     // public LocText crossWallLabel;
//
//     public KToggle showRangeToggle;
//     public KImage showRangeCheckMark;
//     public LocText showRangeLabel;
//
//     public override bool IsValidForTarget(GameObject go)
//     {
//         Debug.Log("SolidTransferArmControlSideScreen.IsValidForTarget running");
//         return go.GetComponent<SolidTransferArmControl>() != null;
//     }
//
//     protected override void OnPrefabInit()
//     {
//         base.OnPrefabInit();
//     }
//
//     protected override void OnSpawn()
//     {
//         base.OnSpawn();
//         // crossWallToggle.onValueChanged += OnCrossWallValueChanged;
//         showRangeToggle.onValueChanged += OnShowRangeValueChanged;
//     }
//
//     public override void SetTarget(GameObject go)
//     {
//         base.SetTarget(go);
//         if (go == null)
//         {
//             Debug.LogError("The target object provided was null");
//         }
//         else
//         {
//             target = go.GetComponent<SolidTransferArmControl>();
//             if (target == null)
//                 target = go.GetSMI<SolidTransferArmControl>();
//             if (target == null)
//                 Debug.LogError("The target provided does not have a SolidTransferArmControl component");
//             else
//             {
//                 titleKey = target.ControlTitleKey;
//                 // crossWallLabel.text = target.CrossWallCheckboxLabel;
//                 // crossWallToggle.transform.parent.GetComponent<ToolTip>()
//                 //     .SetSimpleTooltip(target.CrossWallCheckboxTooltip);
//                 // crossWallToggle.isOn = target.GetCrossWall();
//                 // crossWallCheckMark.enabled = crossWallToggle.isOn;
//                 showRangeLabel.text = target.HideRangeCheckboxLabel;
//                 showRangeToggle.transform.parent.GetComponent<ToolTip>()
//                     .SetSimpleTooltip(target.HideRangeCheckboxTooltip);
//                 showRangeToggle.isOn = target.GetHideRange();
//                 showRangeCheckMark.enabled = showRangeToggle.isOn;
//             }
//         }
//     }
//
//     public override void ClearTarget()
//     {
//         base.ClearTarget();
//         target = null;
//     }
//
//     // private void OnCrossWallValueChanged(bool value)
//     // {
//     //     target.SetCrossWall(value);
//     //     crossWallCheckMark.enabled = value;
//     // }
//
//     private void OnShowRangeValueChanged(bool value)
//     {
//         target.SetHideRange(value);
//         showRangeCheckMark.enabled = value;
//     }
// }