using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Discord_Bot
{
    class EditableCommands
    {
        private const string configFolder = "Resources";
        private const string configFile = "config.json";
        private const string commandFile = "commands.json";
    
        public static TimelyConfig bot;

        static EditableCommands()
        {
            if (!Directory.Exists(configFolder))
                 Directory.CreateDirectory(configFolder);
            if (!File.Exists(configFolder + "/" + commandFile))
            {
                bot = new TimelyConfig();
                string json = JsonConvert.SerializeObject(bot, Formatting.Indented);
                //File.WriteAllText(configFolder + "/" + commandFile, json);
            }
            else
            {
                string json = File.ReadAllText(configFolder + "/" + commandFile);
                bot = JsonConvert.DeserializeObject<TimelyConfig>(json);
            }
        }

        public class TimelyConfig
        {
            public uint timelyHours { get; set; }
            public uint timelyPoints { get; set; }
        }

        public class BotConfig
        {
            public string token { get; set; }
            public string cmdPrefix { get; set; }
            public string osuapikey { get; set; }
        }


        public static void JsonInit()
        {
            if (!File.Exists(commandFile))
            {
                Console.WriteLine("File commands.json not found, creating...");
                TimelyConfig config = new TimelyConfig()
                {
                    timelyPoints = 500,
                    timelyHours = 24
                };
                File.WriteAllText(commandFile, JsonConvert.SerializeObject(config, Formatting.Indented));
                Console.WriteLine("Wrote that shit bro.");
                Console.WriteLine("Default values loaded: " +
                    "\nTimely Points: 500" +
                    "\nTimely Hours: 24");
                //creates new file from user input
            }
            //the file exists
            else
            {
                //lets try to see if the json matches your object
                try
                {
                    string jsoninput = File.ReadAllText(commandFile);
                    TimelyConfig output = JsonConvert.DeserializeObject<TimelyConfig>(jsoninput);
                    //if it didnt deserialize properly
                    if (output == null)
                    {
                        Console.WriteLine("Unable to deserialize file.");
                        Console.WriteLine("Creating a new one based on existing object...");
                        TimelyConfig config = new TimelyConfig()
                        {
                            timelyPoints = 500,
                            timelyHours = 24
                        };
                        File.WriteAllText(commandFile, JsonConvert.SerializeObject(config, Formatting.Indented));
                        Console.WriteLine("Wrote that shit bro.");
                    }
                }
                catch
                {
                    //same shit, if it errors when trying to convert the json
                    Console.WriteLine("Unable to deserialize file.");
                    Console.WriteLine("Would you like to create a new one based off your object?");
                    string input = Console.ReadLine();
                    //same shit as above
                    TimelyConfig config = new TimelyConfig()
                    {
                        timelyPoints = 500,
                        timelyHours = 24
                    };
                    File.WriteAllText(commandFile, JsonConvert.SerializeObject(config, Formatting.Indented));
                    Console.WriteLine("Wrote that shit bro.");
                }
            }
        }
    }
}