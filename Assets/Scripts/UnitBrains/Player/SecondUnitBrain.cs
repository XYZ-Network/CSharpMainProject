using System;
using System.Collections.Generic;
using System.Linq;
using Model;
using Model.Runtime.Projectiles;
using UnityEngine;
using Utilities;

namespace UnitBrains.Player
{
    public class SecondUnitBrain : DefaultPlayerUnitBrain
    {
        public override string TargetUnitName => "Cobra Commando";
        private const float OverheatTemperature = 3f;
        private const float OverheatCooldown = 2f;
        private float _temperature = 0f;
        private float _cooldownTime = 0f;
        private bool _overheated;

        #region ---------------------------------------- ОПАСНЫЕ ЦЕЛИ ----------------------------------------

        // ДЗ 6 - "a. Создаем новое поле для хранения целей, к которым нужно идти, но которые вне зоны досягаемости."
        private List<Vector2Int> PolSpTseli_SamyeOpasnye = new List<Vector2Int>();

        #endregion --------------------------------------------------------------------------------

        #region ---------------------------------------- МЕТОД "GenerateProjectiles" ----------------------------------------

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;
            ///////////////////////////////////////
            // Homework 1.3 (1st block, 3rd module)
            ///////////////////////////////////////           

            /*/
            Versiya 2 (nachalo)
            /*/
            
            int PTemperatura = GetTemperature(); // Uznat' temperaturu (iz metoda) i zapisat' ee (v peremennuyu).

            if (PTemperatura >= OverheatTemperature) return; // Proverit', peregrelos' li oruzhie.

            for (int i = 0; i <= PTemperatura; i++) // Ot kolichestva tsiklov zavisit kolichestvo snaryadov v odnom vystrele.
            {
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
            }

            IncreaseTemperature(); // Posle kazhdogo vystrela uvelichit' temperaturu.

            // Debug.Log($"Temperatura pered vystrelom: {PTemperatura}. Kolichestvo snaryadov v vystrele: {PTemperatura + 1}");
            
            /*/
            Versiya 2 (konets)
            /*/

            /*/
            Versiya 1 (nachalo)
            /*/

            /*/
            GetTemperature();

            if (_temperature > OverheatTemperature)
            {
                Debug.Log("_temperature > OverheatTemperature (ostyvanie).");
            }
            
            else
            {
                int PNomerVystrela;

                if (_temperature < 1)
                {
                    PNomerVystrela = 1;
                }
                else if (_temperature < 2)
                {
                    PNomerVystrela = 2;
                }
                else
                {
                    PNomerVystrela = 3;
                }

                void MVyletOdnogoSnaryada()
                {
                    var projectile = CreateProjectile(forTarget);
                    AddProjectileToList(projectile, intoList);
                }

                switch (PNomerVystrela)
                {
                    case 1:
                        Debug.Log("Temperatura pered vystrelom:");
                        Debug.Log(_temperature);
                        Debug.Log("Kolichestvo snaryadov v vystrele: 1.");
                        MVyletOdnogoSnaryada();
                        IncreaseTemperature();
                        Debug.Log("Temperatura posle vystrela:");
                        Debug.Log(_temperature);
                        Debug.Log("---------------------------------------------------------------");
                        break;
                    case 2:
                        Debug.Log("Temperatura pered vystrelom:");
                        Debug.Log(_temperature);
                        Debug.Log("Kolichestvo snaryadov v vystrele: 2.");
                        MVyletOdnogoSnaryada();
                        MVyletOdnogoSnaryada();
                        IncreaseTemperature();
                        Debug.Log("Temperatura posle vystrela:");
                        Debug.Log(_temperature);
                        Debug.Log("---------------------------------------------------------------");
                        break;
                    case 3:
                        Debug.Log("Temperatura pered vystrelom:");
                        Debug.Log(_temperature);
                        Debug.Log("Kolichestvo snaryadov v vystrele: 3.");
                        MVyletOdnogoSnaryada();
                        MVyletOdnogoSnaryada();
                        MVyletOdnogoSnaryada();
                        IncreaseTemperature();
                        Debug.Log("Temperatura posle vystrela:");
                        Debug.Log(_temperature);
                        Debug.Log("---------------------------------------------------------------");
                        break;  
                    default:
                        Debug.Log("Default (vystrela net).");
                        break;
                }                
            }
            /*/

