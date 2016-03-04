using Microsoft.Azure.Devices.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TempSensor.IoTHubTest
{
    class Program
    {
        // Define the connection string to connect to IoT Hub
        private const string DeviceConnectionString = "<replace>";
        static void Main(string[] args)
        {
            // Create the IoT Hub Device Client instance
            DeviceClient deviceClient = DeviceClient.CreateFromConnectionString(DeviceConnectionString);

            // Send an event
            SendEvent(deviceClient).Wait();

            // Receive commands in the queue
            ReceiveCommands(deviceClient).Wait();

            Console.WriteLine("Exited!\n");
        }
        // Create a message and send it to IoT Hub.
        static async Task SendEvent(DeviceClient deviceClient)
        {
            string dataBuffer;
            dataBuffer = Guid.NewGuid().ToString();
            Message eventMessage = new Message(Encoding.UTF8.GetBytes(dataBuffer));
            await deviceClient.SendEventAsync(eventMessage);
        }
        // Receive messages from IoT Hub
        static async Task ReceiveCommands(DeviceClient deviceClient)
        {
            Console.WriteLine("\nDevice waiting for commands from IoTHub...\n");
            Message receivedMessage;
            string messageData;
            while (true)
            {
                receivedMessage = await deviceClient.ReceiveAsync(TimeSpan.FromSeconds(1));

                if (receivedMessage != null)
                {
                    messageData = Encoding.ASCII.GetString(receivedMessage.GetBytes());
                    Console.WriteLine("\t{0}> Received message: {1}", DateTime.Now.ToLocalTime(), messageData);
                    await deviceClient.CompleteAsync(receivedMessage);
                }
            }
        }
    }
}
