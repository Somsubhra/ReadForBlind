using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using ReadForBlind.Resources;

namespace ReadForBlind
{
    public partial class MainPage : PhoneApplicationPage
    {
        Listener listener;
        // Constructor
        public MainPage()
        {
            InitializeComponent();
            listener = new Listener();
            // Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();
        }

        private async void recog(object sender, RoutedEventArgs e)
        {
            String result = await listener.Listen();
            if(result != null)
                MessageBox.Show(result);
        }

        
    }
}