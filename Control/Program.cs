using CCD;
using System;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace Control
{
    class Program
    {
        public static MeasureControl process;
        public static ServiceHost host;


        static void Main(string[] args)
        {
            process = MeasureControl.GetInstance();
            process.Inicia();
            IniciaWS();
        }
        public static void IniciaWS()
        {
            try
            {
                Uri baseAddress = new Uri(string.Format("http://{0}:2480/{1}", "127.0.0.1", "grid"));


                host = new ServiceHost(typeof(MeasureControl), baseAddress);

                var basicHttpBinding = new BasicHttpBinding();
                basicHttpBinding.MaxReceivedMessageSize = 200000;
                ServiceMetadataBehavior smb = new ServiceMetadataBehavior();
                smb.HttpGetEnabled = true;
                smb.MetadataExporter.PolicyVersion = PolicyVersion.Policy15;

                host.Description.Behaviors.Add(smb);
                host.AddServiceEndpoint(typeof(IMeasureControl), basicHttpBinding, baseAddress);

                // Open the ServiceHost to start listening for messages. Since
                // no endpoints are explicitly configured, the runtime will create
                // one endpoint per base address for each service contract implemented
                // by the service.
                host.Open();

                Console.WriteLine("The service is ready at {0}", baseAddress);
                Console.WriteLine("Press <Enter> to stop the service.");
                Console.ReadLine();

            }
            catch (Exception ex)
            {

                Console.WriteLine(ex);
            }
            // Close the ServiceHost.
            //host.Close();

        }
    }
}
