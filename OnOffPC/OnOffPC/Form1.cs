using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Net;
using System.Windows.Forms;

namespace OnOffPC
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void WakeUp(string macAddress)
        {
            // Конвертируем MAC-адрес в байтовый массив
            byte[] macBytes = ParseMacAddress(macAddress);
            byte[] magicPacket = new byte[102];

            // Пакет состоит из 6 байтов, каждый из которых равен 0xFF
            for (int i = 0; i < 6; i++)
            {
                magicPacket[i] = 0xFF;
            }

            // Затем 16 раз повторяется MAC-адрес
            for (int i = 1; i <= 16; i++)
            {
                Buffer.BlockCopy(macBytes, 0, magicPacket, i * 6, 6);
            }

            // Отправляем магический пакет
            using (UdpClient client = new UdpClient())
            {
                client.Connect(IPAddress.Broadcast, 9);
                client.Send(magicPacket, magicPacket.Length);
            }
        }

        private byte[] ParseMacAddress(string macAddress)
        {
            // Убираем разделители
            string cleanMac = macAddress.Replace(":", "").Replace("-", "");
            byte[] macBytes = new byte[6];

            for (int i = 0; i < 6; i++)
            {
                macBytes[i] = Convert.ToByte(cleanMac.Substring(i * 2, 2), 16);
            }

            return macBytes;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // MAC-адрес удаленного компьютера
            string macAddress = " ";
            WakeUp(macAddress);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                // Адрес удаленного компьютера
                string remoteIpAddress = "192.168.88.223";

                // Параметры для команды shutdown
                string arguments = $"/s /m \\\\{remoteIpAddress} /t 0 /f";

                // Запуск команды shutdown с правами администратора
                ProcessStartInfo processInfo = new ProcessStartInfo("shutdown", arguments)
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true
                };

                using (Process process = Process.Start(processInfo))
                {
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    // Проверка на ошибки
                    if (process.ExitCode != 0)
                    {
                        MessageBox.Show($"Error: {error}", "Shutdown Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        MessageBox.Show("Shutdown command sent successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exception: {ex.Message}", "Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
