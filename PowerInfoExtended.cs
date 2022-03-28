using System;
using ColossalFramework.UI;
using UnityEngine;

namespace ExportPower
{
    public class PowerInfoExtended : UIPanel
    {
        private UILabel _costLabel;
        private UILabel _priceLabel;
        private UILabel _incomeLabel;
        private UILabel _excessLabel;
        private UIPanel _infoPanel;
        private UIPanel _stats;
        private UIPanel _legend;
        private float _offset;
        private double _cost;
        private double _price;
        private int _excess;
        private double _income;
        private bool _updated;
        private bool _initialized;

        private void OnGUI()
        {
            if(!_initialized || !_updated) return;
            _costLabel.text = $"Cost: ₡{_cost:N} / MW";
            _priceLabel.text = $"Market Price: ₡{_price:N} / MW";
            _excessLabel.text = $"Sellable Capacity: {_excess} MW";
            _incomeLabel.text = $"Estimated Income: ₡{_income:N0} per week";
            _updated = false;
        }

        public void SetPowerInfo(double cost, double price, int excess, double income)
        {
            TryInitialize();
            _cost = cost;
            _price = price;
            _excess = excess;
            _income = income;
            _updated = true;
        }
        

        private void resizeInfoPanel(float offset) {

            // capture existing component positions before resizing container
            var list = _infoPanel.GetComponentsInChildren<UIComponent>();
            var existingPosition = new Vector3[list.Length];
            for (var i = 1; i < list.Length; i++)
            {
                existingPosition[i] = list[i].relativePosition;
            }

            // resize container
            _infoPanel.height += offset;
            
            // reposition components to their previous relative positions
            for (var i = 1; i < list.Length; i++)
            {
                list[i].relativePosition = existingPosition[i];
            }

            // resize stats
            _stats.height += offset;
            
            // shift legend
            _legend.relativePosition = new Vector3(
                _legend.relativePosition.x,
                _legend.relativePosition.y + offset,
                _legend.relativePosition.z);
        }

        public override void Start()
        {
            TryInitialize();
        }
        
        private void TryInitialize()
        {
            if (_initialized) return;
        var view = UIView.GetAView();
            _infoPanel = view.FindUIComponent<UIPanel>("(Library) ElectricityInfoViewPanel");
            if (_infoPanel == null) return;
            _stats = _infoPanel.Find<UIPanel>("Panel");
            _legend = _infoPanel.Find<UIPanel>("Legend");
            var production = _stats.Find<UILabel>("Production");
            var labelHeight = production.height + 2;
            _offset = labelHeight * 4;
            resizeInfoPanel(_offset);

            // BUG: these labels show up in a fainter font stroke and color than the one we're
            // trying to mimick and I cannot figure out why :(
            _excessLabel = _stats.AddUIComponent<UILabel>();
            _excessLabel.height = production.height;
            _excessLabel.font = production.font;
            _excessLabel.color = production.color;
            _excessLabel.bottomColor = production.bottomColor;
            _excessLabel.textColor = production.textColor;
            _excessLabel.textAlignment = production.textAlignment;
            _excessLabel.textScale = production.textScale;
            _excessLabel.textScaleMode = production.textScaleMode;
            _excessLabel.name = "Excess";
            _excessLabel.relativePosition = new Vector3(
                production.relativePosition.x,
                production.relativePosition.y + labelHeight,
                production.relativePosition.z
            );

            _costLabel = _stats.AddUIComponent<UILabel>();
            _costLabel.height = production.height;
            _costLabel.font = production.font;
            _costLabel.textColor = production.textColor;
            _costLabel.opacity = production.opacity;
            _costLabel.textAlignment = production.textAlignment;
            _costLabel.textScale = production.textScale;
            _costLabel.textScaleMode = production.textScaleMode;
            _costLabel.name = "Cost";
            _costLabel.relativePosition = new Vector3(
                _excessLabel.relativePosition.x,
                _excessLabel.relativePosition.y + labelHeight,
                _excessLabel.relativePosition.z
            );

            _priceLabel = _stats.AddUIComponent<UILabel>();
            _priceLabel.height = production.height;
            _priceLabel.font = production.font;
            _priceLabel.textColor = production.textColor;
            _priceLabel.textAlignment = production.textAlignment;
            _priceLabel.textScale = production.textScale;
            _priceLabel.textScaleMode = production.textScaleMode;
            _priceLabel.name = "Price";
            _priceLabel.relativePosition = new Vector3(
                _costLabel.relativePosition.x,
                _costLabel.relativePosition.y + labelHeight,
                _costLabel.relativePosition.z
            );

            _incomeLabel = _stats.AddUIComponent<UILabel>();
            _incomeLabel.height = production.height;
            _incomeLabel.font = production.font;
            _incomeLabel.textColor = production.textColor;
            _incomeLabel.textAlignment = production.textAlignment;
            _incomeLabel.textScale = production.textScale;
            _incomeLabel.textScaleMode = production.textScaleMode;
            _incomeLabel.name = "Income";
            _incomeLabel.relativePosition = new Vector3(
                _priceLabel.relativePosition.x,
                _priceLabel.relativePosition.y + labelHeight,
                _priceLabel.relativePosition.z
            );
            _initialized = true;
        }

        public override void OnDestroy()
        {
            if (!_initialized) return;
            Destroy(_costLabel);
            Destroy(_priceLabel);
            Destroy(_incomeLabel);
            Destroy(_excessLabel);
            resizeInfoPanel(-_offset);
        }
    }
}