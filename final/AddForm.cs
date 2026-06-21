using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace final
{
    public partial class AddForm : Form
    {
        public Instrument NewInstrument { get; set; }
        public AddForm()
        {
            InitializeComponent();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            // 1. 防呆檢查：確認每個格子都有填寫，沒填就不准存檔
            if (string.IsNullOrWhiteSpace(txtId.Text) ||
                string.IsNullOrWhiteSpace(cmbCategory.Text) ||
                string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("請填寫所有欄位資料！", "輸入錯誤");
                return; // 提早結束，擋住後面的存檔動作
            }

            if (!Regex.IsMatch(txtId.Text, "^[A-Za-z0-9-]+$"))
            {
                MessageBox.Show("編號格式錯誤！只能包含英文字母、數字與橫槓(-)，不可輸入中文。", "格式錯誤");
                return; // 提早結束，擋住存檔
            }

            // 2. 將輸入的資料打包成一個樂器物件 (新買的樂器，狀態自動預設為「在庫」)
            NewInstrument = new Instrument
            {
                編號 = txtId.Text,
                分類 = cmbCategory.Text,
                名稱 = txtName.Text,
                狀態 = "在庫"
            };

            // 3. 告訴系統「操作成功 (OK)」，並自動關閉這個小視窗
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
