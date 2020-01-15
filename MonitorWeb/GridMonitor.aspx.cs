using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace MonitorWeb
{

    public partial class GridMonitor : System.Web.UI.Page
    {
        
        protected void Page_Load(object sender, EventArgs e)
        {

                BoardRepeater.DataSource = Application["BoardDatabase"];
                BoardRepeater.DataBind();

            if (!IsPostBack)
            {

                Button1.Text = Global._estadoB1 ? "ON" : "OFF";
                Button2.Text =Global._estadoB2 ? "ON" : "OFF";
                Button3.Text = Global._estadoB3 ? "ON" : "OFF";
                Button4.Text = Global._manual ? "ON" : "OFF";
                string re = string.Empty;
                try
                {
                     re = Global.measureControl.Resumen();
                    var rel = re.Split(';');
                    if (rel[0] != string.Empty)
                    {
                        double pro = double.Parse(rel[9]) + double.Parse(rel[16]) / 10;
                        double exp = double.Parse(rel[10]) + double.Parse(rel[15]);
                        double imp= double.Parse(rel[11]) + double.Parse(rel[14]);
                        double fac = exp + imp; // double.Parse(rel[12])  ;
                        double total = imp + pro;  // double.Parse(rel[13]);

                        //Promedio,Max, Producción, Exportado, Importado, TotalFactura, Total, PromedioActual,MaxActual, ProducciónActual, ExportadoActual, ImportadoActual, TotalFacturaActual, TotalActual
                        //_todayImportkWh, _todayExportkWh, _inverter.Inverter_stat.kw_day
                        lblPromedio.Text = rel[0];
                        max.Text = rel[1];
                        produccion.Text = rel[2];
                        lblExportado.Text = rel[3];
                        lblImportado.Text = rel[4];
                        Facturado.Text = rel[5];
                        Total.Text = rel[6];

                        lblPromedioActual.Text = rel[7];
                        maxActual.Text = rel[8];
                        produccionActual.Text = string.Format("{0:0.00}",pro);
                        lblExportadoActual.Text = string.Format("{0:0.00}", exp);
                        lblImportadoActual.Text = string.Format("{0:0.00}", imp);
                        FacturadoActual.Text = string.Format("{0:0.00}", fac);
                        TotalActual.Text = string.Format("{0:0.00}", total);
                    }
                }
                catch (Exception ex)
                {

                    lblPromedio.Text = re;

                    TotalActual.Text = ex.Message;
                }
            }
        }

        protected void Button1_Click(object sender, EventArgs e)
        {

            Global.measureControl.SetEstado("LedRio", !Global._estadoB1);
            Button1.Text = !Global._estadoB1 ? "ON" : "OFF"; ;


        }

        protected void Button2_Click(object sender, EventArgs e)
        {

            Global.measureControl.SetEstado("Bomba02", !Global._estadoB2);
            Button2.Text = !Global._estadoB2 ? "ON" : "OFF";
        }

        protected void Button3_Click(object sender, EventArgs e)
        {

            Global.measureControl.SetEstado("Filtro", !Global._estadoB3);
            Button3.Text = !Global._estadoB3 ? "ON" : "OFF";
        }

        protected void Button4_Click(object sender, EventArgs e)
        {
            Global._manual = Global.measureControl.Manual(!Global._manual);
            Button4.Text = Global._manual ? "ON" : "OFF";
        }
    }
}