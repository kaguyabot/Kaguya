using KaguyaProjectV2.KaguyaBot.Core.DataStorage.JsonStorage;
using KaguyaProjectV2.KaguyaBot.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace KaguyaProjectV2.KaguyaBot.Core.Logger
{
    class Logger
    {
        ConfigModel config = new ConfigModel();
        

        public Task LogCommand()
        {
            return Task.CompletedTask;
        }

        public string LogString(LogLevel logLevel)
        {
            
        }
    }
}