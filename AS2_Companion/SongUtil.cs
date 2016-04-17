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
    - Cleanup 'parent' situation
    - Make sure the XML match doesn't get stuck looping
    - Handle post request exception
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
                if (Path.GetFileName(file) != outputLog)
                {
                    MessageBox.Show(String.Format("Invalid file: {0}", file));
                    break;
                }

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

                    if (scoreMatch.Success)
                    {
                        HandleScoreMatch(parent, scoreMatch, songList);
                        continue;
                    }

                    artistMatch = Regex.Match(line, "duration(.+)artist:(.+)");

                    if (artistMatch.Success)
                    {
                        HandleArtistMatch(parent, artistMatch, songList);
                        continue;
                    }

                    xmlMatch = Regex.Match(line, @"<document><user userid='(.+)' regionid='(.+)' email='(.+)' canpostscores='(.+)'></user><modeid modeid='(.+)'></modeid><modename modename='(.+)'></modename><scoreboards songid='(\d+)' modeid='(\d+)'>");

                    if (xmlMatch.Success)
                    {
                        HandleXmlMatch(output, xmlMatch, songList);
                        continue;
                    }
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
                //PostSongData(xmlString); // post the xml to the web server
            }
        }

        static void HandleScoreMatch(MainForm parent, Match scoreMatch, List<Song> songList)
        {
            Song songInfo;
            Song.Score scoreInfo = new Song.Score(); // Instantiate the new score

            string songTitle = scoreMatch.Groups[2].Value;
            string songScore = scoreMatch.Groups[1].Value;

            if (songList.Exists(song => song.Title == songTitle)) // If the song is already there add the score to it
            {
                var index = songList.FindIndex(song => song.Title == songTitle); // get the song index

                scoreInfo.Value = songScore;

                songInfo = songList[index]; // get the song object
                songInfo.AddScore(scoreInfo); // add the new score

                songList.RemoveAt(index); // remove it from the old index
                songList.Add(songInfo); // add it back at the last index to prevent data mismatch
            }
            else // Otherwise create the song and add it to the list
            {
                songInfo = new Song();
                songInfo.Title = songTitle;
                songInfo.Artist = " "; // Instantiate the artist field
                scoreInfo.Value = songScore;
                songInfo.AddScore(scoreInfo);
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
                songInfo.Artist = songArtist;
            }
        }

        static void HandleXmlMatch(StreamReader input, Match xmlStart, List<Song> songList)
        {
            string line = "";
            Song songInfo = songList.Last();
            Song.Score scoreInfo = songInfo.Scores.Last();
            Match xmlInfo, nextEntry;
            bool xmlEnd = false;

            Scoreboard scoreBoard = new Scoreboard(); // Instantiate the scoreboard
            scoreBoard.Global = new Scoreboard.Category(); // Instantiate the global entry
            Scoreboard.Category boardCategory = scoreBoard.Global; // Set the global entry
            Scoreboard.Entry scoreEntry;

            songInfo.SongID = xmlStart.Groups[7].Value; // Set the song ID
            songInfo.UserID = xmlStart.Groups[1].Value; // Set the user 
            songInfo.UserRegion = xmlStart.Groups[2].Value; // Set user region
            songInfo.UserEmail = xmlStart.Groups[3].Value; // Set user email
            scoreInfo.Mode = xmlStart.Groups[6].Value; // Set song mode

            songInfo.SetCanPost(xmlStart.Groups[4].Value); // Set if cheats were detected

            while (!xmlEnd) // While we aren't done parsing the scoreboard
            {
                line = input.ReadLine(); // Read the next line of scoreboard

                nextEntry = Regex.Match(line, "</scoreboard>");

                if (nextEntry.Success) // Check if we need to change entries
                {
                    if (scoreBoard.Regional != null)
                    {
                        xmlEnd = true; // We're done parsing
                        songInfo.AddScoreboard(scoreBoard); // Add the scoreboard to the song
                        break;
                    }

                    if (boardCategory == scoreBoard.Global)
                    {
                        scoreBoard.Friends = new Scoreboard.Category(); // Instantiate the friends entry
                        boardCategory = scoreBoard.Friends; // Set the friends entry
                    }
                    else
                    {
                        scoreBoard.Regional = new Scoreboard.Category(); // Instantiate the regional entry
                        boardCategory = scoreBoard.Regional; // Set the regional entry
                    }

                    for (int i = 0; i < 2; i++)
                    {
                        line = input.ReadLine();
                        //Console.WriteLine(line);
                    }
                }

                xmlInfo = Regex.Match(line, @"<ride userid='(.+)' steamid='(.+)' score='(.+)' charid='(.+)' ridetime='(.+)'>(<comment>(.+)</comment>)?<modename>(.+)</modename><username>(.+)</username>");

                scoreEntry = new Scoreboard.Entry();

                scoreEntry.UserID = xmlInfo.Groups[1].Value;
                scoreEntry.SteamID = xmlInfo.Groups[2].Value;
                scoreEntry.Score = xmlInfo.Groups[3].Value;
                scoreEntry.RideTime = xmlInfo.Groups[5].Value;
                scoreEntry.Mode = xmlInfo.Groups[8].Value;
                scoreEntry.Username = xmlInfo.Groups[9].Value;

                scoreEntry.Comment = " "; // Instantiate the comment field
                if (!String.IsNullOrEmpty(xmlInfo.Groups[7].Value))
                {
                    scoreEntry.Comment = xmlInfo.Groups[7].Value;
                }

                boardCategory.Entries.Add(scoreEntry);
            }
        }

        static string SerializeSongData(List<Song> songList)
        {
            //var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "//AS2Companion.xml";
            //System.IO.FileStream file = System.IO.File.Create(path);

            var serializer = new System.Xml.Serialization.XmlSerializer(typeof(List<Song>));

            StringWriter textWriter = new StringWriter();

            serializer.Serialize(textWriter, songList);

            return textWriter.ToString();
        }

        static void PostSongData(string xmlString)
        {
            //Decode the string
            string decoded = xmlString.Replace("&amp;", "&").Replace("lt;", "<").Replace("&gt;", ">").Replace("&quot;", "\"").Replace("&apos;", "'");

            //Create the request
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("");
            byte[] bytes;
            bytes = System.Text.Encoding.ASCII.GetBytes(decoded);
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = bytes.Length;
            request.Method = "POST";

            //Get the reponse
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
