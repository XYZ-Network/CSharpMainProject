using System;
using Model.Config;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace View
{
    public class UnitCardView : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private TextMeshProUGUI _costText;
        [SerializeField] private TextMeshProUGUI _name;
        
        private Action<UnitConfig> _onClick;
        private UnitConfig _unitConfig;
        
        public void Initialize(UnitConfig unitConfig, Action<UnitConfig> onClick)
        {
            _unitConfig = unitConfig;
            _icon.sprite = unitConfig.Icon;
            _costText.text = unitConfig.Cost.ToString();
            
            if (_name != null)
                _name.text = unitConfig.Name;
            _onClick = onClick;
        }
        
        public void OnClicked()
        {
            _onClick?.Invoke(_unitConfig);
        }
    }
}