            /*/
            Versiya 1 (konets)
            /*/

            ///////////////////////////////////////
        }

        #endregion --------------------------------------------------------------------------------

        #region ---------------------------------- МЕТОД "Получение цели из списка опасных целей" ----------------------------------
        public override Vector2Int GetNextStep()
        {
            /*/
            ДЗ 6 - "4. В методе GetNextStep() нужно описать получение цели из списка целей. Если целей там нет или цель
            в области атаки, нужно вернуть позицию юнита. Метод уже написан, просто измени реализацию, удалив заглушку
            return base.GetNextStep()."
            /*/
            // return base.GetNextStep();
            Vector2Int v = PolSpTseli_SamyeOpasnye.Count > 0 ? PolSpTseli_SamyeOpasnye[0] : unit.Pos;
            
            // ДЗ 6 - "5. Если цель есть, но вне области атаки, в GetNextStep() вызвать у текущей позиции метод
            // CalcNextStepTowards(), передав туда цель. Он рассчитает, куда идти, чтобы достигнуть цели."
            if (IsTargetInRange(v))
            {
                return unit.Pos;
            }
            else
            {
                return unit.Pos.CalcNextStepTowards(v);
            }
        }

        #endregion --------------------------------------------------------------------------------
        #region ---------------------------------------- МЕТОД "SelectTargets" ----------------------------------------

        // ДЗ 7 - Начало.
        // "a. Создай статическое поле, равное 0 для выдачи номеров - это счетчик."
        private static int _pVydachaNomerov_Schetchik = 0;

        // "b. Создай поле с номером юнита.", "2. Номер юнита стоит задать сразу при инициализации.",
        // "Присвой ему текущий номер статичного счетчика и сразу счетчик инкрементируй для следующего юнита."
        private int _pYunit_Nomer = _pVydachaNomerov_Schetchik++;
        
        // "c. Создай константу - поле, по которому будет рассматриваться максимум целей для умного выбора. Присвой полю значение 3."
        private const int _pUmnyjVybor_Maksimum = 3;
        

