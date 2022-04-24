using System.Net;
using System.Text.RegularExpressions;

namespace Nvm_GUI
{
    public partial class Form1 : Form
    {
        private static string windowTitle = "";
        private static readonly HttpClient client = new HttpClient();
        private static List<string> nodeAllVersionList = new List<string>();
        private static List<string> nodeLocVersionList = new List<string>();
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.checkNvm();
            windowTitle = this.Text+" ";
            this.GetLocNodeVersion();
        }
        private void checkNvm()
        { 
            string output = CmdHelper.ExecuteCommandSync(@"nvm");
            if(output==""||output.IndexOf("不是内部或外部命令") > 0)
            {
                MessageBox.Show("当前系统未安装“nvm”环境", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.Close();
                return;
            }
        }
        private void LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", ((LinkLabel)sender).Text);      //打开网页
        }
        private void GetLocNodeVersion()
        {
            this.Text = windowTitle+"正在加载...";
            this.listBox1.Enabled = this.button_delete.Enabled = this.button_use.Enabled = false;
            if (nodeLocVersionList.Count > 0)
            {
                this.listBox1.Items.Clear();
                nodeLocVersionList.Clear();
            }
            string cmd = @"nvm list";
            string[] rows = CmdHelper.ExecuteCommandSync(cmd).Split('\n');
            foreach (string row in rows)
            {
                if (row.Length > 0)
                {
                    string v = row.Trim();
                    this.listBox1.Items.Add(v);
                    nodeLocVersionList.Add(v);
                }
            }
            if (rows.Length > 0)
            {
                this.listBox1.Enabled = this.button_delete.Enabled = this.button_use.Enabled = true;
                this.listBox1.SetSelected(0, true);
            }
            else MessageBox.Show("未发现Node运行环境", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            this.Text = windowTitle;
        }
        private async void GetAllNodeVersion()
        {
            try
            {
                this.Text = windowTitle + "正在加载..."; 
                this.listBox2.Enabled = this.button_download.Enabled = this.button_loadVersionList.Enabled = false;
                if (nodeAllVersionList.Count > 0)
                {
                    this.listBox2.Items.Clear();
                    nodeAllVersionList.Clear();
                }
                string url = "https://nodejs.org/dist/";
                string responseString = await client.GetStringAsync(url);
                Regex reg = new Regex(@"(?is)<a(?:(?!href=).)*href=(['""]?)(?<url>[^""\s>]*)\1[^>]*>(?<text>(?:(?!</?a\b).)*)</a>");
                MatchCollection mc = reg.Matches(responseString);
                foreach (Match m in mc)
                {
                    string text = m.Groups["text"].Value;
                    if (text[0] == 'v' && text[text.Length - 1] == '/') nodeAllVersionList.Add(text.Substring(1, text.Length - 2));
                    //Console.WriteLine(m.Groups["url"].Value + "\r\n");
                    //Console.WriteLine(m.Groups["text"].Value + "\r\n");
                }
                //request.ContentType = "text/html";
                //request.Accept = "text/html,application/xhtml+xml,*/*";
                //request.Method = "Get";
                //request.Timeout = 30000;
                //string res = null;
                //using (WebResponse response = request.GetResponse() as HttpWebResponse)
                //{
                //    res=response.ToString();
                //}  
                nodeAllVersionList.Sort((string x, string y) => -x.CompareTo(y));
                if (nodeAllVersionList.Count > 0)
                {
                    this.listBox2.Enabled = true;
                    foreach (string v in nodeAllVersionList)
                    {
                        this.listBox2.Items.Add(v);
                    }
                    this.listBox2.SetSelected(0, true);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("“https://nodejs.org/dist/”请求失败！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //throw;
            }
            this.Text = windowTitle;
            this.button_download.Enabled = this.button_loadVersionList.Enabled = true;
        }
        private void button_loadVersionList_Click(object sender, EventArgs e)
        {
            this.GetAllNodeVersion();
        }

        private void button_download_Click(object sender, EventArgs e)
        {
            if (nodeAllVersionList.Count == 0) return;
            int index=this.listBox2.SelectedIndex;
            string nodeLocVersion = nodeAllVersionList[index];
            foreach (string v in nodeLocVersionList)
            {
                if (v.IndexOf(nodeLocVersion)>=0)
                {
                    MessageBox.Show("当前版本已存在！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
            }
            DialogResult dr = MessageBox.Show("确定安装版本“" + nodeLocVersion + "”？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
            if (dr == DialogResult.OK)
            {
                this.Text = windowTitle + "操作中...";
                string cmd = @"nvm install " + nodeLocVersion;
                string rows = CmdHelper.ExecuteCommandSync(cmd);
                MessageBox.Show(rows, "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.GetLocNodeVersion();
                this.Text = windowTitle; 
            }
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int index = this.tabControl1.SelectedIndex;
            if (index == 0 && nodeLocVersionList.Count == 0) this.GetLocNodeVersion();
            if (index == 1 &&  nodeAllVersionList.Count == 0) this.GetAllNodeVersion();
        }

        private void button_delete_Click(object sender, EventArgs e)
        {
            if (nodeLocVersionList.Count == 0) return;
            int index = this.listBox1.SelectedIndex;
            string nodeLocVersion = nodeLocVersionList[index];
            if (nodeLocVersion[0] == '*')
            {
                MessageBox.Show("当前环境正在被使用，无法删除！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            DialogResult dr = MessageBox.Show("确定删除版本“" + nodeLocVersion + "”？", "警告", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
            if (dr == DialogResult.OK)
            {
                this.Text = windowTitle + "操作中...";
                string cmd = @"nvm uninstall " + nodeLocVersion;
                string rows = CmdHelper.ExecuteCommandSync(cmd);
                MessageBox.Show(rows, "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.GetLocNodeVersion();
                this.Text = windowTitle; 
            }
        }

        private void button_use_Click(object sender, EventArgs e)
        {
            if (nodeLocVersionList.Count == 0) return;
            int index = this.listBox1.SelectedIndex;
            string nodeLocVersion = nodeLocVersionList[index];
            if (nodeLocVersion[0] == '*') return;
            DialogResult dr = MessageBox.Show("确定切换版本“" + nodeLocVersion + "”？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
            if (dr == DialogResult.OK)
            {
                this.Text = windowTitle + "操作中...";
                string cmd = @"nvm use " + nodeLocVersion;
                string rows = CmdHelper.ExecuteCommandSync(cmd);
                MessageBox.Show(rows, "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.GetLocNodeVersion();
                this.Text = windowTitle;
            }
        }

        private void listBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.button_use_Click(sender, e);
        }

        private void listBox2_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.button_download_Click(sender,e);
        }
    }
}