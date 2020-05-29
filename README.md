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
dotnet rosmsg config --help
```

### Custom NuGet feeds
ROSMSG supports custom NuGet feeds.
The official NuGet feed nuget.org is added by default.

List configured NuGet feeds:
```
dotnet rosmsg config feeds
```

Add new feed:
```
dotnet rosmsg config feeds add <NAME> --source <URL|PATH> [--protocol <PROTOCOL_VERSION>]
```

Remove an existing feed:
```
dotnet rosmsg config feeds remove <NAME>
```