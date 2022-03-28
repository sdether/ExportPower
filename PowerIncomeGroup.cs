using System;
using ColossalFramework.UI;
using UnityEngine;

namespace ExportPower
{
    public class PowerIncomeGroup : UIPanel
    {

        private UIPanel _incomeGroup;
        private UIPanel _incomeLayout;
        private UILabel _incomelabel;
        private double _income;
        private bool _updated;
        private bool _initialized;
        private const float Resize = 4;

        public override void Start()
        {
            TryInitialize();
        }
        
        private void TryInitialize()
        {
            if (_initialized) return;
            
            _incomeLayout = UIView.GetAView()?
                .FindUIComponent<UIPanel>("FullScreenContainer")?
                .Find<UIPanel>("EconomyPanel")?
                .Find<UIPanel>("IncomesExpensesPanel")?
                .Find<UITabContainer>("TabContainer")?
                .Find<UIPanel>("Overview")?
                .Find<UIPanel>("IncomeContainer")?
                .Find<UIPanel>("LayoutPanel");
            if (_incomeLayout == null)
            {
                return;
            }
            var industrialIncomeGroup = _incomeLayout.Find<UIPanel>("IndustrialIncomeGroup");
            var industrialIcon = industrialIncomeGroup.Find<UISprite>("Ind");
            var industrialLabel = industrialIncomeGroup.Find<UILabel>("IncomeIndustrialTotal");
            
            // make room for our new income group
            foreach (var c in _incomeLayout.components)
            {
                c.height -= Resize;
            }
            
            // create new income group, mimicking the industrial group
            _incomeGroup = _incomeLayout.AddUIComponent<UIPanel>();
            _incomeGroup.name = "PowerIncomeGroup";
            _incomeGroup.color = industrialIncomeGroup.color;
            _incomeGroup.size = industrialIncomeGroup.size;
            _incomeGroup.backgroundSprite = industrialIncomeGroup.backgroundSprite;
            _incomeGroup.anchor = UIAnchorStyle.Bottom | UIAnchorStyle.Left | UIAnchorStyle.Right;
            _incomeGroup.tooltip = "Weekly income from selling excess Electrical capacity";
            var icon = _incomeGroup.AddUIComponent<UISprite>();
            icon.spriteName = "InfoIconElectricity";
            icon.name = "Icon";
            icon.size = industrialIcon.size;
            icon.relativePosition = industrialIcon.relativePosition;
            _incomelabel = _incomeGroup.AddUIComponent<UILabel>();
            _incomelabel.autoSize = false;
            _incomelabel.backgroundSprite = industrialLabel.backgroundSprite;
            _incomelabel.text = $"₡{_income:N}";
            _incomelabel.textAlignment = industrialLabel.textAlignment;
            _incomelabel.textColor = industrialLabel.textColor;
            _incomelabel.textScale = industrialLabel.textScale;
            _incomelabel.textScaleMode = industrialLabel.textScaleMode;
            _incomelabel.font = industrialLabel.font;
            _incomelabel.size = industrialLabel.size;
            _incomelabel.padding = industrialLabel.padding;
            _incomelabel.relativePosition = industrialLabel.relativePosition;
            _initialized = true;
        }

        public override void OnDestroy()
        {
            if (!_initialized) return;
            
            Destroy(_incomeGroup);
            foreach (var c in _incomeLayout.components)
            {
                c.height += Resize;
            }
        }

        public void SetIncome(double income)
        {
            TryInitialize();
            _income = income;
            _updated = true;
        }

        private void OnGUI()
        {
            if (!_updated) return;
            _incomelabel.text = $"₡{_income:N}";
            _updated = false;
        }
    }
}