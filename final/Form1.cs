using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace final
{
    public partial class MainForm : Form
    {
        List<Instrument> instrumentList = new List<Instrument>();
        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // 設定檔案名稱
            string filePath = "Instruments.csv";
            

            // 1. 【安全網】檢查檔案是否存在？如果沒有，只產生第一行的「欄位標題」
            if (!File.Exists(filePath))
            {
                string[] defaultData = {
            "編號,分類,名稱,狀態"
        };
                File.WriteAllLines(filePath, defaultData);
            }

            // 2. 讀取 CSV 檔案裡面的所有文字
            string[] lines = File.ReadAllLines(filePath);

            // 3. 逐行解析資料 (從 i=1 開始，跳過第一行的中文標題)
            for (int i = 1; i < lines.Length; i++)
            {
                // 為了防止讀到完全空白的行，加一個檢查
                if (string.IsNullOrWhiteSpace(lines[i])) continue;

                string[] parts = lines[i].Split(','); // 用逗號切割資料
                if (parts.Length == 4)
                {
                    // 將切割好的文字，放進樂器物件裡
                    instrumentList.Add(new Instrument
                    {
                        編號 = parts[0],
                        分類 = parts[1],
                        名稱 = parts[2],
                        狀態 = parts[3]
                    });
                }
            }

            // 4. 將資料綁定到表格上顯示
            dgvInventory.DataSource = instrumentList;

            UpdateStats();
        }
        private void SaveToCSV()
        {
            List<string> output = new List<string>();
            output.Add("編號,分類,名稱,狀態"); // 先加入第一行的標題

            // 把清單裡的每一項樂器，重新組合成逗號分隔的字串
            foreach (var item in instrumentList)
            {
                output.Add($"{item.編號},{item.分類},{item.名稱},{item.狀態}");
            }

            // 覆蓋寫入原本的檔案
            File.WriteAllLines("Instruments.csv", output);
        }

        private void UpdateStats()
        {
            int availableCount = instrumentList.Count(x => x.狀態 == "在庫");
            int borrowCount = instrumentList.Count(x => x.狀態 == "借出");
            int repairCount = instrumentList.Count(x => x.狀態 == "維修中");

            lblStats.Text = $"目前在庫：{availableCount} 把   |   借出中：{borrowCount} 把   |   維修保養中：{repairCount} 把";
        }

        private void btnBorrow_Click(object sender, EventArgs e)
        {
            // 確認使用者有沒有在表格裡選中任何一行
            if (dgvInventory.CurrentRow != null)
            {
                // 取得選中的是第幾行
                int index = dgvInventory.CurrentRow.Index;

                // 防呆檢查：必須是「在庫」才能借
                if (instrumentList[index].狀態 == "在庫")
                {
                    instrumentList[index].狀態 = "借出"; // 修改狀態

                    // 刷新表格畫面
                    dgvInventory.DataSource = null;
                    dgvInventory.DataSource = instrumentList;

                    // 呼叫剛剛寫好的存檔功能
                    SaveToCSV();
                    UpdateStats();
                    MessageBox.Show("借出登記成功！", "系統提示");
                }
                else
                {
                    MessageBox.Show("這個樂器目前無法借出喔！（可能已經借出或維修中）", "操作失敗");
                }
            }
        }

        private void btnReturn_Click(object sender, EventArgs e)
        {
            if (dgvInventory.CurrentRow != null)
            {
                int index = dgvInventory.CurrentRow.Index;

                // 防呆檢查：必須是「借出」才能還
                if (instrumentList[index].狀態 == "借出" || instrumentList[index].狀態 == "維修中")
                {
                    instrumentList[index].狀態 = "在庫";

                    dgvInventory.DataSource = null;
                    dgvInventory.DataSource = instrumentList;

                    SaveToCSV();
                    UpdateStats();
                    MessageBox.Show("歸還登記成功！", "系統提示");
                }
                else
                {
                    MessageBox.Show("這個樂器目前並沒有被借出喔！", "操作失敗");
                }
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            // 1. 產生我們剛剛做好的第二個視窗 (AddForm)
            AddForm form2 = new AddForm();

            // 2. 開啟視窗 (ShowDialog 會讓小視窗強制擋在前面，直到關閉為止)
            // 如果使用者成功按下「確認新增」並回傳 OK
            if (form2.ShowDialog() == DialogResult.OK)
            {
                // 3. 從 form2 的包裹裡拿出新樂器，加入到我們的清單中
                instrumentList.Add(form2.NewInstrument);

                // 4. 重新整理表格畫面，讓新樂器顯示出來
                dgvInventory.DataSource = null;
                dgvInventory.DataSource = instrumentList;

                // 5. 呼叫存檔功能，完美寫入 CSV
                SaveToCSV();
                UpdateStats();
                MessageBox.Show("成功新增樂器並已存檔！", "系統提示");
            }
        }

        private void btnRepair_Click(object sender, EventArgs e)
        {
            if (dgvInventory.CurrentRow != null)
            {
                int index = dgvInventory.CurrentRow.Index;

                // 防呆檢查：必須是「在庫」才能送修
                if (instrumentList[index].狀態 == "在庫")
                {
                    instrumentList[index].狀態 = "維修中"; // 修改狀態

                    dgvInventory.DataSource = null;
                    dgvInventory.DataSource = instrumentList;

                    SaveToCSV();
                    UpdateStats(); // 更新數量統計
                    MessageBox.Show("送修登記成功！", "系統提示");
                }
                else
                {
                    MessageBox.Show("這把樂器目前無法送修！（可能已經借出或已在維修中）", "操作失敗");
                }
            }
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            string keyword = txtSearch.Text.Trim(); // 取得使用者輸入的文字並去掉前後空白

            // 如果搜尋框是空的，就顯示完整的清單
            if (string.IsNullOrEmpty(keyword))
            {
                dgvInventory.DataSource = null;
                dgvInventory.DataSource = instrumentList;
            }
            else
            {
                // 使用 LINQ 篩選出「名稱」或「編號」或「分類」包含關鍵字的樂器
                var filteredList = instrumentList.Where(item =>
                    item.名稱.Contains(keyword) ||
                    item.編號.Contains(keyword) ||
                    item.分類.Contains(keyword)
                ).ToList();

                // 將表格的資料換成過濾後的結果
                dgvInventory.DataSource = null;
                dgvInventory.DataSource = filteredList;
            }
        }
    }


    public class Instrument
    {
        public string 編號 { get; set; }
        public string 分類 { get; set; }
        public string 名稱 { get; set; }
        public string 狀態 { get; set; }
    }
}
