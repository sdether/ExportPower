using System;
using System.Collections.Generic;
using System.Linq;
using ColossalFramework;
using ColossalFramework.UI;
using ICities;
using UnityEngine;
using UEObject = UnityEngine.Object;

namespace ExportPower
{
    public class PowerManager : MonoBehaviour, IDisposable
    {
        private static readonly Logger logger =
            new Logger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static PowerManager _instance;

        public static PowerManager Instance
        {
            get
            {
                if (_instance != null) return _instance;
                var previousInstance = FindObjectOfType<PowerManager>();
                if (previousInstance != null)
                {
                    Destroy(previousInstance);
                }

                var gameObject = new GameObject(nameof(PowerManager));
                _instance = gameObject.AddComponent<PowerManager>();
                logger.Log("created PowerManager");

                return _instance;
            }
        }

        public static bool Exists => _instance != null;

        public static void Ensure()
        {
            if (Singleton<BuildingManager>.exists)
            {
                Instance.Initialize();
            }
        }

        public static void TryDispose()
        {
            if (_instance != null)
            {
                _instance.Dispose();
            }
        }

        private bool _isDisposed;
        readonly BuildingManager _buildingManager = Singleton<BuildingManager>.instance;
        private readonly Dictionary<ushort, PowerPlant> _powerplants = new Dictionary<ushort, PowerPlant>();
        private IManagers _managers => (IManagers) Singleton<SimulationManager>.instance.m_ManagersWrapper;
        private DateTime _periodStart = DateTime.MinValue;
        private const int HoursPerWeek = 24 * 7;
        private PowerInfoExtended _powerInfoExtended;
        private PowerIncomeGroup _powerIncomeGroup;
        public readonly Settings Settings;

        public PowerManager()
        {
            logger.Log($"Created PowerManager [{GetType().Assembly.FullName}]");
            Settings = Settings.Load();
            _buildingManager.EventBuildingCreated += HandleBuildingCreated;
            _buildingManager.EventBuildingReleased += HandleBuildingReleased;
            Initialize();
        }

        private void Initialize()
        {
            LoadPowerPlants();
            Inspect();

        }
        private void HandleBuildingCreated(ushort buildingID)
        {
            AddPowerPlant(buildingID);
        }

        private void HandleBuildingReleased(ushort buildingID)
        {
            if (!_powerplants.TryGetValue(buildingID, out var powerPlant)) return;
            _powerplants.Remove(buildingID);
            logger.Log($"Removed {powerPlant}");
        }

        public void LoadPowerPlants()
        {
            logger.Log("checking for power plants");
            if (_buildingManager.m_buildings == null) return;
            _powerplants.Clear();
            for (ushort buildingID = 0; buildingID < _buildingManager.m_buildings.m_size; buildingID++)
            {
                AddPowerPlant(buildingID);
            }
        }

        private PowerPlant AddPowerPlant(ushort buildingID)
        {
            var powerplant = PowerPlant.FromBuildingID(buildingID);
            if (powerplant == null)
            {
                return null;
            }

            logger.Log($"Added {powerplant}");
            _powerplants.Add(buildingID, powerplant);
            return powerplant;
        }

        public void Inspect()
        {
            foreach (var powerplant in _powerplants.Values)
            {
                powerplant.Inspect();
            }

            var info = Info;
            var actualCapacityMW = (info.ActualCapacity / 1000.0);
            var potentialCapacityMW = info.PotentialCapacity / 1000.0;
            var consumptionMW = info.Consumption / 1000.0;
            var maintenance = info.Cost / 100.0;
            logger.Log($"cost: ${maintenance}, " +
                       $"capacity: {actualCapacityMW}/{potentialCapacityMW}, " +
                       $"consumption: {consumptionMW}");
        }

        public PowerInfo Info
        {
            get
            {
                var powerplantCapacity = 0;
                var maintenanceCost = 0;
                // Note: Rider wants to refactor this into LINQ, but that makes
                // Cities Skylines unhappy, so it's staying as an old skool foreach
                foreach (var powerplant in _powerplants.Values)
                {
                    var info = powerplant.Info;
                    powerplantCapacity += info.Production;
                    maintenanceCost += info.MaintenanceCost;
                }

                var DMinstance = Singleton<DistrictManager>.instance;
                var dmArray = DMinstance.m_districts;
                var d = dmArray.m_buffer[0];
                var capacity = d.GetElectricityCapacity();
                var consumption = d.GetElectricityConsumption();
                return new PowerInfo(powerplantCapacity, capacity, consumption, maintenanceCost);
            }
        }

