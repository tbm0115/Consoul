using System;

namespace Consoul.Entry
{
    public interface ILineEntry : IEntry
    {
        string Message { get; set; }
    }
}
