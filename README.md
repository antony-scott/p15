# Background
p15 (or project-15) is a developer tool intended to make life that little bit easier by
automating a few of the tedious things that soak up time during a "day in the life"

The name comes from the idea that a developer should be able to get any piece of software
(consisting of multiple moving parts) running on their development machine inside 15 minutes
(including making a refreshing drink of their choice!)

It's basically a process manager and a log viewer at it's heart, with a few other goodies
thrown in for no charge!

# Pre-requisites
* Nuget CommandLine
* git / hg
* powershell 6+

# Installation
TBC

# Features
* per-application context menu
  * Syncing repositories from remote sources (hg/git)
  * building software (using build.ps1 if found, or msbuild if not)
  * opening application folders in file explorer
  * opening application logs folder in file explorer
  * opening terminal in application folders
  * opening the IDE (solution)
* log viewer
  * handling of multi-line log entries (with collapsible view)
  * colourising of log entries
  * auto-scrolling of log views
  * clear function for log views
  * handling of adb logcat sourced logs
* automatic creation (and permission granting) of MSMQs
* Barcode rendering on-screen with live editing (barcode will be
defined in the package yaml file)
* automatic sync of listed repositories (for example sql-server patch repo)
* startup tasks (e.g. - starting docker containers)

# Package file format
Packages are a concept of representing a collection of repositories which consitute a software
package. Each package in p15 is represented by a yaml file

_example package file_
```yaml
name: my awesome software package

applications:
  - name: Web Front End
    application_type: web
    source: https://bitbucket.org/<username>/web_front_end.git

  - name: ESB Application
    application_type: esb
    arguments: --run-as-console-app
    source: https://bitbucket.org/<username>/esb_back_end.git

  - name: Mobile App
    application_type: mobile
    source: https://bitbucket.org/<username>/mobile_app.git
    logs: adb logcat -v long -s mobileapp

bookmarks:
  Getting Started: https://mycompany.net/gettingstarted

intents:
  - name: Barcode
    command: shell am broadcast -a com.awesome.BARCODE --es com.symbol.datawedge.data_string '{value}' --es com.symbol.datawedge.label_type '{symbology}'

barcodes:
  - name: Setup
    symbology: QRCODE
    values:
      local: http://{ipaddress}:8086/
      emulator: http://{android-host-loopback}:8086/
      PROD: https://mycompany.com/api/
  - name: Package (2D)
    symbology: PDF417
    values:
      1234: id=1&ver=1&type=1&track=1234&date=20210709T095400+00
```

# Configuration

### Environment Variables

* **DevRootFolder** > all repositories will be cloned into sub-folders within this folder.
If the environment variable is not currently set, p15 will look for `C:\_Code\` and `C:\Code\`
folders and set the environment variable to the first one it finds.

### Config files
Application settings are stored in a p15.config.json file in the same folder as the executable.
Settings can be overridden by addings a .p15config file in your user folder

_example p15.config.json file_
```json
{
  "AppSettings": {
    "RavenServers": [
      {
        "Version": 2,
        "Location": "C:\\Data\\RavenDB 2.5\\Server\\Raven.Server.exe",
        "Port": 8082
      },
      {
        "Version": 3,
        "Location": "C:\\Data\\RavenDB-3.5.2\\Server\\Raven.Server.exe",
        "Port": 8083
      }
    ],
    "PackagesRepository": "https://bitbucket.org/<username>/p15-packages.git",
    "ManagedRepositories": [
      {
        "Name": "Master SQL Server Repo",
        "Url": "https://bitbucket.org/<username>/databases_sql-server-databases_master.git",
        "Folder": ".sqlserver.master"
      },
      {
        "Name": "Mobile SQL Server Repo",
        "Url": "https://bitbucket.org/<username>/databases_sql-server-databases_mobile.git",
        "Folder": ".sqlserver.mobile"
      }
    ]
  }
}
```

* **RavenServers** >
p15 will automatically start all raven servers listed in the **RavenServers** section and poll them until the `/version` endpoint responds with a success http code to confirm they're running.

* **PackagesRepository** >
This is where p15 will look to pull down the latest packages files

* **ManagedRepositories** >
p15 will update/clone these repositories on startup, helping you to keep up-to-date with cross-cutting repositories. The folder is relative to the RootFolder

* **StartupTasks** >
p15 can start execute command on startup.

* **Terminal** >
p15 will load `pwsh` as the terminal (right-click on app), this can be overridden here

_example .p15config file_
```json
{
  "AppSettings": {
    "Terminal": "C:\\Program Files\\Git\\git-bash.exe",
    "StartupTasks": [
      "docker start sqlserver"
    ]
  }
}
```

# Technology
p15 is written in C# using the [.NET Core 3.1](https://devblogs.microsoft.com/dotnet/announcing-net-core-3-1/)
framework. UI duties are handled by [Avalonia](http://avaloniaui.net/) (a
cross-platform UI library), which means p15 is cross-platform, so it utilises
[Powershell Core](https://github.com/PowerShell/PowerShell) for system level operations.

# Powershell Module
p15 auto-installs a powershell module, called **p15**, which can be used from a pwsh prompt.

### Functions of note
| Function Name | Description | Parameters |
| :- | :- | :- |
| Sync-Folder | pull/clone remote repository using either git or hg (auto-detected based on url) | Path, Url |
| Start-WebApplication | Finds the web application within the path and starts it in IIS | Path |
| Start-EsbService |Finds the ESB application within the path and starts it (_Arguments can be used to pass arguments, such as `--run-as-console-app` to the exe_) | Path, Arguments |
| Initialize-MissingMessageQueues | Searches config files of any applications in the path and creates any missing MSMQs, it will also give the current user full permissions to the MSMQs (created or pre-existing)  | Path |
| Build-Application | runs `.\build.ps1` (if it exists) or runs `nuget restore` followed by `msbuild` | Path |
| Get-EsbProcessId | returns the process id of the running ESB process | Project |
| Get-WebProcessId | returns the process id of the running web application | Project |
| Open-Firewall | Create an inbound firewall rule (if non already exists) for Domain & Private profiles and places it in the **p15** rule group  | Project, Port |
| Open-MobileDeveloperPortal | Gets the ip address of the connected android device (via adb) and opens up the mobile developer portal in the system's default web browser | _none_ |

# Roadmap

### vNext
* open port in firewall for local identity server
* allow extra folders to be monitored for log files

### vNext+1
* activity stream - a way of viewing log entries from multiple
application in one view, the log entries will be interleaved in
time order
* clear function activity stream
* readme (html/txt/md) viewer
* usb device detection (and auto connection for ad logcat purposes)
* log view / activity stream filtering (think kibana!)
* integration with powershell cmdlet (for example - download/restore of databases)
* log view / activity stream search
