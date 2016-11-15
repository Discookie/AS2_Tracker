using System;
using System.Diagnostics;

namespace ASh_Tracker
{
    /*
    TODO:
    Convert to non-static class
    Cleanup 'parent' situation
    */

    public static class ProcessHandler
    {
        static MainForm parent;
        static bool processRunning;
        static Process[] audiosurf;

        /// <summary>
        /// Monitors the audiosurf process
        /// </summary>
        public static void CheckProcess()
        {
            if (processRunning) return; // Don't continue if we already raised the event

            audiosurf = Process.GetProcessesByName("Audioshield");
            if (audiosurf.Length == 0) return; // Don't continue if it's not running

            foreach (Process process in audiosurf)
            {
                process.EnableRaisingEvents = true;
                process.Exited += new EventHandler(audiosurf_Exited);
                processRunning = true;

                parent.SetLabelStatus(ProcessStatus()); // Set the label to the process status
                break; // We only need to watch one process
            }
        }

        //Handle Exited event
        static void audiosurf_Exited(object sender, System.EventArgs e)
        {
            processRunning = false;

            parent.SetLabelStatus(ProcessStatus()); // Set the label to the process status

            string[] files = { Properties.Settings.Default.outputPath };
            SongUtil.LoadSongList(parent, files);

            //Console.WriteLine("Audiosurf EXITED.");
        }

        public static bool IsProcessRunning()
        {
            return processRunning;
        }

        public static string ProcessStatus()
        {
            string running;
            if (processRunning)
                running = "Audioshield is running!";
            else
                running = "Audioshield is not running.";

            return running;
        }

        /*
        Handle process timer
        */
        static System.Timers.Timer processTimer;
        public static void StartTimer(MainForm _parent)
        {
            parent = _parent;

            // Create a timer with a two second interval.
            processTimer = new System.Timers.Timer(2000);

            // Hook up the Elapsed event for the timer. 
            processTimer.Elapsed += OnTimedEvent;

            // Have the timer fire repeated events (true is the default)
            processTimer.AutoReset = true;

            // Start the timer
            processTimer.Enabled = true;

            GC.KeepAlive(processTimer);
        }

        static void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            CheckProcess();
        }
    }
}
