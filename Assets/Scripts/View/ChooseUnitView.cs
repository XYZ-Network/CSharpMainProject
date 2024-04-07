using System;
using Controller;
using Model;
using Model.Config;
using TMPro;
using UnityEngine;
using Utilities;

namespace View
{
    public class ChooseUnitView : MonoBehaviour
    {
        [SerializeField] private RectTransform _root;
        [SerializeField] private RectTransform _unitListParent;
        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private TextMeshProUGUI _balanceText;
        [SerializeField] private UnitCardView _unitCardPrefab;

        private IReadOnlyRuntimeModel _model;
        
        private void Start()
        {
            _model = ServiceLocator.Get<IReadOnlyRuntimeModel>();
            SetupCards(ServiceLocator.Get<Settings>());
        }
        
        private void Update()
        {
            var visible = _model.Stage == RuntimeModel.GameStage.ChooseUnit;
            if (visible != _root.gameObject.activeSelf)
                _root.gameObject.SetActive(visible);

            if (visible)
            {
                _balanceText.text = $"Balance: {_model.RoMoney[RuntimeModel.PlayerId]}";
                _levelText.text = $"Level: {_model.Level}";
            }
        }

        private void SetupCards(Settings settings)
        {
            foreach (var unitConfig in settings.PlayerUnits.Keys)
            {
                var card = Instantiate(_unitCardPrefab, _unitListParent);
                card.Initialize(unitConfig, OnUnitChosen);
            }
        }

        private void OnUnitChosen(UnitConfig unit)
        {
            ServiceLocator.Get<IPlayerUnitChoosingListener>().OnPlayersUnitChosen(unit);
        }
    }
}