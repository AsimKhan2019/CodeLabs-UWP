using System;
using Windows.UI.Xaml.Controls;

namespace Microsoft.Labs.SightsToSee.Models
{
    /// <summary>
    /// Data to represent an item in the nav menu.
    /// </summary>
    public class NavMenuItem
    {
        public string Label { get; set; }
        public char SymbolAsChar { get; set; }
        public Type DestPage { get; set; }
        public object Arguments { get; set; }
    }
}
