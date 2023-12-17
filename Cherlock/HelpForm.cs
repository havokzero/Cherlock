using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Cherlock_form
{
    public partial class HelpForm : Form
    {
        private System.Windows.Forms.Timer titleScrollTimer;
        private string scrollText = "  Made by HaVoK ~2023";
        private int scrollPosition = 0;

        public HelpForm()
        {
            InitializeComponent();
            InitializeHelpFormControls();
            InitializeScrollingTitle();
            this.Opacity = 0.7; // Adjust as needed
        }

        private void InitializeScrollingTitle()
        {
            titleScrollTimer = new System.Windows.Forms.Timer();
            titleScrollTimer.Interval = 400; // Adjust for speed
            titleScrollTimer.Tick += TitleScrollTimer_Tick;
            titleScrollTimer.Start();
        }

        private void TitleScrollTimer_Tick(object sender, EventArgs e)
        {
            string title = scrollText.Substring(scrollPosition) + scrollText.Substring(0, scrollPosition);
            this.Text = title;

            scrollPosition++;
            if (scrollPosition >= scrollText.Length) scrollPosition = 0;
        }

        private void InitializeComponent()
        {
            // Initialize form properties
            this.Text = "Help Information";
            this.Width = 400;
            this.Height = 300;
            // Other properties as needed
        }

        private void InitializeHelpFormControls()
        {
            // Create a TableLayoutPanel
            TableLayoutPanel tableLayoutPanel = new TableLayoutPanel();
            tableLayoutPanel.Dock = DockStyle.Fill;
            tableLayoutPanel.RowCount = 2; // Two rows
            tableLayoutPanel.ColumnCount = 1; // One column
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50F)); // 50% for the regular text
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50F)); // 50% for the link

            // Initialize the label for the regular text
            Label regularTextLabel = new Label();
            regularTextLabel.Text = "Fuck you Canaroo!\nSuck My Dick Pepe!\nMake Some Calls Munch!";
            regularTextLabel.TextAlign = ContentAlignment.MiddleCenter;
            regularTextLabel.Font = new Font("Arial Rounded MT Bold", 16, FontStyle.Regular);
            regularTextLabel.Dock = DockStyle.Fill;

            // Initialize the LinkLabel for the YouTube link
            LinkLabel youtubeLinkLabel = new LinkLabel();
            youtubeLinkLabel.Text = "HaVoK Cause of Concern.";
            youtubeLinkLabel.TextAlign = ContentAlignment.MiddleCenter;
            youtubeLinkLabel.Font = new Font("Arial Rounded MT Bold", 16, FontStyle.Regular);
            youtubeLinkLabel.Dock = DockStyle.Fill;

            int linkStart = youtubeLinkLabel.Text.IndexOf("HaVoK Cause of Concern");
            int linkLength = "HaVoK Cause of Concern".Length;
            youtubeLinkLabel.LinkArea = new LinkArea(linkStart, linkLength);

            youtubeLinkLabel.LinkClicked += new LinkLabelLinkClickedEventHandler(HelpLabel_LinkClicked);

            // Add the controls to the TableLayoutPanel
            tableLayoutPanel.Controls.Add(regularTextLabel, 0, 0);
            tableLayoutPanel.Controls.Add(youtubeLinkLabel, 0, 1);

            // Add the TableLayoutPanel to the form
            this.Controls.Add(tableLayoutPanel);
        }

        private void HelpLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // Open the URL when the link is clicked
            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "https://www.youtube.com/channel/UCJp5sseLmHNXyWQ3vPYqvJA",
                UseShellExecute = true
            };
            System.Diagnostics.Process.Start(psi);
        }
    }
    
}

