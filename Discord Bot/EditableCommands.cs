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
        private const string configFile = "commands.json";

        public static BotConfig bot;

        static EditableCommands()
        {
           if (!Directory.Exists(configFolder))
                Directory.CreateDirectory(configFolder);

           if (!File.Exists(configFolder + "/" + configFile))
            {
                bot = new BotConfig();
                string json = JsonConvert.SerializeObject(bot, Formatting.Indented);
                //File.WriteAllText(configFolder + "/" + configFile, json);
            }
            else
            {
                string json = File.ReadAllText(configFolder + "/" + configFile);
                bot = JsonConvert.DeserializeObject<BotConfig>(json);
            }
        }

        public class BotConfig
        {
            public uint timelyHours { get; set; }
            public uint timelyPoints { get; set; }
        }

        public static void JsonInit()
        {
            //does the file exist?
            if (!File.Exists(configFile))
            {
                //if it doesnt
                Console.WriteLine("File commands.json not found, creating...");
                Console.WriteLine("Would you wanna create one, or just say fuck it and load defaults? [Y/N]");
                string input = Console.ReadLine();
                //if they wanna beuild a new file
                if (input.ToLower() == "y" || input.ToLower() == "yes")
                {
                    //they said yes
                    Console.WriteLine("Configuration:");
                    Console.WriteLine("How long should the wait between bonus points be available? Answer with a single integer for how many hours.");
                    string timelyHours = Console.ReadLine();
                    uint timelyHrs = Convert.ToUInt32(timelyHours, 16);
                    Console.WriteLine($"How many points should be given every {timelyHours}");
                    string timelyPoints = Console.ReadLine();
                    uint timelyPts = Convert.ToUInt32(timelyPoints, 16);
                    BotConfig config = new BotConfig()
                    {
                        timelyPoints = timelyPts,
                        timelyHours = timelyHrs
                    };
                    File.WriteAllText(configFile, JsonConvert.SerializeObject(config, Formatting.Indented));
                    Console.WriteLine("Wrote that shit bro.");
                    //creates new file from user input
                }
                //they dont want to build the file
                else
                {
                    Console.WriteLine("Welp fuck off then.");
                    Environment.Exit(0);
                    //exit because otherwise its broke
                }
            }
            //the file exists
            else
            {
                //lets try to see if the json matches your object
                try
                {
                    string jsoninput = File.ReadAllText(configFile);
                    BotConfig output = JsonConvert.DeserializeObject<BotConfig>(jsoninput);
                    //if it didnt deserialize properly
                    if (output == null)
                    {
                        Console.WriteLine("Unable to deserialize file.");
                        Console.WriteLine("Would you like to create a new one based off your object?");
                        string input = Console.ReadLine();
                        //same shit as above
                        if (input.ToLower() == "y" || input.ToLower() == "yes")
                        {
                            Console.WriteLine("First Variable");
                            string first = Console.ReadLine();
                            Console.WriteLine("Second Variable");
                            string second = Console.ReadLine();
                            BotConfig config = new BotConfig()

                            {
                                timelyPoints = Convert.ToUInt32(first, 16),
                                timelyHours = Convert.ToUInt32(second, 16)
                            };
                            File.WriteAllText(configFile, JsonConvert.SerializeObject(config, Formatting.Indented));
                            Console.WriteLine("Wrote that shit bro.");
                        }
                        //they dont wanna create a new file even tho the object doesnt match the json
                        else
                        {
                            Console.WriteLine("Welp fuck off then.");
                            Environment.Exit(0);
                            //itll be broke if you dont exit
                        }
                    }

                }
                catch
                {
                    //same shit, if it errors when trying to convert the json
                    Console.WriteLine("Unable to deserialize file.");
                    Console.WriteLine("Would you like to create a new one based off your object?");
                    string input = Console.ReadLine();
                    //same shit as above
                    if (input.ToLower() == "y" || input.ToLower() == "yes")
                    {
                        Console.WriteLine("First Variable");
                        string first = Console.ReadLine();
                        Console.WriteLine("Second Variable");
                        string second = Console.ReadLine();
                        BotConfig config = new BotConfig()
                        {
                            timelyPoints = Convert.ToUInt32(first, 16),
                            timelyHours = Convert.ToUInt32(second, 16)
                        };
                        File.WriteAllText(configFile, JsonConvert.SerializeObject(config, Formatting.Indented));
                        Console.WriteLine("Wrote that shit bro.");
                    }
                    //they dont wanna create a new file even tho the object doesnt match the json
                    else
                    {
                        Console.WriteLine("Welp fuck off then.");
                        Environment.Exit(0);
                        //itll be broke if you dont exit
                    }
                }
                //JsonShit jShit = new JsonShit()
                //{
                //    timelyPoints = "This shit1.",
                //    timelyHours = "this shit2."
                //};
            }
        }

    }
}