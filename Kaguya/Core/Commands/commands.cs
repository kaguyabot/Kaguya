using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kaguya.Core.Commands
{
    public static class Commands
    {
        private static List<Command> commands;

        private static string commandsFile = "Resources/commands.json";

        static Commands()
        {
            if (DataStorage2.SaveExists(commandsFile))
            {
                commands = DataStorage2.LoadCommands(commandsFile).ToList();
            }
            else
            {
                commands = new List<Command>();
                SaveCommands();
            }
        }

        public static Command GetCommand()
        {
            return GetOrCreateCommand();
        }

        private static Command GetOrCreateCommand()
        {
            var result = commands;

            var command = result.FirstOrDefault();
            if (command == null) command = CreateCommand();
            return command;
        }

        private static Command CreateCommand()
        {
            var newCommand = new Command() //Sets default values for commands.json on file creation.
            {
                TimelyHours = 24,
                TimelyPoints = 500
            };

            commands.Add(newCommand);
            SaveCommands();
            return newCommand;
        }

        public static void SaveCommands()
        {
            DataStorage2.SaveCommands(commands, commandsFile);
        }

    }
}
