using System;
using Types.record;
using UnityEngine;

namespace ScriptableObject.PickUpItem
{
    [CreateAssetMenu(fileName = "RewardKeeperConfig", menuName = "Scriptable Objects/RewardKeeperConfig")]
    public class RewardKeeperConfig : UnityEngine.ScriptableObject, IHaveDataForSave
    {
        [SerializeField] int _money;
        [SerializeField] int _diamond;
        [SerializeField] int _energy;

        Action<IHaveDataForSave> _saveAction;
        event Action<int> OnMoneyChanged;
        event Action<int> OnDiamondChanged;
        event Action<int> OnEnergyChanged;

        void IHaveDataForSave.SaveToSaveData(SaveData saveData)
        {
            saveData.Money = _money;
            saveData.Diamond = _diamond;
            saveData.Energy = _energy;
        }

        void IHaveDataForSave.LoadSaveData(SaveData saveData)
        {
            _money = saveData.Money;
            _diamond = saveData.Diamond;
            _energy = saveData.Energy;

            OnMoneyChanged?.Invoke(_money);
            OnDiamondChanged?.Invoke(_diamond);
            OnEnergyChanged?.Invoke(_energy);
        }

        Action IHaveDataForSave.SubscribeWithUnsubscribe(Action<IHaveDataForSave> saveAction)
        {
            _saveAction = saveAction;
            return () => _saveAction = null;
        }

        public int GetMoney() => _money;
        public int GetDiamond() => _diamond;
        public int GetEnergy() => _energy;

        public void IncreaseMoney(int value) => ChangeValue(ref _money, value, OnMoneyChanged);
        public void DecreaseMoney(int value) => ChangeValue(ref _money, -value, OnMoneyChanged);

        public void IncreaseDiamond(int value) => ChangeValue(ref _diamond, value, OnDiamondChanged);
        public void DecreaseDiamond(int value) => ChangeValue(ref _diamond, -value, OnDiamondChanged);

        public void IncreaseEnergy(int value) => ChangeValue(ref _energy, value, OnEnergyChanged);
        public void DecreaseEnergy(int value) => ChangeValue(ref _energy, -value, OnEnergyChanged);

        /// <summary>
        /// Changes a value, ensuring it remains within the valid range of int and not negative.
        /// </summary>
        /// <param name="field">The field to modify (passed by reference).</param>
        /// <param name="delta">The amount to change (positive or negative).</param>
        /// <param name="onValueChanged">Event to invoke if the value changes.</param>
        private void ChangeValue(ref int field, int delta, Action<int> onValueChanged)
        {
            int oldValue = field;

            if (delta > 0)
            {
                field = Mathf.Clamp(field + delta, 0, int.MaxValue);
            }
            else
            {
                field = Mathf.Max(0, field + delta);
            }

            if (field != oldValue)
            {
                _saveAction?.Invoke(this);
                onValueChanged?.Invoke(field);
            }
        }

        public void SubscribeToOnMoneyChangedEvent(Action<int> action) => OnMoneyChanged += action;
        public void UnsubscribeToOnMoneyChangedEvent(Action<int> action) => OnMoneyChanged -= action;

        public void SubscribeToOnDiamondChangedEvent(Action<int> action) => OnDiamondChanged += action;
        public void UnsubscribeToOnDiamondChangedEvent(Action<int> action) => OnDiamondChanged -= action;

        public void SubscribeToOnEnergyChangedEvent(Action<int> action) => OnEnergyChanged += action;
        public void UnsubscribeToOnEnergyChangedEvent(Action<int> action) => OnEnergyChanged -= action;
    }
}
