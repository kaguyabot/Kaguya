~~~ReadMe for Stage's Discord Bot V1.0~~~

References:
- To use json files instead of hardcoding, create a new class, copy from Config.cs, 
create/use the existing "Public Struct BotConfig", then create a "public <name>" for whatever it is 
you're trying to create. 

Example: 

public struct BotConfig
        {
            public uint timelyPoints;
			public uint timelyHours;
        }

For now you MUST write these in the json yourself and configure them, like this: 
{
    "timelyPoints": "300",
	"timelyHours": "24"
}