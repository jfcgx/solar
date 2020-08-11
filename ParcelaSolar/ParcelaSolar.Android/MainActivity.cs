using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using CCDStandar;
using System.IO;
using System.Linq;

namespace ParcelaSolar.Droid
{
    [Activity(Label = "ParcelaSolar", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        public static ControladorConsumo ccd;
        protected override void OnCreate(Bundle savedInstanceState)
        {

            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            LoadApplication(new App());



            var rr = Android.OS.Environment.ExternalStorageDirectory;

            var ssdsd = new DirectoryInfo(Path.Combine(rr.AbsolutePath, "Solar"));

            var sds = ssdsd.GetFiles();

            var archivo = sds.First(p => p.Name.Equals("dispositivo.txt"));

            var dispositivos = System.IO.File.ReadAllLines(archivo.FullName).ToList();

            ccd = ControladorConsumo.GetInstance(ssdsd.FullName);
         
            ccd.Inicia(dispositivos);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}