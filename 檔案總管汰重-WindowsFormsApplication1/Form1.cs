using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
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
            if (string.IsNullOrWhiteSpace(textBox1.Text) || string.IsNullOrWhiteSpace(textBox1.Text)) return;
            if (textBox1.Text == textBox2.Text)
            {
                MessageBox.Show("比對兩造雙方之路徑不能一樣！", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            Color cl = BackColor;
            BackColor = Color.Red; Refresh();
            deleteDuplicateFilesFromSource(textBox1.Text, textBox2.Text);
            BackColor = cl; Refresh();
        }

        private void deleteDuplicateFilesFromSource(string sourceDir, string DestDir)
        {
            io.DirectoryInfo di = new io.DirectoryInfo(sourceDir);
            io.FileInfo[] fiArray = di.GetFiles("*.*", io.SearchOption.AllDirectories);
            io.DirectoryInfo di2 = new io.DirectoryInfo(DestDir);
            io.FileInfo[] fiArray2 = di2.GetFiles("*.*", io.SearchOption.AllDirectories);
            if (io.Directory.Exists(sourceDir) && io.Directory.Exists(DestDir))
            {
                //foreach (var itemF in io.Directory.GetFiles(sourceDir))//iterF(F:from;source)
                foreach (io.FileInfo itemF in fiArray)//iterF(F:from;source)
                {//F=from 來源檔（拿來比較之檔，相同則刪除）;T=to 目的檔（被比較的檔案，相同則保留）;
                    io.FileInfo f = new io.FileInfo(itemF.FullName);
                    //foreach (var itemT in io.Directory.GetFiles(DestDir))//iterT(T:to;destination)
                    foreach (io.FileInfo itemT in fiArray2)//iterT(T:to;destination)
                    {
                        io.FileInfo ft = new io.FileInfo(itemT.FullName);
                        if (checkBox2.Checked)//not identical
                        {
                            if (f.LastWriteTime == ft.LastWriteTime && f.Length == ft.Length && f.Extension == ft.Extension)//不比對檔名，只比對副檔名
                            {
                                DeleteFileRemoveReadOnly(itemF);
                                break;//check the next file in the source(from) directory
                            }
                        }
                        else
                        {
                            if (f.LastWriteTime == ft.LastWriteTime && f.Length == ft.Length && f.Name == ft.Name)//比對檔名(identical)
                            {
                                //if (io.File.GetAttributes(itemF.FullName).ToString().IndexOf(io.FileAttributes.ReadOnly.ToString()) != -1)
                                //{
                                //    io.File.SetAttributes(itemF.FullName, io.FileAttributes.Normal);
                                //}
                                DeleteFileRemoveReadOnly(itemF);
                                break;//check the next file in the source(from) directory
                            }
                        }
                    }
                }
                //di = new io.DirectoryInfo(sourceDir);
                fiArray = di.GetFiles("*.*", io.SearchOption.AllDirectories);
                io.DirectoryInfo[] diSubfolders;
                if (fiArray.Length == 0)
                {//當沒有檔案，只剩資料夾時
                    diSubfolders = di.GetDirectories("*.*", io.SearchOption.AllDirectories);
                    if (diSubfolders.Length == 0)
                    {//如果沒有子資料夾/子目錄時，就直接刪除處理的資料夾目錄：
                        io.Directory.Delete(sourceDir); //if no more files in this directory then delete this directory
                                                        //若但用 if (io.Directory.GetFiles(sourceDir).Count() == 0）來判斷，則當尚有子目錄時會出錯
                    }
                    else
                    {//如果有子資料夾/子目錄時，須逐一清空各資料夾目錄（當某一資料夾含有子目錄時，不能直接將其刪除）：
                        diSubfolders = ClearEmptyFolders(di, diSubfolders);
                    }
                }
                else
                {//還有檔案，又有空資料夾時
                    //di = new io.DirectoryInfo(sourceDir);
                    diSubfolders = di.GetDirectories("*.*", io.SearchOption.AllDirectories);
                    ClearEmptyFolders(di, diSubfolders);
                }
                //MessageBox.Show("done!");
            }
            else
            {
                MessageBox.Show("路徑有誤！");
            }
        }

        //直接刪除檔案，不管有沒有唯讀屬性 20210508
        public static void DeleteFileRemoveReadOnly(FileInfo fileInfo)
        {
            NoReadonly(fileInfo);
            fileInfo.Delete();//上下兩式作用相同
            //io.File.Delete(fileInfo.FullName);//delete from the source file                            
        }

        public static DirectoryInfo[] ClearEmptyFolders(DirectoryInfo di, DirectoryInfo[] diSubfolders)
        {
            IEnumerable<io.DirectoryInfo> sdI =
                from dI in di.GetDirectories("*.*", io.SearchOption.AllDirectories)
                where dI.GetFiles("*.*", io.SearchOption.AllDirectories).Count() == 0 && dI.GetDirectories
                ("*.*", io.SearchOption.AllDirectories).Count() == 0
                select dI;
            while (sdI.Count() > 0)
            {
                foreach (DirectoryInfo sdi in sdI)
                {
                    if (sdi.GetFiles("*.*", io.SearchOption.AllDirectories).Length == 0)
                    {
                        NoReadonly(sdi);
                        io.DirectoryInfo[] dis = sdi.GetDirectories("*.*", io.SearchOption.AllDirectories);
                        if (dis.Length == 0)
                        {
                            sdi.Delete();
                        }
                        sdI = from dI in di.GetDirectories("*.*", io.SearchOption.AllDirectories)
                              where dI.GetFiles("*.*", io.SearchOption.AllDirectories).Count() == 0 && dI.GetDirectories
                              ("*.*", io.SearchOption.AllDirectories).Count() == 0
                              select dI;

                    }
                }
            }
            #region past method
            //while (diSubfolders.Length > 0)
            //{
            //    for (int i = 0; i < diSubfolders.Length; i++)
            //    {
            //        try
            //        {
            //            if (diSubfolders[i].Attributes == io.FileAttributes.ReadOnly)
            //                di.Attributes &= ~io.FileAttributes.ReadOnly;
            //            diSubfolders[i].Delete(); i--;
            //            diSubfolders =
            //                di.GetDirectories
            //                ("*.*", io.SearchOption.AllDirectories);
            //        }
            //        catch { continue; }
            //    }
            //}
            #endregion
            if (di.GetFiles("*.*", io.SearchOption.AllDirectories).Count() == 0)
            {
                //if (di.Attributes.ToString().IndexOf(io.FileAttributes.ReadOnly.ToString()) != -1)
                //    di.Attributes &= ~io.FileAttributes.ReadOnly;//https://stackoverflow.com/questions/2316308/remove-readonly-attribute-from-directory
                NoReadonly(di);
                di.Delete();
            }
            return diSubfolders;
        }

        public static bool IsReadonly(DirectoryInfo di)
        {
            if (di.Attributes.ToString().IndexOf(io.FileAttributes.ReadOnly.ToString()) != -1) return true; else return false;
        }
        public static bool IsReadonly(FileInfo fi)
        {
            if (fi.Attributes.ToString().IndexOf(io.FileAttributes.ReadOnly.ToString()) != -1) return true; else return false;
        }
        public static void SwitchReadonly(DirectoryInfo di)
        {
            di.Attributes &= ~io.FileAttributes.ReadOnly;
        }
        public static void SwitchReadonly(FileInfo fi)
        {
            fi.Attributes &= ~io.FileAttributes.ReadOnly;
        }
        public static void NoReadonly(DirectoryInfo di)
        {
            if (IsReadonly(di)) di.Attributes &= ~io.FileAttributes.ReadOnly;
        }
        public static void NoReadonly(FileInfo fi)
        {
            if (IsReadonly(fi)) fi.Attributes &= ~io.FileAttributes.ReadOnly;
        }
        private void textBox1_Click(object sender, EventArgs e)
        {
            //textBox1.Text = "";
            textBox1.Text = Clipboard.GetText();
        }
        private void textBox2_Click(object sender, EventArgs e)
        {
            //textBox2.Text = "";
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

        private void textBox1_MouseDown(object sender, MouseEventArgs e)
        {
            TextBox tbx = (TextBox)sender;
            switch (e.Button)
            {
                case MouseButtons.Right:
                    Process pc = new Process();
                    if (Directory.Exists(tbx.Text) == false) return;
                    pc.StartInfo.FileName = tbx.Text;
                    pc.Start();
                    break;
                default:
                    break;
            }
        }
    }
}
