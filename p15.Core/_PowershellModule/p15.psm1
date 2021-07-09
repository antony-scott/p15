[Reflection.Assembly]::LoadWithPArtialName("System.Messaging") > $null
$msmq = [System.Messaging.MessageQueue]

function Get-SourceControlType {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $true)]
        [string] $Path
    )

    Process {
        $folder = Join-Path -Path $Path -ChildPath ".hg"
        if (Test-Path -Path $folder) {
            return "hg"
        }

        $folder = Join-Path -Path $Path -ChildPath ".git"
        if (Test-Path -Path $folder) {
            return "git"
        }

        return ""
    }
}

function Pull-Folder {
    [CmdletBinding()]
    Param
    (
        [Parameter(Mandatory = $true)]
        [string] $Path,

        [Parameter(Mandatory = $true)]
        [string] $Url,

        [Parameter(Mandatory = $false)]
        [bool] $Update = $true
    )

    Process {
    
        Write-Host "Pulling from [$Url] to [$Path] ..."

        $folderCreated = $false

        if (-not (Test-Path -Path $Path)) {
            Write-Information "Pull-Folder: Creating folder $Path"
            New-Item -Path $Path -ItemType Directory > $null
            $folderCreated = $true
        }
        Set-Location -Path $Path

        if ($Url.endswith(".git")) {
            if ($folderCreated) {
                Write-Information "Pull-Folder: git clone $Url ($Path)"
                $cmd = "& git clone $Url ."
                Invoke-Expression $cmd | Write-Information
            }
            else {
                if (Test-Path -Path ".git") {
                    if ($Update) {
                        Write-Information "Pull-Folder: git pull ($Path)"
                        $cmd = "& git pull"
                        Invoke-Expression $cmd | Write-Information
                    }
                }
                else {
                    Write-Information "Pull-Folder: git clone $Url ($Path)"
                    $cmd = "& git clone $Url ."
                    Invoke-Expression $cmd | Write-Information
                }
            }
        }
        else {
            if ($folderCreated) {
                Write-Information "Pull-Folder: hg clone $Url ($Path)"
                $cmd = "& hg clone $Url ."
                Invoke-Expression $cmd | Write-Information
            }
            else {
                if (Test-Path -Path ".hg") {
                    if ($Update) {
                        Write-Information "Pull-Folder: hg pull --update $Url ($Path)"
                        $cmd = "& hg pull --update"
                        Invoke-Expression $cmd | Write-Information
                    }
                }
                else {
                    Write-Information "Pull-Folder: hg clone $Url ($Path)"
                    $cmd = "& hg clone $Url ."
                    Invoke-Expression $cmd | Write-Information
                }
            }
        }
    }
}

function Push-Folder {
    [CmdletBinding()]
    Param
    (
        [Parameter(Mandatory = $true)]
        [string] $Path,

        [Parameter(Mandatory = $true)]
        [string] $Url
    )

    Process {
    
        Write-Host "Pushing from [$Path] to [$Url] ..."

        Set-Location -Path $Path

        if ($Url.endswith(".git")) {
            Write-Information "Push-Folder: git push && git push --tags"
            $cmd = "& git push && git push --tags"
            Invoke-Expression $cmd | Write-Information
        }
        else {
            Write-Information "Push-Folder: hg push"
            $cmd = "& hg push"
            Invoke-Expression $cmd | Write-Information
        }
    }
}


function Open-SourceControlWorkbench {
    Param
    (
        [Parameter(Mandatory = $true)]
        [string] $Path
    )

    Process {

        if (-not (Test-Path -Path $Path)) {
            Write-Error "Folder doesn't exist => $Path"
        }
        else {

            $type = Get-SourceControlType -Path $Path

            if ($type -eq "hg") {
                Set-Location $Path
                & thgw.exe
            }
            elseif ($type -eq "git") {
                & TortoiseGitProc.exe /command:repobrowser /path:"$Path"
            }
        }
    }
}

function Get-LatestChangesetTimestamp {
    Param
    (
        [Parameter(Mandatory = $true)]
        [string] $Path
    )

    Process {
        if (-not (Test-Path -Path $Path)) {
            Write-Error "Folder doesn't exist => $Path"
        }
        else {
            Set-Location $Path

            $type = Get-SourceControlType -Path $Path

            if ($type -eq "git") {
                return git log -n 1 --format=%ai
            }
            elseif ($type -eq "hg") {
                return hg log -r tip -T"{date|isodate}"
            }
            else {
                throw "unknown repository type!"
            }
        }
    }
}

