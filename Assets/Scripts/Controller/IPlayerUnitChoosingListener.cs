using Model.Config;

namespace Controller
{
    //коммент
    public interface IPlayerUnitChoosingListener
    {
        void OnPlayersUnitChosen(UnitConfig unitConfig);
    }
}