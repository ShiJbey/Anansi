using System;

namespace Calypso.Relationships
{
    /// <summary>
    /// A single relationships stat such as friendship, romance, or trust
    /// </summary>
    public class RelationshipStat
    {
        #region Fields
        private int _baseValue;
        private int _boost;
        private int _value;
        #endregion

        #region Properties
        /// <summary>
        /// The value of the stat without any boost
        /// </summary>
        public int BaseValue
        {
            get
            {
                return _baseValue;
            }

            set
            {
                _baseValue = value;
                _value = _baseValue + _boost;
                OnValueChanged?.Invoke(_value);
            }
        }
        /// <summary>
        /// The value boost applied to the base to produce the final value
        /// </summary>
        public int Boost
        {
            get
            {
                return _boost;
            }

            set
            {
                _boost = value;
                _value = _baseValue + _boost;
                OnValueChanged?.Invoke(_value);
            }
        }
        /// <summary>
        /// The final value of the stat, including the base and the final value
        /// </summary>
        public int Value => _value;
        #endregion

        #region Actions
        public Action<int> OnValueChanged;
        #endregion

        public RelationshipStat(int baseValue)
        {
            _boost = 0;
            _value = 0;
            BaseValue = baseValue;
        }
    }
}
