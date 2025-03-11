using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using EasyModbus;

namespace pj_01
{
    public partial class Form1: Form
    {
        private ModbusClient _modbusClient;
        private int registerAddress = 0;
        private int slaveId = 1;
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (_modbusClient != null && _modbusClient.Connected)
            {
                try
                {
                    _modbusClient.WriteSingleRegister(registerAddress, 0);

                    _modbusClient.Disconnect();
                    _modbusClient = null;

                    this.Invoke((MethodInvoker)delegate
                    {
                        button1.Text = "Connect";
                        button2.Enabled = false;
                        textBox1.Enabled = false;
                        textBox2.Enabled = false;
                        label1.Enabled = false;
                        textBox1.Clear();
                        textBox2.Clear();
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi ngắt kết nối: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return;
            }

            if (comboBox1.SelectedItem == null)
            {
                MessageBox.Show("Chưa chọn cổng COM!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string selectedPort = comboBox1.SelectedItem.ToString();
            try
            {
                _modbusClient = new ModbusClient(selectedPort)
                {
                    Baudrate = 9600,
                    Parity = Parity.None,
                    StopBits = StopBits.One
                };
                _modbusClient.Connect();

                this.Invoke((MethodInvoker)delegate
                {
                    button1.Text = "Disconnect";
                    button2.Enabled = true;
                    textBox1.Enabled = true;
                    textBox2.Enabled = true;
                    label1.Enabled = true;
                });

                MessageBox.Show("Đã kết nối với " + selectedPort, "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kết nối: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                if (!int.TryParse(textBox1.Text, out int valueToWrite))
                {
                    MessageBox.Show("Giá trị nhập không hợp lệ! Vui lòng nhập một số nguyên.", "Lỗi nhập liệu", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                _modbusClient.UnitIdentifier = (byte)slaveId;
                _modbusClient.WriteSingleRegister(registerAddress, valueToWrite);

                ReadDataReceive();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi gửi dữ liệu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void ReadDataReceive()
        {
            try
            {
                if (_modbusClient != null && _modbusClient.Connected)
                {
                    int[] res = _modbusClient.ReadHoldingRegisters(registerAddress, 1);
                    byte[] responseData = _modbusClient.receiveData;
                    string rawDataHex = BitConverter.ToString(responseData).Replace("-", " ");

                    this.Invoke((MethodInvoker)delegate
                    {
                        textBox2.Clear();
                        textBox2.AppendText($"Nhận: {rawDataHex}{Environment.NewLine}");
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi đọc dữ liệu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            button2.Enabled = false;
            textBox1.Enabled = false;
            textBox2.Enabled = false;
            label1.Enabled = false;
            comboBox1.Items.AddRange(SerialPort.GetPortNames());
            if (comboBox1.Items.Count > 0)
            {
                comboBox1.SelectedIndex = 0; 
            }
        }
    }
}
