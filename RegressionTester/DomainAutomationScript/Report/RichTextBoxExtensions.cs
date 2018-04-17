using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace DevRegTest
{
    static class RichTextBoxExtensions
    {
        public static void AppendText(this RichTextBox box, string text)
        {
            //box.SelectionStart = box.TextLength;
            //box.SelectionLength = 0;

            //box.SelectionColor = color;
            //box.AppendText(text);
            //box.SelectionColor = box.ForeColor;
        }
    }
}
