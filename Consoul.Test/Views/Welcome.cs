﻿using Consoul.Attributes;
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
            Title = (new Entry.BannerEntry("Welcome to the cavern of secrets!")).Message + "\r\nWould you like to begin";
        }

        [ViewOption("Yes, let's start!")]
        public void Yes()
        {
            Story = new Story();
            Story.Progress(typeof(PickUpStick));
        }

    }
    
}