using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp2
{
    public partial class FormAdd : Form
    {
        public FormAdd()
        {
            InitializeComponent();

            cmbStatus.Items.Add("运行");
            cmbStatus.Items.Add("停止");
            cmbStatus.Items.Add("故障");
            cmbStatus.SelectedIndex = 0;

            txtTime.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        private void btnDefine_Click(object sender, EventArgs e)
        {
            try
            {
                SqlConnection conn = new SqlConnection("Server=localhost;Database=baseDB;Trusted_Connection=true");

                conn.Open();

                string sql = "INSERT INTO table1 ([设备],[状态],[时间]) VALUES (@设备,@状态,@时间)";

                SqlCommand cmd = new SqlCommand(sql, conn);

                cmd.Parameters.AddWithValue("@设备", txtEquipment.Text);
                cmd.Parameters.AddWithValue("@状态", cmbStatus.Text);
                cmd.Parameters.AddWithValue("@时间", DateTime.Parse(txtTime.Text));

                cmd.ExecuteNonQuery();


                conn.Close();

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("添加失败：" + ex.Message);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
