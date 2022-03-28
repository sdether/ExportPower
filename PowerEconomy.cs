using System.Collections.Generic;
using ICities;
using UnityEngine;

namespace ExportPower
{
    public class PowerEconomy : EconomyExtensionBase
    {
        private static readonly Logger logger = new Logger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public override void OnReleased()
        {
            logger.Log($"released economy");
            PowerManager.TryDispose();
            base.OnReleased();
        }
        
        public override long OnUpdateMoneyAmount(long internalMoneyAmount)
        {
            PowerManager.Instance.CalculateIncome();
            return base.OnUpdateMoneyAmount(internalMoneyAmount);
        }

    }
}