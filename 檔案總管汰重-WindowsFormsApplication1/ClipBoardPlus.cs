using System.Collections.Specialized;
using System.Windows.Forms;


namespace ExplorerFilemanager
{
    public static class ClipBoardPlus
    {
        public static void CopyFiles(string fileFullname)
        {
            //StringCollection A =
            //        new StringCollection();
            //A.Add(fileFullname);//此與下等式，VisualStudio建議simple化
            StringCollection A =new StringCollection{fileFullname};
            Clipboard.SetFileDropList(A);
        }

        //https://docs.microsoft.com/zh-tw/dotnet/desktop/winforms/advanced/how-to-add-data-to-the-clipboard?view=netframeworkdesktop-4.8
        // Demonstrates SetFileDropList, ContainsFileDroList, and GetFileDropList
        public static System.Collections.Specialized.StringCollection
            SwapClipboardFileDropList(
            System.Collections.Specialized.StringCollection replacementList)
        {
            System.Collections.Specialized.StringCollection returnList = null;
            if (Clipboard.ContainsFileDropList())
            {
                returnList = Clipboard.GetFileDropList();
                Clipboard.SetFileDropList(replacementList);
            }
            return returnList;
        }

    }
}