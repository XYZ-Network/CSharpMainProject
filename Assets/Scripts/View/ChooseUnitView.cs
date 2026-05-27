using Controller;
using Model;
using Model.Config;
using Model.Runtime.ReadOnly;
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
        private bool _cardsCreated;

        private void Start()
        {
            TryInitialize();
        }

        private void Update()
        {
            if (!TryInitialize())
                return;

            bool visible = _model.Stage == RuntimeModel.GameStage.ChooseUnit;

            if (_root != null && visible != _root.gameObject.activeSelf)
                _root.gameObject.SetActive(visible);

            if (!visible)
                return;

            if (_balanceText != null)
                _balanceText.text = $"Balance: {_model.RoMoney[RuntimeModel.PlayerId]}";

            if (_levelText != null)
                _levelText.text = $"Level: {_model.Level}";
        }

        private bool TryInitialize()
        {
            if (_model == null)
                _model = ServiceLocator.Get<IReadOnlyRuntimeModel>();

            if (_model == null)
                return false;

            if (!_cardsCreated)
            {
                Settings settings = ServiceLocator.Get<Settings>();

                if (settings == null)
                    return false;

                SetupCards(settings);
                _cardsCreated = true;
            }

            return true;
        }

        private void SetupCards(Settings settings)
        {
            if (_unitCardPrefab == null || _unitListParent == null)
                return;

            foreach (var unitConfig in settings.PlayerUnits.Keys)
            {
                var card = Instantiate(_unitCardPrefab, _unitListParent);
                card.Initialize(unitConfig, OnUnitChosen);
            }
        }

        private void OnUnitChosen(UnitConfig unit)
        {
            var listener = ServiceLocator.Get<IPlayerUnitChoosingListener>();

            if (listener == null)
                return;

            listener.OnPlayersUnitChosen(unit);
        }
    }
}