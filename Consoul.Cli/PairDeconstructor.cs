using System;

namespace ConsoulLibrary
{
    /// <summary>
    /// Provides custom parsing for dictionary and lookup option values.
    /// </summary>
    /// <typeparam name="TKey">The key type produced by the deconstructor.</typeparam>
    /// <typeparam name="TValue">The value type produced by the deconstructor.</typeparam>
    public abstract class PairDeconstructor<TKey, TValue> : IPairDeconstructor
    {
        /// <summary>
        /// Attempts to split a single option value into a key and value pair.
        /// </summary>
        /// <param name="input">The raw option value.</param>
        /// <param name="key">The parsed key.</param>
        /// <param name="value">The parsed value.</param>
        /// <returns><c>true</c> when the input could be parsed; otherwise, <c>false</c>.</returns>
        public abstract bool TryDeconstruct(string input, out TKey key, out TValue value);

        Type IPairDeconstructor.KeyType => typeof(TKey);

        Type IPairDeconstructor.ValueType => typeof(TValue);

        bool IPairDeconstructor.TryDeconstruct(string input, out object key, out object value)
        {
            TKey typedKey;
            TValue typedValue;
            bool result = TryDeconstruct(input, out typedKey, out typedValue);
            key = typedKey;
            value = typedValue;
            return result;
        }
    }
}
