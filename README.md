# SlnFilter
Generates a trimmed down .sln from a given one, excluding specified projects.

Usage:
    slnfilter.exe <input.sln> <config.txt> <output.sln>
    
config.txt is a list of projects to exclude, with wildcards supported. Every line starts with `!`:

```
!*.IntegrationTests
!ProjectName
!src\Relative\Path\To\Project.csproj
```

# MSBuildDumper
Tool that dumps the evaluated values of all properties and items for a project without building it.

## Available on Chocolatey:
    cinst MSBuildDumper

## Usage:
    MSBuildDumper MyProject.csproj > props.txt
    
    