        protected override List<Vector2Int> SelectTargets()
        {
            ///////////////////////////////////////
            // Homework 1.4 (1st block, 4rd module)
            ///////////////////////////////////////

            #region ---------------------------------------- 1. ПОЛУЧИТЬ ВСЕ ЦЕЛИ ----------------------------------------

            // ДЗ 6 - Начало.
            // List<Vector2Int> result = GetReachableTargets();
            // ДЗ 6 - "1. Вместо достижимых целей получи все с помощью метода GetAllTargets()".
            // Отзыв к ДЗ 7 - "a. Для начала произведем поиск целей. Очистим список текущих целей, добавленный в прошлом
            // домашнем задании."
            //List<Vector2Int> result = new List<Vector2Int>();
            List<Vector2Int> result = new ();
            // Отзыв к ДЗ 6 - "Например, вот этот блок можно в принципе исключить:"
            /*/
            foreach (Vector2Int v in GetAllTargets())
            {
                result.Add(v);
            }
            /*/

            #endregion --------------------------------------------------------------------------------
            #region ---------------------------------- 2. ОПРЕДЕЛИТЬ БЛИЖАЙШУЮ ЦЕЛЬ ИЗ ВСЕХ ----------------------------------

            // ДЗ 4 - Начало.
            // ДЗ 4 - "1. Определи какая из целей в result находится ближе всего к нашей базе. Используй подход, который мы разобрали в уроке «Подготовка к домашнему заданию»".
            // Для отзыва к ДЗ 7.
            //float pTekuscheeNaimensheeRasstoyanie = float.MaxValue;
            // Для отзыва к ДЗ 7.
            //Vector2Int pBlizhajshayaTsel = Vector2Int.zero;

            // Отзыв к ДЗ 7 - "Пройдемся по всем целям и добавим их заново в очищенный список."
            PolSpTseli_SamyeOpasnye.Clear();

            // Отзыв к ДЗ 6 - "Для этого, мы прямо вот тут можем вызвать метод GetAllTargets():"
            // foreach (Vector2Int i in result)
            foreach (Vector2Int i in GetAllTargets())
            {
                // ДЗ 4. 2. Для определения расстояния от конкретной цели до нашей базы, используй метод DistanceToOwnBase. Ты не увидишь его реализации в этом скрипте, но не волнуйся, это не помешает тебе его вызвать. Этот метод принимает цель, расстояние от которой до базы мы хотим узнать, а возвращает как раз это расстояние.
                // Для отзыва к ДЗ 7.
                /*/
                float pRasstoyanieDoBazy = DistanceToOwnBase(i);
                if (pRasstoyanieDoBazy < pTekuscheeNaimensheeRasstoyanie)
                {
                    pTekuscheeNaimensheeRasstoyanie = pRasstoyanieDoBazy;
                    pBlizhajshayaTsel = i;
                }
                /*/
                PolSpTseli_SamyeOpasnye.Add(i);
            }
            // Debug.Log(pTekuscheeNaimensheeRasstoyanie);

            // Отзыв к ДЗ 7 - "b. Если в списке целей никого нет, то добавляем в список базу противника."
            if (PolSpTseli_SamyeOpasnye.Count == 0)
            {
                PolSpTseli_SamyeOpasnye.Add(runtimeModel.RoMap.Bases[IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId]);
            }

            // Отзыв к ДЗ 7 - "c. Производим сортировку целей по дистанции. Для этого вызовем метод сортировки
            // SortByDistanceToOwnBase(List) и передадим туда список целей."
            SortByDistanceToOwnBase(PolSpTseli_SamyeOpasnye);

            // Отзыв к ДЗ 7 - "d. Рассчитаем номер текущего юнита и определим, цель под каким номером следует бить. Например,
            // если цель одна, а номер юнита 3, цель выбирается первая (т.е. 0)."
            int _pTsel_Nomer = _pYunit_Nomer % _pUmnyjVybor_Maksimum;
            int bestTargetNum = Mathf.Min(_pTsel_Nomer, PolSpTseli_SamyeOpasnye.Count - 1);
            Vector2Int pTselDlyaYunita = PolSpTseli_SamyeOpasnye[bestTargetNum];

            // Отзыв к ДЗ 7 - "e. Проверяем, что цель в радиусе атаки и добавляем в результат, если это так."
            if (IsTargetInRange(pTselDlyaYunita))
            {
                result.Add(pTselDlyaYunita);
                Debug.Log("_________________________ Ближайше-опасная цель выбрана.");
            }

                #endregion --------------------------------------------------------------------------------
                #region ---------------------------- 3. БЛИЖАЙШУЮ ЦЕЛЬ ДОБАВИТЬ В СПИСОК ОПАСНЫХ ЦЕЛЕЙ ----------------------------

                // ДЗ 6 - "b. Записываем самую опасную цель в эту коллекцию."
                // Для отзыва к ДЗ 7.
                /*/
                PolSpTseli_SamyeOpasnye.Clear();
                PolSpTseli_SamyeOpasnye.Add(pBlizhajshayaTsel);
                Debug.Log("_________________________ Ближайшая цель добавлена.");
                /*/

                #endregion --------------------------------------------------------------------------------

                /*/
                // ДЗ 4 - "3. После того как ты найдешь ближайшую к базе цель, в том случае если она действительно была найдена, очисти список result и добавь в него эту цель".
                // if (pBlizhajshayaTsel != Vector2Int.zero)
                // Отзыв к ДЗ 4 - "Тут вы выполняете проверку условием и лучше его задать так: if (pTekuscheeNaimensheeRasstoyanie < float.MaxValue). Так мы точно избавимся от ситуации, что цель не была найдена по какой-то причине и добавим в лист требуемый результат".
                if (pTekuscheeNaimensheeRasstoyanie < float.MaxValue)
                {
                    Debug.Log($"_________________________ Координаты ближайшей цели: {pBlizhajshayaTsel}.");
                    result.Clear();
                    result.Add(pBlizhajshayaTsel);
                    Debug.Log("_________________________ Ближайшая цель добавлена.");
                }
                /*/

                // Для отзыва к ДЗ 7.
                /*/
                if (PolSpTseli_SamyeOpasnye.Count > 0)
                {
                    #region ------------------------------------ 4. ОТСОРТИРОВАТЬ ОПАСНЫЕ ЦЕЛИ ------------------------------------

                    // ДЗ 7 - "c. Производим сортировку целей по дистанции.
                    // "Для этого вызовем метод сортировки SortByDistanceToOwnBase(List) и передадим туда список целей."
                    SortByDistanceToOwnBase(PolSpTseli_SamyeOpasnye);

                    #endregion --------------------------------------------------------------------------------
                    #region ------------------------------------ 5. РАССЧИТАТЬ И ОПРЕДЕЛИТЬ ------------------------------------

                    // ДЗ 7
                    // "d. Рассчитаем номер текущего юнита и определим, цель под каким номером следует бить."
                    // Debug.Log($"_________________________ {_pYunit_Nomer}.");
                    var v = PolSpTseli_SamyeOpasnye.Take(_pUmnyjVybor_Maksimum);
                    int _pTsel_Nomer = _pYunit_Nomer % v.Count();
                    Vector2Int pTselDlyaYunita = PolSpTseli_SamyeOpasnye[_pTsel_Nomer];

                    #endregion --------------------------------------------------------------------------------
                    #region ------------------------------ 6. ОПРЕДЕЛИТЬ ДОСЯГАЕМОСТЬ БЛИЖАЙШЕЙ ЦЕЛИ ------------------------------
                    #endregion --------------------------------------------------------------------------------

                    // ДЗ 6 - "Если цель в зоне досягаемости, то добавляем в result."
                    //if (IsTargetInRange(PolSpTseli_SamyeOpasnye[0]))

                    // ДЗ 7
                    if (IsTargetInRange(pTselDlyaYunita))
                    {
                        #region ------------------------------ 7. ВЕРНУТЬ ДОСЯГАЕМУЮ БЛИЖАЙШУЮ ЦЕЛЬ ------------------------------

                        // Отзыв к ДЗ 6 - "Соответственно, после этого нам не придется выполнять операцию очистки листа result, вот тут:"
                        // result.Clear();
                        //result.Add(PolSpTseli_SamyeOpasnye[0]);

                        // ДЗ 7
                        result.Add(pTselDlyaYunita);

                        Debug.Log("_________________________ Ближайше-опасная цель выбрана.");

                        #endregion --------------------------------------------------------------------------------
                    }
                }
                /*/
                // Для отзыва к ДЗ 7.
                /*/
                else
                {
                    #region ----------------------------- 8. ПРИ ОТСУТСТВИИ ЦЕЛЕЙ ВЕРНУТЬ БАЗУ ПРОТИВНИКА -----------------------------

                    // ДЗ 6 - "3. Если целей нет, добавляем в цели базу противника."
                    result.Add(runtimeModel.RoMap.Bases[IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId]);

                    #endregion --------------------------------------------------------------------------------
                }
                /*/

                /*/
                while (result.Count > 1)
                {
                    result.RemoveAt(result.Count - 1);
                }
                /*/

                // ДЗ 4 - "4. Верни список result".
                return result;
            // ДЗ 4 - Конец.
            // ДЗ 6 - Конец.
            // ДЗ 7 - Конец.

            ///////////////////////////////////////
        }

        #endregion --------------------------------------------------------------------------------

        public override void Update(float deltaTime, float time)
        {
            if (_overheated)
            {
                _cooldownTime += Time.deltaTime;
                float t = _cooldownTime / (OverheatCooldown / 10);
                _temperature = Mathf.Lerp(OverheatTemperature, 0, t);
                if (t >= 1)
                {
                    _cooldownTime = 0;
                    _overheated = false;
                }
            }
        }

        private int GetTemperature()
        {
            if (_overheated) return (int)OverheatTemperature;
            else return (int)_temperature;
        }

        private void IncreaseTemperature()
        {
            _temperature += 1f;
            if (_temperature >= OverheatTemperature) _overheated = true;
        }
    }
}