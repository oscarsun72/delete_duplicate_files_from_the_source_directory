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
            deleteDuplicateFilesFromSourceLinq(textBox1.Text, textBox2.Text);
            BackColor = cl; Refresh();
        }

        // This method accepts two strings the represent two files to
        // compare. A return value of 0 indicates that the contents of the files
        // are the same. A return value of any other value indicates that the
        // files are not the same.(這句「A return value of 0 indicates that the contents of the files
        // are the same.」應該改成「A return value of true(-1) indicates that the contents of the files……」
        // 0=fasle 怎麼會回傳0呢？)
        private bool FileCompare(string file1, string file2)
        {//https://docs.microsoft.com/zh-tw/troubleshoot/dotnet/csharp/create-file-compare?source=docs
            int file1byte;
            int file2byte;
            FileStream fs1;
            FileStream fs2;

            // Determine if the same file was referenced two times.
            if (file1 == file2)
            {
                // Return true to indicate that the files are the same.
                return true;
            }

            // Open the two files.
            fs1 = new FileStream(file1, FileMode.Open);
            fs2 = new FileStream(file2, FileMode.Open);

            // Check the file sizes. If they are not the same, the files
            // are not the same.
            if (fs1.Length != fs2.Length)
            {
                // Close the file
                fs1.Close();
                fs2.Close();

                // Return false to indicate files are different
                return false;
            }

            // Read and compare a byte from each file until either a
            // non-matching set of bytes is found or until the end of
            // file1 is reached.
            do
            {
                // Read one byte from each file.
                file1byte = fs1.ReadByte();
                file2byte = fs2.ReadByte();
            }
            while ((file1byte == file2byte) && (file1byte != -1));

            // Close the files.
            fs1.Close();
            fs2.Close();

            // Return the success of the comparison. "file1byte" is
            // equal to "file2byte" at this point only if the files are
            // the same.
            return ((file1byte - file2byte) == 0);//相同檔案則byte也一樣，相減後之差為0 則與 0 相等，即傳回 true
        }
        void deleteDuplicateFilesInaDirectoryLinq(DirectoryInfo di)
        {
            IEnumerable<FileInfo> fiIEnum = di.GetFiles("*.*", SearchOption.AllDirectories);
            IEnumerable<FileInfo> fiIEnumDuplicate;
            if (fiIEnum.Count() == 0) return;
            if (checkBox2.Checked)//不比對檔名
            {
                foreach (FileInfo fi in fiIEnum)
                {   //取得所有重複的檔案
                    fiIEnumDuplicate = from f in fiIEnum
                                       where f.FullName != fi.FullName //不包括本身（即至少留下一個檔案）
                                         && f.Length == fi.Length && f.LastWriteTime == fi.LastWriteTime &&
                                         f.Extension == fi.Extension
                                       select f;
                    foreach (var item in fiIEnumDuplicate)
                    {
                        if (FileCompare(item.FullName, fi.FullName))//因為只比對副檔名，還是要小心謹慎點好！
                            DeleteFileRemoveReadOnly(item);//刪除所有重複的檔案
                    }
                    fiIEnum = di.GetFiles("*.*", SearchOption.AllDirectories);//刪除後更新檔案目錄清單
                }
            }
            else
            {
                foreach (FileInfo fi in fiIEnum)
                {   //取得所有重複的檔案
                    fiIEnumDuplicate = from f in fiIEnum
                                       where f.FullName != fi.FullName //不包括本身（即至少留下一個檔案）
                                         && f.Length == fi.Length && f.LastWriteTime == fi.LastWriteTime &&
                                         f.Name == fi.Name//前後二者唯有此處條件略異耳20210511
                                       select f;
                    foreach (var item in fiIEnumDuplicate)
                    {
                        DeleteFileRemoveReadOnly(item);//刪除所有重複的檔案
                    }
                    fiIEnum = di.GetFiles("*.*", SearchOption.AllDirectories);//刪除後更新檔案目錄清單
                }
            }
            ClearEmptyFolders(di, di.GetDirectories());//清空所有空的資料夾
        }
        private void deleteDuplicateFilesFromSourceLinq(string sourceDir, string DestDir)
        {
            if (io.Directory.Exists(sourceDir) && io.Directory.Exists(DestDir))
            {

                io.DirectoryInfo di = new io.DirectoryInfo(sourceDir);
                IEnumerable<FileInfo> fiIEnum;
                io.DirectoryInfo di2 = new io.DirectoryInfo(DestDir);
                deleteDuplicateFilesInaDirectoryLinq(di2);//先刪除目的檔案中所有重複的檔案
                io.FileInfo[] fiArray2 = di2.GetFiles("*.*", io.SearchOption.AllDirectories);
                //foreach (io.FileInfo itemF in fiArray)//iterF(F:from;source)
                //{//F=from 來源檔（拿來比較之檔，相同則刪除）;T=to 目的檔（被比較的檔案，相同則保留）;
                foreach (io.FileInfo itemT in fiArray2)//iterT(T:to;destination)
                {
                    if (checkBox2.Checked)//not identical
                    {
                        fiIEnum = from f in di.GetFiles("*.*", io.SearchOption.AllDirectories)
                                  where f.LastWriteTime == itemT.LastWriteTime &&
                                  f.Length == itemT.Length && f.Extension == itemT.Extension
                                  select f; //不比對檔名，只比對副檔名
                        foreach (FileInfo itemF in fiIEnum)
                        {
                            if (FileCompare(itemF.FullName, itemT.FullName))//只比對副檔名還是要謹慎一點，就是有剛好以上條件俱同而實際不同的檔案，測試時發現的，所幸。感恩感恩　南無阿彌陀佛 20210511
                                DeleteFileRemoveReadOnly(itemF);
                        }
                    }
                    else
                    {
                        fiIEnum = from f in di.GetFiles("*.*", io.SearchOption.AllDirectories)
                                  where f.LastWriteTime == itemT.LastWriteTime &&
                                  f.Length == itemT.Length && f.Name == itemT.Name//比對檔名(identical)
                                  select f;
                        foreach (FileInfo itemF in fiIEnum)
                        {
                            DeleteFileRemoveReadOnly(itemF);
                        }

                    }
                    //check the next file in the Destination directory
                }
                //}
                //di = new io.DirectoryInfo(sourceDir);
                fiIEnum = di.GetFiles("*.*", io.SearchOption.AllDirectories);
                io.DirectoryInfo[] diSubfolders;
                if (fiIEnum.Count() == 0)
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
        private void deleteDuplicateFilesFromSource(string sourceDir, string DestDir)
        {
            if (io.Directory.Exists(sourceDir) && io.Directory.Exists(DestDir))
            {
                io.DirectoryInfo di = new io.DirectoryInfo(sourceDir);
                io.FileInfo[] fiArray = di.GetFiles("*.*", io.SearchOption.AllDirectories);
                io.DirectoryInfo di2 = new io.DirectoryInfo(DestDir);
                io.FileInfo[] fiArray2 = di2.GetFiles("*.*", io.SearchOption.AllDirectories);
                //foreach (var itemF in io.Directory.GetFiles(sourceDir))//iterF(F:from;source)
                foreach (io.FileInfo itemF in fiArray)//iterF(F:from;source)
                {//F=from 來源檔（拿來比較之檔，相同則刪除）;T=to 目的檔（被比較的檔案，相同則保留）;
                    //foreach (var itemT in io.Directory.GetFiles(DestDir))//iterT(T:to;destination)
                    foreach (io.FileInfo itemT in fiArray2)//iterT(T:to;destination)
                    {
                        if (checkBox2.Checked)//not identical
                        {
                            if (itemF.LastWriteTime == itemT.LastWriteTime && itemF.Length == itemT.Length && itemF.Extension == itemT.Extension)//不比對檔名，只比對副檔名
                            {
                                DeleteFileRemoveReadOnly(itemF);
                                break;//check the next file in the source(from) directory
                            }
                        }
                        else
                        {
                            if (itemF.LastWriteTime == itemT.LastWriteTime && itemF.Length == itemT.Length && itemF.Name == itemT.Name)//比對檔名(identical)
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
            e.Effect = DragDropEffects.All;
            //if (e.Data.GetDataPresent(DataFormats.FileDrop))
            //{
            //    e.Effect = DragDropEffects.All;//调用DragDrop事件 
            //}
            //else
            //{
            //    e.Effect = DragDropEffects.None;
            //}
        }

        private void textBox1_DragDrop(object sender, DragEventArgs e)
        {//https://www.google.com/search?rlz=1C1OKWM_zh-TWTW847TW847&ei=W7wBXbDjG8Kk8AWfkYGoBQ&q=c%23+%E6%8B%96%E6%8B%89%E7%89%A9%E4%BB%B6+%E8%B3%87%E6%96%99%E5%A4%BE&oq=c%23+%E6%8B%96%E6%8B%89%E7%89%A9%E4%BB%B6+%E8%B3%87%E6%96%99&gs_l=psy-ab.3.0.33i160l2.11461.20026..22176...0.0..0.99.232.3......0....1..gws-wiz.......0i30.KqdNNbPjI8A
            //https://wijtb.nctu.me/archives/269/
            string[] filePaths = (string[])e.Data.GetData(DataFormats.FileDrop);
            this.textBox1.Text = filePaths[0];//目前只取多種選取的第一個
            //拖放作業目前只取多種選取的第一個，蓋若有多個資料夾要與一個目的夾比對，可以移至同一個母資料夾下，再指定該新建的母資料夾為來源夾即可
        }

        private void textBox2_DragDrop(object sender, DragEventArgs e)
        {
            string[] filePaths = (string[])e.Data.GetData(DataFormats.FileDrop);
            this.textBox2.Text = filePaths[0];//目前只取多種選取的第一個
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
                case MouseButtons.Middle:
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
