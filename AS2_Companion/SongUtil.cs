﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace AS2_Companion
{
    /*
    TODO:
    Cleanup 'parent' situation
    Make sure the XML match doesn't get stuck looping

    Each scoreboard entry doesn't need to include mode
    Include mode as part of the song instead
    */

    public static class SongUtil
    {
        /// <summary>
        /// Load the song list from the given file
        /// </summary>
        ///
        /// <param name="_files">
        /// An array of strings containing file paths to be loaded
        /// </param>

        public static DateTime? LastLogWrite; // Stores the last write time of the log

        public static void LoadSongList(MainForm parent, string[] _files)
        {
            string outputLog = "output_log.txt";
            string[] files = _files;

            List<Song> songList = new List<Song>();

            if (ProcessHandler.IsProcessRunning())
            {
                MessageBox.Show("Error: Cannot load songs while Audiosurf is running.");
                return;
            }

            foreach (string file in files)
            {
                if (Path.GetFileName(file) == outputLog)
                {
                    if (LastLogWrite == File.GetLastWriteTime(file)) // Don't load the same file
                    {
                        MessageBox.Show("Error: The songs from this file have already loaded.");
                        break;
                    }

                    string line;
                    Match scoreMatch, xmlMatch, artistMatch;
                    StreamReader output = new StreamReader(file);

                    while ((line = output.ReadLine()) != null)
                    {
                        scoreMatch = Regex.Match(line, @"setting score (\d+) for song: (.+)");
                        artistMatch = Regex.Match(line, "duration(.+)artist:(.+)");
                        xmlMatch = Regex.Match(line, @"<document><user userid='(.+)' regionid='(.+)' email='(.+)' canpostscores='(.+)'></user><modeid modeid='(.+)'></modeid><modename modename='(.+)'></modename><scoreboards songid='(\d+)' modeid='(\d+)'>");

                        if (scoreMatch.Success)
                            HandleScoreMatch(parent, scoreMatch, songList);

                        if (artistMatch.Success)
                            HandleArtistMatch(parent, artistMatch, songList);

                        if (xmlMatch.Success)
                            HandleXmlMatch(output, xmlMatch, songList);
                    }

                    output.Close();

                    if (Song.Count <= 0) // If no songs were loaded notify the user
                    {
                        MessageBox.Show(String.Format("Error: No submitted scores were found in path {0}", file));
                        break;
                    }

                    if (!parent.songBox.Visible)
                    {
                        parent.songBox.DisplayMember = "Title";

                        // We may require invoking so use DoUI
                        parent.DoUI(() =>
                        {
                            parent.label1.Visible = false;
                            parent.songBox.Visible = true;
                        });
                    }

                    LastLogWrite = File.GetLastWriteTime(file); // Store the write time of the last loaded file

                    MessageBox.Show(String.Format("Successfully loaded {0:n0} songs from path {1}", Song.Count, file));

                    string xmlString = SerializeSongData(songList); // serialize the song data to a string
                    //Console.WriteLine(xmlString);
                    //PostRequest(xmlString); // post the string to the web server
                }
                else
                {
                    MessageBox.Show(String.Format("Invalid file: {0}", file));
                    break;
                }
            }
        }

        static void HandleScoreMatch(MainForm parent, Match scoreMatch, List<Song> songList)
        {
            Song songInfo;

            string songTitle = scoreMatch.Groups[2].Value;
            string songScore = scoreMatch.Groups[1].Value;

            if (songList.Exists(song => song.Title == songTitle)) // If the song is already there add the score to it
            {
                songInfo = songList.Single(song => song.Title == songTitle);
                songInfo.AddScore(songScore);
            }
            else // Otherwise create the song and add it to the list
            {
                songInfo = new Song();
                songInfo.SetTitle(songTitle);
                songInfo.AddScore(songScore);
                songList.Add(songInfo);
                parent.songBox.Items.Add(songInfo);
            }
        }

        static void HandleArtistMatch(MainForm parent, Match artistMatch, List<Song> songList)
        {
            Song songInfo = songList.Last();

            string songArtist = artistMatch.Groups[2].Value;

            if (songList.Exists(song => song.Artist == songArtist)) // If the artist is already there do nothing
            {
                return;
            }
            else // Otherwise add the artist to the song
            {
                songInfo.SetArtist(songArtist);
            }
        }

        static void HandleXmlMatch(StreamReader input, Match xmlStart, List<Song> songList)
        {
            string line = "";
            Song songInfo = songList.Last();
            Match xmlEnd = Regex.Match(line, "</scoreboard>");
            Match xmlInfo;

            Dictionary<string, string> scoreEntry;

            songInfo.SetID(xmlStart.Groups[7].Value); // Set the song ID
            songInfo.SetUserID(xmlStart.Groups[1].Value); // Set the user 
            songInfo.SetUserRegion(xmlStart.Groups[2].Value); // Set user region
            songInfo.SetUserEmail(xmlStart.Groups[3].Value); // Set user email 
            songInfo.SetMode(xmlStart.Groups[6].Value); // Set song mode
            songInfo.SetCanPost(xmlStart.Groups[4].Value); // Set if cheats were detected

            while (!xmlEnd.Success) // While we aren't at the end of the scoreboard
            {
                line = input.ReadLine(); // Read the next line of scoreboard
                xmlEnd = Regex.Match(line, "</scoreboard>"); // Match if we're at the end yet
                scoreEntry = new Dictionary<string, string>();

                if (xmlEnd.Success) break; // Don't continue if it's the end

                xmlInfo = Regex.Match(line, @"<ride userid='(.+)' steamid='(.+)' score='(.+)' charid='(.+)' ridetime='(.+)'>(<comment>(.+)</comment>)?<modename>(.+)</modename><username>(.+)</username>");

                scoreEntry["UserID"] = xmlInfo.Groups[1].Value;
                scoreEntry["SteamID"] = xmlInfo.Groups[2].Value;
                scoreEntry["Score"] = xmlInfo.Groups[3].Value;
                scoreEntry["RideTime"] = xmlInfo.Groups[5].Value;
                //scoreEntry["Comment"] = xmlInfo.Groups[7].Value;
                scoreEntry["Mode"] = xmlInfo.Groups[8].Value;
                scoreEntry["Username"] = xmlInfo.Groups[9].Value;

                songInfo.AddScoreboardEntry(scoreEntry);
            }
        }

        static string SerializeSongData(List<Song> songList)
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "//AS2Companion.xml";
            System.IO.FileStream file = System.IO.File.Create(path);

            var serializer = new System.Xml.Serialization.XmlSerializer(typeof(List<Song>));

            StringWriter textWriter = new StringWriter();

            serializer.Serialize(textWriter, songList);

            return textWriter.ToString();
        }

        /*static void PrintScoreboardData(Song songInfo)
        {
            try
            {
                foreach (Dictionary<string, string> entry in songInfo.Scoreboard)
                {
                    Console.WriteLine("UserID: " + entry["UserID"]);
                    Console.WriteLine("SteamID: " + entry["SteamID"]);
                    Console.WriteLine("Score: " + entry["Score"]);
                    Console.WriteLine("RideTime: " + entry["RideTime"]);
                    Console.WriteLine("Mode: " + entry["Mode"]);
                    Console.WriteLine("Username: " + entry["Username"]);
                    Console.WriteLine(" ");
                }
            }
            catch (Exception ex)
            {
                //Handle exception
                Console.WriteLine("There was an error printing scoreboard data!");
                Console.WriteLine(ex.Message);
            }
        }*/

        static void PostRequest(string xmlString)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("");
            byte[] bytes;
            bytes = System.Text.Encoding.ASCII.GetBytes(xmlString);
            request.ContentType = "text/xml; encoding='utf-8'";
            request.ContentLength = bytes.Length;
            request.Method = "POST";
            Stream requestStream = request.GetRequestStream();
            requestStream.Write(bytes, 0, bytes.Length);
            requestStream.Close();
            HttpWebResponse response;
            response = (HttpWebResponse)request.GetResponse();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream responseStream = response.GetResponseStream();
                string responseStr = new StreamReader(responseStream).ReadToEnd();

                Console.WriteLine(responseStr);
            }
        }
    }
}