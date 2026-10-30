using System;
using System.Collections.Generic;
using UnityEngine;

namespace PassiveLiquidVentInput
{
    public class PassiveLiquidVentInput : StateMachineComponent<PassiveLiquidVentInput.SMInstance>, ISim1000ms,
        IGameObjectEffectDescriptor
    {
        
        public static float MIN_WORK_MASS = 1f;

        public static float MIN_PASS_MASS = 0.011f;

        public static float MASS_INCREASE_FACTOR = 0.011f;
        
        public static float MAX_PASS_MASS = 100f;
        
        protected override void OnSpawn()
        {
            smi.StartSM();
            dispenser.GetConduitManager().AddConduitUpdater(OnConduitUpdate,
                ConduitFlowPriority.LastPostUpdate);
        }

        protected override void OnCleanUp()
        {
            dispenser.GetConduitManager().RemoveConduitUpdater(OnConduitUpdate);
            base.OnCleanUp();
        }

        private void OnConduitUpdate(float dt)
        {
            conduitBlockedStatusGuid = selectable.ToggleStatusItem(
                Db.Get().BuildingStatusItems.ConduitBlocked, conduitBlockedStatusGuid, dispenser.blocked,
                null);
        }

        public bool IsLogicOff => !operational.GetFlag(LogicOperationalController.LogicOperationalFlag);

        public bool IsButtonOff => !operational.GetFlag(BuildingEnabledButton.EnabledFlag);
        public int Cell => Grid.PosToCell(transform.GetPosition());

        public float Mass => Grid.Mass[Cell];
        public bool IsLiquidInCell => Grid.Element[Cell].IsState(Element.State.Liquid);
        public bool IsEnoughMass => Grid.Mass[Cell] >= MIN_WORK_MASS;

        public void Sim1000ms(float dt)
        {
            noElementStatusGuid = selectable.ToggleStatusItem(Db.Get().BuildingStatusItems.NoLiquidElementToPump,
                noElementStatusGuid, !IsLiquidInCell, null);
        }

        public void UpdateConsumption(float dt)
        {
            UpdateConsumptionRate();
            SetAnimSpeed(0.75f + 0.075f * consumer.consumptionRate);
        }

        private void UpdateConsumptionRate()
        {
            if (!(IsLiquidInCell & IsEnoughMass)) return;
            var newConsumptionRate = MIN_PASS_MASS +
                                     (Mass - MIN_WORK_MASS) *
                                     MASS_INCREASE_FACTOR;
            consumer.consumptionRate = Mathf.Clamp(Mathf.Round(newConsumptionRate * 1000f) / 1000f,
                MIN_PASS_MASS, MAX_PASS_MASS);
            consumer.RefreshConsumptionRate();
        }

        public void SetAnimSpeed(float factor)
        {
            animController.PlaySpeedMultiplier = factor;
            float positionPercent = animController.GetPositionPercent();
            animController.Play(animController.CurrentAnim.hash, animController.PlayMode);
            animController.SetPositionPercent(positionPercent);
        }

        public List<Descriptor> GetDescriptors(GameObject go)
        {
            List<Descriptor> list = new List<Descriptor>();
            Descriptor item = default(Descriptor);
            item.SetupDescriptor(
                string.Format(STRINGS.BUILDINGS.PREFABS.PASSIVELIQUIDVENTINPUT.DESCRIPTORS.REQ_PRESSURE_DESC,
                    GameUtil.GetFormattedMass(MIN_WORK_MASS)),
                string.Format(STRINGS.BUILDINGS.PREFABS.PASSIVELIQUIDVENTINPUT.DESCRIPTORS.REQ_PRESSURE_TOOLTIP,
                    GameUtil.GetFormattedMass(MIN_WORK_MASS)));
            list.Add(item);
            item = default;
            item.SetupDescriptor(STRINGS.BUILDINGS.PREFABS.PASSIVELIQUIDVENTINPUT.DESCRIPTORS.DEP_PRESSURE_DESC,
                string.Format(STRINGS.BUILDINGS.PREFABS.PASSIVELIQUIDVENTINPUT.DESCRIPTORS.DEP_PRESSURE_TOOLTIP,
                    GameUtil.GetFormattedMass(MASS_INCREASE_FACTOR)));
            list.Add(item);
            return list;
        }

        [MyCmpReq] private Operational operational;

        [MyCmpGet] private KSelectable selectable;

        [MyCmpReq] private ElementConsumer consumer;

        [MyCmpGet] private ConduitDispenser dispenser;

        [MyCmpReq] private KBatchedAnimController animController;

        private Guid conduitBlockedStatusGuid;

        private Guid noElementStatusGuid;

        public class SMInstance : GameStateMachine<States, SMInstance, PassiveLiquidVentInput, object>.GameInstance
        {
            public SMInstance(PassiveLiquidVentInput master) : base(master)
            {
            }

            public bool IsOperational => master.operational.IsOperational;

            public bool IsEnoughLiquidMass => master.IsLiquidInCell && master.IsEnoughMass;

            public bool IsClosed => master.IsLogicOff | master.IsButtonOff;

            public bool IsOpenedIdle => !master.IsLogicOff & !master.IsButtonOff & !IsOperational;

            public void Update(float dt)
            {
                master.UpdateConsumption(dt);
            }
        }

        public class States : GameStateMachine<States, SMInstance,
            PassiveLiquidVentInput>
        {
            public override void InitializeStates(out BaseState defaultState)
            {
                defaultState = Idle;
                Closed.QueueAnim("off")
                    .EventTransition(GameHashes.OperationalFlagChanged, Idle,
                        smi => smi.IsOpenedIdle).EventTransition(
                        GameHashes.OperationalChanged, PumpingPre,
                        smi => smi.IsOperational & smi.IsEnoughLiquidMass);
                Idle.QueueAnim("on").Update("Idle",
                    delegate(SMInstance smi, float dt)
                    {
                        if (smi.IsEnoughLiquidMass && smi.IsOperational)
                        {
                            smi.GoTo(Pumping);
                        }
                    }).EventTransition(GameHashes.OperationalFlagChanged, Closed,
                    smi => smi.IsClosed);
                PumpingPre.QueueAnim("working_pre").OnAnimQueueComplete(Pumping);
                Pumping.Enter(delegate(SMInstance smi) { smi.master.operational.SetActive(true); }).Exit(
                        delegate(SMInstance smi)
                        {
                            smi.master.operational.SetActive(false);
                            smi.master.SetAnimSpeed(1f);
                        }).QueueAnim("working_loop", true).Update("Pumping",
                        delegate(SMInstance smi, float dt)
                        {
                            if (smi.IsEnoughLiquidMass)
                            {
                                smi.Update(dt);
                                return;
                            }

                            smi.GoTo(Idle);
                        }, UpdateRate.SIM_1000ms)
                    .EventTransition(GameHashes.OperationalChanged, Idle,
                        smi => smi.IsOpenedIdle).EventTransition(
                        GameHashes.OperationalChanged, PumpingPst,
                        smi => smi.IsClosed);
                PumpingPst.QueueAnim("working_pst").OnAnimQueueComplete(Closed);
            }

            public State Closed;

            public State Idle;

            public State PumpingPre;

            public State Pumping;

            public State PumpingPst;
        }
    }
}