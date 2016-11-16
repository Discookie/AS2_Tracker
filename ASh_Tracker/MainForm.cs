using System;
using System.Windows.Forms;

namespace ASh_Tracker
{
    /*
    TODO: Add minimize button
    */

    public partial class MainForm : Form
    {
        /*
        Construct the form
        */
        public MainForm()
        {
            InitializeComponent();

            if (Properties.Settings.Default.outputPath != "")
                loadFileToolStripMenuItem.Enabled = true; // Enable file loading if path isn't empty

            ProcessHandler.StartTimer(this); // Instantiate process timer with MainForm as the parent
            SetLabelStatus(ProcessHandler.ProcessStatus()); // Set the label to the process status
        }

        /*
        Drag drop logic
        */
        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            SongUtil.LoadSongList(this, files); // Attempt to load the song list from the dropped files
        }

        /*
        Label logic
        */
        public void SetLabelStatus(string status)
        {
            // We may require invoking so use DoUI
            DoUI(() =>
            {
                label1.Text = status;
            });
        }

        /*
        Song listbox logic
        */
        private void songBox_DoubleClick_1(object sender, EventArgs e)
        {
            ShowSelectedScores();
        }

        private void ShowSelectedScores()
        {
            if (songBox.SelectedItem == null) return;

            Song songInfo = (Song)songBox.SelectedItem;
            string songURL = "http://audioshield.as2tracker.com/song/" + songInfo.SongID;

            System.Diagnostics.Process.Start(songURL);

            /*int count = 0;
            int scoreToInt;
            bool parseResult;
            foreach (var score in songInfo.Scores)
            {
                count += 1; // Increment score count
                parseResult = Int32.TryParse(score.Value, out scoreToInt); // Attempt to parse the score to an integer so we can format it

                if (parseResult)
                    MessageBox.Show(String.Format("Score {0}: {1:n0}", count, scoreToInt));
                else
                    MessageBox.Show(String.Format("Score {0}: {1}", count, score));
            }*/
        }

        /*
        System tray logic
        */
        private void MainForm_Resize(object sender, System.EventArgs e)
        {
            if (FormWindowState.Minimized == WindowState)
            {
                Hide();
                taskNotification("Minimized", "ASh Tracker was moved to system tray");
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
        }

        private void notifyIcon1_BalloonTipClicked(object sender, EventArgs e)
        {
            if (FormWindowState.Minimized == WindowState)
            {
                Show();
                WindowState = FormWindowState.Normal;
            }
        }

        public void taskNotification(string title, string body)
        {
            if (!Properties.Settings.Default.shouldTrayNotify) return;

            if (title != null)
            {
                notifyIcon1.BalloonTipTitle = title;
            }

            if (body != null)
            {
                notifyIcon1.BalloonTipText = body;
            }

            notifyIcon1.ShowBalloonTip(3000);
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            notifyIcon1.Visible = false;
            notifyIcon1.Dispose();
        }

        /*
        Toolstrip logic
        */
        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OptionsMenu options = new OptionsMenu(this);
            options.ShowDialog(); // Open the options menu
        }

        private void loadFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string[] files = { Properties.Settings.Default.outputPath };
            SongUtil.LoadSongList(this, files); // Attempt to load song list from the settings path
        }

        private void clearListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SongUtil.LastLogWrite = null; // So we can safely reload the previous file
            songBox.Items.Clear(); // Clear all songbox items
            Song.Count = 0; // Reset song count
            songBox.Visible = false;
            label1.Visible = true;
        }

        /*
        Handle UI requests that may require invoking
        */
        public delegate void InvokeAction();
        public void DoUI(InvokeAction call)
        {
            if (IsDisposed)
            {
                return;
            }
            if (InvokeRequired)
            {
                try
                {
                    Invoke(call);
                }
                catch (InvalidOperationException)
                {
                    //Handle error
                }
            }
            else {
                call();
            }
        }
    }
}
