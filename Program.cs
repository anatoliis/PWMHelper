using System;
using System.Windows.Forms;
using igfxDHLib;

namespace PWMHelper
{
    class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            DataHandler dh = new igfxDHLib.DataHandler();
            int targetPwmFrequency = 2000;
            
            int frequencyFromArgument = ParseArguments(args);
            if (frequencyFromArgument != 0)
                targetPwmFrequency = frequencyFromArgument;
            
            byte[] baseData = GetDataFromDriver(dh);
            int unknowValue = BitConverter.ToInt32(baseData, 0);
            int currentPwmFrequency = BitConverter.ToInt32(baseData, 4);
            
            if (currentPwmFrequency == targetPwmFrequency)
                return;
            
            SetNewFrequency(dh, baseData, targetPwmFrequency);
        }

        private static int ParseArguments(string[] args)
        {
            if (args.Length == 0)
                return 0;

            int argValue;
            if (!int.TryParse(args[0], out argValue))
                return 0;

            if (argValue < 200 || argValue > 25000)
            {
                MessageBox.Show("Invalid value. PMM frequency must be in range of 200-25000Hz");
                return 0;  
            }

            return argValue;
        }

        private static byte[] GetDataFromDriver(DataHandler dh)
        {
            uint error = 0;
            byte[] data = new byte[8];
            
            dh.GetDataFromDriver(ESCAPEDATATYPE_ENUM.GET_SET_PWM_FREQUENCY, 4, ref error, ref data[0]);
            if (error != 0)
            {
                MessageBox.Show(string.Format("Failed to get current PWM: {0:X}", error));
                return null;
            }
            return data;
        }

        private static void SetNewFrequency(DataHandler dh, byte[] baseData, int targetPwmFrequency)
        {
            UpdateBaseDataWithNewFrequency(baseData, targetPwmFrequency);
            uint error = 0;
            
            dh.SendDataToDriver(ESCAPEDATATYPE_ENUM.GET_SET_PWM_FREQUENCY, 4, ref error, ref baseData[0]);

            if (error != 0)
                MessageBox.Show(string.Format("Failed to set PWM to {0}Hz (error {1:X})", targetPwmFrequency, error));
        }
        
        private static void UpdateBaseDataWithNewFrequency(byte[] baseData, int targetPwmFrequency)
        {
            byte[] b = BitConverter.GetBytes(targetPwmFrequency);
            Array.Copy(b, 0, baseData, 4, 4);
        }
    }
}
