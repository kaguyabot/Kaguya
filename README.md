# NOTICE
**THIS PROJECT IS DISCONTINUED!**

Anyone may fork or otherwise continue this project on their own accord, if they wish, by following the 
instructions below. I will no longer be maintaining this project in any shape or form. The project is simply too large for me to continue maintaining part time, especially with new Discord API features such as slash commands and components.


# Kaguya
[![CodeFactor](https://www.codefactor.io/repository/github/kaguyabot/kaguya/badge/v4-open-beta)](https://www.codefactor.io/repository/github/kaguyabot/kaguya/overview/v4-open-beta)
<a class="discord-widget" href="http://discord.gg/aumCJhr" title="Join us on Discord"> <img src="https://discordapp.com/api/guilds/546880579057221644/widget.png?style=shield"> </a> </a>

Kaguya is designed to be an easy-to-use, all-in-one Discord bot solution. Kaguya is currently in an **open beta** state. Users are encouraged to try out the beta and report bugs, feedback, and feature requests in this repository.

# Developing Kaguya
To run Kaguya, please make sure you meet the following prerequisites:
- A desktop platform with the [.NET 5.0 SDK](https://dotnet.microsoft.com/download) installed.
- [MySQL Version 8.0.18](https://dev.mysql.com/downloads/mysql/) or later installed.
- If modifying the code, I recommend using an IDE with intelligent code completion and syntax highlighting, such as [Visual Studio 2019+](https://visualstudio.microsoft.com/downloads/), [Jetbrains Rider](https://www.jetbrains.com/rider/), or [Visual Studio Code](https://visualstudio.microsoft.com/downloads/).
    - Ensure your IDE supports C# 9 and .NET 5.
- Before submitting your code for review, please review [the C# styling guidelines](https://www.dofactory.com/reference/csharp-coding-standards) that this project adheres to.
  - If using an IDE that supports a `.editorconfig` file, such as [Jetbrains Rider](https://www.jetbrains.com/rider/), you can use the 
  predefined configurations located within this file for automatic code refactoring before submitting.

## Downloading the source code
Clone:
```
$ cd (your installation directory)
$ git clone https://github.com/kaguyabot/Kaguya.git
$ cd Kaguya
```

Update to the latest version of the source code:
```
$ git pull
```

## Building
Kaguya requires several steps in order to actually get going, all involving a MySQL database.

Create a MySQL Database
```
$ mysql -u root -p
mysql> CREATE DATABASE kaguya;
```

Verify successful database creation:
```
mysql> SHOW DATABASES;

(output)
+--------------------+
| Database           |
+--------------------+
| information_schema |
| kaguya             | (<-- this line should be present)
| mysql              |
| performance_schema |
+--------------------+
4 rows in set (0.00 sec)
```

Navigate to your installation folder and find the `appsettings.Development.json` file.
You can do these steps through the command line or through a text editor.

Navigate to the project directory.
```
$ cd (clone_directory)/Kaguya/Kaguya/Kaguya
$ ls

(output)
appsettings.Development.json  Database/  Global.cs        Migrations/  Program.cs   Startup.cs
appsettings.json              Discord/   Kaguya.csproj    obj/         Properties/  Web/
bin/
```

Copy the contents of the `appsettings.json` file to `appsettings.Development.json`.
```
$ cp appsettings.json appsettings.Development.json
```

Modify `appsettings.Development.json` to include the following:
- BotToken
- OwnerId (your discord user ID)
- Database connection string


Open the file in vim, or edit in a text editor (VS Code).
```bash
$ vi -v appsettings.Development.json
or
$ code appsettings.Development.json
```
Your file should now look something like this (note that the "AdminSettings" section will change overtime, this is just an example). 
  - Replace `"OwnerId": 0` with your Discord user ID.
  - `"OsuApiKey"` can be left blank, but is required if you wish to execute any osu! related commands.

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "AllowedHosts": "*",
  "AdminSettings": {
    "OwnerId": 0,
    "OsuApiKey": ""
  },
  "DiscordSettings": {
    "BotToken": "YOUR_TOKEN",
    "MessageCacheSize": 50,
    "AlwaysDownloadUsers": true
  },
  "ConnectionStrings": {
    "Database": "server=localhost;user=root;password=YOUR_PASSWORD;database=kaguya"
  }
}
```

- Save this file and close it.

Now, we need to use entity framework to setup the database for us.

Setting up Entity Framework:
Check to see that you have Entity Framework on your system:
```
$ dotnet ef

(output)

                     _/\__
               ---==/    \\
         ___  ___   |.    \|\
        | __|| __|  |  )   \\\
        | _| | _|   \_/ |  //|\\
        |___||_|       /   \\\/\\

Entity Framework Core .NET Command-line Tools 5.0.1

Usage: dotnet ef [options] [command]

Options:
  --version        Show version information
  -h|--help        Show help information
  -v|--verbose     Show verbose output.
  --no-color       Don't colorize output.
  --prefix-output  Prefix output with level.

Commands:
  database    Commands to manage the database.
  dbcontext   Commands to manage DbContext types.
  migrations  Commands to manage migrations.

Use "dotnet ef [command] --help" for more information about a command.
```

If you don't see the above output, you need to install Entity Framework:
```
$ dotnet tool install --global dotnet-ef
```

Navigate to the folder containing `Kaguya.csproj` if you have not already done so. Note that this is the same folder as the previous step.

Run:
```
$ dotnet ef database update

(output)
Build started...
Build succeeded.
Done.
```

You are now able to run Kaguya.
```
$ dotnet run
```

## Contributing
If you wish to contribute to the codebase, please note the following:
- Create a fork of the code.
- All commands must adhere to the guidelines below.
- Submit your changes to us through a pull request into the `v4-open-beta` branch. In your PR, please give a brief overview of what you changed. Ensure your commits have concise description messages.

### Creating Commands
To create a Kaguya command, a very specific structure is used. This is so that the `$help` command can properly generate documentation for all commands at runtime.
A command can be as simple or as complex as necessary, this guide will include both examples.

**C# file template** (all Kaguya Commands are built out from this code):
```cs
using Discord.Commands;
using Kaguya.Discord.Attributes;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace $NAMESPACE$
{
    [Module()]
    [Group("$COMMAND$")]
    [Alias()]
    [RequireUserPermission()]
    [RequireBotPermission()]
    public class $CLASS$ : KaguyaBase<$CLASS$>
    {
        private readonly ILogger<$CLASS$> _logger;

        public $CLASS$(ILogger<$CLASS$> logger) : base(logger)
        {
            _logger = logger;
        }
        
        [Command]
        [Summary()]
        [Remarks()] // Delete if no remarks needed.
        public async Task NAME_ME()
        {
            
        }
    }
}
```

The attributes are extremely important, and there are other attributes not shown in the template that are used throughout the command-base. If you wish to take a deep dive into the `$help` documentation algorithm, please refer to [Help.cs](https://github.com/kaguyabot/Kaguya/blob/v4-open-beta/Kaguya/Kaguya/Discord/Commands/Reference/Help.cs).

An understanding of [CommandAttributes.cs](https://github.com/kaguyabot/Kaguya/blob/v4-open-beta/Kaguya/Kaguya/Discord/Attributes/CommandAttributes.cs) is necessary as well, though your IDE will be able to help you for the most part when it comes to filling in the blanks in the above template.

### Command Attributes
**(Recommended)** For general information on the basics of Discord.NET command writing, please read [this FAQ](https://docs.stillu.cc/faq/commands/general.html?tabs=cmdattrib).

- The `[Module()]` attribute simply takes in a `CommandModule` that serves as a category. 
  - Your command should be created in the appropriate directory that matches your `CommandModule`. For example, if you are writing an Administration command, your attribute should be `[Module(CommandModule.Administration)]`, your namespace should be `Kaguya.Discord.Commands.Administration`, and your working directory should be `Kaguya/Discord/Commands/Administration`.

- `[Group()]` is used to specify the base name of the command you are writing. Every command must have a base group (otherwise known as the "Module" in the Discord.NET documentation, NOT to be confused with `CommandModule`).
  - Group names are always lowercase.
  - Use a short and memorable name for your group.
- `[Command]`
  - Always inherits (and extends) the `[Group]` name.
  - Used as the name for the subcommand.
  - If the subcommand name is the same as the group name, leave the command attribute as: `[Command]`.
  - If the command is instead a *sub command*, it should be written as such: `[Command("-subcommandname")]` - don't forget to include the `-` character in-front.
  - Command names are always lowercase.
  - Example:
    ```cs
    [Group("example")]
    public class MyCommand : {...}
    {
      [Command] // Invoke: $example
      public async Task ExampleCommand()
      {}

      [Command("-sub")] // Invoke: $example -sub
      public async Task ExampleSubCommand()
      {}
    }
    ```
  - Finally, if the command you are running has a long processing time (3+ seconds) or has intentional delays, you should specify this through `[Command(RunMode = RunMode.Async)]`, otherwise the thread will be halted. View [CrossGamble.cs](https://github.com/kaguyabot/Kaguya/blob/feature/readme/Kaguya/Kaguya/Discord/Commands/Games/CrossGamble.cs) for a great example of when to use `RunMode.Async`.
- `[Alias()]` **(optional)** is simply an even shorter "shortcut" for the group name. This can also be assigned underneath the `[Command]` attribute to give an alias to a subcommand.
  - Do not give aliases to sub-commands that directly inherit the `[Group]` name.
- For `[RequireUserPermission()]` and `[RequireBotPermission()]` **(both optional)**, [read here]([RequireBotPermission(GuildPermission.BanMembers)]).
  - If your command does not require specific Discord user or bot permissions (executable by any regular user), delete these attributes from your command.

For the sake of this program, the `[Summary()]` and `[Remarks()]` attributes are used solely for the automatic generation of command documentation.
- `[Summary]` is used for a description of what the command does. The user will be reading this directly, and it is all they will know on what a command does and how to use it.

- `[Remarks]` is used strictly for what command parameters are allowed to be passed into the command.
  - `"<param>"` indicates to the user that a parameter is required.
  - `"[param]"` indicates to the user that a parameter is optional.
    - These need to correspond to what is taken in by the command itself. i.e. don't have a parameter be required to the user when in fact it is optional in the code. Command documentation may be automatically generated by command parameters in the future.
  - **Important:** If specifying multiple ways to use a command, separate entries with a `\n` character.
    - Example: `[Remarks("\n<param 1> [optional param]\n[optional param 1] [optional param 2]")]`
      - Note the `\n` at the very front of the string. This indicates to the user the command can be used by itself with no parameters *and* in the other ways written.
      - Let's say this command is `$foo`. Then, this is displayed to the user as:
      ```
      (user input: $help foo)
      
      (output)
      {...}

      Usage:
      $foo
      $foo <param 1> [optional param]
      $foo [optional param 1] [optional param 2]

      {...}
      ```

Other custom attributes:
- `[Example()]` can be added to a command **method** (not group / module / class) to show the user how your command is supposed to be used.
  - Add 1 attribute per example.
  - Example:
    ```cs
    [Module(CommandModule.IDontExist)]
    [Group("Foo")]
    public class Foo : {...}
    {
        [Command]
        [Example()] // Compile time error CS7036
        [Example(null)] // Runtime error System.ArgumentNullException
        [Example("")] // Shows the user this command can be used without arguments.
        [Example("Do penguins fly?")]
        [Example("Why did the chicken cross the road?")]
        public async Task FooCommand([Remainder]string userQuestion = null)
        {
            // Process input
        }
    }
    ```
- `[ModuleRestriction()]` is used to define restrictions for certain classes of users. This generally should not be included in any code you submit, but instead let us know in your PR that you intend for this command to be restricted to either bot owners or premium users only.
- `[CommandMetadata()]` is used if you intend to write command documentation at the class level, instead of at the command level. This is useful if you write many commands that do similar things. View [Emotion.cs](https://github.com/kaguyabot/Kaguya/blob/v4-open-beta/Kaguya/Kaguya/Discord/Commands/Fun/Emotion.cs) for an example.

### Command Examples
For examples on production-ready commands, please browse through the [Commands folder](https://github.com/kaguyabot/Kaguya/tree/v4-open-beta/Kaguya/Kaguya/Discord/Commands).

### Database changes
If making any changes to the database (`Kaguya.Database.Context` or `Kaguya.Database.Context.Models` namespaces):

- Run:
  ```
  $ dotnet ef migrations add your_brief_description
  $ dotnet ef database update
  ```
- Ensure the changes you have made work correctly.
- Tell us explicitly in your PR.
- In general, you should never delete an item out of any model, unless it is completely unused by the rest of the program.

If making an entirely new database table, do the following **in order**:
- Create a model under `Kaguya.Database.Context.Models`.

- Reference an existing similar model for design structure.
- Create an interface under `Kaguya.Database.Context.Interfaces` that inherits from `IRepository`. Include additional methods if needed for accessing the database in a specific way.
  ```cs
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using Kaguya.Database.Model;

  namespace Kaguya.Database.Interfaces
  {                            // long is the type of ID this example object has.
    public interface IFooRepository : IRepository<long, Foo>
    {
        // Example - this could be anything from the DB for this object.
        // The interface can also be completely empty.
        public Task<IList<Foo>> GetLargestFooAsync();
    }
  }
  ```
- Create a repository under `Kaguya.Database.Repositories`. View [the other repositories](https://github.com/kaguyabot/Kaguya/tree/v4-open-beta/Kaguya/Kaguya/Database/Repositories) for examples.
  ```cs
  namespace Kaguya.Database.Interfaces
  {
    public class FooRepository : IFooRepository
    {
      // Implement missing methods.
    }
  }
  ```
- Insert your new type into the `DbSet<T>` inside of [KaguyaDbContext.cs](https://github.com/kaguyabot/Kaguya/blob/v4-open-beta/Kaguya/Kaguya/Database/Context/KaguyaDbContext.cs). We organize the properties in alphabetical order by type.
  ```cs
  {...}

  public DbSet<Foo> Foos { get; set; }
  
  {...}
  ```
- Index your queries after you have built your repository. Anything inside of a LINQ expression is indexable. This is done by modifying the `OnModelCreating()` method in this class. [Indexing instructions](https://github.com/kaguyabot/Kaguya/blob/v4-open-beta/Kaguya/Kaguya/Database/Context/KaguyaDbContext.cs#L42) are listed inside of this method.
- Finally, add your new repository as a scoped service in [Startup.cs](https://github.com/kaguyabot/Kaguya/blob/v4-open-beta/Kaguya/Kaguya/Startup.cs#L58)
  ```cs
  {...}

  services.AddScoped<FooRepository>();

  {...}
  ```
