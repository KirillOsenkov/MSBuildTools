# SlnFilter
Generates a trimmed down .sln from a given one, excluding specified projects.

This is useful when you're working with a large solution and you're only interested in a subset of the projects.
Instead of generating a .slnf file and manually maintaining a copy, just keep the config.txt around with the list of exclusions. It's much easier to maintain and you can regenerate the trimmed solution from the original at any time. Since you can use * wildcards and trim either by project name or path these are more resilient to project moves, renames, etc. 
Another benefit is that if new projects are added to the solution, they will by default appear in the generated solution unless you manually exclude them.

Usage:
    slnfilter.exe <input.sln> <config.txt> <output.sln>
    
config.txt is a list of projects to exclude, with wildcards supported. Every line starts with `!`:

```
!*.IntegrationTests
!ProjectName
!src\Relative\Path\To\Project.csproj
```

Related tools: 
 * [https://microsoft.github.io/slngen](https://microsoft.github.io/slngen)
 * [https://github.com/AndyGerlicher/SlnfGen](https://github.com/AndyGerlicher/SlnfGen)

# MSBuildDumper
Tool that dumps the evaluated values of all properties and items for a project without building it.

## Available on Chocolatey:
    cinst MSBuildDumper

## Usage:
    MSBuildDumper MyProject.csproj > props.txt
    
    
