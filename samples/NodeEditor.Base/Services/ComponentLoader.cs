using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using NodeEditorDemo.Models;

namespace NodeEditorDemo.Services;

public static class ComponentLoader
{
    private static readonly Dictionary<string, ComponentDefinition> _cache = new(StringComparer.OrdinalIgnoreCase);
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    public static string ComponentsDirectory
    {
        get
        {
            var baseDir = AppContext.BaseDirectory;
            var localPath = Path.Combine(baseDir, "Data", "Components");
            if (Directory.Exists(localPath)) return localPath;

            var current = new DirectoryInfo(baseDir);
            while (current != null)
            {
                var target = Path.Combine(current.FullName, "samples", "NodeEditor.Base", "Data", "Components");
                if (Directory.Exists(target)) return target;
                current = current.Parent;
            }

            return localPath;
        }
    }

    public static ComponentDefinition? LoadFromFile(string filePath)
    {
        if (!File.Exists(filePath)) return null;
        var json = File.ReadAllText(filePath);
        return Deserialize(json);
    }

    public static ComponentDefinition? Deserialize(string json)
    {
        return JsonSerializer.Deserialize<ComponentDefinition>(json, _jsonOptions);
    }

    private static bool _allLoaded;

    public static void EnsureLoaded()
    {
        if (_allLoaded) return;
        _allLoaded = true;
        var dir = ComponentsDirectory;
        if (Directory.Exists(dir))
        {
            var files = Directory.GetFiles(dir, "*.json");
            foreach (var file in files)
            {
                var def = LoadFromFile(file);
                if (def != null)
                {
                    _cache[def.Name] = def;
                    var baseName = Path.GetFileNameWithoutExtension(file);
                    _cache[baseName] = def;
                    _cache[baseName.Replace("_", " ")] = def;
                }
            }
        }
    }

    public static IEnumerable<ComponentDefinition> LoadAllComponents()
    {
        EnsureLoaded();
        return _cache.Values.Distinct();
    }

    public static ComponentDefinition? GetComponent(string name)
    {
        if (_cache.TryGetValue(name, out var cached)) return cached;

        EnsureLoaded();

        if (_cache.TryGetValue(name, out cached)) return cached;

        // Try normalized filename matching: "ceramic_capacitor.json", "ceramic capacitor.json"
        var normalized = name.ToLowerInvariant().Replace(" ", "_");
        var file = Path.Combine(ComponentsDirectory, $"{normalized}.json");
        var loaded = LoadFromFile(file);
        if (loaded != null)
        {
            _cache[name] = loaded;
            _cache[loaded.Name] = loaded;
            return loaded;
        }

        return null;
    }

    public static void Register(ComponentDefinition def)
    {
        if (!string.IsNullOrEmpty(def.Name))
        {
            _cache[def.Name] = def;
        }
    }
}