        private bool CtrlCmdDown =>
            Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) ||
            Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.RightCommand);

        private bool ShiftDown => Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        private void Update()
        {
            if (_isDisposed || !Settings.Debug)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.P) && this.CtrlCmdDown && this.ShiftDown && Singleton<EconomyManager>.exists)
            {
                Inspect();
            }

            if (Input.GetKeyDown(KeyCode.D) && this.CtrlCmdDown && this.ShiftDown && Singleton<EconomyManager>.exists)
            {
                Dispose();
            }
        }

        private void Start()
        {
            logger.Log("Started PowerManager");
            var dummy = new GameObject("DummyContainer");
            _powerInfoExtended = dummy.AddComponent<PowerInfoExtended>();
            _powerIncomeGroup = dummy.AddComponent<PowerIncomeGroup>();
        }

        public void Dispose()
        {
            logger.Log($"disposing");
            Destroy(this);
            _isDisposed = true;
            _buildingManager.EventBuildingCreated -= HandleBuildingCreated;
            _buildingManager.EventBuildingReleased -= HandleBuildingReleased;
            if (_instance == this)
            {
                _instance = null;
            }
        }

        private void OnDestroy()
        {
            logger.Log("destroying");
            Destroy(_powerInfoExtended);
            Destroy(_powerIncomeGroup);
        }

        public void CalculateIncome()
        {
            if (_periodStart == DateTime.MinValue)
            {
                _periodStart = _managers.threading.simulationTime;
                logger.Log("starting first pay period");
            }
            else
            {
                var now = _managers.threading.simulationTime;
                var elapsed = now - _periodStart;
                if (elapsed.TotalHours < 1) return;
                var fraction = elapsed.TotalHours / HoursPerWeek;
                var info = Info;
                if (info.HasExcessCapacity)
                {
                    var weeklyIncome = (int) (info.ExcessCapacity * info.PricePerUnit);
                    var periodIncome = (int) (weeklyIncome * fraction);
                    if (periodIncome > 0)
                    {
                        if (_powerInfoExtended != null)
                        {
                            _powerInfoExtended.SetPowerInfo(
                                info.CostPerUnit * 10,
                                info.PricePerUnit * 10,
                                info.ExcessCapacity / 1000,
                                weeklyIncome / 100.0
                            );
                        }

                        // This shows up under income, but not in one of the breakouts.
                        // The Economy panel reflects the most recent pay out. I.e. income is immediately added to
                        // the wallet and stays visible in the Economy panel for a week thereafter.
                        Singleton<EconomyManager>.instance.AddResource(
                            EconomyManager.Resource.PublicIncome,
                            periodIncome,
                            ItemClass.Service.Electricity,
                            ItemClass.SubService.None,
                            ItemClass.Level.None
                        );
                        long income;
                        long expense;
                        Singleton<EconomyManager>.instance.GetIncomeAndExpenses(
                            ItemClass.Service.Electricity,
                            ItemClass.SubService.None,
                            ItemClass.Level.None,
                            out income,
                            out expense);
                        if (_powerIncomeGroup != null)
                        {
                            _powerIncomeGroup.SetIncome(income / 100.0);
                        }

                        logger.Log($"Cost: ${info.CostPerUnit}/u, Market Price: ${info.PricePerUnit}/u, " +
                                   $"Income: ${weeklyIncome / 100.0} weekly / " +
                                   $"${income / 100.0} accumulated / " +
                                   $"${periodIncome / 100.0} period, " +
                                   $"period hours: {elapsed.TotalHours}," +
                                   $" week fraction: {fraction}");
                    }
                }

                _periodStart = now;
            }
        }

        public readonly struct PowerInfo
        {
            public PowerInfo(int potentialCapacity, int actualCapacity, int consumption, int cost)
            {
                PotentialCapacity = potentialCapacity;
                ActualCapacity = actualCapacity;
                Consumption = consumption;
                Cost = cost;
            }

            public int PotentialCapacity { get; }
            public int ActualCapacity { get; }
            public int Consumption { get; }
            public int Cost { get; }

            public int ExcessCapacity => Math.Max(0, ActualCapacity - Consumption);

            public bool HasExcessCapacity => ExcessCapacity > 0;

            public double CostPerUnit => Cost / (double) PotentialCapacity;

            public double PricePerUnit
            {
                get
                {
                    var excessCapacity = ExcessCapacity;
                    double premiumFactor = Instance.Settings.PremiumFactor;
                    double discountFactor = Instance.Settings.DiscountFactor;
                    var premium = Math.Min(Consumption, excessCapacity);
                    var atCost = Math.Min(2 * Consumption, excessCapacity - premium);
                    var discount = excessCapacity - premium - atCost;
                    var priceFactor = (premium * premiumFactor + atCost + discount * discountFactor) / excessCapacity;
                    return CostPerUnit * priceFactor;
                }
            }
        }
    }
}