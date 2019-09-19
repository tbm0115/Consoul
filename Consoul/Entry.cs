using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Consoul
{
    public interface ILineEntry
    {
        string Message { get; set; }
        ConsoleColor Color { get; set; }
    }
    public class LineEntry:ILineEntry
    {
        public string Message { get; set; }
        public ConsoleColor Color { get; set; } = ConsoleColor.White;

        public LineEntry(string message, ConsoleColor color = ConsoleColor.White)
        {
            Message = message;
            Color = color;
        }
    }
    public class DynamicLineEntry<T> : ILineEntry
    {
        public string Message { get; set; }
        public ConsoleColor Color { get; set; } = ConsoleColor.White;

        public DynamicLineEntry(string message, ConsoleColor color = ConsoleColor.White)
        {
            Message = message;
            Color = color;
        }
    }
    public class ViewOption : ILineEntry
    {
        public string Message { get; set; }
        public ConsoleColor Color { get; set; } = ConsoleColor.White;
        public Expression<Func<object>> Action { get; set; }

        public ViewOption(string message, Expression<Func<object>> action, ConsoleColor color = ConsoleColor.White)
        {
            Message = message;
            Color = color;
            Action = action;
        }


    }
}
