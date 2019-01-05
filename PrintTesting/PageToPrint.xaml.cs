using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Printing;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Printing;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace PrintTesting
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PageToPrint : Page
    {
        PrintDocument printDoc;
        IPrintDocumentSource printDocSrc;

        public PageToPrint()
        {
            this.InitializeComponent();
            txtblkEnteredText.Text = App.TestText;
        }

        public virtual void RegisterForPrinting()
        {
            printDoc = new PrintDocument();
            printDocSrc = printDoc.DocumentSource;
            printDoc.Paginate += CreatePrintPreviewPages;
            printDoc.GetPreviewPage += GetPrintPreviewPage;
            printDoc.AddPages += AddPrintPages;

            PrintManager printMan = PrintManager.GetForCurrentView();
            printMan.PrintTaskRequested += PrintTastRequested;
        }

        private void PrintTastRequested(PrintManager sender, PrintTaskRequestedEventArgs args)
        {
            throw new NotImplementedException();
        }

        private void AddPrintPages(object sender, AddPagesEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void GetPrintPreviewPage(object sender, GetPreviewPageEventArgs e)
        {
            throw new NotImplementedException();
        }

        protected virtual void CreatePrintPreviewPages(object sender, PaginateEventArgs e)
        {
            // clear the cache of preview pages
            printPreviewPages.Clear();
        }
    }
}
