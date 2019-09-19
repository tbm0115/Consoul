using Consoul.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Consoul.Views;

namespace Consoul.Test.Views
{
    public class Welcome : StaticView
    {
        public Story Story { get; set; }

        public Welcome() : base()
        {
            string message = "Welcome to the cavern of secrets!";
            int padding = 5;
            Title = $"{String.Join("", Enumerable.Repeat("*", (padding * 2) + message.Length))}\r\n" +
                $"{String.Join("", Enumerable.Repeat(" ", padding))}" + 
                $"{message}" +
                $"{String.Join("", Enumerable.Repeat(" ", padding))}\r\n" + 
                $"{String.Join("", Enumerable.Repeat("*", (padding * 2) + message.Length))}\r\n\r\nWould you like to begin?";

        }

        [ViewOption("Yes, let's start!")]
        public void Yes()
        {
            Story = new Story();
            Story.Progress(typeof(PickUpStick));
        }

    }
    
}
