using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace DevRegTest
{
    public class TreeViewMetaData
    {
        public CheckBox Name { get; set; }

        public TextBlock Author { get; set; }

        public TextBlock Process { get; set; }

        public TextBlock Version { get; set; }

        public Image Extra { get; set; }

        public List<TreeViewMetaData> SubItems { get; set; }
    }
}