function Find-MSBuild {
    $vswhere = "${ENV:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"

    $path = & $vswhere -latest -prerelease -products * -requires Microsoft.Component.MSBuild -property installationPath
    if ($path) {
        $tool = join-path $path 'MSBuild\Current\Bin\MSBuild.exe'
        if (-not (test-path $tool)) {
            $tool = join-path $path 'MSBuild\15.0\Bin\MSBuild.exe'
            if (-not (test-path $tool)) {
                throw 'Failed to find MSBuild'
            }
        }
    }
    return $tool
}

function Get-WebSiteInformation {
    Param
    (
        [Parameter(Mandatory = $true)]
        [string] $Filename
    )

    Process {
        $xml = [xml] (Get-Content -Path $Filename)
        $iisUrl = $xml.Project.ProjectExtensions.VisualStudio.FlavorProperties.WebProjectProperties.IISUrl
        $uri = [System.Uri] $iisUrl

        @{ Port = $uri.Port; HasPath = $uri.LocalPath.Length -gt 1; Path = $uri.LocalPath; }
    }
}

function Start-WebApplication {
    Param
    (
        [Parameter(Mandatory = $true)]
        [string] $Path
    )

    Process {
        $projectName = Split-Path $Path -Leaf

        $appcmd = "C:\Program Files\IIS Express\appcmd.exe"
        $iiscmd = "C:\Program Files\IIS Express\iisexpress.exe"

        $csprojFilename = Get-ChildItem -Path "$Path" -Filter "$projectName.csproj" -Recurse | Select-Object -First 1 | Select -ExpandProperty FullName

        $physicalPath = Split-Path -Path "$csprojFilename"
        $websiteInfo = Get-WebSiteInformation -Filename "$csprojFilename"

        $configFilename = Get-ChildItem -Path "$Path" -Filter "applicationhost.config" -Recurse | Select-Object -First 1 | Select -ExpandProperty FullName

        if ($configFilename -eq $null) {
            Write-Debug "Copying applicationhost.config template file"
            Copy-Item -Path "C:\Program Files\IIS Express\config\templates\PersonalWebServer\applicationhost.config" -Destination "$Path"

            $configFilename = Get-ChildItem -Path "$Path" -Filter "applicationhost.config" -Recurse | Select-Object -First 1 | Select -ExpandProperty FullName

            $cmd = "& '$appcmd' delete site WebSite1 /apphostconfig:'$configFilename'"
            Write-Debug "Deleting 'WebSite1' site"
            Invoke-Expression $cmd
            
            $port = $websiteInfo.Port
            $cmd = "& '$appcmd' add site /name:'$projectName' /bindings:http://*:$port /physicalPath:'$physicalPath' /apphostconfig:'$configFilename'"
            Write-Debug "Adding '$projectName' site"
            Invoke-Expression $cmd

            $hasPath = $websiteInfo.HasPath
            if ($hasPath -eq $true) {
                $vpath = $websiteInfo.Path
                $cmd = "& '$appcmd' add app /site.name:'$projectName' /path:$vpath /physicalpath:'$physicalPath' /apphostconfig:'$configFilename'"
                Write-Debug "adding path '$vpath' to '$projectName' site"
                Invoke-Expression $cmd
            }
        }

        Open-Firewall -Description $projectName -Port $websiteInfo.Port | Out-Null

        Write-Debug "Starting '$projectName' site"
        $process = Start-Process -Verb runAs -FilePath "$iiscmd" -ArgumentList "/config:$configFilename","/site:$projectName" -PassThru -WindowStyle Minimized
        
        $retval = "" | Select-Object -Property ProcessId, Port
        $retval.ProcessId = $process.Id
        $retval.Port = $websiteInfo.Port
        $retval | ConvertTo-Json
    }
}

function Get-RunningWebApplications {
    Process {
        Get-WmiObject -Query "SELECT ProcessId,CommandLine FROM Win32_Process WHERE Name='iisexpress.exe'" |
        Foreach-Object {
            $exe, $args = ($_.CommandLine -split " /");
            [PSCustomObject] @{
                ProcessId = $_.ProcessID;
                Site = ($args | Where-Object {$_ -Like "site:*" } ) -replace "site:", "" }
            }
    }
}

