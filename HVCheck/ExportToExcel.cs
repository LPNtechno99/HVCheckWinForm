using Microsoft.Office.Interop.Excel;
using Excel = Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace HVCheck
{
    public class ExportToExcel
    {
        public void Export(System.Data.DataTable dt, string sheetName, string title)
        {
            try
            {
                //Tạo các đối tượng Excel

                Microsoft.Office.Interop.Excel.Application oExcel = new Microsoft.Office.Interop.Excel.Application();

                Microsoft.Office.Interop.Excel.Workbooks oBooks;

                Microsoft.Office.Interop.Excel.Sheets oSheets;

                Microsoft.Office.Interop.Excel.Workbook oBook;

                Microsoft.Office.Interop.Excel.Worksheet oSheet;

                //Tạo mới một Excel WorkBook 

                oExcel.Visible = true;

                oExcel.DisplayAlerts = false;

                oExcel.Application.SheetsInNewWorkbook = 1;

                oBooks = oExcel.Workbooks;

                oBook = (Microsoft.Office.Interop.Excel.Workbook)(oExcel.Workbooks.Add(Type.Missing));

                oSheets = oBook.Worksheets;

                oSheet = (Microsoft.Office.Interop.Excel.Worksheet)oSheets.get_Item(1);

                oSheet.Name = sheetName;

                // Tạo phần đầu nếu muốn

                Microsoft.Office.Interop.Excel.Range head = oSheet.get_Range("A1", "F1");

                head.MergeCells = true;

                head.Value2 = title;

                head.Font.Bold = true;

                head.Font.Name = "Tahoma";

                head.Font.Size = "20";

                head.RowHeight = 30;

                head.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
                head.VerticalAlignment = XlHAlign.xlHAlignCenter;

                // Tạo tiêu đề cột 

                Microsoft.Office.Interop.Excel.Range cl1 = oSheet.get_Range("A3", "A3");

                cl1.Value2 = "STT";

                cl1.ColumnWidth = 13;
               
                Microsoft.Office.Interop.Excel.Range cl2 = oSheet.get_Range("B3", "B3");

                cl2.Value2 = "TimeStamp";

                cl2.ColumnWidth = 25.0;
                
                Microsoft.Office.Interop.Excel.Range cl3 = oSheet.get_Range("C3", "C3");

                cl3.Value2 = "Ngày";

                cl3.ColumnWidth = 25;
               
                Microsoft.Office.Interop.Excel.Range cl4 = oSheet.get_Range("D3", "D3");

                cl4.Value2 = "Mã Đại Lý";

                cl4.ColumnWidth = 30;

                Microsoft.Office.Interop.Excel.Range cl5 = oSheet.get_Range("E3", "E3");

                cl5.Value2 = "Tên Đại Lý";

                cl5.ColumnWidth = 40;
                
                Microsoft.Office.Interop.Excel.Range cl6 = oSheet.get_Range("F3", "F3");

                cl6.Value2 = "Mã Sản Phẩm";

                cl6.ColumnWidth = 30;

                Microsoft.Office.Interop.Excel.Range rowHead = oSheet.get_Range("A3", "F3");

                rowHead.Font.Bold = true;
                rowHead.RowHeight = 30;
                rowHead.Font.Size = 14;

                // Kẻ viền

                rowHead.Borders.LineStyle = Microsoft.Office.Interop.Excel.Constants.xlSolid;

                // Thiết lập màu nền

                rowHead.Interior.Color = ColorTranslator.ToOle(Color.Orange);

                rowHead.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
                rowHead.VerticalAlignment = XlHAlign.xlHAlignCenter;


                // Tạo mẳng đối tượng để lưu dữ toàn bồ dữ liệu trong DataTable,

                // vì dữ liệu được được gán vào các Cell trong Excel phải thông qua object thuần.

                object[,] arr = new object[dt.Rows.Count, dt.Columns.Count];

                //Chuyển dữ liệu từ DataTable vào mảng đối tượng

                for (int r = 0; r < dt.Rows.Count; r++)

                {

                    DataRow dr = dt.Rows[r];

                    for (int c = 0; c < dt.Columns.Count; c++)

                    {
                        arr[r, c] = dr[c];
                    }
                }

                //Thiết lập vùng điền dữ liệu

                int rowStart = 4;

                int columnStart = 1;

                int rowEnd = rowStart + dt.Rows.Count - 1;

                int columnEnd = dt.Columns.Count;

                // Ô bắt đầu điền dữ liệu

                Microsoft.Office.Interop.Excel.Range c1 = (Microsoft.Office.Interop.Excel.Range)oSheet.Cells[rowStart, columnStart];

                // Ô kết thúc điền dữ liệu

                Microsoft.Office.Interop.Excel.Range c2 = (Microsoft.Office.Interop.Excel.Range)oSheet.Cells[rowEnd, columnEnd];

                // Lấy về vùng điền dữ liệu

                Microsoft.Office.Interop.Excel.Range range = oSheet.get_Range(c1, c2);

                //Điền dữ liệu vào vùng đã thiết lập

                range.Value2 = arr;

                // Kẻ viền

                range.Borders.LineStyle = Microsoft.Office.Interop.Excel.Constants.xlSolid;
                range.RowHeight = 20;

                // Căn giữa cột STT

                Microsoft.Office.Interop.Excel.Range c3 = (Microsoft.Office.Interop.Excel.Range)oSheet.Cells[rowEnd, columnStart];

                Microsoft.Office.Interop.Excel.Range c4 = oSheet.get_Range(c1, c3);

                oSheet.get_Range(c3, c4).HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;

                // định dạng ô ngày tháng thành ngày tháng chuẩn
                Microsoft.Office.Interop.Excel.Range c5 = (Microsoft.Office.Interop.Excel.Range)oSheet.Cells[rowStart, columnStart + 2];
                Microsoft.Office.Interop.Excel.Range c6 = (Microsoft.Office.Interop.Excel.Range)oSheet.Cells[rowEnd, columnStart + 2];
                Microsoft.Office.Interop.Excel.Range c7 = oSheet.get_Range(c5, c6);
                c7.NumberFormat = "MM/dd/yyyy";

                // định dạng ô timestamp thành kiểu dữ liệu dạng số không có phần thập phân
                Microsoft.Office.Interop.Excel.Range c8 = (Microsoft.Office.Interop.Excel.Range)oSheet.Cells[rowStart, columnStart + 1];
                Microsoft.Office.Interop.Excel.Range c9 = (Microsoft.Office.Interop.Excel.Range)oSheet.Cells[rowEnd, columnStart + 1];
                Microsoft.Office.Interop.Excel.Range c10 = oSheet.get_Range(c8, c9);
                c10.NumberFormat = "0";

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
