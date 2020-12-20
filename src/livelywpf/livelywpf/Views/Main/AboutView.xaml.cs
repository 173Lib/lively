﻿using ModernWpf.Controls;
using System;
using System.Diagnostics;
using System.Drawing.Text;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Threading;
using Page = System.Windows.Controls.Page;

namespace livelywpf.Views
{
    /// <summary>
    /// Interaction logic for AboutView.xaml
    /// </summary>
    public partial class AboutView : Page
    {
        public AboutView()
        {
            InitializeComponent();
            appVersionText.Text = "v" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString() + 
                (Program.IsTestBuild == true? "b":string.Empty);
            try
            {
                //attribution document.
                TextRange textRange = new TextRange(licenseDocument.ContentStart, licenseDocument.ContentEnd);
                using (FileStream fileStream = 
                    File.Open(Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Docs", "license.rtf")), 
                    FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    textRange.Load(fileStream, System.Windows.DataFormats.Rtf);
                }
                licenseFlowDocumentViewer.Document = licenseDocument;
            }
            catch { }
        }

        private async void licenseDocument_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            e.Handled = true;
            var result = await ShowNavigateDialogue(this, e.Uri);
            if (result == ContentDialogResult.Primary)
            {
                Helpers.LinkHandler.OpenBrowser(e.Uri);
            }
        }

        private async Task<ContentDialogResult> ShowNavigateDialogue(object sender, Uri arg)
        {
            ContentDialog confirmDialog = new ContentDialog
            {
                Title = "Do you wish to navigate to external website?",
                Content = arg.ToString(),
                PrimaryButtonText = Properties.Resources.TextYes,
                SecondaryButtonText = Properties.Resources.TextNo,
                DefaultButton = ContentDialogButton.Secondary
            };
            return await confirmDialog.ShowAsync();
        }
    }
}
