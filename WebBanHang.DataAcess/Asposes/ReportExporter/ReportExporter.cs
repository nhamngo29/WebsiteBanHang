using Aspose.Cells;
using Aspose.Words;
using Aspose.Words.MailMerging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Data;
using WebBanHang.DataAcess.Models;
using WebBanHang.DataAcess.Procedures.ProcedureHelpers;
using WebBanHang.DataAcess.Report.Dto;
using FileTypeConst = WebBanHang.DataAcess.Report.Dto.FileTypeConst;
using ReportInfo = WebBanHang.DataAcess.Models.ReportInfo;
namespace WebBanHang.DataAcess.Asposes.ReportExporter
{
    public class ReportExporter : IReportExporter
    {
        private readonly string directionReport;
        private readonly string directionImage;

        private readonly IStoreProcedureProvider _storeProcedureProvider;
        private readonly string _fileUploadRootPath;

        public ReportExporter(IStoreProcedureProvider storeProc)
        {
            this._storeProcedureProvider = storeProc;
            _fileUploadRootPath = "";
            directionReport = "D:/ProjectWebBanaHang/WebBanHang/WebBanHang/wwwroot/Report/";
            directionImage ="";
        }


        private async Task<DataSet> GetDataFromStoreToReport(string storeName, List<ReportParameter> parameters)
        {
            try
            {
                DataSet data = await _storeProcedureProvider.GetMultiDataFromStoredProcedure(storeName, parameters);
                for (int i = 0; i < data.Tables.Count; i++)
                {
                    data.Tables[i].TableName = "table" + i;

                    //BAODNQ 9/8/2022
                    // dat ten bang theo cot MERGE_REGION select tu SQL(neu co)
                    if (data.Tables[i].Columns[0].ColumnName == "MERGE_REGION" && data.Tables[i].Rows[0]["MERGE_REGION"] != null)
                    {
                        data.Tables[i].TableName = data.Tables[i].Rows[0]["MERGE_REGION"].ToString();
                    }
                }

                //Image image = Image.FromFile(data.Tables[0].Rows[0][2].ToString());

                //var destRect = new Rectangle(0, 0, 100, 100);
                //var destImage = new Bitmap(100, 100);

                //destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

                //using (var graphics = Graphics.FromImage(destImage))
                //{
                //    graphics.CompositingMode = CompositingMode.SourceCopy;
                //    graphics.CompositingQuality = CompositingQuality.HighQuality;
                //    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                //    graphics.SmoothingMode = SmoothingMode.HighQuality;
                //    graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                //    using (var wrapMode = new ImageAttributes())
                //    {
                //        wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                //        graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                //    }
                //}

                //data.Tables[0].Rows[0][2] = ImageToByte2(destImage);

                //data.Relations.Add(new DataRelation("CustomerType", data.Tables[0].Columns["Id"], data.Tables[1].Columns["CustomerTypeCode"], false));

                return data;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task<WorkbookDesigner> CreateExcelFileAndDesign(ReportInfo info)
        {
            DataSet data = await GetDataFromStoreToReport(info.StoreName, info.Parameters);
            //data.Relations.Add(new DataRelation("CustomerType_Customer", data.Tables[1].Columns["Id"], data.Tables[0].Columns["CustomerTypeCode"], true));
            Workbook designer = new Workbook(directionReport + info.PathName);



            WorkbookDesigner designWord = new WorkbookDesigner(designer);

            foreach (var item in info.Values)
            {
                designWord.SetDataSource(item.Name, item.Value);
            }

            int TableCount = data.Tables.Count;

            for (int i = 0; i < TableCount; i++)
            {
                int rows = data.Tables[i].Rows.Count;
                int cols = data.Tables[i].Columns.Count;

                for (int row = 0; row < rows; row++)
                    for (int col = 0; col < cols; col++)
                    {
                        var obj = data.Tables[i].Rows[row][col];
                        if (obj.GetType() == typeof(string))
                        {
                            string _value = obj.ToString();
                            bool check = _value.StartsWith('=') ||
                                    _value.StartsWith('+') ||
                                    _value.StartsWith('-') ||
                                    _value.StartsWith('@');

                            if (check)
                            {
                                data.Tables[i].Rows[row][col] = _value.Insert(0, "'");
                            }
                        }
                    }

            }

            designWord.SetDataSource(data);

            designWord.Process(false);
            designWord.Workbook.FileName = info.PathName;
            designWord.Workbook.FileFormat = FileFormatType.Xlsx;
            designWord.Workbook.Settings.CalcMode = CalcModeType.Automatic;
            designWord.Workbook.Settings.RecalculateBeforeSave = true;
            designWord.Workbook.Settings.ReCalculateOnOpen = true;
            designWord.Workbook.Settings.CheckCustomNumberFormat = true;
            designWord.Workbook.Worksheets[0].AutoFitRows();




            return designWord;
        }

        private async Task<MemoryStream> CreateExcelFile(ReportInfo info)
        {
            var designWord = await CreateExcelFileAndDesign(info);

            if (info.ProcessMerge == true)
            {
                ProcessMergeCell(designWord.Workbook);
            }

            MemoryStream str = new MemoryStream();
            switch (info.TypeExport.ToLower())
            {
                case FileTypeConst.Pdf:
                    designWord.Workbook.Save(str, Aspose.Cells.SaveFormat.Pdf);
                    //doc.Save(stream, Aspose.Words.SaveFormat.Pdf);
                    break;
                case FileTypeConst.Excel:
                    designWord.Workbook.Save(str, Aspose.Cells.SaveFormat.Xlsx);
                    //doc.Save(stream, Aspose.Words.SaveFormat.Excel);
                    break;
            }
            return str;

        }

        void ProcessMergeCell(Workbook wb)
        {
            string startMergeMarkup = "StartMerge.";
            string endMergeMarkup = "EndMerge.";
            string textMerge = "";
            foreach (var ws in wb.Worksheets)
            {
                int rowBegin, rowEnd, colBegin, colEnd;
                if (ws.Cells.FirstCell == null)
                {
                    continue;
                }
                rowBegin = ws.Cells.FirstCell.Row;
                rowEnd = ws.Cells.LastCell.Row;
                colBegin = ws.Cells.FirstCell.Column;
                colEnd = ws.Cells.MaxColumn;

                //var CellSStyle = ws.Cells["A"].GetStyle();
                //CellSStyle.
                //ws.Cells["A"].SetStyle()

                for (int rowIndex = rowBegin; rowIndex <= rowEnd; rowIndex++)
                {
                    int colStartMerge = -1, colEndMerge = -1;
                    for (int colIndex = colBegin; colIndex <= colEnd; colIndex++)
                    {
                        var cell = ws.Cells.Rows[rowIndex][colIndex];
                        if (cell.Value != null && cell.Value.ToString().StartsWith(startMergeMarkup))
                        {
                            colStartMerge = colIndex;
                            cell.Value = cell.Value.ToString().Substring(startMergeMarkup.Length);
                            textMerge = cell.Value.ToString();
                        }

                        if (cell.Value != null && cell.Value.ToString().StartsWith(endMergeMarkup) && colStartMerge >= 0)
                        {
                            colEndMerge = colIndex;
                            cell.Value = cell.Value.ToString().Substring(endMergeMarkup.Length);
                            textMerge += cell.Value;
                            var style = ws.Cells.Rows[rowIndex][colStartMerge].GetStyle();
                            ws.Cells.Merge(rowIndex, colStartMerge, 1, colEndMerge - colStartMerge + 1);

                            ws.Cells.Rows[rowIndex][colStartMerge].Value = textMerge;
                            style.HorizontalAlignment = TextAlignmentType.Left;
                            ws.Cells.Rows[rowIndex][colStartMerge].SetStyle(style);
                            textMerge = "";
                        }
                    }
                }
            }
        }

        private async Task<MemoryStream> CreateWordFile(ReportInfo info)
        {
            CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            DataSet data = await GetDataFromStoreToReport(info.StoreName, info.Parameters);
            //data.Relations.Add(new DataRelation("CustomerType_Customer", data.Tables[1].Columns["Id"], data.Tables[0].Columns["CustomerTypeCode"], true));

            data = getDataPrintImage(data);

            Document doc = new Document(directionReport + info.PathName);

            doc.MailMerge.CleanupParagraphsWithPunctuationMarks = true;
            doc.MailMerge.CleanupOptions = MailMergeCleanupOptions.RemoveUnusedFields | MailMergeCleanupOptions.RemoveUnusedRegions;

            doc.MailMerge.Execute(info.Values.Select(x => x.Name).ToArray(), info.Values.Select(x => x.Value).ToArray());

            try
            {
                doc.MailMerge.ExecuteWithRegions(data);

            }
            catch (Exception ex)
            {

                throw ex;
            }



            MemoryStream stream = new MemoryStream();

            switch (info.TypeExport.ToLower())
            {
                case FileTypeConst.Pdf:
                    doc.Save(stream, Aspose.Words.SaveFormat.Pdf);
                    break;
                case FileTypeConst.Word:
                    doc.Save(stream, Aspose.Words.SaveFormat.Docx);
                    break;
            }


            return stream;

        }

        private async Task<MemoryStream> CreateWordFileQR(ReportInfo info)
        {
            CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            DataSet data = await GetDataFromStoreToReport(info.StoreName, info.Parameters);
            var qty = info.Parameters.FirstOrDefault(item => item.Name == "NUMQR").Value;
            var tmp = getDataPrintTemp(data.Tables[0], qty.ToString());
            tmp.TableName = "Newrpt";
            var newData = new DataSet();
            newData.Tables.Add(tmp);
            //data.Relations.Add(new DataRelation("CustomerType_Customer", data.Tables[1].Columns["Id"], data.Tables[0].Columns["CustomerTypeCode"], true));


            Document doc = new Document(directionReport + info.PathName);

            doc.MailMerge.FieldMergingCallback = new HandleMergeImageField();

            doc.MailMerge.CleanupParagraphsWithPunctuationMarks = true;
            doc.MailMerge.CleanupOptions = MailMergeCleanupOptions.RemoveUnusedFields | MailMergeCleanupOptions.RemoveUnusedRegions | MailMergeCleanupOptions.RemoveEmptyParagraphs;

            doc.MailMerge.Execute(info.Values.Select(x => x.Name).ToArray(), info.Values.Select(x => x.Value).ToArray());


            doc.MailMerge.ExecuteWithRegions(newData);


            MemoryStream stream = new MemoryStream();

            switch (info.TypeExport.ToLower())
            {
                case FileTypeConst.Pdf:
                    doc.Save(stream, Aspose.Words.SaveFormat.Pdf);
                    break;
                case FileTypeConst.Word:
                    doc.Save(stream, Aspose.Words.SaveFormat.Docx);
                    break;
            }


            return stream;

        }

        public async Task<MemoryStream> GetReportFile(ReportInfo info)
        {
            MemoryStream str = new MemoryStream();

            switch (info.TypeExport.ToLower())
            {
                case FileTypeConst.Excel:
                    str = await CreateExcelFile(info);
                    break;
                case FileTypeConst.Pdf:
                    if (info.PathName.Split('.')[info.PathName.Split('.').Length - 1] == "xlsx")
                    {
                        str = await CreateExcelFile(info);
                    }
                    else
                    {
                        str = await CreateWordFile(info);
                    }
                    break;
                case FileTypeConst.Word:
                    str = await CreateWordFile(info);
                    break;
            }

            return str;
        }

        public async Task<MemoryStream> GetReportFileQR(ReportInfo info)
        {
            MemoryStream str = new MemoryStream();
            str = await CreateWordFileQR(info);

            //Phucvh 24/10/22 Move In nhãn từ An Bình
            //MemoryStream str = new MemoryStream();
            //str = await CreateWordFileQR_VB(info);

            return str;
        }

        public MemoryStream GetReportFileFromHtml(ReportHtmlInfo info)
        {
            MemoryStream str = new MemoryStream();

            switch (info.TypeExport.ToLower())
            {

                case FileTypeConst.Pdf:
                    str = CreateWordFileFromHtml(info);
                    break;
                case FileTypeConst.Word:
                    str = CreateWordFileFromHtml(info);
                    break;
            }




            return str;
        }
        private MemoryStream CreateWordFileFromHtml(ReportHtmlInfo info)
        {




            Document doc = new Document();
            //switch (info.PageSize)
            //{
            //    case "A3":
            //        foreach (Section section in doc.Sections)
            //        {
            //            section.PageSetup.PaperSize = Aspose.Words.PaperSize.A3;
            //        }
            //        break;
            //    case "A4":
            //        foreach (Section section in doc.Sections)
            //        {
            //            section.PageSetup.PaperSize = Aspose.Words.PaperSize.A4;
            //        }
            //        break;
            //}


            DocumentBuilder builder = new DocumentBuilder(doc);

            builder.InsertHtml(info.HTMLString);

            doc.UpdatePageLayout();


            MemoryStream stream = new MemoryStream();

            switch (info.TypeExport.ToLower())
            {
                case FileTypeConst.Pdf:
                    doc.Save(stream, Aspose.Words.SaveFormat.Pdf);
                    break;
                case FileTypeConst.Word:
                    doc.Save(stream, Aspose.Words.SaveFormat.Docx);
                    break;
            }

            return stream;

        }

        //truongnv
        public async Task<MemoryStream> GetReportFileCustomFomart(ReportInfo info)
        {
            MemoryStream str = new MemoryStream();

            switch (info.TypeExport.ToLower())
            {
                case FileTypeConst.Excel:
                    str = await CreateExcelFileCustomFomart(info);
                    break;
                case FileTypeConst.Pdf:
                    str = await CreateWordFile(info);
                    break;
                case FileTypeConst.Word:
                    str = await CreateWordFile(info);
                    break;
            }




            return str;
        }
        //truongnv
        public async Task<MemoryStream> GetReportFile_BCKH_CustomFomart(ReportInfo info)
        {
            MemoryStream str = new MemoryStream();

            switch (info.TypeExport.ToLower())
            {
                case FileTypeConst.Excel:
                    str = await CreateExcelFile_BCKH_CustomFomart(info);
                    break;
                case FileTypeConst.Pdf:
                    str = await CreateWordFile(info);
                    break;
                case FileTypeConst.Word:
                    str = await CreateWordFile(info);
                    break;
            }
            return str;
        }
        //truongnv
        private async Task<MemoryStream> CreateExcelFile_BCKH_CustomFomart(ReportInfo info)
        {
            var designWord = await CreateExcelFileAndDesign(info);
            Custom_BCKH_Fomart(designWord.Workbook);
            MemoryStream str = new MemoryStream();
            designWord.Workbook.Save(str, Aspose.Cells.SaveFormat.Xlsx);
            return str;
        }
        //truopngnv
        private async Task<MemoryStream> CreateExcelFileCustomFomart(ReportInfo info)
        {
            var designWord = await CreateExcelFileAndDesign(info);


            CustomFomart(designWord.Workbook);


            MemoryStream str = new MemoryStream();
            designWord.Workbook.Save(str, Aspose.Cells.SaveFormat.Xlsx);

            return str;

        }
        //truongnv
        void CustomFomart(Workbook wb)
        {
            string startMergeMarkup = "StartMerge.";
            string endMergeMarkup = "EndMerge.";
            string textMerge = "";
            foreach (var ws in wb.Worksheets)
            {

                int rowBegin, rowEnd, colBegin, colEnd;
                if (ws.Cells.FirstCell == null)
                {
                    continue;
                }
                rowBegin = ws.Cells.FirstCell.Row + 2;
                rowEnd = ws.Cells.LastCell.Row;
                colBegin = ws.Cells.FirstCell.Column;
                colEnd = ws.Cells.LastCell.Column;


                for (int rowIndex = rowBegin; rowIndex <= rowEnd; rowIndex++)
                {
                    int colStartMerge = -1, colEndMerge = -1;
                    var cell = ws.Cells.Rows[rowIndex][colBegin];
                    var style = cell.GetStyle();
                    if (cell.Value != null)
                    {
                        if (cell.Value.ToString().All(char.IsDigit))
                        {
                            style.HorizontalAlignment = TextAlignmentType.Center;
                            cell.SetStyle(style);
                        }
                        else if (cell.Value.ToString().Length <= 2)
                        {
                            style.HorizontalAlignment = TextAlignmentType.Left;
                            style.Font.IsBold = true;
                            cell.SetStyle(style);
                        }
                        else
                        {
                            style.HorizontalAlignment = TextAlignmentType.Right;

                            cell.SetStyle(style);

                        }
                    }
                }
            }

        }
        //truongnv
        void Custom_BCKH_Fomart(Workbook wb)
        {
            string startMergeMarkup = "StartMerge.";
            string endMergeMarkup = "EndMerge.";
            string textMerge = "";
            foreach (var ws in wb.Worksheets)
            {

                int rowBegin, rowEnd, colBegin, colEnd;
                if (ws.Cells.FirstCell == null)
                {
                    continue;
                }
                rowBegin = ws.Cells.FirstCell.Row + 7;
                rowEnd = ws.Cells.LastCell.Row;
                colBegin = ws.Cells.FirstCell.Column;
                colEnd = ws.Cells.LastCell.Column;

                string temp = "";

                //XỬ lý cột 'MỤC'
                for (int rowIndex = rowBegin; rowIndex <= rowEnd; rowIndex++)
                {
                    var cell = ws.Cells.Rows[rowIndex][colBegin];

                    var style = cell.GetStyle();
                    if (cell.Value != null)
                    {
                        //Merge
                        if (temp == cell.Value.ToString())
                        {
                            cell.Value = "";
                        }
                        else
                        {
                            temp = cell.Value.ToString();
                        }

                        style.IsTextWrapped = false;
                        //A, B, C,A1, A2...
                        if ((cell.Value.ToString().Length == 1) || (cell.Value.ToString().Length > 1 && !cell.Value.ToString().Contains('.')))
                        {
                            var rowCells = ws.Cells.Rows[rowIndex][colBegin];
                            style.HorizontalAlignment = TextAlignmentType.Left;
                            rowCells.SetStyle(style);
                        }

                        //A1.01, A2.01...
                        if (cell.Value.ToString().Contains('.'))
                        {
                            var rowCells = ws.Cells.Rows[rowIndex][colBegin];
                            style.HorizontalAlignment = TextAlignmentType.Right;
                            rowCells.SetStyle(style);
                        }
                    }
                }

                for (int rowIndex = rowBegin; rowIndex <= rowEnd; rowIndex++)
                {
                    var cell = ws.Cells.Rows[rowIndex][colBegin + 1];
                    if (cell.Value != null)
                    {
                        //Merge
                        if (temp == cell.Value.ToString())
                        {
                            cell.Value = "";
                        }
                        else
                        {
                            temp = cell.Value.ToString();
                        }
                    }
                    cell = ws.Cells.Rows[rowIndex][colBegin + 2];
                    if (cell.Value != null)
                    {
                        if (string.IsNullOrWhiteSpace(cell.Value.ToString()))
                        {
                            cell.Value = null;
                        }
                    }
                    cell = ws.Cells.Rows[rowIndex][colBegin + 3];
                    if (cell.Value != null)
                    {
                        if (string.IsNullOrWhiteSpace(cell.Value.ToString()))
                        {
                            cell.Value = null;
                        }
                    }
                }
            }
        }














        public async Task<dynamic> CreateExcelFileAndDesignDynamicColumn(ReportInfo info)
        {
            DataSet data = await GetDataFromStoreToReport(info.StoreName, info.Parameters);
            //data.Relations.Add(new DataRelation("CustomerType_Customer", data.Tables[1].Columns["Id"], data.Tables[0].Columns["CustomerTypeCode"], true));
            Workbook designer = new Workbook(directionReport + info.PathName);


            WorkbookDesigner designWord = new WorkbookDesigner(designer);

            foreach (var item in info.Values)
            {
                designWord.SetDataSource(item.Name, item.Value);
            }

            int TableCount = data.Tables.Count;

            for (int i = 0; i < TableCount; i++)
            {
                int rows = data.Tables[i].Rows.Count;
                int cols = data.Tables[i].Columns.Count;

                for (int row = 0; row < rows; row++)
                    for (int col = 0; col < cols; col++)
                    {
                        var obj = data.Tables[i].Rows[row][col];
                        if (obj.GetType() == typeof(string))
                        {
                            string _value = obj.ToString();
                            bool check = _value.StartsWith('=') ||
                                    _value.StartsWith('+') ||
                                    _value.StartsWith('-') ||
                                    _value.StartsWith('@');

                            if (check)
                            {
                                data.Tables[i].Rows[row][col] = _value.Insert(0, "'");
                            }
                        }
                    }

            }
            designWord.SetDataSource(data);

            designWord.Process(false);
            designWord.Workbook.FileName = info.PathName;
            designWord.Workbook.FileFormat = FileFormatType.Xlsx;
            designWord.Workbook.Settings.CalcMode = CalcModeType.Automatic;
            designWord.Workbook.Settings.RecalculateBeforeSave = true;
            designWord.Workbook.Settings.ReCalculateOnOpen = true;
            designWord.Workbook.Settings.CheckCustomNumberFormat = true;




            return new
            {
                designWord = designWord,
                data = data
            };
        }
        void DynamicColumnFomart(Workbook wb, DataSet data)
        {
            var _dataSheetSource = data.Tables[0];
            var _dynamicSource = data.Tables[1];
            foreach (var ws in wb.Worksheets)
            {
                int rowBegin, rowEnd, colBegin, colEnd;
                if (ws.Cells.FirstCell == null)
                {
                    continue;
                }

                rowBegin = ws.Cells.FirstCell.Row;
                rowEnd = ws.Cells.LastCell.Row;
                colBegin = ws.Cells.FirstCell.Column;
                colEnd = ws.Cells.MaxColumn;

                int dynamicCollOffset = -1, dynamicRowOffset = -1;
                string dynamicColName = "";

                //Tìm vị trí chèn
                for (int i = rowBegin; i < rowEnd; i++)
                {
                    for (int j = colBegin; j < colEnd; j++)
                    {
                        if (ws.Cells.Rows[i][j].Value != null)
                        {
                            string regex = @"<dynamic>(.*)<\/dynamic>";
                            Match match = Regex.Match(ws.Cells.Rows[i][j].Value.ToString(), regex);
                            if (match.Success)
                            {
                                dynamicRowOffset = i;
                                dynamicCollOffset = j;
                                dynamicColName = match.Groups[1].Value;
                                goto findDynamicPositionDone;
                            }
                        }
                    }
                }

                findDynamicPositionDone:;

                if (dynamicCollOffset == -1 || dynamicRowOffset == -1)
                {
                    return;
                }

                //Tìm số lượng cột chèn thêm
                int maxDynamicCollCount = 0;
                int maxRowInDynamicSouce = _dynamicSource.Rows.Count;
                for (int i = 0; i < maxRowInDynamicSouce; i++)
                {
                    int x = 0;
                    maxDynamicCollCount = Int32.TryParse(_dynamicSource.Rows[i][2].ToString(), out x) && x > maxDynamicCollCount
                        ? x : maxDynamicCollCount;
                }

                //Chèn thêm cột dựa vào số lượng cột đã tìm được
                for (int i = 0; i < maxDynamicCollCount; i++)
                {
                    ws.Cells.InsertColumn(dynamicCollOffset + i, true);
                    ws.Cells.Rows[dynamicRowOffset][dynamicCollOffset + i].Value = dynamicColName + " " + (i + 1);
                }
                ws.Cells.DeleteColumn(dynamicCollOffset + maxDynamicCollCount);

                //Chèn giá trị vào hàng thuộc mấy cột mới thêm
                int stt = 0;
                for (int i = dynamicRowOffset + 1; i < rowEnd; i++)
                {
                    stt++;
                    int phase = 0;
                    for (int j = dynamicCollOffset; j < maxDynamicCollCount + dynamicCollOffset; j++)
                    {
                        phase++;
                        for (int k = 0; k < _dynamicSource.Rows.Count; k++)
                        {
                            if (_dataSheetSource.Rows.Count > 0)
                            {
                                if (_dynamicSource.Rows[k][1].ToString() == _dataSheetSource.Rows[stt - 1][1].ToString() && _dynamicSource.Rows[k][2].ToString() == phase.ToString())
                                {
                                    ws.Cells.Rows[i][j].Value = _dynamicSource.Rows[k][0];
                                }
                            }
                        }
                    }
                }


                //
                int phaseSum = 0;
                for (int j = dynamicCollOffset; j < maxDynamicCollCount + dynamicCollOffset; j++)
                {
                    string topCellName = ws.Cells.Rows[dynamicRowOffset + 1][j].Name;
                    string botCellName = ws.Cells.Rows[dynamicRowOffset + _dataSheetSource.Rows.Count][j].Name;
                    var cell = ws.Cells.Rows[rowEnd][j];
                    cell.Formula = "=SUM(" + topCellName + ":" + botCellName + ")";
                    var style = cell.GetStyle();
                    style.ShrinkToFit = true;
                    cell.SetStyle(style);
                }


            }
        }
        private async Task<MemoryStream> CreateExcelFileDynamicColumn(ReportInfo info)
        {
            var fileAndDesign = await CreateExcelFileAndDesignDynamicColumn(info);
            DynamicColumnFomart(fileAndDesign.designWord.Workbook, fileAndDesign.data);
            MemoryStream str = new MemoryStream();
            fileAndDesign.designWord.Workbook.Save(str, Aspose.Cells.SaveFormat.Xlsx);
            return str;
        }
        public async Task<MemoryStream> GetReportFileDynamicColumn(ReportInfo info)
        {
            MemoryStream str = new MemoryStream();

            switch (info.TypeExport.ToLower())
            {
                case FileTypeConst.Excel:
                    str = await CreateExcelFileDynamicColumn(info);
                    break;
                case FileTypeConst.Pdf:
                    str = await CreateWordFile(info);
                    break;
                case FileTypeConst.Word:
                    str = await CreateWordFile(info);
                    break;
            }
            return str;
        }





        public async Task<dynamic> CreateExcelFileAndDesignPmGeneralExcel(ReportInfo info)
        {
            DataSet data = await GetDataFromStoreToReport(info.StoreName, info.Parameters);
            //data.Relations.Add(new DataRelation("CustomerType_Customer", data.Tables[1].Columns["Id"], data.Tables[0].Columns["CustomerTypeCode"], true));
            Workbook designer = new Workbook(directionReport + info.PathName);


            WorkbookDesigner designWord = new WorkbookDesigner(designer);

            foreach (var item in info.Values)
            {
                designWord.SetDataSource(item.Name, item.Value);
            }

            int TableCount = data.Tables.Count;

            for (int i = 0; i < TableCount; i++)
            {
                int rows = data.Tables[i].Rows.Count;
                int cols = data.Tables[i].Columns.Count;

                for (int row = 0; row < rows; row++)
                    for (int col = 0; col < cols; col++)
                    {
                        var obj = data.Tables[i].Rows[row][col];
                        if (obj.GetType() == typeof(string))
                        {
                            string _value = obj.ToString();
                            bool check = _value.StartsWith('=') ||
                                    _value.StartsWith('+') ||
                                    _value.StartsWith('-') ||
                                    _value.StartsWith('@');

                            if (check)
                            {
                                data.Tables[i].Rows[row][col] = _value.Insert(0, "'");
                            }
                        }
                    }
            }
            designWord.SetDataSource(data);

            designWord.Process(false);
            designWord.Workbook.FileName = info.PathName;
            designWord.Workbook.FileFormat = FileFormatType.Xlsx;
            designWord.Workbook.Settings.CalcMode = CalcModeType.Automatic;
            designWord.Workbook.Settings.RecalculateBeforeSave = true;
            designWord.Workbook.Settings.ReCalculateOnOpen = true;
            designWord.Workbook.Settings.CheckCustomNumberFormat = true;




            return new
            {
                designWord = designWord,
                data = data
            };
        }
        void PmGeneralExcelFomart(Workbook wb, DataSet data)
        {
            var _dataSheetSource = data.Tables[0];
            var _dynamicSource = data.Tables[1];
            foreach (var ws in wb.Worksheets)
            {
                int rowBegin, rowEnd, colBegin, colEnd;
                if (ws.Cells.FirstCell == null)
                {
                    continue;
                }

                rowBegin = ws.Cells.FirstCell.Row;
                rowEnd = ws.Cells.LastCell.Row;
                colBegin = ws.Cells.FirstCell.Column;
                colEnd = ws.Cells.MaxColumn;

                int dynamicCollOffset = -1, dynamicRowOffset = -1;

                //Tìm vị trí chèn
                for (int i = rowBegin; i < rowEnd; i++)
                {
                    for (int j = colBegin; j < colEnd; j++)
                    {
                        if (ws.Cells.Rows[i][j].Value != null)
                        {
                            string regex = @"%%PRINT_NAME";
                            Match match = Regex.Match(ws.Cells.Rows[i][j].Value.ToString(), regex);
                            if (match.Success)
                            {
                                dynamicRowOffset = i;
                                dynamicCollOffset = j;
                                goto findDynamicPositionDone;
                            }
                        }
                    }
                }

                findDynamicPositionDone:;

                if (dynamicCollOffset == -1 || dynamicRowOffset == -1)
                {
                    return;
                }

                //Tìm số lượng cột chèn thêm
                int maxDynamicCollCount = _dynamicSource.Rows.Count;
                int maxRowInDynamicSouce = _dynamicSource.Columns.Count;

                //Chèn thêm cột dựa vào số lượng cột đã tìm được
                for (int i = 0; i < maxDynamicCollCount - 1; i++)
                {
                    ws.Cells.InsertColumn(dynamicCollOffset + i, true);
                }

                //Điền tên ấn phẩm vào tên cột
                int index = 0;
                for (int i = dynamicCollOffset; i < maxDynamicCollCount + dynamicCollOffset; i++)
                {
                    ws.Cells.Rows[dynamicRowOffset][i].Value = _dynamicSource.Rows[index++][1];
                }

                int rowDyInd = 0, rowGrInd = 0;
                //Điền số lượng vào từng ấn phẩm1
                for (int i = dynamicCollOffset; i < maxDynamicCollCount + dynamicCollOffset; i++)
                {
                    rowDyInd = 0;
                    for (int j = dynamicRowOffset + 1; j <= dynamicRowOffset + _dataSheetSource.Rows.Count; j++)
                    {

                        if (
                            _dataSheetSource.Rows[rowDyInd][0].ToString() == _dynamicSource.Rows[rowGrInd][0].ToString()
                            &&
                            _dataSheetSource.Rows[rowDyInd][8].ToString() == _dynamicSource.Rows[rowGrInd][3].ToString()
                            )
                        {
                            ws.Cells.Rows[j][i].Value = _dynamicSource.Rows[rowGrInd][2];
                        }
                        else
                        {
                            ws.Cells.Rows[j][i].Value = 0;
                        }
                        rowDyInd++;
                    }
                    rowGrInd++;
                }

                int removedColCount = 0;
                //Gộp cột trùng
                PM_GopCotTrung(dynamicCollOffset, maxDynamicCollCount + dynamicCollOffset, dynamicRowOffset, dynamicRowOffset + _dataSheetSource.Rows.Count, ws, ref removedColCount);

                for (int j = dynamicCollOffset; j < maxDynamicCollCount + dynamicCollOffset - removedColCount; j++)
                {
                    string topCellName = ws.Cells.Rows[dynamicRowOffset + 1][j].Name;
                    string botCellName = ws.Cells.Rows[dynamicRowOffset + _dataSheetSource.Rows.Count][j].Name;
                    var cell = ws.Cells.Rows[rowEnd][j];
                    cell.Formula = "=SUM(" + topCellName + ":" + botCellName + ")";
                    var style = cell.GetStyle();
                    style.ShrinkToFit = true;
                    cell.SetStyle(style);
                }
            }
        }

        private void PM_GopCotTrung(int colStart, int colEnd, int rowStart, int rowEnd, Worksheet ws, ref int removedColCount)
        {
            if (colStart > colEnd - 1)
            {
                return;
            }
            //check trung cot
            if (ws.Cells.Rows[rowStart][colStart].Value.ToString() == ws.Cells.Rows[rowStart][colStart + 1].Value.ToString())
            {
                //gop cot
                for (int m = rowStart + 1; m < rowEnd; m++)
                {
                    long a = 0, b = 0;
                    Int64.TryParse(ws.Cells.Rows[m][colStart].Value.ToString(), out a);
                    Int64.TryParse(ws.Cells.Rows[m][colStart + 1].Value.ToString(), out b);
                    ws.Cells.Rows[m][colStart].Value = Math.Max(a, b);
                }
                ws.Cells.DeleteColumn(colStart + 1, true);
                removedColCount = removedColCount + 1;
                PM_GopCotTrung(colStart, colEnd - 1, rowStart, rowEnd, ws, ref removedColCount);
            }
            else
            {
                PM_GopCotTrung(colStart + 1, colEnd, rowStart, rowEnd, ws, ref removedColCount);
            }
        }

        private async Task<MemoryStream> CreateExcelFilePmGeneralExcel(ReportInfo info)
        {
            var fileAndDesign = await CreateExcelFileAndDesignPmGeneralExcel(info);
            PmGeneralExcelFomart(fileAndDesign.designWord.Workbook, fileAndDesign.data);
            MemoryStream str = new MemoryStream();
            fileAndDesign.designWord.Workbook.Save(str, Aspose.Cells.SaveFormat.Xlsx);
            return str;
        }
        public async Task<MemoryStream> GetReportFilePmGeneralExcel(ReportInfo info)
        {
            MemoryStream str = new MemoryStream();

            switch (info.TypeExport.ToLower())
            {
                case FileTypeConst.Excel:
                    str = await CreateExcelFilePmGeneralExcel(info);
                    break;
                case FileTypeConst.Pdf:
                    str = await CreateWordFile(info);
                    break;
                case FileTypeConst.Word:
                    str = await CreateWordFile(info);
                    break;
            }
            return str;
        }

        public async Task<MemoryStream> GetPivotReport(ReportInfo info)
        {
            var designWord = await CreatePivotExcelFile(info);
            MemoryStream str = new MemoryStream();
            designWord.Workbook.Save(str, Aspose.Cells.SaveFormat.Xlsx);
            return str;
        }
        public async Task<WorkbookDesigner> CreatePivotExcelFile(ReportInfo info)
        {
            DataSet data = await GetDataFromStoreToReport(info.StoreName, info.Parameters);
            Workbook designer = new Workbook(directionReport + info.PathName);
            WorkbookDesigner designWord = new WorkbookDesigner(designer);
            foreach (var item in info.Values)
            {
                designWord.SetDataSource(item.Name, item.Value);
            }

            int TableCount = data.Tables.Count;
            for (int i = 0; i < TableCount; i++)
            {
                int rows = data.Tables[i].Rows.Count;
                int cols = data.Tables[i].Columns.Count;

                for (int row = 0; row < rows; row++)
                    for (int col = 0; col < cols; col++)
                    {
                        var obj = data.Tables[i].Rows[row][col];
                        if (obj.GetType() == typeof(string))
                        {
                            string _value = obj.ToString();
                            bool check = _value.StartsWith('=') ||
                                    _value.StartsWith('+') ||
                                    _value.StartsWith('-') ||
                                    _value.StartsWith('@');

                            if (check)
                            {
                                data.Tables[i].Rows[row][col] = _value.Insert(0, "'");
                            }
                        }
                    }

            }
            designWord.SetDataSource(data);

            designWord.Process(false);
            designWord.Workbook.FileName = info.PathName;
            designWord.Workbook.FileFormat = FileFormatType.Xlsx;
            designWord.Workbook.Settings.CalcMode = CalcModeType.Automatic;
            designWord.Workbook.Settings.RecalculateBeforeSave = true;
            designWord.Workbook.Settings.ReCalculateOnOpen = true;
            designWord.Workbook.Settings.CheckCustomNumberFormat = true;

            InsertPivotData(designWord.Workbook, data);

            return designWord;
        }
        private void InsertPivotData(Workbook wb, DataSet data)
        {
            Worksheet ws = wb.Worksheets[0];
            if (ws.Cells.FirstCell == null)
            {
                return;
            }

            DataTable rowsForPivot = data.Tables[0];
            DataTable columnsForPivot = data.Tables[1];
            DataTable cellsForPivot = data.Tables[2];
            dynamic defaultCellValue = data.Tables[3].Rows[0][0];

            FindOptions opts = new FindOptions
            {
                LookInType = LookInType.Values,
                LookAtType = LookAtType.EntireContent
            };

            Cell dynamicRow = ws.Cells.Find("##DYNAMIC_ROW", null, opts);
            // insert new column after this column
            Cell dynamicCol = ws.Cells.Find("##DYNAMIC_COL", null, opts);

            int pivotRowsCount = rowsForPivot.Rows.Count;
            int pivotColsCount = columnsForPivot.Rows.Count;

            for (int i = 0; i < pivotColsCount; i++)
            {
                // Insert new column

                var insColPosition = dynamicCol.Column + i + 1;
                ws.Cells.InsertColumn(insColPosition, true);

                // Copy cells style from template column to new column
                ws.Cells.CopyColumn(ws.Cells, dynamicCol.Column, insColPosition);

                // Insert column name

                var colName = columnsForPivot.Rows[i][1]; // columnsForPivot.Rows[rowIndex][columnIndex]
                ws.Cells.Rows[dynamicCol.Row][insColPosition].Value = colName;

                // Insert cell value for new column

                var colID = columnsForPivot.Rows[i][0].ToString();

                for (int j = 0; j < pivotRowsCount; j++)
                {
                    var rowIndex = dynamicRow.Row + j + 1;
                    var rowID = ws.Cells.Rows[rowIndex][insColPosition].Value;

                    var result = cellsForPivot.Rows.Cast<DataRow>().FirstOrDefault(
                        r => r.Field<dynamic>("ROW_ID").ToString() == rowID.ToString() && r.Field<dynamic>("COL_ID").ToString() == colID.ToString());

                    var cellValue = result != null ? result.Field<dynamic>("CELL_VALUE") : defaultCellValue;

                    ws.Cells.Rows[rowIndex][insColPosition].Value = cellValue;
                }
            }

            // Delete template row and column
            ws.Cells.DeleteColumn(dynamicCol.Column);
            ws.Cells.DeleteRow(dynamicRow.Row);
        }

        public async Task<MemoryStream> GetReportWordGroupFile(ReportInfo info)
        {
            MemoryStream str = new MemoryStream();

            switch (info.TypeExport.ToLower())
            {
                case FileTypeConst.Pdf:
                    str = await CreateWordGroupFile(info);

                    break;
                case FileTypeConst.Word:
                    str = await CreateWordGroupFile(info);
                    break;
            }




            return str;
        }
        private async Task<MemoryStream> CreateWordGroupFile(ReportInfo info)
        {
            CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            DataSet data = await GetDataFromStoreToReport(info.StoreName, info.Parameters);

            // Check data from store exists column by groupId
            if (data.Tables.Count > 1 && info.groupId != null)
            {
                DataColumnCollection columns = data.Tables[0].Columns;
                DataColumnCollection columns1 = data.Tables[1].Columns;
                if (!columns.Contains(info.groupId) || !columns1.Contains(info.groupId))
                {
                    // Set type print is null to pass devide page
                    info.TypePrint = null;
                    info.groupId = null;
                }
            }

            // Nếu truyền groupId thì thực hiện merge theo group conditional
            if (info.groupId != null)
            {
                data.Relations.Add(new DataRelation("CustomerType_Customer", data.Tables[1].Columns[info.groupId], data.Tables[0].Columns[info.groupId], true));
            }



            if (info.TypePrint == "AEPS") // nếu là in hạch toán
            {

                MemoryStream streamformerge = new MemoryStream();
                Document docTotal = new Document(directionReport + info.PathName);

                for (int i = 0; i < data.Tables[1].Rows.Count; i++)
                {
                    DataSet dataSet = new DataSet();
                    DataTable dt0 = null;
                    DataTable dt1 = null;

                    string dt2 = data.Tables[1].Rows[i][info.groupId].ToString();

                    var rows = data.Tables[0].AsEnumerable()
                        .Where(x => ((string)x[info.groupId]) == dt2);
                    var rows1 = data.Tables[1].AsEnumerable()
                        .Where(x => ((string)x[info.groupId]) == dt2);

                    if (rows.Any())
                        dt0 = rows.CopyToDataTable();
                    if (rows.Any())
                        dt1 = rows1.CopyToDataTable();

                    dt0.TableName = "Table0";
                    dt1.TableName = "Table1";

                    dataSet.Tables.Add(dt0);
                    dataSet.Tables.Add(dt1);

                    dataSet.Tables[0].TableName = "Table0";
                    dataSet.Tables[1].TableName = "Table1";

                    // if first merge using total
                    if (i == 0)
                    {

                        docTotal.MailMerge.CleanupParagraphsWithPunctuationMarks = true;
                        docTotal.MailMerge.CleanupOptions = MailMergeCleanupOptions.RemoveUnusedFields | MailMergeCleanupOptions.RemoveUnusedRegions;
                        docTotal.MailMerge.Execute(info.Values.Select(x => x.Name).ToArray(), info.Values.Select(x => x.Value).ToArray());
                        docTotal.MailMerge.ExecuteWithRegions(dataSet);
                    }
                    else // merge the loop doc to total
                    {
                        //new doc to merge for loop
                        Document docMerge = new Document(directionReport + info.PathName);

                        docMerge.MailMerge.CleanupParagraphsWithPunctuationMarks = true;
                        docMerge.MailMerge.CleanupOptions = MailMergeCleanupOptions.RemoveUnusedFields | MailMergeCleanupOptions.RemoveUnusedRegions;
                        docMerge.MailMerge.Execute(info.Values.Select(x => x.Name).ToArray(), info.Values.Select(x => x.Value).ToArray());
                        docMerge.MailMerge.ExecuteWithRegions(dataSet);

                        docTotal.AppendDocument(docMerge, ImportFormatMode.KeepSourceFormatting);
                    }
                }

                switch (info.TypeExport.ToLower())
                {
                    case FileTypeConst.Pdf: docTotal.Save(streamformerge, Aspose.Words.SaveFormat.Pdf); break;
                    case FileTypeConst.Word: docTotal.Save(streamformerge, Aspose.Words.SaveFormat.Docx); break;
                }
                return streamformerge;
            }
            else
            {
                Document doc = new Document(directionReport + info.PathName);


                doc.MailMerge.CleanupParagraphsWithPunctuationMarks = true;
                doc.MailMerge.CleanupOptions = MailMergeCleanupOptions.RemoveUnusedFields | MailMergeCleanupOptions.RemoveUnusedRegions;

                doc.MailMerge.Execute(info.Values.Select(x => x.Name).ToArray(), info.Values.Select(x => x.Value).ToArray());


                doc.MailMerge.ExecuteWithRegions(data);


                MemoryStream stream = new MemoryStream();

                switch (info.TypeExport.ToLower())
                {
                    case FileTypeConst.Pdf:
                        doc.Save(stream, Aspose.Words.SaveFormat.Pdf);
                        break;
                    case FileTypeConst.Word:
                        doc.Save(stream, Aspose.Words.SaveFormat.Docx);
                        break;
                }


                return stream;
            }

        }

        public async Task<MemoryStream> GetReportWordOneByOne(ReportInfo info, string byField)
        {
            MemoryStream str = new MemoryStream();

            switch (info.TypeExport.ToLower())
            {
                case FileTypeConst.Pdf:
                    str = await CreateWordOneByOne(info, byField);
                    break;
                case FileTypeConst.Word:
                    str = await CreateWordOneByOne(info, byField);
                    break;
            }
            return str;
        }

        // In một file chia thành nhiều trang theo tham số chung truyền vào (vd: đơn vị, loại, ...) có nhiều details
        private async Task<MemoryStream> CreateWordOneByOne(ReportInfo info, string byField)
        {
            CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            DataSet data = await GetDataFromStoreToReport(info.StoreName, info.Parameters);

            if (string.IsNullOrEmpty(byField))
            {
                byField = "BRANCH_CODE";
            }
            data.Relations.Add(new DataRelation("CustomerType_Customer", data.Tables[1].Columns[byField], data.Tables[0].Columns[byField], true));

            MemoryStream streamformerge = new MemoryStream();
            Document docTotal = new Document(directionReport + info.PathName);

            for (int i = 0; i < data.Tables[1].Rows.Count; i++)
            {
                DataSet dataSet = new DataSet();
                DataTable dt0 = null;
                DataTable dt1 = null;

                string dt2 = data.Tables[1].Rows[i][byField].ToString();

                var rows = data.Tables[0].AsEnumerable()
                    .Where(x => ((string)x[byField]) == dt2);
                var rows1 = data.Tables[1].AsEnumerable()
                    .Where(x => ((string)x[byField]) == dt2);

                if (rows.Any())
                    dt0 = rows.CopyToDataTable();
                if (rows.Any())
                    dt1 = rows1.CopyToDataTable();

                dt0.TableName = "Table0";
                dt1.TableName = "Table1";

                dataSet.Tables.Add(dt0);
                dataSet.Tables.Add(dt1);

                // Set lại tên thứ tự table TH DB có gen tên khác
                dataSet.Tables[0].TableName = "Table0";
                dataSet.Tables[1].TableName = "Table1";

                // if first merge using total
                if (i == 0)
                {

                    docTotal.MailMerge.CleanupParagraphsWithPunctuationMarks = true;
                    docTotal.MailMerge.CleanupOptions = MailMergeCleanupOptions.RemoveUnusedFields | MailMergeCleanupOptions.RemoveUnusedRegions;
                    docTotal.MailMerge.Execute(info.Values.Select(x => x.Name).ToArray(), info.Values.Select(x => x.Value).ToArray());
                    docTotal.MailMerge.ExecuteWithRegions(dataSet);
                }
                else // merge the loop doc to total
                {
                    //new doc to merge for loop
                    Document docMerge = new Document(directionReport + info.PathName);

                    docMerge.MailMerge.CleanupParagraphsWithPunctuationMarks = true;
                    docMerge.MailMerge.CleanupOptions = MailMergeCleanupOptions.RemoveUnusedFields | MailMergeCleanupOptions.RemoveUnusedRegions;
                    docMerge.MailMerge.Execute(info.Values.Select(x => x.Name).ToArray(), info.Values.Select(x => x.Value).ToArray());
                    docMerge.MailMerge.ExecuteWithRegions(dataSet);

                    docTotal.AppendDocument(docMerge, ImportFormatMode.KeepSourceFormatting);
                }
            }

            switch (info.TypeExport.ToLower())
            {
                case FileTypeConst.Pdf: docTotal.Save(streamformerge, Aspose.Words.SaveFormat.Pdf); break;
                case FileTypeConst.Word: docTotal.Save(streamformerge, Aspose.Words.SaveFormat.Docx); break;
            }
            return streamformerge;

        }

        private DataTable getDataPrintTemp(DataTable dtIn, string Qty)
        {
            int currentLenght = 0;
            int _Qty = int.Parse(Qty);
            DataTable dt = new DataTable();
            dt.Columns.Add("TSCD", typeof(string));
            dt.Columns.Add("MSTS", typeof(string));
            dt.Columns.Add("TGSD", typeof(string));
            dt.Columns.Add("DVSD", typeof(string));
            dt.Columns.Add("DEPT_CODE", typeof(string));
            dt.Columns.Add("SERIAL", typeof(string));
            dt.Columns.Add("NHOMTS", typeof(string));
            dt.Columns.Add("TENTS", typeof(string));
            dt.Columns.Add("GHICHU", typeof(string));
            dt.Columns.Add("DONVI_SD", typeof(string));
            dt.Columns.Add("PBSD", typeof(string));
            dt.Columns.Add("Lenght", typeof(int));
            for (int i = 0; i < dtIn.Rows.Count; i++)
            {
                for (int j = 0; j < _Qty; j++)
                {
                    dt.Rows.Add(dtIn.Rows[i]["TYPE_ID"].ToString(),
                    dtIn.Rows[i]["ASSET_CODE"].ToString(),
                    DateTime.Parse(dtIn.Rows[i]["BUY_DATE_KT"].ToString()).ToString("dd/MM/yyyy"),
                    dtIn.Rows[i]["DVSD"].ToString(),
                    dtIn.Rows[i]["DEPT_CODE"].ToString(),
                    dtIn.Rows[i]["ASSET_SERIAL_NO"].ToString(),
                    dtIn.Rows[i]["NHOM_TS"].ToString(),
                    dtIn.Rows[i]["ASSET_NAME"].ToString(),
                    dtIn.Rows[i]["NOTES"].ToString(),
                    dtIn.Rows[i]["BRANCH_NAME"].ToString(),
                    dtIn.Rows[i]["DEP_NAME"].ToString()
                    );
                    int length = dtIn.Rows[i]["ASSET_NAME"].ToString().Length + dtIn.Rows[i]["BRANCH_NAME"].ToString().Length + dtIn.Rows[i]["DEP_NAME"].ToString().Length + 48;
                    if (currentLenght < length)
                    {
                        currentLenght = length;
                    }
                }
            }

            DataTable dt1 = new DataTable();
            dt1.Columns.Add("TSCD1", typeof(string));
            dt1.Columns.Add("TSCD2", typeof(string));
            dt1.Columns.Add("TSCD3", typeof(string));
            dt1.Columns.Add("MSTS1", typeof(string));
            dt1.Columns.Add("MSTS2", typeof(string));
            dt1.Columns.Add("MSTS3", typeof(string));
            dt1.Columns.Add("TGSD1", typeof(string));
            dt1.Columns.Add("TGSD2", typeof(string));
            dt1.Columns.Add("TGSD3", typeof(string));
            dt1.Columns.Add("DVSD1", typeof(string));
            dt1.Columns.Add("DVSD2", typeof(string));
            dt1.Columns.Add("DVSD3", typeof(string));
            dt1.Columns.Add("BarCode1", typeof(byte[]));
            dt1.Columns.Add("BarCode2", typeof(byte[]));
            dt1.Columns.Add("BarCode3", typeof(byte[]));
            dt1.Columns.Add("DEPT_CODE1", typeof(string));
            dt1.Columns.Add("DEPT_CODE2", typeof(string));
            dt1.Columns.Add("DEPT_CODE3", typeof(string));
            dt1.Columns.Add("SERIAL1", typeof(string));
            dt1.Columns.Add("SERIAL2", typeof(string));
            dt1.Columns.Add("SERIAL3", typeof(string));
            int rowcount = dt.Rows.Count;
            int row = rowcount / 3;
            string barcode1 = "";
            string barcode2 = "";
            string barcode3 = "";
            //if (row * 2 + 1 == rowcount)
            //{
            //    barcode1 = dt.Rows[row * 2][1] + ";" + dt.Rows[row * 2][2]
            //       + ";" + dt.Rows[row * 2][4] + ";" + dt.Rows[row * 2][0];
            //    dt1.Rows.Add(dt.Rows[row * 2][0],
            //        null, dt.Rows[row * 2][1], null,
            //        dt.Rows[row * 2][2], null,
            //        dt.Rows[row * 2][3], null,
            //         GenerateQrCode(barcode1),
            //        null,
            //        dt.Rows[row * 2][4],
            //        null
            //        );
            //}
            //else
            //{
            //ASS_CODE;USE_DATE;BRANCH_ID;TYPE_ID
            for (int i = 0; i < row; i++)
            {
                //barcode1 = "NHÓM TS: " + dt.Rows[i * 2][6] + ";" + "\n"
                //    + "MÃ TS: " + dt.Rows[i * 2][1] + ";" + "\n"
                //    + "TÊN TS: " + dt.Rows[i * 2][7] + ";" + "\n"
                //    + "NGÀY SD: " + dt.Rows[i * 2][2] + ";" + "\n"
                //    + "DVSD: " + dt.Rows[i * 2][3] + ";" + "\n"
                //    + "SERIAL: " + dt.Rows[i * 2][5] + ";" + "\n"
                //    + "GHI CHÚ: " + dt.Rows[i * 2][8];
                //barcode2 = "NHÓM TS: " + dt.Rows[i * 2 + 1][6] + ";" + "\n"
                //    + "MÃ TS: " + dt.Rows[i * 2 + 1][1] + ";" + "\n"
                //    + "TÊN TS: " + dt.Rows[i * 2 + 1][7] + ";" + "\n"
                //    + "NGÀY SD: " + dt.Rows[i * 2 + 1][2] + ";" + "\n"
                //    + "DVSD: " + dt.Rows[i * 2 + 1][3] + ";" + "\n"
                //    + "SERIAL: " + dt.Rows[i * 2 + 1][5] + ";" + "\n"
                //    + "GHI CHÚ: " + dt.Rows[i * 2 + 1][8];
                //barcode1 = "NHÓM TS: " + dt.Rows[i * 2][6] + ";" + "\n"
                barcode1 = "<" + dt.Rows[i * 3][1] + ">"
                   + "<" + dt.Rows[i * 3][7] + ">"
                   + "<" + dt.Rows[i * 3][9] + ">";
                if (dt.Rows[i * 3][10].ToString() != "")
                    barcode1 = barcode1 + "< " + dt.Rows[i * 3][10] + ">";
                barcode1 = barcode1 + "<" + dt.Rows[i * 3][2] + ">";
                if (dt.Rows[i * 3][5].ToString() != "")
                    barcode1 = barcode1 + "<" + dt.Rows[i * 3][5] + ">";

                barcode2 = "<" + dt.Rows[i * 3 + 1][1] + ">"
                         + "<" + dt.Rows[i * 3 + 1][7] + ">"
                         + "<" + dt.Rows[i * 3 + 1][9] + ">";
                if (dt.Rows[i * 3 + 1][10].ToString() != "")
                    barcode2 = barcode2 + "<" + dt.Rows[i * 3 + 1][10] + ">";
                barcode2 = barcode2 + "<" + dt.Rows[i * 3 + 1][2] + ">";
                if (dt.Rows[i * 3 + 1][5].ToString() != "")

                    barcode3 = barcode3 + "<" + dt.Rows[i * 3 + 2][5] + ">";
                barcode3 = "<" + dt.Rows[i * 3 + 2][1] + ">"
                        + "<" + dt.Rows[i * 3 + 2][7] + ">"
                        + "<" + dt.Rows[i * 3 + 2][9] + ">";
                if (dt.Rows[i * 3 + 2][10].ToString() != "")
                    barcode3 = barcode3 + "<" + dt.Rows[i * 3 + 2][10] + ">";
                barcode3 = barcode3 + "<" + dt.Rows[i * 3 + 2][2] + ">";
                if (dt.Rows[i * 3 + 2][5].ToString() != "")
                    barcode3 = barcode3 + "<" + dt.Rows[i * 2 + 2][5] + ">";

                dt1.Rows.Add(
                    dt.Rows[i * 3][6],
                    dt.Rows[i * 3 + 1][6],
                    dt.Rows[i * 3 + 2][6],
                    dt.Rows[i * 3][1],
                    dt.Rows[i * 3 + 1][1],
                    dt.Rows[i * 3 + 2][1],
                    dt.Rows[i * 3][2],
                    dt.Rows[i * 3 + 1][2],
                    dt.Rows[i * 3 + 2][2],
                    dt.Rows[i * 3][3],
                    dt.Rows[i * 3 + 1][3],
                    dt.Rows[i * 3 + 2][3],
                    GenerateQrCode(barcode1, currentLenght),
                    GenerateQrCode(barcode2, currentLenght),
                    GenerateQrCode(barcode3, currentLenght),
                    dt.Rows[i * 3][4],
                    dt.Rows[i * 3 + 1][4],
                    dt.Rows[i * 3 + 2][4],
                    dt.Rows[i * 3][5],
                    dt.Rows[i * 3 + 1][5],
                    dt.Rows[i * 3 + 2][5]
                    );
            }
            if (row * 3 + 1 == rowcount)
            {
                barcode1 = "<" + dt.Rows[row * 3][1] + ">"
                    + "<" + dt.Rows[row * 3][7] + ">"
                    + "<" + dt.Rows[row * 3][9] + " >";
                if (dt.Rows[row * 3][10].ToString() != "")
                    barcode1 = barcode1 + "<" + dt.Rows[row * 3][10] + ">";
                barcode1 = barcode1 + "<" + dt.Rows[row * 3][2] + ">";
                if (dt.Rows[row * 3][5].ToString() != "")
                    barcode1 = barcode1 + "<" + dt.Rows[row * 2][5] + ">";
                dt1.Rows.Add(dt.Rows[row * 3][6], null, null,
                    dt.Rows[row * 3][1], null, null,
                    dt.Rows[row * 3][2], null, null,
                    dt.Rows[row * 3][3], null, null,
                     GenerateQrCode(barcode1, currentLenght),
                    null, null,
                    dt.Rows[row * 3][4],
                    null, null,
                      dt.Rows[row * 3][5],
                    null, null
                    );
            }
            if (row * 3 + 2 == rowcount)
            {
                barcode1 = "<" + dt.Rows[row * 3][1] + ">"
                    + "<" + dt.Rows[row * 3][7] + ">"
                    + "<" + dt.Rows[row * 3][9] + ">";
                if (dt.Rows[row * 3][10].ToString() != "")
                    barcode1 = barcode1 + "<" + dt.Rows[row * 3][10] + ">";
                barcode1 = barcode1 + "<" + dt.Rows[row * 3][2] + ">";
                if (dt.Rows[row * 3][5].ToString() != "")
                    barcode1 = barcode1 + "<" + dt.Rows[row * 2 + 1][5] + ">";
                // barcode 2
                barcode2 = "<" + dt.Rows[row * 3 + 1][1] + ">"
                         + "<" + dt.Rows[row * 3 + 1][7] + ">"
                         + "<" + dt.Rows[row * 3 + 1][9] + ">";
                if (dt.Rows[row * 3 + 1][10].ToString() != "")
                    barcode2 = barcode2 + "<" + dt.Rows[row * 3 + 1][10] + ">";
                barcode2 = barcode2 + "<" + dt.Rows[row * 3 + 1][2] + ">";
                //barcode2 = "<" + dt.Rows[row * 3 + 1][1] + ">"
                //    + "<" + dt.Rows[row * 3 + 1][7] + ">"
                //    + "<" + dt.Rows[row * 3 + 1][9] + ">";
                //if (dt.Rows[row * 3 + 1][10].ToString() != "")
                //    barcode2 = barcode1 + "<" + dt.Rows[row * 3 + 1][10] + ">";
                //barcode2 = barcode2 + "<" + dt.Rows[row * 3 + 1][2] + ">";

                if (dt.Rows[row * 3 + 1][5].ToString() != "")
                    barcode2 = barcode2 + "<" + dt.Rows[row * 2 + 1][5] + ">";
                dt1.Rows.Add(
                    dt.Rows[row * 3][6], dt.Rows[row * 3 + 1][6], null,
                    dt.Rows[row * 3][1], dt.Rows[row * 3 + 1][1], null,
                    dt.Rows[row * 3][2], dt.Rows[row * 3 + 1][2], null,
                    dt.Rows[row * 3][3], dt.Rows[row * 3 + 1][3], null,
                    GenerateQrCode(barcode1, currentLenght), GenerateQrCode(barcode2, currentLenght), null,
                    dt.Rows[row * 3][4], dt.Rows[row * 3 + 1][4], null,
                    dt.Rows[row * 3][5], dt.Rows[row * 3 + 1][5], null
                    );
            }
            //}
            return dt1;
        }
        private byte[] GenerateQrCode(string data, int maxStr)
        {
            //PHONGNT Tạm thời cộng chuỗi để QR đều nhau
            int LenInsert = maxStr == 96 ? maxStr + 20 : maxStr;
            if (data.Length < LenInsert)
            {
                LenInsert = maxStr - data.Length;
                while (LenInsert > 0)
                {
                    data += " ";
                    LenInsert--;
                }
            }
            //END

            //var writer = new BarcodeWriter();
            //QrCodeEncodingOptions options = new QrCodeEncodingOptions
            //{
            //    DisableECI = true,
            //    CharacterSet = "UTF-8",
            //    Width = 450,
            //    Height = 450,
            //    Margin = 0,
            //    ErrorCorrection = ZXing.QrCode.Internal.ErrorCorrectionLevel.L
            //};


            //writer.Format = BarcodeFormat.QR_CODE;
            //writer.Options = options;
            //var result = writer.Write(data);

            return null;
        }

        private byte[] ImageToByte2(Bitmap img)
        {
            byte[] byteArray = new byte[0];
            using (MemoryStream stream = new MemoryStream())
            {
                img.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                stream.Close();
                byteArray = stream.ToArray();
            }
            return byteArray;
        }

        #region Phucvh 24/10/22 In nhãn Move từ AN BÌNH
        private async Task<MemoryStream> CreateWordFileQR_VB(ReportInfo info)
        {
            CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            DataSet data = await GetDataFromStoreToReport(info.StoreName, info.Parameters);
            var qty = info.Parameters.FirstOrDefault(item => item.Name == "NUMQR").Value;
            var tmp = getDataPrintTemp_VB(data.Tables[0], qty.ToString());
            tmp.TableName = "Newrpt";
            var newData = new DataSet();
            newData.Tables.Add(tmp);
            //data.Relations.Add(new DataRelation("CustomerType_Customer", data.Tables[1].Columns["Id"], data.Tables[0].Columns["CustomerTypeCode"], true));


            Document doc = new Document(directionReport + info.PathName);

            DocumentBuilder builder = new DocumentBuilder(doc)
            {
                PageSetup =
                {
                    TopMargin = 23.8
                    //BottomMargin = 70.0
                },

            };

            doc.MailMerge.FieldMergingCallback = new HandleMergeImageField();

            doc.MailMerge.CleanupParagraphsWithPunctuationMarks = true;
            doc.MailMerge.CleanupOptions = MailMergeCleanupOptions.RemoveUnusedFields | MailMergeCleanupOptions.RemoveUnusedRegions | MailMergeCleanupOptions.RemoveEmptyParagraphs;

            doc.MailMerge.Execute(info.Values.Select(x => x.Name).ToArray(), info.Values.Select(x => x.Value).ToArray());


            doc.MailMerge.ExecuteWithRegions(newData);


            MemoryStream stream = new MemoryStream();

            switch (info.TypeExport.ToLower())
            {
                case FileTypeConst.Pdf:
                    doc.Save(stream, Aspose.Words.SaveFormat.Pdf);
                    break;
                case FileTypeConst.Word:
                    doc.Save(stream, Aspose.Words.SaveFormat.Docx);
                    break;
            }


            return stream;
        }
        private DataTable getDataPrintTemp_VB(DataTable dtIn, string Qty)
        {
            int _Qty = int.Parse(Qty);
            DataTable dt = new DataTable();

            dt.Columns.Add("GROUP_NAME", typeof(string));
            dt.Columns.Add("ASSET_CODE", typeof(string));
            dt.Columns.Add("ASSET_NAME", typeof(string));
            dt.Columns.Add("TGSD", typeof(string));
            dt.Columns.Add("DVSD", typeof(string));
            dt.Columns.Add("EMP_NAME", typeof(string));
            dt.Columns.Add("ASSET_SERIAL_NO", typeof(string));
            dt.Columns.Add("NOTES", typeof(string));

            for (int i = 0; i < dtIn.Rows.Count; i++)
            {
                for (int j = 0; j < _Qty; j++)
                {
                    dt.Rows.Add(
                        dtIn.Rows[i]["NHOM_TS"].ToString(),
                        dtIn.Rows[i]["ASSET_CODE"].ToString(),
                        dtIn.Rows[i]["ASSET_NAME"].ToString(),
                        dtIn.Rows[i]["USE_DATE"].ToString(),
                        dtIn.Rows[i]["DVSD"].ToString(),
                        dtIn.Rows[i]["EMP_NAME"].ToString(),
                        dtIn.Rows[i]["ASSET_SERIAL_NO"].ToString(),
                        dtIn.Rows[i]["NOTES"].ToString()
                        );
                }
            }
            DataTable dt1 = new DataTable();
            //1
            dt1.Columns.Add("BarCode1", typeof(byte[]));
            dt1.Columns.Add("NHOM_TS1", typeof(string));
            dt1.Columns.Add("ASSET_CODE1", typeof(string));
            dt1.Columns.Add("ASSET_NAME1", typeof(string));
            dt1.Columns.Add("USE_DATE1", typeof(string));
            dt1.Columns.Add("DVSD1", typeof(string));
            dt1.Columns.Add("EMP_NAME1", typeof(string));
            dt1.Columns.Add("ASSET_SERIAL_NO1", typeof(string));
            dt1.Columns.Add("NOTES1", typeof(string));
            //2
            dt1.Columns.Add("BarCode2", typeof(byte[]));
            dt1.Columns.Add("NHOM_TS2", typeof(string));
            dt1.Columns.Add("ASSET_CODE2", typeof(string));
            dt1.Columns.Add("ASSET_NAME2", typeof(string));
            dt1.Columns.Add("USE_DATE2", typeof(string));
            dt1.Columns.Add("DVSD2", typeof(string));
            dt1.Columns.Add("EMP_NAME2", typeof(string));
            dt1.Columns.Add("ASSET_SERIAL_NO2", typeof(string));
            dt1.Columns.Add("NOTES2", typeof(string));
            //3
            dt1.Columns.Add("BarCode3", typeof(byte[]));
            dt1.Columns.Add("NHOM_TS3", typeof(string));
            dt1.Columns.Add("ASSET_CODE3", typeof(string));
            dt1.Columns.Add("ASSET_NAME3", typeof(string));
            dt1.Columns.Add("USE_DATE3", typeof(string));
            dt1.Columns.Add("DVSD3", typeof(string));
            dt1.Columns.Add("EMP_NAME3", typeof(string));
            dt1.Columns.Add("ASSET_SERIAL_NO3", typeof(string));
            dt1.Columns.Add("NOTES3", typeof(string));

            int rowcount = dt.Rows.Count;
            int row = rowcount / 3;
            string barcode1 = "";
            string barcode2 = "";
            string barcode3 = "";

            //
            for (int i = 0; i < row; i++)
            {
                //barcode1 = "NHÓM TS: " + dt.Rows[i * 3][0] + ";" + "\n"
                //         + "MÃ TS: " + dt.Rows[i * 3][1] + ";" + "\n"
                //         + "TÊN TS: " + dt.Rows[i * 3][2] + ";" + "\n"
                //         + "NGÀY SD: " + dt.Rows[i * 3][3] + ";" + "\n"
                //         + "DVSD: " + dt.Rows[i * 3][4] + ";" + "\n"
                //         + "Người SD: " + dt.Rows[i * 3][5] + ";" + "\n"
                //         + "SERIAL: " + dt.Rows[i * 3][6] + ";" + "\n";

                //barcode2 = "NHÓM TS: " + dt.Rows[i * 3 + 1][0] + ";" + "\n"
                //         + "MÃ TS: " + dt.Rows[i * 3 + 1][1] + ";" + "\n"
                //         + "TÊN TS: " + dt.Rows[i * 3 + 1][2] + ";" + "\n"
                //         + "NGÀY SD: " + dt.Rows[i * 3 + 1][3] + ";" + "\n"
                //         + "DVSD: " + dt.Rows[i * 3 + 1][4] + ";" + "\n"
                //         + "Người SD: " + dt.Rows[i * 3 + 1][5] + ";" + "\n"
                //         + "SERIAL: " + dt.Rows[i * 3 + 1][6] + ";" + "\n";

                //barcode3 = "NHÓM TS: " + dt.Rows[i * 3 + 2][0] + ";" + "\n"
                //         + "MÃ TS: " + dt.Rows[i * 3 + 2][1] + ";" + "\n"
                //         + "TÊN TS: " + dt.Rows[i * 3 + 2][2] + ";" + "\n"
                //         + "NGÀY SD: " + dt.Rows[i * 3 + 2][3] + ";" + "\n"
                //         + "DVSD: " + dt.Rows[i * 3 + 2][4] + ";" + "\n"
                //         + "Người SD: " + dt.Rows[i * 3 + 2][5] + ";" + "\n"
                //         + "SERIAL: " + dt.Rows[i * 3 + 2][6] + ";" + "\n";

                barcode1 = "MÃ TS: " + dt.Rows[i * 3][1] + ";" + "\n";

                barcode2 = "MÃ TS: " + dt.Rows[i * 3 + 1][1] + ";" + "\n";

                barcode3 = "MÃ TS: " + dt.Rows[i * 3 + 2][1] + ";" + "\n";
                //AddTable

                dt1.Rows.Add(
                    GenerateQrCode_VB(barcode1),
                    dt.Rows[i * 3][0],
                    dt.Rows[i * 3][1],
                    dt.Rows[i * 3][2],
                    dt.Rows[i * 3][3],
                    dt.Rows[i * 3][4],
                    dt.Rows[i * 3][5],
                    dt.Rows[i * 3][6],
                    dt.Rows[i * 3][7],

                    GenerateQrCode_VB(barcode2),
                    dt.Rows[i * 3 + 1][0],
                    dt.Rows[i * 3 + 1][1],
                    dt.Rows[i * 3 + 1][2],
                    dt.Rows[i * 3 + 1][3],
                    dt.Rows[i * 3 + 1][4],
                    dt.Rows[i * 3 + 1][5],
                    dt.Rows[i * 3 + 1][6],
                    dt.Rows[i * 3 + 1][7],

                    GenerateQrCode_VB(barcode3),
                    dt.Rows[i * 3 + 2][0],
                    dt.Rows[i * 3 + 2][1],
                    dt.Rows[i * 3 + 2][2],
                    dt.Rows[i * 3 + 2][3],
                    dt.Rows[i * 3 + 2][4],
                    dt.Rows[i * 3 + 2][5],
                    dt.Rows[i * 3 + 2][6],
                    dt.Rows[i * 3 + 2][7]
                    );
            }
            if (row * 3 + 1 == rowcount)
            {
                //barcode1 = "NHÓM TS: " + dt.Rows[row * 3][0] + ";" + "\n"
                //         + "MÃ TS: " + dt.Rows[row * 3][1] + ";" + "\n"
                //         + "TÊN TS: " + dt.Rows[row * 3][2] + ";" + "\n"
                //         + "NGÀY SD: " + dt.Rows[row * 3][3] + ";" + "\n"
                //         + "DVSD: " + dt.Rows[row * 3][4] + ";" + "\n"
                //         + "Người SD: " + dt.Rows[row * 3][5] + ";" + "\n"
                //         + "SERIAL: " + dt.Rows[row * 3][6] + ";" + "\n";

                barcode1 = "MÃ TS: " + dt.Rows[row * 3][1] + ";" + "\n";

                dt1.Rows.Add(
                    GenerateQrCode_VB(barcode1),
                    dt.Rows[row * 3][0],
                    dt.Rows[row * 3][1],
                    dt.Rows[row * 3][2],
                    dt.Rows[row * 3][3],
                    dt.Rows[row * 3][4],
                    dt.Rows[row * 3][5],
                    dt.Rows[row * 3][6],
                    dt.Rows[row * 3][7],

                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,

                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null
                    );
            }
            if (row * 3 + 2 == rowcount)
            {
                //barcode1 = "NHÓM TS: " + dt.Rows[row * 3][0] + ";" + "\n"
                //         + "MÃ TS: " + dt.Rows[row * 3][1] + ";" + "\n"
                //         + "TÊN TS: " + dt.Rows[row * 3][2] + ";" + "\n"
                //         + "NGÀY SD: " + dt.Rows[row * 3][3] + ";" + "\n"
                //         + "DVSD: " + dt.Rows[row * 3][4] + ";" + "\n"
                //         + "Người SD: " + dt.Rows[row * 3][5] + ";" + "\n"
                //         + "SERIAL: " + dt.Rows[row * 3][6] + ";" + "\n";

                //barcode2 = "NHÓM TS: " + dt.Rows[row * 3 + 1][0] + ";" + "\n"
                //         + "MÃ TS: " + dt.Rows[row * 3 + 1][1] + ";" + "\n"
                //         + "TÊN TS: " + dt.Rows[row * 3 + 1][2] + ";" + "\n"
                //         + "NGÀY SD: " + dt.Rows[row * 3 + 1][3] + ";" + "\n"
                //         + "DVSD: " + dt.Rows[row * 3 + 1][4] + ";" + "\n"
                //         + "Người SD: " + dt.Rows[row * 3 + 1][5] + ";" + "\n"
                //         + "SERIAL: " + dt.Rows[row * 3 + 1][6] + ";" + "\n";

                barcode1 = "MÃ TS: " + dt.Rows[row * 3][1] + ";" + "\n";

                barcode2 = "MÃ TS: " + dt.Rows[row * 3 + 1][1] + ";" + "\n";

                dt1.Rows.Add(
                    GenerateQrCode_VB(barcode1),
                    dt.Rows[row * 3][0],
                    dt.Rows[row * 3][1],
                    dt.Rows[row * 3][2],
                    dt.Rows[row * 3][3],
                    dt.Rows[row * 3][4],
                    dt.Rows[row * 3][5],
                    dt.Rows[row * 3][6],
                    dt.Rows[row * 3][7],

                    GenerateQrCode_VB(barcode2),
                    dt.Rows[row * 3 + 1][0],
                    dt.Rows[row * 3 + 1][1],
                    dt.Rows[row * 3 + 1][2],
                    dt.Rows[row * 3 + 1][3],
                    dt.Rows[row * 3 + 1][4],
                    dt.Rows[row * 3 + 1][5],
                    dt.Rows[row * 3 + 1][6],
                    dt.Rows[row * 3 + 1][7],

                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null
                    );
            }
            return dt1;
        }

        private DataSet getDataPrintImage(DataSet data)
        {
            DataSet dsResult = data;
            DataSet dsTemp = new DataSet();

            for (int i = 0; i < data.Tables.Count; i++)
            {

                //Nếu xuất image 
                if (data.Tables[i].Columns.IndexOf("EXPORT_IMAGE") != -1)
                {
                    DataTable newDataTable = new DataTable();
                    newDataTable.TableName = "TblExportImage" + (i + 1).ToString();
                    newDataTable.Columns.Add("QtyImageOnRow", typeof(int));
                    newDataTable.Columns.Add("Image", typeof(byte[]));
                    newDataTable.Columns.Add("Title", typeof(string));
                    if (data.Tables[i].Columns.IndexOf("QtyImageOnRow") != -1)
                    {
                        newDataTable.Rows.Add(Convert.ToInt32(data.Tables[i].Rows[0][Convert.ToInt32(data.Tables[i].Columns.IndexOf("QtyImageOnRow"))]), null);
                    }
                    else
                    {
                        newDataTable.Rows.Add(1);
                    }

                    //đếm số dòng trong 1 table
                    int rowCount = data.Tables[i].Rows.Count;

                    //get index title image
                    int indexColumnTitle = data.Tables[i].Columns["Title"].Ordinal;

                    for (int j = 0; j < rowCount; j++)
                    {
                        //Đếm số cột trong 1 table
                        int columnCount = data.Tables[i].Columns.Count;
                        for (int k = 0; k < columnCount; k++)
                        {
                            if (data.Tables[i].Columns[k].ColumnName.Contains("EXPORT_IMAGE_"))
                            {
                                data.Tables[i].Rows[j][k] = directionImage + data.Tables[i].Rows[j][k];

                                var result = GetAndSetSizeImage(data.Tables[i], j, k);
                                try
                                {
                                    string dataTitle = "";
                                    if (columnCount != -1)
                                    {
                                        dataTitle = data.Tables[i].Rows[j][indexColumnTitle].ToString();
                                    }

                                    newDataTable.Rows.Add(newDataTable.Rows[0][0], ImageToByte2(result), dataTitle);
                                }
                                catch (Exception ex)
                                {
                                    throw;
                                }
                            }
                        }
                    }

                    if (newDataTable.Rows.Count > 0)
                    {
                        dsTemp.Tables.Add(newDataTable);
                    }
                }
            }

            foreach (DataTable tbl in dsTemp.Tables)
            {
                DataTable dt = new DataTable();
                dt.TableName = tbl.TableName;
                int qtyImageOnRow = Convert.ToInt32(tbl.Rows[0]["QtyImageOnRow"]);
                tbl.Rows.RemoveAt(0);
                int rowCount = tbl.Rows.Count;
                int row = 0;

                if (rowCount <= qtyImageOnRow)
                {
                    row = 1;
                    qtyImageOnRow = rowCount;
                }
                else
                {
                    row = rowCount / qtyImageOnRow;
                }

                for (int i = 0; i < qtyImageOnRow; i++)
                {
                    dt.Columns.Add("Image" + (i + 1).ToString(), typeof(byte[]));
                    dt.Columns.Add("Title" + (i + 1).ToString(), typeof(string));
                }



                //for (int i = 0; i < (tbl.Rows.Count - 1) / qtyImageOnRow; i++)
                for (int i = 0; i < row; i++)
                {
                    if (qtyImageOnRow == 1)
                    {
                        dt.Rows.Add(tbl.Rows[i * qtyImageOnRow][1], tbl.Rows[i * qtyImageOnRow][2]);
                    }
                    else if (qtyImageOnRow == 2)
                    {
                        dt.Rows.Add(
                            tbl.Rows[i * qtyImageOnRow][1], tbl.Rows[i * qtyImageOnRow][2],
                            tbl.Rows[i * qtyImageOnRow + 1][1], tbl.Rows[i * qtyImageOnRow + 1][2]);
                    }
                    else if (qtyImageOnRow == 3)
                    {
                        dt.Rows.Add(
                            tbl.Rows[i * qtyImageOnRow][1], tbl.Rows[i * qtyImageOnRow][2],
                            tbl.Rows[i * qtyImageOnRow + 1][1], tbl.Rows[i * qtyImageOnRow + 1][2],
                            tbl.Rows[i * qtyImageOnRow + 2][1], tbl.Rows[i * qtyImageOnRow + 2][2]);
                    }
                    else if (qtyImageOnRow == 4)
                    {
                        dt.Rows.Add(
                            tbl.Rows[i + 1 * qtyImageOnRow][1], tbl.Rows[i * qtyImageOnRow][2],
                            tbl.Rows[i + 1 * qtyImageOnRow + 1][1], tbl.Rows[i + 1 * qtyImageOnRow + 1][2],
                            tbl.Rows[i + 1 * qtyImageOnRow + 2][1], tbl.Rows[i + 1 * qtyImageOnRow + 2][2],
                            tbl.Rows[i + 1 * qtyImageOnRow + 3][1], tbl.Rows[i + 1 * qtyImageOnRow + 3][2]);
                    }
                    else if (qtyImageOnRow == 5)
                    {
                        dt.Rows.Add(
                            tbl.Rows[i + 1 * qtyImageOnRow][1], tbl.Rows[i * qtyImageOnRow][2],
                            tbl.Rows[i + 1 * qtyImageOnRow + 1][1], tbl.Rows[i + 1 * qtyImageOnRow + 1][2],
                            tbl.Rows[i + 1 * qtyImageOnRow + 2][1], tbl.Rows[i + 1 * qtyImageOnRow + 2][2],
                            tbl.Rows[i + 1 * qtyImageOnRow + 3][1], tbl.Rows[i + 1 * qtyImageOnRow + 3][2],
                            tbl.Rows[i + 1 * qtyImageOnRow + 4][1], tbl.Rows[i + 1 * qtyImageOnRow + 4][2]);
                    }

                    if (row * qtyImageOnRow + 1 == rowCount)
                    {
                        dt.Rows.Add(MappingDataToRow(row, 1, qtyImageOnRow, dt, tbl));
                    }
                    else if (row * qtyImageOnRow + 2 == rowCount)
                    {
                        dt = MappingDataToRow(row, 2, qtyImageOnRow, dt, tbl);
                    }
                    else if (row * qtyImageOnRow + 3 == rowCount)
                    {
                        dt.Rows.Add(MappingDataToRow(row, 3, qtyImageOnRow, dt, tbl));
                    }
                    else if (row * qtyImageOnRow + 4 == rowCount)
                    {
                        dt.Rows.Add(MappingDataToRow(row, 4, qtyImageOnRow, dt, tbl));
                    }
                    else if (row * qtyImageOnRow + 5 == rowCount)
                    {
                        dt.Rows.Add(MappingDataToRow(row, 5, qtyImageOnRow, dt, tbl));
                    }
                }

                dsResult.Tables.Add(dt);
            }


            return dsResult;
        }

        private DataTable MappingDataToRow(int row, int qtyItemAddNewOnRow, int qtyImageOnRow, DataTable data, DataTable datagoc)
        {
            DataTable result = data;

            if (qtyItemAddNewOnRow == 1)
            {


                //result.Add(data.Rows[row + 1 * qtyImageOnRow][1], data.Rows[row + 1 * qtyImageOnRow][2]);
            }
            else if (qtyItemAddNewOnRow == 2)
            {
                try
                {
                    result.Rows.Add(
                             datagoc.Rows[row * qtyImageOnRow][1], datagoc.Rows[row * qtyImageOnRow][2],
                             datagoc.Rows[row * qtyImageOnRow + 1][1], datagoc.Rows[row * qtyImageOnRow + 1][2]);
                }
                catch (Exception ex)
                {

                    throw ex;
                }

            }
            else if (qtyItemAddNewOnRow == 3)
            {

            }
            else if (qtyItemAddNewOnRow == 4)
            {

            }
            else if (qtyItemAddNewOnRow == 5)
            {

            }

            //if(qtyItemAddNewOnRow < qtyImageOnRow)
            //{
            //    for (int i = qtyItemAddNewOnRow; i < qtyImageOnRow; i++)
            //    {
            //        dt.Rows[qtyItemAddNewOnRow + 1][1] = null;
            //        dt.Rows[qtyItemAddNewOnRow + 1][2] = null;
            //    }
            //}

            return result;
        }

        private Bitmap GetAndSetSizeImage(DataTable dataIn, int row, int column)
        {
            Image image = Image.FromFile(dataIn.Rows[row][column].ToString());

            int width = image.Width;
            int height = image.Height;

            int locationX = 0;
            int locationY = 0;

            float xDpi = image.HorizontalResolution;
            float yDpi = image.VerticalResolution;

            if (dataIn.Columns.IndexOf("WIDTH") != -1)
            {
                width = Convert.ToInt32(dataIn.Rows[row][dataIn.Columns.IndexOf("WIDTH")]);
            }

            if (dataIn.Columns.IndexOf("HEIGHT") != -1)
            {
                height = Convert.ToInt32(dataIn.Rows[row][dataIn.Columns.IndexOf("HEIGHT")]);
            }

            if (dataIn.Columns.IndexOf("locationX") != -1)
            {
                locationX = Convert.ToInt32(dataIn.Rows[row][dataIn.Columns.IndexOf("locationX")]);
            }

            if (dataIn.Columns.IndexOf("locationY") != -1)
            {
                locationY = Convert.ToInt32(dataIn.Rows[row][dataIn.Columns.IndexOf("locationY")]);
            }

            if (dataIn.Columns.IndexOf("xDpi") != -1)
            {
                xDpi = xDpi * (float.Parse(dataIn.Rows[row][dataIn.Columns.IndexOf("xDpi")].ToString()) / 100);
            }

            if (dataIn.Columns.IndexOf("yDpi") != -1)
            {
                yDpi = yDpi * (float.Parse(dataIn.Rows[row][dataIn.Columns.IndexOf("yDpi")].ToString()) / 100);
            }

            if (dataIn.Columns.IndexOf("xDpiDefault") != -1)
            {
                xDpi = float.Parse(dataIn.Rows[row][dataIn.Columns.IndexOf("xDpiDefault")].ToString());
            }

            if (dataIn.Columns.IndexOf("yDpiDefault") != -1)
            {
                yDpi = float.Parse(dataIn.Rows[row][dataIn.Columns.IndexOf("yDpiDefault")].ToString());
            }

            var destRect = new Rectangle(locationX, locationY, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(xDpi, yDpi);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, locationX, locationY, width, height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        private byte[] GenerateQrCode_VB(string data)
        {

            //var writer = new BarcodeWriter();
            //QrCodeEncodingOptions options = new QrCodeEncodingOptions
            //{
            //    DisableECI = true,
            //    CharacterSet = "UTF-8",
            //    Width = 450,
            //    Height = 450,
            //    Margin = 1
            //};

            //writer.Format = BarcodeFormat.QR_CODE;
            //writer.Options = options;
            //var result = writer.Write(data);

            return null;
        }

        #endregion

    }
    public class HandleMergeImageField : IFieldMergingCallback
    {

        void IFieldMergingCallback.FieldMerging(FieldMergingArgs e)
        {
            // Do nothing.
            if (e.FieldValue is bool)

            {
                DocumentBuilder builder = new DocumentBuilder(e.Document);

                // Move the “cursor” to the current merge field.

                builder.MoveToMergeField(e.FieldName);

                // It is nice to give names to check boxes. Lets generate a name such as MyField21 or so.

                string checkBoxName = string.Format("{0}{1}", e.FieldName, e.RecordIndex);

                // Insert a check box.

                builder.InsertCheckBox(checkBoxName, (bool)e.FieldValue, 0);

                // Nothing else to do for this field.

                return;

            }
        }
        ///
        /// This is called when mail merge engine encounters Image:XXX merge field in the document.

        /// You have a chance to return an Image object, file name or a stream that contains the image.

        ///
        void IFieldMergingCallback.ImageFieldMerging(ImageFieldMergingArgs e)
        {
            try
            {
                if (e.FieldValue != System.DBNull.Value)
                {
                    DocumentBuilder builder = new DocumentBuilder(e.Document);
                    builder.MoveToField(e.Field, true);

                    // Insert image and specify its size
                    builder.InsertImage((byte[])e.FieldValue, ConvertUtil.MillimeterToPoint(16), ConvertUtil.MillimeterToPoint(16));
                    e.Field.Remove();
                }
            }
            catch
            {

            }
        }
    }
}
