using RaizBarApp.Models;
using System.Text.Json.Serialization;

[JsonSerializable(typeof(Project))]
[JsonSerializable(typeof(ProjectTask))]
[JsonSerializable(typeof(ProjectsJson))]
[JsonSerializable(typeof(Category))]
[JsonSerializable(typeof(Tag))]
[JsonSerializable(typeof(List<Bebida>))]
[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true)]
public partial class JsonContext : JsonSerializerContext
{
}