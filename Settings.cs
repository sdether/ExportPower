using System.IO;
using System.Xml.Serialization;

namespace ExportPower
{
    public class Settings
    {

        public struct Data
        {
            public float PremiumFactor;
            public float DiscountFactor;
            public bool Debug;
        }

        private const string SettingsFile = "export-power-settings.xml";
        
        public static Settings Load()
        {
            if (File.Exists(SettingsFile))
            {
                TextReader reader = null;
                try
                {
                    var serializer = new XmlSerializer(typeof(Data));
                    reader = new StreamReader(SettingsFile);
                    return new Settings((Data) serializer.Deserialize(reader));
                }
                finally
                {
                    if (reader != null)
                        reader.Close();
                }
            }

            return new Settings(new Data
            {
                PremiumFactor = 1.5f,
                DiscountFactor = 0.8f,
                Debug = false
            }).Save();
        }
        
        private static readonly Logger logger =
            new Logger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private Data _data;

        public float PremiumFactor
        {
            get => _data.PremiumFactor;
            set
            {
                _data.PremiumFactor = value;
                logger.Log($"PremiumFactor: {_data.PremiumFactor}");
                Save();
            }
        }

        public float DiscountFactor
        {
            get => _data.DiscountFactor;
            set
            {
                _data.DiscountFactor = value;
                logger.Log($"DiscountFactor: {_data.DiscountFactor}");
                Save();
            }
        }

        public bool Debug
        {
            get => _data.Debug;
            set
            {
                _data.Debug = value;
                UnityEngine.Debug.Log($"[ExportPower.Setting] Debug: {_data.Debug}");
                Save();
            }
        }

        private Settings(Data data)
        {
            _data = data;
        }

        public Settings Save()
        {
            TextWriter writer = null;
            try
            {
                var serializer = new XmlSerializer(typeof(Data));
                writer = new StreamWriter(SettingsFile, false);
                serializer.Serialize(writer, _data);
            }
            finally
            {
                if (writer != null)
                    writer.Close();
            }
            return this;
        }
    }
}