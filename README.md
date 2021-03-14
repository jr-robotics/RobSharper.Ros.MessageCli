# dotnet ROSMSG

dotnet ROSMSG is a .Net Core global tool used to build .Net Packages from ROS message packages.
 
## Installation

```
dotnet tool install -g RobSharper.Ros.MessageCli
```

## Usage
```
dotnet rosmsg build <ROS_MESSAGE_PACKAGE_FOLDER> <DESTINATION_FOLDER> [--filter FILTER_EXPR[ FILTER_EXPR]*] [--nupkg] [--dll]
```
|Argument | Description |
|---|---|
|ROS_MESSAGE_PACKAGE_FOLDER | Path to a directory containing one or more ROS message packages. |
|DESTINATION_FOLDER | Folder where the generated messages will be copied. |
| --filter FILTER_EXPR[ FILTER_EXPR]* | Only create message packages matching the given filter expression. FILTER_EXPR is the name of a package. It can start or end with * to filter for a ends-with or starts-with exression. Seperate multiple FILTER_EXPR with a space | 
| --nupkg | [Default] Create nuget package. |
| --dll | Create dll. |



Use help option to get further help:
```
dotnet rosmsg build --help
```

## Configuration

```
dotnet rosmsg --help
```

### Custom NuGet feeds
ROSMSG supports custom NuGet feeds.
The official NuGet feed nuget.org is added by default.

Print usage:
```
dotnet rosmsg config-feeds --help
```

List configured NuGet feeds:
```
dotnet rosmsg config-feeds
```

Add new feed:
```
dotnet rosmsg config-feeds add <NAME> <SOURCE_URL|SOURCE_PATH> [<PROTOCOL_VERSION>]
```

Remove an existing feed:
```
dotnet rosmsg config-feeds remove <NAME>
```

### Default output format
Defines the default output format.
This can either be **nupkg** or **dll**.

Print usage:
```
dotnet rosmsg config-output --help
```

Show current value:
```
dotnet rosmsg config-output
```

Set new value:
```
dotnet rosmsg config-output nupkg
dotnet rosmsg config-output dll
```

### Default namespace
Defines the default root namespace for generated messages.

Print usage:
```
dotnet rosmsg config-namespace --help
```

Show current value:
```
dotnet rosmsg config-namespace
```

Set new value:
```
dotnet rosmsg config-namespace <NAMESPACE>
```

### Default code generator
dotnet ROSMSG supports code generators for different .net ROS implementations.

Supported generators are:

| Config option | ROS implementation | Notes |
| ------------- | ------------------ | ----- |
| robsharper    | RobSharper.ROS     | [RobSharper.Ros](https://github.com/jr-robotics?q=RobSharper.Ros) is a set of packages and tools from [JOANNEUM RESEARCH ROBOTICS](https://www.joanneum.at/robotics/en/). |
| rosnet        | ROS.Net            | This generator uses the [Xamla fork](https://github.com/Xamla/ROS.NET) of ROS.Net, which is available as [NuGet package](https://www.nuget.org/packages/Uml.Robotics.Ros/). |


Print usage:
```
dotnet rosmsg config-codegenerator --help
```

Show current value:
```
dotnet rosmsg config-codegenerator
```

Set new value:
```
dotnet rosmsg config-codegenerator <robsharper|rosnet>
```