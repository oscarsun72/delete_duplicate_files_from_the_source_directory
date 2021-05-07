using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using io = System.IO;

namespace 檔案總管汰重_WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text== textBox2.Text)
            {
                MessageBox.Show("比對兩造雙方之路徑不能一樣！","",MessageBoxButtons.OK,MessageBoxIcon.Error);
                return;
            }
            io.DirectoryInfo di = new io.DirectoryInfo(textBox1.Text);
            io.FileInfo[] fiArray = di.GetFiles("*.*", io.SearchOption.AllDirectories);
            io.DirectoryInfo di2 = new io.DirectoryInfo(textBox2.Text);
            io.FileInfo[] fiArray2 = di2.GetFiles("*.*", io.SearchOption.AllDirectories);
            if (io.Directory.Exists(textBox1.Text) && io.Directory.Exists(textBox2.Text))
            {
                //foreach (var itemF in io.Directory.GetFiles(textBox1.Text))//iterF(F:from;source)
                foreach (io.FileInfo itemF in fiArray)//iterF(F:from;source)
                {
                    io.FileInfo f = new io.FileInfo(itemF.FullName);
                    //foreach (var itemT in io.Directory.GetFiles(textBox2.Text))//iterT(T:to;destination)
                    foreach (io.FileInfo itemT in fiArray2)//iterT(T:to;destination)
                    {
                        io.FileInfo ft = new io.FileInfo(itemT.FullName);
                        if (f.LastWriteTime == ft.LastWriteTime && f.Length == ft.Length && f.Name == ft.Name)
                        //if ( f.LastWriteTime==ft.LastWriteTime && f.Length==ft.Length  )//不比對檔名
                        {
                            if (io.File.GetAttributes(itemF.FullName).ToString().IndexOf(io.FileAttributes.ReadOnly.ToString()) != -1)
                            {
                                io.File.SetAttributes(itemF.FullName, io.FileAttributes.Normal);
                            }
                            io.File.Delete(itemF.FullName);//delete from the source file                            
                            break;//check the next file in the source directory
                        }
                    }
                }
                di = new io.DirectoryInfo(textBox1.Text);
                fiArray = di.GetFiles("*.*", io.SearchOption.AllDirectories);
                if (fiArray.Length == 0)
                {
                    io.DirectoryInfo[] diSubfolders = di.GetDirectories("*.*", io.SearchOption.AllDirectories);
                    if (diSubfolders.Length == 0)
                    {
                        io.Directory.Delete(textBox1.Text); //if no more files in this directory then delete this directory
                                                            //若但用 if (io.Directory.GetFiles(textBox1.Text).Count() == 0）來判斷，則當尚有子目錄時會出錯
                    }
                    else
                    {
                        while (diSubfolders.Length > 0)
                        {
                            for (int i = 0; i < diSubfolders.Length; i++)
                            {
                                try { diSubfolders[i].Delete(); i--; diSubfolders = di.GetDirectories("*.*", io.SearchOption.AllDirectories); }
                                catch { continue; }
                            }
                        }
                        io.Directory.Delete(textBox1.Text);
                    }
                }
                else                
                    MessageBox.Show("done!");
            }
            else
            {
                MessageBox.Show("路徑有誤！");
            }
        }


        private void textBox1_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";
            textBox1.Text = Clipboard.GetText();
        }
        private void textBox2_Click(object sender, EventArgs e)
        {
            textBox2.Text = "";
            textBox2.Text = Clipboard.GetText();
        }


        private void Form1_DragEnter(object sender, DragEventArgs e)
        {//https://wijtb.nctu.me/archives/269/
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.All;//调用DragDrop事件 
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void textBox1_DragDrop(object sender, DragEventArgs e)
        {//https://www.google.com/search?rlz=1C1OKWM_zh-TWTW847TW847&ei=W7wBXbDjG8Kk8AWfkYGoBQ&q=c%23+%E6%8B%96%E6%8B%89%E7%89%A9%E4%BB%B6+%E8%B3%87%E6%96%99%E5%A4%BE&oq=c%23+%E6%8B%96%E6%8B%89%E7%89%A9%E4%BB%B6+%E8%B3%87%E6%96%99&gs_l=psy-ab.3.0.33i160l2.11461.20026..22176...0.0..0.99.232.3......0....1..gws-wiz.......0i30.KqdNNbPjI8A
            //https://wijtb.nctu.me/archives/269/
            string[] filePaths = (string[])e.Data.GetData(DataFormats.FileDrop);
            this.textBox1.Text = filePaths[0];
        }

        private void textBox2_DragDrop(object sender, DragEventArgs e)
        {
            string[] filePaths = (string[])e.Data.GetData(DataFormats.FileDrop);
            this.textBox2.Text = filePaths[0];
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                this.TopMost = true;
            }
            else
            {
                this.TopMost = false;
            }
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }
    }
}
