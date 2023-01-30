using System.IO;
using System.Linq;
using System.Text;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;

namespace DispatcherWeb
{
    public static class ReportExtensions
    {
        public static Paragraph AddParagraph(this Cell cell, string paragraphText, TextMeasurement tm)
        {
            if (tm.Font.Size == 0)
            {
                throw new System.Exception("TextMeasurement font size was not explicitly set");
            }
            return cell.AddParagraph(AdjustIfTooWideToFitIn(tm, cell, paragraphText ?? ""));
        }

        public static byte[] SaveToBytesArray(this Document document)
        {
            using (var stream = new MemoryStream())
            {
                document.SaveToMemoryStream(stream);
                return stream.ToArray();
            }
        }

        public static MemoryStream SaveToMemoryStream(this Document document, MemoryStream stream)
        {
            document.UseCmykColor = true;

            var renderer = new PdfDocumentRenderer(true)
            {
                Document = document
            };

            renderer.RenderDocument();

            renderer.PdfDocument.Save(stream, false);
            renderer.PdfDocument.Close();
            renderer.PdfDocument.Dispose();
            stream.Flush();
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }

        private static string AdjustIfTooWideToFitIn(TextMeasurement tm, Cell cell, string text)
        {
            Column column = cell.Column;
            Unit rightPadding = Unit.FromMillimeter(1.2);
            Unit availableWidth = column.Width - column.Table.Borders.Width - cell.Borders.Width - rightPadding;

            if (cell.MergeRight > 0)
            {
                for (var i = 0; i < cell.MergeRight; i++)
                {
                    availableWidth += cell.Table.Columns[cell.Column.Index + i + 1].Width;
                }
                availableWidth -= Unit.FromMillimeter(3);
            }

            var tooWideWords = text.Split(" ".ToCharArray()).Distinct().Where(s => TooWide(s, availableWidth, tm));

            var adjusted = new StringBuilder(text);
            foreach (string word in tooWideWords)
            {
                var replacementWord = MakeFit(word, availableWidth, tm);
                adjusted.Replace(word, replacementWord);
            }

            return adjusted.ToString();
        }

        private static bool TooWide(string word, Unit width, TextMeasurement tm)
        {
            double f = tm.MeasureString(word, UnitType.Point).Width;
            return f > width.Point;
        }

        /// <summary>
        /// Makes the supplied word fit into the available width
        /// </summary>
        /// <returns>modified version of the word with inserted Returns at appropriate points</returns>
        private static string MakeFit(string word, Unit width, TextMeasurement tm)
        {
            var adjustedWord = new StringBuilder();
            var current = string.Empty;
            foreach (char c in word)
            {
                if (TooWide(current + c, width, tm))
                {
                    adjustedWord.Append(current);
                    adjustedWord.Append(Chars.CR);
                    current = c.ToString();
                }
                else
                {
                    current += c;
                }
            }
            adjustedWord.Append(current);

            return adjustedWord.ToString();
        }
    }
}
