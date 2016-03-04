// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Devices.Spi;
using Windows.Devices.Gpio;
using Windows.Devices.Enumeration;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System.Diagnostics;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409
// I modified the original template to match the requirements of this demo project

namespace TempSensor
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        enum ADCChip  {
            mcp3002, // 2 channel 10 bit
            mcp3202, // 2 channel 12 bit
            mcp3008, // 8 channel 10 bit
            mcp3208  // 8 channel 12 bit
        }
        ADCChip whichADCChip = ADCChip.mcp3008;

        public MainPage()
        {
            this.InitializeComponent();
            this.InitializeComponent();
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Tick += Timer_Tick;
            timer.Start();

            sendDataTimer = new DispatcherTimer();
            sendDataTimer.Interval = TimeSpan.FromMinutes(10);
            sendDataTimer.Tick += SendDataTimer_Tick;
            sendDataTimer.Start();

            whichADCChip = ADCChip.mcp3202;
            switch (whichADCChip)
            {
                case ADCChip.mcp3002:
                    {
                        // To line everything up for ease of reading back (on byte boundary) we 
                        // will pad the command start bit with 1 leading "0" bit

                        // Write 0SGO MNxx xxxx xxxx
                        // Read  ???? ?N98 7654 3210
                        // S = start bit
                        // G = Single / Differential
                        // O = Chanel data 
                        // M = Most significant bit mode 
                        // ? = undefined, ignore
                        // N = 0 "Null bit"
                        // 9-0 = 10 data bits

                        // 0110 1000 = 1 0 pad bit, start bit, Single ended, odd (channel 0), MSFB only, 2 clocking bits
                        // 0000 0000 = 8 clocking bits
                        readBuffer = new byte[2] { 0x00, 0x00 };
                        writeBuffer = new byte[2] { 0x68, 0x00 };
                    }
                    break;
                    // I created this configuration myself using the chip datasheet, so it might not be accurate
                    // Please feel free to share with me any improvement you might find
                case ADCChip.mcp3202:
                    {
                        /* mcp3208 is 12 bits output */
                        // To line everything up for ease of reading back (on byte boundary) we 
                        // will pad the command start bit with 5 leading "0" bits

                        // Write xxxx xxxS GOMx xxxx xxxx xxxx
                        // Read  xxxx xxxx xxxN BA98 7654 3210
                        // S = start bit
                        // G = Single / Differential
                        // O = Chanel data 
                        // M = Most significant bit mode 
                        // ? = undefined, ignore
                        // N = 0 "Null bit"
                        // B-0 = 12 data bits


                        // 0000 0001 = 7 pad bits, start bit
                        // 1010 0000 = single ended, odd (channel 0), MSFB only, 5 clocking bits
                        // 0000 0000 = 8 clocking bits
                        readBuffer = new byte[3] { 0x00, 0x00, 0x00 };
                        writeBuffer = new byte[3] { 0x01, 0xA0, 0x00 };

                    }
                    break;

                case ADCChip.mcp3008:
                    {
                        // To line everything up for ease of reading back (on byte boundary) we 
                        // will pad the command start bit with 7 leading "0" bits

                        // Write 0000 000S GDDD xxxx xxxx xxxx
                        // Read  ???? ???? ???? ?N98 7654 3210
                        // S = start bit
                        // G = Single / Differential
                        // D = Chanel data 
                        // ? = undefined, ignore
                        // N = 0 "Null bit"
                        // 9-0 = 10 data bits

                        // 0000 01 = 7 pad bits, start bit
                        // 1000 0000 = single ended, channel bit 2, channel bit 1, channel bit 0, 4 clocking bits
                        // 0000 0000 = 8 clocking bits
                        readBuffer = new byte[3] { 0x00, 0x00, 0x00 };
                        writeBuffer = new byte[3] { 0x01, 0x80, 0x00 };
                    }
                    break;

                case ADCChip.mcp3208:
                    {
                        /* mcp3208 is 12 bits output */
                        // To line everything up for ease of reading back (on byte boundary) we 
                        // will pad the command start bit with 5 leading "0" bits

                        // Write 0000 0SGD DDxx xxxx xxxx xxxx
                        // Read  ???? ???? ???N BA98 7654 3210
                        // S = start bit
                        // G = Single / Differential
                        // D = Chanel data 
                        // ? = undefined, ignore
                        // N = 0 "Null bit"
                        // B-0 = 12 data bits


                        // 0000 0110 = 5 pad bits, start bit, single ended, channel bit 2
                        // 0000 0000 = channel bit 1, channel bit 0, 6 clocking bits
                        // 0000 0000 = 8 clocking bits
                        readBuffer = new byte[3] { 0x00, 0x00, 0x00 };
                        writeBuffer = new byte[3] { 0x06, 0x00, 0x00 };
                    }
                    break;
            }

            InitSPI();
        }



        private async void InitSPI()
        {
            try
            {
                var settings = new SpiConnectionSettings(SPI_CHIP_SELECT_LINE);
                settings.ClockFrequency = 500000;// 10000000;
                settings.Mode = SpiMode.Mode0; //Mode3;

                string spiAqs = SpiDevice.GetDeviceSelector(SPI_CONTROLLER_NAME);
                var deviceInfo = await DeviceInformation.FindAllAsync(spiAqs);
                SpiDisplay = await SpiDevice.FromIdAsync(deviceInfo[0].Id, settings);
            }

            /* If initialization fails, display the exception and stop running */
            catch (Exception ex)
            {
                throw new Exception("SPI Initialization Failed", ex);
            }
        }

        private async void SendDataTimer_Tick(object sender, object e)
        {
            await SendDataToAzure();
        }
        private void Timer_Tick(object sender, object e)
        {
            DisplayTextBoxContents();
        }
        public void DisplayTextBoxContents()
        {
            SpiDisplay.TransferFullDuplex(writeBuffer, readBuffer);
            res = convertToInt(readBuffer);
            //voltage = ADC_value / 4096 * 5.0 

            //TMP36 is 0.5V at 0 C and 10 mV per degree

            //so...

            //Temp_in_C = (voltage - 0.5) / 0.01

            var voltage = ((double)res / 4096) * 5.0;
            tempInC = (voltage - 0.5) / 0.01;


            textPlaceHolder.Text = String.Format("{0} °C", tempInC.ToString("##.#"));
            Debug.WriteLine(textPlaceHolder.Text);
        }
        public int convertToInt(byte[] data)
        {
            int result = 0;
            switch (whichADCChip)
            {
                case ADCChip.mcp3002:
                    {
                        /*mcp3002 10 bit output*/
                        result = data[0] & 0x03;
                        result <<= 8;
                        result += data[1];
                    }
                    break;
                case ADCChip.mcp3202:
                    {
                        /*mcp3202 12 bit output*/
                        result = data[1] & 0x0F;
                        result <<= 8;
                        result += data[2];
                    }
                    break;
                case ADCChip.mcp3008:
                    {
                        /*mcp3008 10 bit output*/
                        result = data[1] & 0x03;
                        result <<= 8;
                        result += data[2];
                    }
                    break;

                case ADCChip.mcp3208:
                    {
                        /* mcp3208 is 12 bits output */
                         result = data[1] & 0x0F;
                         result <<= 8;
                         result += data[2];
                    }
                    break;
            }

            return result;
        }

        /*RaspBerry Pi2  Parameters*/
        private const string SPI_CONTROLLER_NAME = "SPI0";  /* For Raspberry Pi 2, use SPI0                             */
        private const Int32 SPI_CHIP_SELECT_LINE = 0;       /* Line 0 maps to physical pin number 24 on the Rpi2        */


        byte[] readBuffer = null;                           /* this is defined to hold the output data*/
        byte[] writeBuffer = null;                          /* we will hold the command to send to the chipbuild this in the constructor for the chip we are using */


        private SpiDevice SpiDisplay;

        // create a timer
        private DispatcherTimer timer;
        private DispatcherTimer sendDataTimer;
        int res;
        double tempInC;

        string connStr = "{connection string with shared key for device}"; 
        /// <summary>
        /// Sends data to Azure IoT Hub to later be an input for Stream Analytics
        /// </summary>
        /// <returns></returns>
        private async Task SendDataToAzure()
        {
            DeviceClient deviceClient = DeviceClient.CreateFromConnectionString(connStr, TransportType.Http1);

            var telemetryDataPoint = new
            {
                DeviceId = "KeozPi",
                Temp = tempInC.ToString("##.#"),
                CreatedAt = DateTimeOffset.UtcNow
            };
            var messageString = JsonConvert.SerializeObject(telemetryDataPoint);
            var message = new Message(Encoding.ASCII.GetBytes(messageString));

            try {
                await deviceClient.SendEventAsync(message);
                Debug.WriteLine(String.Concat("sent message: ", messageString));

            }
            catch (Exception ex)
            {
                Debug.WriteLine(String.Concat("error: ", ex.Message));

            }

        }


    }

}
