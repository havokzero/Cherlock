using Cherlock_form;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Cherlock
{
    public partial class Form1 : Form
    {
        private TextBox usernameTextBox;
        private Button runButton;
        private Button stopButton;
        private RichTextBox resultsRichTextBox;
        private CancellationTokenSource cancellationTokenSource;
        private CherlockClass cherlock;
        private System.Windows.Forms.Timer titleScrollTimer;
        private string scrollText = "HaVok ";
        private int scrollPosition = 0;

        public Form1()
        {
            InitializeComponent();
            InitializeUIElements();
            cherlock = new CherlockClass(usernameTextBox, runButton, resultsRichTextBox);

            // Call InitializeScrollingTitle to start the scrolling text
            InitializeScrollingTitle();
        }

        private void InitializeUIElements()
        {
            // Initialize usernameTextBox
            this.usernameTextBox = new TextBox();
            this.usernameTextBox.Location = new System.Drawing.Point(12, 12);
            this.usernameTextBox.Multiline = true;
            this.usernameTextBox.Name = "usernameTextBox";
            this.usernameTextBox.Size = new System.Drawing.Size(200, 50);
            this.usernameTextBox.BackColor = System.Drawing.Color.FromArgb(30, 30, 30); // Dark color
            this.usernameTextBox.ForeColor = System.Drawing.Color.White; // White text
            this.usernameTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            this.Controls.Add(usernameTextBox);

            this.HelpButton = true;  // Enable the help button
            this.MaximizeBox = false; // Disable the maximize button
            this.MinimizeBox = false; // Disable the minimize button



            // Initialize runButton
            this.runButton = new Button();
            this.runButton.Location = new System.Drawing.Point(12, 130);
            this.runButton.Name = "runButton";
            this.runButton.Size = new System.Drawing.Size(100, 30);
            this.runButton.Text = "Run";
            this.runButton.Click += new System.EventHandler(this.runButton_Click);
            this.Controls.Add(runButton);

            // Initialize stopButton
            this.stopButton = new Button();
            this.stopButton.Location = new System.Drawing.Point(120, 130);
            this.stopButton.Name = "stopButton";
            this.stopButton.Size = new System.Drawing.Size(100, 30);
            this.stopButton.Text = "Stop";
            this.stopButton.Enabled = false; // Initially disabled
            this.stopButton.Click += new System.EventHandler(this.stopButton_Click);
            this.Controls.Add(stopButton);

            // Initialize resultsRichTextBox
            this.resultsRichTextBox = new RichTextBox();
            this.resultsRichTextBox.Location = new System.Drawing.Point(12, 180);
            this.resultsRichTextBox.Name = "resultsRichTextBox";
            this.resultsRichTextBox.Size = new System.Drawing.Size(500, 300);
            this.resultsRichTextBox.ReadOnly = true;
            this.resultsRichTextBox.BackColor = System.Drawing.Color.FromArgb(30, 30, 30); // Dark color
            this.resultsRichTextBox.ForeColor = System.Drawing.Color.White; // White text
            this.resultsRichTextBox.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.resultsRichTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            this.Controls.Add(resultsRichTextBox);

            this.Controls.Add(resultsRichTextBox);
        }

        private void InitializeScrollingTitle()
        {
            this.titleScrollTimer = new System.Windows.Forms.Timer();
            this.titleScrollTimer.Interval = 300; // Adjust the speed by changing the interval
            this.titleScrollTimer.Tick += new EventHandler(TitleScrollTimer_Tick);
            this.titleScrollTimer.Start();
        }

        private void TitleScrollTimer_Tick(object sender, EventArgs e)
        {
            string title = scrollText.Substring(scrollPosition) + scrollText.Substring(0, scrollPosition);
            this.Text = title;

            scrollPosition++;
            if (scrollPosition >= scrollText.Length) scrollPosition = 0;
        }

        private async void runButton_Click(object sender, EventArgs e)
        {
            runButton.Enabled = false;
            stopButton.Enabled = true;
            resultsRichTextBox.Clear();

            // Create a new CancellationTokenSource
            cancellationTokenSource = new CancellationTokenSource();

            // Pass the CancellationToken to the SearchForUsernames method
            await cherlock.SearchForUsernames(usernameTextBox.Text, resultsRichTextBox, cancellationTokenSource.Token);

            runButton.Enabled = true;
            stopButton.Enabled = false;
        }

        private void stopButton_Click(object sender, EventArgs e)
        {
            // Cancel the ongoing search
            cancellationTokenSource?.Cancel();

            // Update button states
            runButton.Enabled = true;
            stopButton.Enabled = false;
        }

        private async void SearchButton_Click(object sender, EventArgs e)
        {
            string input = usernameTextBox.Text;
            resultsRichTextBox.Clear();

            if (string.IsNullOrWhiteSpace(input))
            {
                resultsRichTextBox.AppendText("No usernames provided.\n");
                return;
            }

            // Initialize the CancellationTokenSource
            cancellationTokenSource?.Dispose(); // Dispose of any existing source
            cancellationTokenSource = new CancellationTokenSource();

            // Call SearchForUsernames to perform the search with the cancellation token
            await cherlock.SearchForUsernames(input, resultsRichTextBox, cancellationTokenSource.Token);
        }

        protected override void OnHelpButtonClicked(CancelEventArgs e)
        {
            base.OnHelpButtonClicked(e);
            e.Cancel = true;
            HelpForm helpForm = new HelpForm();
            helpForm.Show();
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}

