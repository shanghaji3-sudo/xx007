using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.Sql;
using System.Data.SqlClient;
using Excel = Microsoft.Office.Interop.Excel;

namespace WindowsFormsApp2
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            btnConnnect.Enabled = true;
            btnDisconnect.Enabled = false;
            btnRefresh.Enabled = false;
            btnAdd.Enabled = false;
            btnDel.Enabled = false;
            btnEdit.Enabled = false;

            cmbStatus.Items.Add("全部");
            cmbStatus.Items.Add("运行");
            cmbStatus.Items.Add("停止");
            cmbStatus.Items.Add("故障");
            cmbStatus.SelectedIndex = 0;
        }

        SqlConnection conn = new SqlConnection();
        private void SQLConnect()
        {
            try
            {
                conn.ConnectionString = "Server=localhost;Database=baseDB;Trusted_Connection=true";
                conn.Open();
            }
            catch (Exception ex)
            {
                txtLog.AppendText("连接失败" + ex.Message + "\r\n");
            }
        }

        private void SQLRefresh()
        {
            SqlCommand cmd = conn.CreateCommand();
            cmd.Connection = conn;
            cmd.CommandText = "Select*from table1;";

            SqlDataAdapter adapter = new SqlDataAdapter();
            adapter.SelectCommand = cmd;

            DataSet ds = new DataSet();
            adapter.Fill(ds);

            dgv.DataSource = ds.Tables[0];
            dgv.Columns["设备"].Width = 134;
            dgv.Columns["状态"].Width = 134;
            dgv.Columns["时间"].Width = 214;


            dgv.Columns["Id"].Visible = false;
        }

        private void btnConnnect_Click(object sender, EventArgs e)
        {
            SQLConnect();

            if (conn.State != ConnectionState.Open)
                return;

            SQLRefresh();
            txtLog.AppendText("连接成功\r\n");

            btnConnnect.Enabled = false;
            btnDisconnect.Enabled = true;
            btnRefresh.Enabled = true;
            btnAdd.Enabled = true;
            btnDel.Enabled = true;
            btnEdit.Enabled = true;
        }

        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            try
            {
                conn.Close();
                txtLog.AppendText("已断开连接" + "\r\n");
                btnConnnect.Enabled = true;
                btnDisconnect.Enabled = false;
                btnRefresh.Enabled = false;
                btnAdd.Enabled = false;
                btnDel.Enabled = false;
                btnEdit.Enabled = false;
            }
            catch (Exception ex)
            {
                txtLog.AppendText(ex.Message + "\r\n");
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            SQLRefresh();
            txtLog.AppendText("已刷新" + "\r\n");
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            FormAdd form = new FormAdd();
            if (form.ShowDialog() == DialogResult.OK)
            {
                SQLRefresh();
                txtLog.AppendText("添加成功\r\n");
            }
        }

        private void btnDel_Click(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0)
            {
                txtLog.AppendText("请先选择要删除的那行数据");
                return;
            }

            int id = Convert.ToInt32(dgv.SelectedRows[0].Cells["Id"].Value);
            DialogResult result = MessageBox.Show("确定删除吗？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result != DialogResult.Yes)
            {
                return;
            }
            else
            {
                try
                {
                    string sql = "DELETE FROM table1 WHERE Id=@Id";

                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@Id", id);

                    cmd.ExecuteNonQuery();

                    SQLRefresh();

                    txtLog.AppendText("删除成功，数据已刷新\r\n");
                }
                catch (Exception ex)
                {
                    txtLog.AppendText(ex.Message);
                }
            }

        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0)
            {
                txtLog.AppendText("请先选择要修改的那行数据");
                return;
            }
            DataGridViewRow row = dgv.SelectedRows[0];
            int id = Convert.ToInt32(row.Cells["Id"].Value);

            string equipment = row.Cells["设备"].Value.ToString();
            string status = row.Cells["状态"].Value.ToString();
            string time = Convert.ToDateTime(row.Cells["时间"].Value).ToString("yyyy-MM-dd HH:mm:ss");

            FormEdit form = new FormEdit(id, equipment, status, time);
            if (form.ShowDialog() == DialogResult.OK)
            {
                SQLRefresh();
                txtLog.AppendText("修改成功\r\n");
            }
        }

        private void btnCheck_Click(object sender, EventArgs e)
        {
            try
            {
                string sql = "SELECT * FROM table1 WHERE 1=1";

                if (conn.State != ConnectionState.Open)
                {
                    MessageBox.Show("请先连接数据库！");
                    return;
                }

                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;

                if (txtEquipment.Text != "")
                {
                    sql += " AND [设备] LIKE @name";
                    cmd.Parameters.AddWithValue("@name", "%" + txtEquipment.Text + "%");
                }
                if (cmbStatus.Text != "" && cmbStatus.Text != "全部")
                {
                    sql += " AND [状态] = @status";
                    cmd.Parameters.AddWithValue("@status", cmbStatus.Text);
                }

                cmd.CommandText = sql;

                DataTable dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);

                dgv.DataSource = dt;
                txtLog.AppendText("查询到 " + dt.Rows.Count + " 条数据\r\n");
            }
            catch (Exception ex)
            {
                MessageBox.Show("查询失败：" + ex.Message);
            }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            try
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "Excel文件|*.xlsx";

                if (sfd.ShowDialog() != DialogResult.OK)
                    return;

                Excel.Application excel = new Excel.Application();
                Excel.Workbook workbook = excel.Workbooks.Add();
                Excel.Worksheet sheet = workbook.Worksheets[1];

                for (int i = 0; i < dgv.Columns.Count; i++)
                {
                    if (dgv.Columns[i].Visible)
                        sheet.Cells[1, i + 1] = dgv.Columns[i].HeaderText;
                }

                for (int i = 0; i < dgv.Rows.Count; i++)
                {
                    int col = 1;

                    if (dgv.Rows[i].IsNewRow)
                        continue;

                    for (int j = 0; j < dgv.Columns.Count; j++)
                    {
                        if (dgv.Columns[j].Visible)
                        {
                            sheet.Cells[i + 2, col] = dgv.Rows[i].Cells[j].Value;
                            col++;
                        }
                    }
                }

                workbook.SaveAs(sfd.FileName);
                workbook.Close();
                excel.Quit();

                txtLog.AppendText("导出成功！"+"\r\n");
            }
            catch (Exception ex)
            {
                txtLog.AppendText("导出失败"+ex.Message+"\r\n");
            }
        }
    }
}
