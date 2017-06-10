using System;
using System.Threading.Tasks;

using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Common.Exceptions;
using System.Diagnostics;


namespace RegistroDispositivo
{
    class Program
    {
        static RegistryManager registryManager;
        static string connectionString = "tobechanged";

        static void Main(string[] args)
        {
            registryManager = RegistryManager.CreateFromConnectionString(connectionString);

            AddDeviceAsync().Wait();
        }

        private static async Task AddDeviceAsync()
        {
            string deviceId = "raspwintfm";
            Device device;
            try
            {
                device = await registryManager.AddDeviceAsync(new Device(deviceId));

                Debug.WriteLine("Ok: Generated device key: {0}", device.Authentication.SymmetricKey.PrimaryKey);
            }
            catch (DeviceAlreadyExistsException)
            {
                Debug.WriteLine("Error Existe");
                device = await registryManager.GetDeviceAsync(deviceId);
            }
            catch (Exception e)
            {
                Debug.WriteLine("ERROR: " + e.Message);
            }            
        }
    }
}
