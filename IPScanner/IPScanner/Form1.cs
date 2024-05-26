using System;
using System.Net;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace IPScanner
{
    public partial class Form1 : Form
    {
        [DllImport("iphlpapi.dll", ExactSpelling = true)]
        private static extern int SendARP(int destIP, int srcIP, byte[] pMacAddr, ref uint phyAddrLen);

        [DllImport("Ws2_32.dll")]
        private static extern int inet_addr(string cp);

        public Form1()
        {
            InitializeComponent();

            dataGridView1.ColumnCount = 2;
            dataGridView1.Columns[0].Name = "IP-адрес";
            dataGridView1.Columns[1].Name = "MAC-адрес";
        }

        private void ScanNetwork(IPAddress startIP, IPAddress endIP)
        {
            label1.Text = "Загрузка...";
            byte[] startBytes = startIP.GetAddressBytes();
            byte[] endBytes = endIP.GetAddressBytes();  

            // Сканирование IP-адресов в диапазоне
            while (CompareIPAddresses(startBytes, endBytes) <= 0)
            {
                IPAddress currentIP = new IPAddress(startBytes);
                try
                {
                    string mac = GetMacAddress(currentIP);
                    if (!string.IsNullOrEmpty(mac))
                    {
                        dataGridView1.Rows.Add(currentIP.ToString(), mac);
                    }
                }
                catch (Exception ex)
                {
                    // Обработка исключений
                    MessageBox.Show($"Не удалось получить MAC-адрес для IP: {currentIP}\nОшибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                // Увеличение последнего байта IP-адреса
                IncrementIPAddress(startBytes);
            }
        }

        private int CompareIPAddresses(byte[] ip1, byte[] ip2)
        {
            for (int i = 0; i < ip1.Length; i++)
            {
                int result = ip1[i].CompareTo(ip2[i]);
                if (result != 0)
                {
                    return result;
                }
            }
            return 0;
        }

        private void IncrementIPAddress(byte[] ip)
        {
            for (int i = ip.Length - 1; i >= 0; i--)
            {
                if (ip[i] < 255)
                {
                    ip[i]++;
                    break;
                }
                else
                {
                    ip[i] = 0;
                }
            }
        }

        private string GetMacAddress(IPAddress ip)
        {
            byte[] macAddr = new byte[6];
            uint macAddrLen = (uint)macAddr.Length;

            int result = SendARP(BitConverter.ToInt32(ip.GetAddressBytes(), 0), 0, macAddr, ref macAddrLen);

            //if (result != 0)
            //{
            //    throw new InvalidOperationException("ARP запрос не удался.");
            //}

            string[] str = new string[(int)macAddrLen];
            for (int i = 0; i < macAddrLen; i++)
            {
                str[i] = macAddr[i].ToString("x2");
            }

            return string.Join(":", str);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string startIP = textBox1.Text;
            string endIP = textBox2.Text;

            if (IPAddress.TryParse(startIP, out IPAddress startIPAddress) &&
                IPAddress.TryParse(endIP, out IPAddress endIPAddress))
            {
                ScanNetwork(startIPAddress, endIPAddress);
            }
            else
            {
                MessageBox.Show("Введите корректные IP-адреса.");
            }
            label1.Text = "Готово";
        }
    }
}
