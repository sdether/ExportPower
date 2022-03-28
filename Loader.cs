using System;
using ColossalFramework;
using ICities;
using UnityEngine;

namespace ExportPower
{
    public class Loader : ILoadingExtension
    {
        private static Logger logger = new Logger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public void OnCreated(ILoading loading)
        {
            logger.Log($"OnCreated - mode: {loading.currentMode}, complete: {loading.loadingComplete}");
            if (loading.currentMode == AppMode.Game && loading.loadingComplete)
            {
                PowerManager.Ensure();
            }
        }

        public void OnReleased()
        {
            logger.Log("OnReleased");
            PowerManager.TryDispose();
        }

        public void OnLevelLoaded(LoadMode mode)
        {
            logger.Log($"OnLevelLoaded - mode: {mode}");
            PowerManager.Ensure();
        }

        public void OnLevelUnloading()
        {
            logger.Log("OnLevelUnloading");
            PowerManager.TryDispose();
        }
    }
}