using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Collections;


namespace Helpers.WPFHelpers.WindowsHelpers
{
    /// <summary>
    /// Interaction logic for ExceptionsViewer.xaml
    /// </summary>
    public partial class ExceptionsViewer : Window
    {
        public List<ExceptionListItem> Exceptions { get; set; }

        public class ExceptionListItem
        {
            public string Title { get; set; }
            public string Message { get; set; }
            public string StackTrace { get; set; }
            public Exception Exception { get; set; }
        }

        public ExceptionsViewer(Exception e, string windowTitle)
        {
            Exceptions = new List<ExceptionListItem>();
            InitializeComponent();

            this.Title = windowTitle;

            //Cycle thorugh exceptions and add them to the list
            Exception tmp;
            tmp = e;
            if (Owner != null) this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            while (tmp != null)
            {
                var type = tmp.GetType();
                Exceptions.Add(new ExceptionListItem
                {
                    Exception = tmp,
                    Title = type.ToString(),
                    Message = tmp.Message,
                    StackTrace = tmp.StackTrace
                });
                tmp = tmp.InnerException;
            }

            //Update the binding and sho the dialog
            BindingOperations.GetBindingExpression(ErrorList, ListView.ItemsSourceProperty).UpdateSource();
            this.ShowDialog();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ErrorList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ExceptionText.Document.Blocks.Clear();
            if (e.AddedItems.Count == 1)
            {
                Paragraph para = new Paragraph();
                para.Inlines.AddRange(RenderException((ExceptionListItem)e.AddedItems[0]));
                ExceptionText.Document.Blocks.Add(para);
            }
        }

        private List<Inline> RenderException(ExceptionListItem item)
        {
            //Create text
            List<Inline> para = new List<Inline>();
            Inline inline = new Bold(new Run(item.Title));
            inline.FontSize = 16;
            para.Add(inline);

            AddProperty(ref para, "Message", item.Message);
            AddProperty(ref para, "StackTrace", item.StackTrace);

            //use reflextion
            System.Reflection.PropertyInfo[] properties = item.Exception.GetType().GetProperties();
            foreach (var p in properties)
            {
                if (p.Name != "InnerException" && p.Name != "Message" && p.Name != "StackTrace")
                {
                    var value = p.GetValue(item.Exception, null);
                    if (value != null)
                    {
                        AddProperty(ref para, p.Name, value);
                    }
                }
            }
            para.Add(new LineBreak());
            return para;
        }

        private void AddProperty(ref List<Inline> para, string propName, object propVal)
        {
            if (propVal != null)
            {
                if (propVal is IDictionary)
                {
                    IDictionary dic = (IDictionary)propVal;
                    if (dic.Count == 0) return;
                    para.Add(new LineBreak());
                    para.Add(new LineBreak());
                    para.Add(new Bold(new Run(propName + ": ")));
                    foreach (var i in dic.Keys)
                    {
                        para.Add(new LineBreak());
                        para.Add(new Run(i + " = " + dic[i]));
                    }
                }
                else if (propVal is IEnumerable)
                {
                    IEnumerable enm = (IEnumerable)propVal;
                    string line = "";
                    foreach (var i in enm)
                    {
                        line += i.ToString();
                    }
                    if (line != "")
                    {
                        para.Add(new LineBreak());
                        para.Add(new LineBreak());
                        para.Add(new Bold(new Run(propName + ": ")));
                        para.Add(new LineBreak());
                        para.Add(new Run(line));
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(propVal.ToString())) return;
                    para.Add(new LineBreak());
                    para.Add(new LineBreak());
                    para.Add(new Bold(new Run(propName + ": ")));
                    para.Add(new LineBreak());
                    para.Add(new Run((string)propVal.ToString()));
                }
            }
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            Paragraph final = new Paragraph();
            Inline inline = new Bold(new Run("Error Report"));
            inline.FontSize = 16;
            final.Inlines.Add(inline);
            final.Inlines.Add(new LineBreak());
            final.Inlines.Add(new LineBreak());
            //Cycle through exceptions and make full listing
            foreach (var i in Exceptions)
            {
                final.Inlines.AddRange(RenderException(i));
                
                final.Inlines.Add(new LineBreak());
                final.Inlines.Add(new Run("____________________________________________________"));
                final.Inlines.Add(new LineBreak());
            }

            var doc = new FlowDocument();
            doc.Blocks.Add(final);

            // Now place the doc contents on the clipboard in both
            // rich text and plain text format.
            TextRange range = new TextRange(doc.ContentStart, doc.ContentEnd);
            DataObject data = new DataObject();

            using (Stream stream = new MemoryStream())
            {
                //Save output as RTF to data stream
                range.Save(stream, DataFormats.Rtf);
                //Set the data in our data object (in RTF)
                data.SetData(DataFormats.Rtf, Encoding.UTF8.GetString((stream as MemoryStream).ToArray()));
            }
            //Also save in data object as text
            data.SetData(DataFormats.StringFormat, range.Text);
            Clipboard.SetDataObject(data);
        }
    }
}
