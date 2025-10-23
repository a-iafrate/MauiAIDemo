using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace App1
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
         
            // Navigate to AI Chat page by default
            if (NavView.MenuItems.Count > 0)
      {
              NavView.SelectedItem = NavView.MenuItems[0];
         ContentFrame.Navigate(typeof(AiChatPage));
      }
        }

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
   {
            if (args.SelectedItem is NavigationViewItem item)
     {
       var tag = item.Tag?.ToString();
   
                switch (tag)
      {
        case "AiChat":
             ContentFrame.Navigate(typeof(AiChatPage));
                   break;
    case "Ocr":
            ContentFrame.Navigate(typeof(OcrPage));
       break;
   case "Imaging":
 ContentFrame.Navigate(typeof(ImagingPage));
   break;
       }
         }
     }
    }
}
