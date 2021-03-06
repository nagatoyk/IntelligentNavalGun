﻿using Sakuno.KanColle.Amatsukaze.Game.Models.Raw;

namespace Sakuno.KanColle.Amatsukaze.Game.Models
{
    public class ShipStatus : ModelBase
    {
        Ship r_Ship;

        public bool IsMaximum => FirepowerBase.IsMaximum && TorpedoBase.IsMaximum && AABase.IsMaximum && ArmorBase.IsMaximum;

        public int Firepower => r_Ship.RawData.Firepower[0];
        public ShipModernizationStatus FirepowerBase { get; private set; }

        public int Torpedo => r_Ship.RawData.Torpedo[0];
        public ShipModernizationStatus TorpedoBase { get; private set; }

        public int AA => r_Ship.RawData.AA[0];
        public ShipModernizationStatus AABase { get; private set; }

        public int Armor => r_Ship.RawData.Armor[0];
        public ShipModernizationStatus ArmorBase { get; private set; }

        public int Evasion => r_Ship.RawData.Evasion[0];

        public int ASW => r_Ship.RawData.ASW[0];

        public int LoS => r_Ship.RawData.LoS[0];

        public int Luck => r_Ship.RawData.Luck[0];
        public ShipModernizationStatus LuckBase { get; private set; }

        internal ShipStatus(Ship rpShip)
        {
            r_Ship = rpShip;

            FirepowerBase = new ShipModernizationStatus();
            TorpedoBase = new ShipModernizationStatus();
            AABase = new ShipModernizationStatus();
            ArmorBase = new ShipModernizationStatus();
            LuckBase = new ShipModernizationStatus();
        }

        internal void Update(ShipInfo rpInfo, RawShip rpData)
        {
            FirepowerBase.Update(rpInfo.FirepowerMinimum, rpInfo.FirepowerMaximum, rpData.ModernizedStatus[0]);
            TorpedoBase.Update(rpInfo.TorpedoMinimum, rpInfo.TorpedoMaximum, rpData.ModernizedStatus[1]);
            AABase.Update(rpInfo.AAMinimum, rpInfo.AAMaximum, rpData.ModernizedStatus[2]);
            ArmorBase.Update(rpInfo.ArmorMinimum, rpInfo.ArmorMaximum, rpData.ModernizedStatus[3]);
            LuckBase.Update(rpInfo.LuckMinimum, rpInfo.LuckMaximum, rpData.ModernizedStatus[4]);

            OnPropertyChanged(nameof(Firepower));
            OnPropertyChanged(nameof(Torpedo));
            OnPropertyChanged(nameof(AA));
            OnPropertyChanged(nameof(Armor));
            OnPropertyChanged(nameof(Evasion));
            OnPropertyChanged(nameof(LoS));
            OnPropertyChanged(nameof(ASW));
            OnPropertyChanged(nameof(Luck));
        }
    }
}
