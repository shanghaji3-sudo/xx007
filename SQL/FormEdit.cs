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
    public partial class FormEdit : Form
    {

        private int id;

        public FormEdit(int id, string equipment, string status, string time)
        {
            InitializeComponent();

            this.id = id;

            cmbStatus.Items.Add("运行");
            cmbStatus.Items.Add("停止");
            cmbStatus.Items.Add("故障");

            txtEquipment.Text = equipment;
            cmbStatus.Text = status;
            txtTime.Text = time;
        }

        private void btnDefine_Click(object sender, EventArgs e)
        {
            try
            {
                string equipment = txtEquipment.Text;
                string status = cmbStatus.Text;

                DateTime time;
                if (!DateTime.TryParse(txtTime.Text, out time))
                {
                    MessageBox.Show("时间格式不正确");
                    return;
                }

                SqlConnection conn = new SqlConnection("Server=localhost;Database=baseDB;Trusted_Connection=true");
                conn.Open();
                string sql = "UPDATE table1 SET [设备]=@设备, [状态]=@状态, [时间]=@时间 WHERE Id=@Id";
                SqlCommand cmd = new SqlCommand(sql, conn);

                cmd.Parameters.AddWithValue("@设备", equipment);
                cmd.Parameters.AddWithValue("@状态", status);
                cmd.Parameters.AddWithValue("@时间", time);
                cmd.Parameters.AddWithValue("@Id", id);

                cmd.ExecuteNonQuery();
                conn.Close();
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                this.Close();
            }

        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
