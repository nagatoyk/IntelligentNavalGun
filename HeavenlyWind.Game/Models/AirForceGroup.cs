﻿using Sakuno.KanColle.Amatsukaze.Game.Models.Raw;
using System;

namespace Sakuno.KanColle.Amatsukaze.Game.Models
{
    public class AirForceGroup : RawDataWrapper<RawAirForceGroup>, IID
    {
        static int[] r_FighterFPBouns = { 0, 0, 2, 5, 9, 14, 14, 22 };
        static int[] r_SeaplaneBomberFPBouns = { 0, 0, 1, 1, 1, 3, 3, 6 };
        static int[] r_InternalFPBonus = { 10, 25, 40, 55, 70, 85, 100, 120 };

        public int ID => RawData.ID;

        string r_Name;
        public string Name
        {
            get { return r_Name; }
            internal set
            {
                if (r_Name != value)
                {
                    r_Name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        int r_CombatRadius;
        public int CombatRadius
        {
            get { return r_CombatRadius; }
            internal set
            {
                if (r_CombatRadius != value)
                {
                    r_CombatRadius = value;
                    OnPropertyChanged(nameof(CombatRadius));
                }
            }
        }

        int r_FighterPower;
        public int FighterPower
        {
            get { return r_FighterPower; }
            internal set
            {
                if (r_FighterPower != value)
                {
                    r_FighterPower = value;
                    OnPropertyChanged(nameof(FighterPower));
                }
            }
        }

        AirForceGroupOption r_Option;
        public AirForceGroupOption Option
        {
            get { return r_Option; }
            internal set
            {
                if (r_Option != value)
                {
                    r_Option = value;
                    OnPropertyChanged(nameof(Option));

                    UpdateFighterPower();
                    UpdateLBASConsumption();
                }
            }
        }

        public int LBASFuelConsumption { get; private set; }
        public int LBASBulletConsumption { get; private set; }

        public IDTable<AirForceSquadron> Squadrons { get; } = new IDTable<AirForceSquadron>();

        public AirForceSquadronRelocationCountdown Relocation { get; private set; }

        internal protected AirForceGroup(RawAirForceGroup rpRawData) : base(rpRawData)
        {
            OnRawDataUpdated();
        }

        protected override void OnRawDataUpdated()
        {
            Name = RawData.Name;
            Option = RawData.Option;

            Squadrons.UpdateRawData(RawData.Squadrons, r => new AirForceSquadron(this, r), (rpData, rpRawData) => rpData.Update(rpRawData));

            CombatRadius = RawData.CombatRadius;

            UpdateFighterPower();
            UpdateLBASConsumption();
        }

        internal void UpdateFighterPower()
        {
            var rFighterPower = .0;

            EquipmentInfo rReconnaissancePlane = null;

            foreach (var rSquadron in Squadrons.Values)
            {
                if (rSquadron.State != AirForceSquadronState.Idle)
                    continue;

                var rPlane = rSquadron.Plane;
                var rInfo = rPlane.Info;

                switch (rInfo.Type)
                {
                    case EquipmentType.CarrierBasedRecon:
                    case EquipmentType.ReconSeaplane:
                    case EquipmentType.LargeFlyingBoat:
                        if (rReconnaissancePlane == null || rReconnaissancePlane.LoS < rInfo.LoS || rReconnaissancePlane.Type > rInfo.Type)
                            rReconnaissancePlane = rInfo;
                        break;
                }

                if (!rInfo.CanParticipateInFighterCombat)
                    continue;

                double rResult;

                if (r_Option == AirForceGroupOption.AirDefense)
                    rResult = rInfo.AA + rInfo.Interception + rInfo.AntiBomber * 2.0;
                else
                    rResult = rInfo.AA + rInfo.Interception * 1.5;

                rResult *= Math.Sqrt(rSquadron.Count);

                switch (rInfo.Type)
                {
                    case EquipmentType.CarrierBasedFighter:
                        rResult += rPlane.Level * .2 * Math.Sqrt(rSquadron.Count);
                        break;
                    case EquipmentType.CarrierBasedDiveBomber:
                        rResult += rPlane.Level * .25 * Math.Sqrt(rSquadron.Count);
                        break;
                }

                if (rSquadron.Count > 0)
                {
                    var rProficiency = rPlane.Proficiency;

                    switch (rInfo.Type)
                    {
                        case EquipmentType.CarrierBasedFighter:
                        case EquipmentType.SeaplaneFighter:
                        case EquipmentType.InterceptorFighter:
                            rResult += r_FighterFPBouns[rProficiency];
                            break;

                        case EquipmentType.SeaplaneBomber:
                            rResult += r_SeaplaneBomberFPBouns[rProficiency];
                            break;
                    }

                    rResult += Math.Sqrt(r_InternalFPBonus[rProficiency] / 10.0);
                }

                rFighterPower += rResult;
            }

            if (rReconnaissancePlane != null && r_Option == AirForceGroupOption.AirDefense)
                switch (rReconnaissancePlane.Type)
                {
                    case EquipmentType.CarrierBasedRecon:
                        if (rReconnaissancePlane.LoS < 8)
                            rFighterPower *= 1.2;
                        else if (rReconnaissancePlane.LoS > 8)
                            rFighterPower *= 1.3;
                        break;

                    default:
                        if (rReconnaissancePlane.LoS < 8)
                            rFighterPower *= 1.1;
                        else if (rReconnaissancePlane.LoS == 8)
                            rFighterPower *= 1.13;
                        else
                            rFighterPower *= 1.16;
                        break;
                }

            FighterPower = (int)rFighterPower;
        }

        internal void UpdateRelocationCountdown()
        {
            AirForceSquadronRelocationCountdown rResult = null;

            foreach (var rSquadron in Squadrons.Values)
            {
                if (rSquadron.State != AirForceSquadronState.Relocating || !rSquadron.Relocation.TimeToComplete.HasValue)
                    continue;

                if (rResult == null || rResult.TimeToComplete.Value < rSquadron.Relocation.TimeToComplete.Value)
                    rResult = rSquadron.Relocation;
            }

            Relocation = rResult;
            OnPropertyChanged(nameof(Relocation));
        }

        internal void UpdateLBASConsumption()
        {
            var rFuelConsumption = 0;
            var rBulletConsumption = 0;

            if (r_Option == AirForceGroupOption.Sortie)
                foreach (var rSquadron in Squadrons.Values)
                {
                    if (rSquadron.State != AirForceSquadronState.Idle)
                        continue;

                    var rIcon = rSquadron.Plane.Info.Icon;
                    var rPlaneCount = rSquadron.Count;

                    if (rPlaneCount == rSquadron.MaxCount)
                    {
                        switch (rIcon)
                        {
                            case EquipmentIconType.LandBasedAttackAircraft:
                                rFuelConsumption += 27;
                                rBulletConsumption += 12;
                                break;

                            case EquipmentIconType.CarrierBasedRecon:
                                rFuelConsumption += 4;
                                rBulletConsumption += 3;
                                break;

                            default:
                                rFuelConsumption += 18;
                                rBulletConsumption += 11;
                                break;
                        }
                    }
                    else
                    {
                        rBulletConsumption += (int)(rPlaneCount * 2 / 3.0);

                        if (rIcon == EquipmentIconType.LandBasedAttackAircraft)
                            rFuelConsumption += (int)(rPlaneCount * 1.5);
                        else
                            rFuelConsumption += rPlaneCount;
                    }
                }

            LBASFuelConsumption = rFuelConsumption;
            LBASBulletConsumption = rBulletConsumption;

            OnPropertyChanged(nameof(LBASFuelConsumption));
            OnPropertyChanged(nameof(LBASBulletConsumption));
        }
    }
}