function Start-EsbService {
    [CmdletBinding()]
    param (
        [Parameter()]
        [string] $Path,

        [Parameter()]
        [string[]] $Arguments
    )

    Process {
        $projectName = Split-Path $Path -Leaf

        $exeFile = Get-ChildItem -Path "$Path" -Filter "$projectName.exe" -Recurse | Select-Object -First 1 | Select -ExpandProperty FullName
        if ($exeFile -eq $null) {
            $exeFile = Get-ChildItem -Path "$Path" -Filter "NServiceBus.Host.exe" -Recurse | Select-Object -First 1 | Select -ExpandProperty FullName
        }

        if ($exeFile -ne $null) {
            Write-Host "Starting $exeFile"
            $exePath = Split-Path -Path $exeFile
            $process = Start-Process -FilePath "$exeFile" -WorkingDirectory "$exePath" -ArgumentList $Arguments -PassThru -WindowStyle Minimized
            
            $retval = "" | Select-Object -Property ProcessId
            $retval.ProcessId = $process.Id
            $retval | ConvertTo-Json
        } else {
            Write-Error "Cannot find NServiceBus.Host.exe or $projectName.exe in $Path"
        }
        return $null
    }
}

function Get-RunningEsbApplications {
    Process {
        # is there a better way to detect this? for example: MessageRouter doesn't use NServiceBus.Host.exe
        Get-WmiObject -Query "SELECT ProcessId,CommandLine FROM Win32_Process WHERE Name='NServiceBus.Host.exe'" |
        Foreach-Object {
            $segments = $_.CommandLine -split '[\\/]'
            [PSCustomObject] @{
                ProcessId = $_.ProcessID;
                Segments = $segments;
            }
        }
    }
}

function Get-ConfigFilename {
    [CmdletBinding()]
    param (
        [Parameter()]
        [string] $Path
    )
    Process {
        $projectName = Split-Path $Path -Leaf
        $csprojFilename = Get-ChildItem -Path "$Path" -Filter "$projectName.csproj" -Recurse | Select-Object -First 1 | Select -ExpandProperty FullName
        $folder = Split-Path -Path "$csprojFilename"
        $configFilename = Get-ChildItem -Path "$folder" -Filter "app.config" | Select-Object -First 1 | Select -ExpandProperty FullName
        if ($configFilename -eq $null) {
            $configFilename = Get-ChildITem -Path "$folder" -Filter "web.config" | Select-Object -First 1 | Select -ExpandProperty FullName
        }
        return $configFilename
    }
}

function Find-MessageQueues {
    [CmdletBinding()]
    param (
        [Parameter(ValueFromPipeline)]
        [string] $Path
    )
    Process {
        $queues = {@()}.Invoke()
        if (Test-Path -Path $Path) {
            $configFilename = Get-ConfigFilename -Path $Path
            $xml = [xml](Get-Content $configFilename)

            $xpathQueries = @(
                '//add[@key="EndpointName"]/@value#inputqueue',
                '//add[@key="InputQueue"]/@value#inputqueue',
                '//MessageEndpointMappings/add/@Endpoint',
                '//MessageForwardingInCaseOfFaultConfig/@ErrorQueue',
                '//AuditConfig/@QueueName',
                '//MsmqTransportConfig/@InputQueue',
                '//MsmqTransportConfig/@ErrorQueue',
                '//MsmqSubscriptionStorageConfig/@Queue'
            )

            $xpathQueries | Foreach {
                $parts = $_ -split "#"
                $xpathQuery = $parts[0]
                $result = Select-Xml $xpathQuery $xml
                if ($result -ne $null) {
                    $queues.Add($result.Node.value)
                    if ($parts.length -gt 1) {
                        switch ($parts[1]) {
                            "inputqueue" {
                                $queues.Add($result.Node.value + ".timeouts")
                                $queues.Add($result.Node.value + ".timeoutsdispatcher")
                                $queues.Add($result.Node.value + ".retries")
                            }
                        }
                    }
                }
            }
        }
        return $queues
    }
}

