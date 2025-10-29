using Model.Config;

namespace Controller
//Пробую загружать скрип в GitHub
{
    public interface IPlayerUnitChoosingListener
    {
        void OnPlayersUnitChosen(UnitConfig unitConfig);
    }
}