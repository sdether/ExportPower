using ColossalFramework;
using UnityEngine;

namespace ExportPower
{
    public class PowerPlant
    {
        private static readonly Logger logger = new Logger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ushort _buildingID;

        public static PowerPlant FromBuildingID(ushort buildingID)
        {
            var building = Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingID];
            if ((building.m_flags & Building.Flags.Created) == 0) return null;
            var info = building.Info;
            if (info.GetService() != ItemClass.Service.Electricity) return null;
            var powerplantAI = info.m_buildingAI as PowerPlantAI;
            return powerplantAI != null ? new PowerPlant(buildingID) : null;
        }

        private static PowerPlantAI GetAI(Building building)
        {
            return building.Info.m_buildingAI as PowerPlantAI;
        }

        private PowerPlant(ushort buildingID)
        {
            _buildingID = buildingID;
        }

        public override string ToString()
        {
            return $"{GetBuilding().Info.name}[{_buildingID}]";
        }

        private Building GetBuilding()
        {
            return Singleton<BuildingManager>.instance.m_buildings.m_buffer[_buildingID];
        }

        public void Inspect()
        {
            var building = GetBuilding();
            var ai = GetAI(building);
            var rate = ai.GetElectricityRate(_buildingID, ref building);
            var budget = ai.GetBudget(_buildingID, ref building);
            var maintenance = building.Info.GetMaintenanceCost();
            var info = Info;
            logger.Log($"[{this}] " +
                       $"active: {info.IsActive}, " +
                       $"production rate: {rate} ({info.Production}KW), " +
                       $"maintenance cost: {maintenance} ({info.MaintenanceCost}), " +
                       $"budget: {budget}");
        }

        public PowerPlantInfo Info
        {
            get
            {
                var building = GetBuilding();
                var isActive = (building.m_flags & (Building.Flags.Evacuating | Building.Flags.Active)) ==
                               Building.Flags.Active;
                if (!isActive)
                {
                    return new PowerPlantInfo(false, 0, 0);
                }

                var ai = GetAI(building);
                var rate = ai.GetElectricityRate(_buildingID, ref building);
                var budget = ai.GetBudget(_buildingID, ref building);
                var standardMaintenance = building.Info.GetMaintenanceCost();
                var budgetedMaintenance = (int) (standardMaintenance * budget / 100.0 / 6.25);
                var productionRate = (int) (rate * 1000 / 62.5);
                return new PowerPlantInfo(true, budgetedMaintenance, productionRate);
            }
        }

        public readonly struct PowerPlantInfo
        {
            public PowerPlantInfo(bool isActive, int maintenanceCost, int production)
            {
                IsActive = isActive;
                MaintenanceCost = maintenanceCost;
                Production = production;
            }

            public bool IsActive { get; }
            public int MaintenanceCost { get; }
            public int Production { get; }
        }
    }
}