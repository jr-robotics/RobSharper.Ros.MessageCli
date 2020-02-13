# dotnet ROSMSG

## Installation

```
dotnet tool install -g RobSharper.Ros.MessageCli --version 1.0.0-rc1
```

## Usage
```
dotnet rosmsg build <ROS_MESSAGE_PACKAGE_FOLDER> <DESTINATION_FOLDER>
```
|Placeholder | Description |
|---|---|
|ROS_MESSAGE_PACKAGE_FOLDER | Path to a directory containing one or more ROS message packages. |
|DESTINATION_FOLDER | Folder where the generated messages will be copied. |



Use help option to get help:
```
dotnet rosmsg build --help
```

## Configuration

```
dotnet rosmsg config --help
```

### Custom NuGet feeds
ROSMSG support custom nuget feeds.

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