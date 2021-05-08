# delete_duplicate_files_from_the_source_directory
檔案總管汰重-WindowsFormsApplication1 delete duplicate files from the source directory
* 比對兩個資料夾（包括其底下全部的子資料夾/子目錄中的檔案），刪除來源資料夾相同的檔案。（即操作介面上方文字方塊中的檔案）
- 所定義之「相同」，乃檔案長度（大小）、最後修改日期（時間）相同（詳參[程式碼](https://github.com/oscarsun72/delete_duplicate_files_from_the_source_directory/blob/5cc57c4aa45d558fb6746d363079c54b12468829/%E6%AA%94%E6%A1%88%E7%B8%BD%E7%AE%A1%E6%B1%B0%E9%87%8D-WindowsFormsApplication1/Form1.cs#L39)）
* 分「比對檔名」和「不比對檔名（略過檔名，只比對副檔名）」二種模式
* [清空底下已無檔案的目錄（資料夾）](https://github.com/oscarsun72/delete_duplicate_files_from_the_source_directory/blob/5cc57c4aa45d558fb6746d363079c54b12468829/%E6%AA%94%E6%A1%88%E7%B8%BD%E7%AE%A1%E6%B1%B0%E9%87%8D-WindowsFormsApplication1/Form1.cs#L99)；若全部檔案已清除，則整個刪除起初所指定的來源資料夾。

感恩感恩　南無阿彌陀佛

末學的所有一切（當然包括這裡的一切程式碼）一律開源且不保留任何權利，不屑智慧財產權。餘不贅。佛弟子孫守真任真甫合十敬啟。感恩感恩　南無阿彌陀佛