function Initialize-MessageQueues {
    [CmdletBinding()]
    param (
        [Parameter(ValueFromPipeline)]
        [string[]] $QueueNames
    )

    Process {
        $username = New-Object System.String "$env:UserDomain\$env:UserName"
        $rights = [System.Messaging.MessageQueueAccessRights]::FullControl
        $entryType = [System.Messaging.AccessControlEntryType]::Set

        # Create any missing queues
        $QueueNames |
            Foreach-Object {
                $queueName = '.\Private$\' + $_
                $q = $msmq::Exists($queueName) ? (New-Object System.Messaging.MessageQueue $queueName) : ($msmq::Create($queueName, $true))
                $q.SetPermissions($username, $rights, $entryType)
        }
    }
}

function Initialize-MissingMessageQueues {
    [CmdletBinding()]
    param (
        [Parameter()]
        [string] $Path
    )
    Process {
        $Path | Find-MessageQueues | Initialize-MessageQueues
    }
}

function Build-Application {
    [CmdletBinding()]
    param (
        [Parameter()]
        [string] $Path
    )
    Process {
        Set-Location -Path $Path
        $scriptFile = Join-Path -Path $Path -ChildPath "build.ps1"
        if (Test-Path -Path $scriptFile) {
            # parse the build.ps1 file & add "-Configuration Debug" if the script supports the Configuration parameter !!
            $script = [System.Management.Automation.Language.Parser]::ParseFile( $scriptFile,[ref]$null,[ref]$null)
            $configurationParameter = $script.ParamBlock.Parameters | where Name -Like '$Configuration'
            $argumentList = @("-NoExit", "-File ""$scriptFile""")
            if ($configurationParameter -ne $null) {
                $argumentList += "-Configuration Debug"
            }
            Start-Process "pwsh.exe" -ArgumentList $argumentList -WindowStyle Normal
        } else {
            nuget restore
            msbuild
        }
    }
}

function Get-EsbProcessId {
    [CmdletBinding()]
    param (
        [Parameter()]
        [string] $Project
    )
    Process {
        $searcher = [wmisearcher]"SELECT ProcessId,CommandLine FROM Win32_Process WHERE Name='NServiceBus.Host.exe' OR Name='$Project.exe'"
        $processId = $searcher.Get() | Where-Object CommandLine -Match "\\$Project\\" | Select-Object -Property ProcessId
        $processId | ConvertTo-Json
    }
}

function Get-WebProcessId {
    [CmdletBinding()]
    param (
        [Parameter()]
        [string] $Project
    )
    Process {
        $searcher = [wmisearcher]"SELECT ProcessId,CommandLine FROM Win32_Process WHERE Name='iisexpress.exe'"
        $processId = $searcher.Get() | Where-Object CommandLine -Match $Project | Select-Object -Property ProcessId
        $processId | ConvertTo-Json
    }
}

function Get-ProcessIsRunning {
    [CmdletBinding()]
    param (
        [Parameter()]
        [int] $ProcessId
    )
    Process {
        $process = Get-Process -Id $ProcessId
        if ($process -ne $null) {
            @{ProcessId = $ProcessId;} | ConvertTo-Json
        }
    }
}

function Open-Firewall {
    [CmdletBinding()]
    param (
        [Parameter()]
        [string] $Description,

        [Parameter()]
        [int] $Port
    )
    Process {
        $rule = Get-NetFirewallPortFilter | Where LocalPort -eq $Port | Get-NetFirewallRule
        if ($rule -eq $null) {
            New-NetFirewallRule -Action Allow -Direction Inbound -DisplayName $Description -Group "p15" -InterfaceType Any -LocalPort $Port -Profile Domain,Private -Protocol TCP
        }
    }
}

function Open-MobileDeveloperPortal {
    $IPv4RegexNew = 'inet\s+addr:((?:(?:1\d\d|2[0-5][0-5]|2[0-4]\d|0?[1-9]\d|0?0?\d)\.){3}(?:1\d\d|2[0-5][0-5]|2[0-4]\d|0?[1-9]\d|0?0?\d))'
    $String = adb shell ifconfig wlan0
    $ipAddress = [regex]::Matches($String, $IPv4RegexNew) | %{ $_.Groups[1].Value } | Select-Object -First 1
    $url = "http://" + $ipAddress + ":8080"
    start $url
}
