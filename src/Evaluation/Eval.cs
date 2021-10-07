using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Microsoft.Build.Locator;
using Microsoft.Build.Logging;

/// <summary>
/// Needs to be a separate type to ensure none of the MSBuild assemblies load before MSBuildLocator is registered
/// </summary>
class Entrypoint
{
    static void Main(string[] args)
    {
        MSBuildLocator.RegisterDefaults();
        Evaluator.Run(args);
    }
}

class Evaluator
{
    private readonly ProjectCollection evaluationProjectCollection = new ProjectCollection();
    private object projectRootElementCache;
    public ConcurrentDictionary<string, ProjectRootElement> _cache;

    private static readonly Type projectCollectionType = typeof(ProjectCollection);
    private static readonly Type loadedProjectCollectionType = typeof(ProjectCollection).GetNestedType("LoadedProjectCollection", BindingFlags.NonPublic);
    private static readonly Type disposableReaderWriterLockSlimType = typeof(ProjectCollection).GetNestedType("DisposableReaderWriterLockSlim", BindingFlags.NonPublic);

    public static void Run(string[] args)
    {
        if (args.Length == 1 && args[0] is string path && File.Exists(path) && path.EndsWith("proj", System.StringComparison.OrdinalIgnoreCase))
        {
            new Evaluator().Evaluate(path);
        }
        else
        {
            Console.WriteLine("Pass a path to a .csproj file to evaluate");
        }
    }

    public Evaluator()
    {
        var simpleCacheType = typeof(BinaryLogger).Assembly.GetType("Microsoft.Build.Evaluation.SimpleProjectRootElementCache");
        var simpleCache = Activator.CreateInstance(simpleCacheType, nonPublic: true);
        _cache = simpleCache.GetFieldValue<ConcurrentDictionary<string, ProjectRootElement>>("_cache");
        projectRootElementCache = simpleCache;
        SetRootElementCache(evaluationProjectCollection, projectRootElementCache);
    }

    private static void SetRootElementCache(ProjectCollection collection, object cache)
    {
        collection.SetFieldValue("<ProjectRootElementCache>k__BackingField", cache);
    }

    private void CopyField(object source, object destination, string fieldName)
    {
        var value = source.GetFieldValue(fieldName);
        destination.SetFieldValue(fieldName, value);
    }

    private void Evaluate(string path)
    {
        VisitEvaluatedProject(path, project =>
        {
            var compileItems = project.GetItems("Compile");
            foreach (var item in compileItems)
            {
                Console.WriteLine(item.EvaluatedInclude);
            }
        });
    }

    private ProjectCollection GetProjectCollectionForEvaluation(IDictionary<string, string> properties)
    {
        var result = (ProjectCollection)FormatterServices.GetUninitializedObject(projectCollectionType);
        var _loadedProjectCollection = Activator.CreateInstance(loadedProjectCollectionType);
        result.SetFieldValue("_loadedProjects", _loadedProjectCollection);
        result.SetFieldValue("_locker", Activator.CreateInstance(disposableReaderWriterLockSlimType));
        result.SetFieldValue("<ToolsetLocations>k__BackingField", evaluationProjectCollection.ToolsetLocations);
        result.SetFieldValue("_maxNodeCount", 1);
        SetRootElementCache(result, this.projectRootElementCache);
        result.OnlyLogCriticalEvents = true;
        CopyField(evaluationProjectCollection, result, "_loggingService");
        CopyField(evaluationProjectCollection, result, "_globalProperties");
        CopyField(evaluationProjectCollection, result, "_toolsets");
        CopyField(evaluationProjectCollection, result, "_defaultToolsVersion");
        CopyField(evaluationProjectCollection, result, "_toolsetsVersion");

        if (properties != null)
        {
            var globalProperties = result.GetFieldValue("_globalProperties");

            foreach (var property in properties)
            {
                var prop = (ProjectPropertyInstance)Reflector.InvokeStaticMethod(typeof(ProjectPropertyInstance), "Create", property.Key, property.Value);
                globalProperties.InvokeMethod("Set", prop);
            }
        }

        return result;
    }

    public void VisitEvaluatedProject(string projectFilePath, Action<Project> action, IDictionary<string, string> properties = null)
    {
        try
        {
            properties ??= new Dictionary<string, string>();

            var projectCollection = GetProjectCollectionForEvaluation(properties);
            var loaded = projectCollection.GetLoadedProjects(projectFilePath);
            var project = loaded.FirstOrDefault();

            if (project == null)
            {
                project = new Project(
                    projectFilePath,
                    projectCollection.GlobalProperties,
                    toolsVersion: "Current",
                    subToolsetVersion: null,
                    projectCollection,
                    ProjectLoadSettings.IgnoreMissingImports | ProjectLoadSettings.IgnoreEmptyImports | ProjectLoadSettings.IgnoreInvalidImports);
            }

            action(project);

            projectCollection.UnloadProject(project);
        }
        catch
        {
        }
    }
}
