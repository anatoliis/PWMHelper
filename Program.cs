using System;
using System.Windows.Forms;
using igfxDHLib;

namespace PWMHelper
{
    static class MessageString
    {
        public const string InvalidValue = "Invalid value. PMM frequency must be in range of {0}-{1}Hz";
        public const string ErrorReadingData = "Failed to get current PWM: {0:X}";
        public const string ErrorWritingData = "Failed to set PWM to {0}Hz (error {1:X})";
        public const string CurrentFrequency = "Current frequency: {0}";
    }
    
    class Program
    {
        private const int MinFrequency = 200;
        private const int MaxFrequency = 25000;
        private static DataHandler _dh = new DataHandler();
        
        [STAThread]
        public static void Main(string[] args)
        {
            int targetPwmFrequency = ParseArguments(args);
            if (targetPwmFrequency == -1)
                return;
            
            byte[] baseData = ReadDataFromDriver();
            if (baseData == null)
                return;
            
            int currentPwmFrequency = BitConverter.ToInt32(baseData, 4);
            if (targetPwmFrequency == 0)
            {
                ShowCurrentFrequency(currentPwmFrequency);
                return;
            }
            
            if (currentPwmFrequency == targetPwmFrequency)
                return;
            
            SetNewFrequency(baseData, targetPwmFrequency);
        }

        private static int ParseArguments(string[] args)
        {
            if (args.Length == 0)
                return 0;

            int argValue;
            if (!int.TryParse(args[0], out argValue))
                return 0;

            if (argValue < MinFrequency || argValue > MaxFrequency)
            {
                MessageBox.Show(string.Format(MessageString.InvalidValue, MinFrequency, MaxFrequency));
                return -1;
            }

            return argValue;
        }

        private static byte[] ReadDataFromDriver()
        {
            uint error = 0;
            byte[] baseData = new byte[8];
            
            _dh.GetDataFromDriver(ESCAPEDATATYPE_ENUM.GET_SET_PWM_FREQUENCY, 4, ref error, ref baseData[0]);
            if (error != 0)
            {
                MessageBox.Show(string.Format(MessageString.ErrorReadingData, error));
                return null;
            }
            return baseData;
        }

        private static void SetNewFrequency(byte[] baseData, int targetPwmFrequency)
        {
            UpdateBaseDataWithNewFrequency(baseData, targetPwmFrequency);
            uint error = 0;
            
            _dh.SendDataToDriver(ESCAPEDATATYPE_ENUM.GET_SET_PWM_FREQUENCY, 4, ref error, ref baseData[0]);

            if (error != 0)
                MessageBox.Show(string.Format(MessageString.ErrorWritingData, targetPwmFrequency, error));
        }
        
        private static void UpdateBaseDataWithNewFrequency(byte[] baseData, int targetPwmFrequency)
        {
            byte[] b = BitConverter.GetBytes(targetPwmFrequency);
            Array.Copy(b, 0, baseData, 4, 4);
        }

        private static void ShowCurrentFrequency(int currentFrequency)
        {
            MessageBox.Show(string.Format(MessageString.CurrentFrequency, currentFrequency));
        }
    }
}
