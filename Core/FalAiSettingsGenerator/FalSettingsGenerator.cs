using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using Microsoft.CodeAnalysis;

namespace FalAiSettingsGenerator;

[Generator]
public class FalSettingsGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        //not needed
    }

    public void Execute(GeneratorExecutionContext context)
    {
        var jsonFile = context.AdditionalFiles.First(path => path.Path.EndsWith("falConfig.json"));
        var jsonText = jsonFile.GetText()!.ToString();
        if(string.IsNullOrWhiteSpace(jsonText)) return;
        var jsonDocument = JsonDocument.Parse(jsonText);

        string className = "FalClientSettings";
        StringBuilder sourceBuilder = new StringBuilder();

        sourceBuilder.Append($@"namespace SnailAI 
{{
    public class {className} 
    {{
");

        foreach (var property in jsonDocument.RootElement.EnumerateObject())
        {
            string propertyName = property.Name;
            string propertyValue = property.Value.ToString();

            sourceBuilder.Append($@"public const string {propertyName} = ""{propertyValue}"";");
        }

        sourceBuilder.Append("\n }\n}");

        context.AddSource("FalClientSettings.g.cs", sourceBuilder.ToString());
    }
}