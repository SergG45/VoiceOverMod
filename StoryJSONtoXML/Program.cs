using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;

namespace StoryJSONtoXML
{
    internal class Program
    {
        static void Main()
        {
            // Checking file
            if (!File.Exists("story.txt") || !File.Exists(@"Locations.txt")) { Environment.Exit(0); }

            dynamic array = null;

            // Deserialize
            using (StreamReader r = new StreamReader("story.txt"))
            {
                string json = r.ReadToEnd();
                try
                {
                    array = JsonConvert.DeserializeObject(json);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.ReadKey();
                    Environment.Exit(0);
                }
            }

            // Reading Locations.txt
            List<String> LocationList = new List<String>();
            string[] lines = File.ReadAllLines(@"Locations.txt");
            foreach (string line in lines)
            {
                if (line != "")
                {
                    LocationList.Add(line);
                }
            }
            if (LocationList.Count == 0) { Environment.Exit(0); }

            // Forming line list
            List<string> FileList = new List<string>();
            foreach (var item in array)
            {
                string part = item.ToString();
                var split = Regex.Split(part.Trim(), "\r\n|\r|\n");
                foreach (string line in split)
                {
                    FileList.Add(line.Trim());
                }
            }

            // Writing document
            int numerator = 1;
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            XmlWriter xmltextWriter = XmlWriter.Create(@"Story.xml", settings);
            List<LinesClass> LineList = new List<LinesClass>();

            // Start document
            xmltextWriter.WriteStartDocument();
            xmltextWriter.WriteStartElement("ROOT");
            xmltextWriter.WriteStartElement("Locations");

            Regex lineRegex = new Regex("^\\\"\\^(.+?)::(.+)\\\",", RegexOptions.Compiled);
            Regex locationRegex = new Regex("^\\\"(.+?)\\\": \\[", RegexOptions.Compiled);
            string currentLocation = "";
            bool inLocation = false;

            for (var i = 0; i < FileList.Count; i++)
            {
                Match loc = locationRegex.Match(FileList[i]);
                if (loc.Success)
                {
                    string newLocation = loc.Groups[1].ToString().Trim();
                    if (LocationList.Contains(newLocation))
                    {
                        if (inLocation)
                        {
                            if (LineList.Count > 0)
                            {
                                // Create a location element
                                xmltextWriter.WriteStartElement("Location");
                                xmltextWriter.WriteAttributeString("Name", currentLocation);
                                foreach (LinesClass line in LineList)
                                {
                                    // Create a line element
                                    xmltextWriter.WriteStartElement("Line");
                                    xmltextWriter.WriteAttributeString("Index", numerator.ToString("D4"));
                                    xmltextWriter.WriteAttributeString("Character", line.Character);
                                    xmltextWriter.WriteAttributeString("Text", line.Text);
                                    xmltextWriter.WriteEndElement();
                                    numerator++;
                                }
                                xmltextWriter.WriteEndElement();
                                LineList.Clear();
                            }
                            currentLocation = newLocation;
                        }
                        else
                        {
                            inLocation = true;
                            currentLocation = newLocation;
                        }
                    }
                }
                Match liner = lineRegex.Match(FileList[i]);
                if (liner.Success)
                {
                    string character = liner.Groups[1].ToString().Trim();
                    string text = liner.Groups[2].ToString();

                    // Checking PLAYER_NAME
                    if (FileList[i + 3] == "\"VAR?\": \"PLAYER_NAME\"")
                    {
                        text += "PLAYER_NAME";
                        text += FileList[i + 7];
                    }

                    text = Regex.Replace(text, "<.+?>", "").Trim();
                    text = text.Replace("\\t", "");
                    text = text.Replace("\\", "");
                    text = text.Replace("\"", "");
                    text = text.Replace("^", "");
                    text = text.Replace("…", "...");
                    if (text.EndsWith(" ,"))
                        text = text.Substring(0, text.Length - 2);
                    if (text.EndsWith(","))
                        text = text.Substring(0, text.Length - 1);
                    
                    LineList.Add(new LinesClass { Character = character, Text = text.Trim() });
                }
            }

            // Ending document
            xmltextWriter.WriteEndElement();
            xmltextWriter.WriteEndElement();
            xmltextWriter.Flush();
            xmltextWriter.Close();
        }
        public class LinesClass
        {
            public string Character { get; set; }
            public string Text { get; set; }
        }
    }

}
