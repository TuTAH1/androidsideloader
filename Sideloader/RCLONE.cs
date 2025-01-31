﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AndroidSideloader
{
    class rcloneFolder
    {
        public string Path { get; set; }
        public string Name { get; set; }
        public string Size { get; set; }
        public string ModTime { get; set; }

    }

    class SideloaderRCLONE
    {
        public static List<string> RemotesList = new List<string>();

        public static string RcloneGamesFolder = "Quest Games";

        //This shit sucks but i'll switch to programatically adding indexes from the gamelist txt sometimes maybe

        public static int GameNameIndex = 0;
        public static int ReleaseNameIndex = 1;
        public static int PackageNameIndex = 2;
        public static int VersionCodeIndex = 3;
        public static int ReleaseAPKPathIndex = 4;
        public static int VersionNameIndex = 5;

        public static List<string> gameProperties = new List<string>();
        /* Game Name
         * Release Name
         * Release APK Path
         * Package Name
         * Version Code
         * Version Name
         */
        public static List<string[]> games = new List<string[]>();

        public static string Nouns = Environment.CurrentDirectory + "\\nouns";
        public static string ThumbnailsFolder = Environment.CurrentDirectory + "\\thumbnails";
        public static string NotesFolder = Environment.CurrentDirectory + "\\notes";

        public static void UpdateNouns(string remote)
        {
            RCLONE.runRcloneCommand($"sync \"{remote}:{RcloneGamesFolder}/.meta/nouns\" \"{Nouns}\"");
        }

        public static void UpdateGamePhotos(string remote)
        {
            RCLONE.runRcloneCommand($"sync \"{remote}:{RcloneGamesFolder}/.meta/thumbnails\" \"{ThumbnailsFolder}\"");
        }

        public static void UpdateGameNotes(string remote)
        {

            RCLONE.runRcloneCommand($"sync \"{remote}:{RcloneGamesFolder}/.meta/notes\" \"{NotesFolder}\"");
        }

        public static void RefreshRemotes()
        {
            RemotesList.Clear();
            var remotes = RCLONE.runRcloneCommand("listremotes").Output.Split('\n');

            Logger.Log("Loaded following remotes: ");
            foreach (string r in remotes)
            {
                if (r.Length > 1)
                {
                    var remote = r.Remove(r.Length - 1);
                    if (remote.Contains("mirror"))
                    {
                        Logger.Log(remote);
                        RemotesList.Add(remote);
                    }
                }
            }
        }

        public static void initGames(string remote)
        {
            gameProperties.Clear();
            games.Clear();
            string tempGameList = RCLONE.runRcloneCommand($"cat \"{remote}:{RcloneGamesFolder}/VRP-GameList.txt\"").Output;
            if (MainForm.debugMode)
            {
                File.WriteAllText("VRP-GamesList.txt", tempGameList);
            }
            if (!tempGameList.Equals(""))
            {
                string[] gameListSplited = tempGameList.Split(new[] { '\n' });
                gameListSplited = gameListSplited.Skip(1).ToArray();
                foreach (string game in gameListSplited)
                {
                    if (game.Length > 1)
                    {
                        string[] splitGame = game.Split(';');
                        games.Add(splitGame);
                    }
                }
            }
            
        }

        public static void updateConfig(string remote)
        {
            string localHash = "";
            try { localHash = File.ReadAllText(Environment.CurrentDirectory + "\\rclone\\hash.txt"); } catch { } //file may not exist

            string hash = RCLONE.runRcloneCommand($"md5sum \"{remote}:Quest Homebrew/Sideloading Methods/1. Rookie Sideloader - VRP Edition/VRP.download.config\"").Output;
            try { hash = hash.Substring(0, hash.LastIndexOf(" ")); } catch { return; } //remove stuff after hash

            Debug.WriteLine("The local file hash is " + localHash + " and the current a file hash is " + hash);

            if (!string.Equals(localHash, hash))
            {
                RCLONE.runRcloneCommand(string.Format($"copy \"{remote}:Quest Homebrew/Sideloading Methods/1. Rookie Sideloader - VRP Edition/VRP.download.config\" \"{Environment.CurrentDirectory}\\rclone\""));
                File.WriteAllText(Environment.CurrentDirectory + "\\rclone\\hash.txt", hash);
            }
        }
    }
}
