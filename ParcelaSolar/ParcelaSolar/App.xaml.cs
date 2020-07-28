using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ParcelaSolar.Services;
using ParcelaSolar.Views;
using CCDStandar;
using System.IO;
using System.Collections.Generic;

namespace ParcelaSolar
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();

            DependencyService.Register<MockDataStore>();
            MainPage = new MainPage();
        }

        protected override void OnStart()
        {

        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
