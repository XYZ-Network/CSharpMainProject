using Model.Config;

namespace Controller
{
    public interface IPlayerUnitChoosingListener
    {
        void OnPlayersUnitChosen(UnitConfig unitConfig);
    }
}