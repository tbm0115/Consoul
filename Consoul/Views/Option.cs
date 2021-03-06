﻿using ConsoulLibrary.Entry;
using System;
using System.Linq.Expressions;

namespace ConsoulLibrary.Views
{
    public class Option : IOption
    {
        public LineEntry Entry { get; set; }

        public OptionAction Action { get; set; }

        public ConsoleColor Color { get; set; }

        public Option(string message, OptionAction action, ConsoleColor? color = null)
        {
            if (color == null)
                color = RenderOptions.DefaultColor;
            Entry = new LineEntry(message, (ConsoleColor)color);
            Action = action;
            Color = Entry.Color;
        }

        public string BuildMessage(Expression<Func<string, string>> template = null)
        {
            if (template != null)
            {
                return template.Compile().Invoke(Entry.Message);
            }
            return Entry.Message;
        }
    }
}
