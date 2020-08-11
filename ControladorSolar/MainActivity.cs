using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Timers;
using Android;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using CCDStandar;
using Java.Lang;

namespace ControladorSolar
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true, ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : AppCompatActivity
    {
        public static ControladorConsumo ccd;
        public static string loger = string.Empty;
        public static bool ocupado;
        Timer Timer = new Timer(2000);

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            FloatingActionButton fab = FindViewById<FloatingActionButton>(Resource.Id.fab);
            fab.Click += FabOnClick;

            this.Window.SetFlags(WindowManagerFlags.KeepScreenOn, WindowManagerFlags.KeepScreenOn);

           
            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.WriteExternalStorage) != (int)Permission.Granted)
            {
                ActivityCompat.RequestPermissions(this, new string[] { Manifest.Permission.WriteExternalStorage }, 0);
            }

            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.ReadExternalStorage) != (int)Permission.Granted)
            {
                ActivityCompat.RequestPermissions(this, new string[] { Manifest.Permission.ReadExternalStorage }, 0);
            }

            Timer.Elapsed += Timer_Elapsed;
            Timer.Start();

            var rr = Android.OS.Environment.ExternalStorageDirectory;

            var ssdsd = new DirectoryInfo(Path.Combine(rr.AbsolutePath, "Solar"));

            var sds = ssdsd.GetFiles();

            var archivo = sds.First(p => p.Name.Equals("dispositivo.txt"));

            var dispositivos = System.IO.File.ReadAllLines(archivo.FullName).ToList();

            ccd = ControladorConsumo.GetInstance(ssdsd.FullName);
            ccd.LogEvent += Ccd_LogEvent;
            ccd.Inicia(dispositivos);
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            TextView txtHome1 = FindViewById<TextView>(Resource.Id.txtView);
           
            RunOnUiThread(() => { txtHome1.Text = loger;});
        }

        private void Ccd_LogEvent(object sender, LogEventArgs e)
        {
            Console.WriteLine(e.data);
            loger += e.data + System.Environment.NewLine;

            List<string> lineas = loger.Split(System.Environment.NewLine).ToList();

            if (lineas.Count() > 30)
            {
                lineas.RemoveRange(0, lineas.Count() - 30);
            }
            loger = string.Join(System.Environment.NewLine, lineas);
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            if (id == Resource.Id.action_settings)
            {
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        private void FabOnClick(object sender, EventArgs eventArgs)
        {
            View view = (View) sender;
            Snackbar.Make(view, "Replace with your own action", Snackbar.LengthLong)
                .SetAction("Action", (Android.Views.View.IOnClickListener)null).Show();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
	}
}
