using System;

namespace ConsoulLibrary
{
    internal interface IPairDeconstructor
    {
        Type KeyType { get; }

        Type ValueType { get; }

        bool TryDeconstruct(string input, out object key, out object value);
    }
}
