using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace EspSpectrum.Core.Display;

public class DisplayConfigWriter(ILogger<DisplayConfigWriter> logger,
    string? appSetting = null) : IDisplayConfigManager
{
    private readonly ILogger<DisplayConfigWriter> _logger = logger;
    private readonly string _filePath = appSetting ?? Path.Combine(AppContext.BaseDirectory, "appsettings.json");

    /// <summary>
    /// Reads DisplayConfig from the JSON file
    /// </summary>
    public async Task<DisplayConfig> ReadConfig()
    {
        if (!File.Exists(_filePath))
        {
            throw new FileNotFoundException($"Can't find appsettings.json", _filePath);
        }

        var jsonText = await File.ReadAllTextAsync(_filePath);

        // Deserialize entire document
        using var document = JsonDocument.Parse(jsonText);
        var root = document.RootElement;

        // Deserialize only the DisplayConfig-relevant properties
        var displayConfigJson = new Dictionary<string, JsonElement>();
        foreach (var property in root.EnumerateObject().Where(property => IsDisplayConfigProperty(property.Name)))
        {
            displayConfigJson[property.Name] = property.Value;
        }

        var reducedJson = JsonSerializer.Serialize(displayConfigJson);
        var displayConfig = JsonSerializer.Deserialize<DisplayConfig>(reducedJson);

        return displayConfig ?? new DisplayConfig();
    }

    /// <summary>
    /// Updates specific properties of DisplayConfig in the JSON file
    /// </summary>
    public async Task UpdateConfig(Action<DisplayConfig> updateAction)
    {
        // Read the entire JSON file
        var jsonText = await File.ReadAllTextAsync(_filePath);

        // Deserialize the entire JSON to get both DisplayConfig properties and other properties
        using var document = JsonDocument.Parse(jsonText);
        var rootElement = document.RootElement;

        // Convert the root element to a DisplayConfig object
        // Note: This will only pick up the DisplayConfig properties
        var displayConfig = JsonSerializer.Deserialize<DisplayConfig>(jsonText)!;

        // Apply the updates to DisplayConfig
        updateAction(displayConfig);

        // Create a new JSON object that merges the updated DisplayConfig with other root properties
        using var ms = new MemoryStream();
        using var writer = new Utf8JsonWriter(ms, new JsonWriterOptions { Indented = false });

        writer.WriteStartObject();

        // Serialize DisplayConfig properties
        var displayConfigJson = JsonSerializer.Serialize(displayConfig);
        using var displayConfigDoc = JsonDocument.Parse(displayConfigJson);

        // First, write all DisplayConfig properties
        foreach (var property in displayConfigDoc.RootElement.EnumerateObject())
        {
            property.WriteTo(writer);
        }

        // Then, write all other properties from the original document that aren't DisplayConfig properties
        foreach (var property in rootElement.EnumerateObject())
        {
            // Skip properties that are part of DisplayConfig (they've already been written with updated values)
            if (IsDisplayConfigProperty(property.Name))
                continue;

            // Write non-DisplayConfig properties as-is
            property.WriteTo(writer);
        }

        writer.WriteEndObject();
        await writer.FlushAsync();

        // Convert the memory stream to string and write to file
        ms.Seek(0, SeekOrigin.Begin);
        var updatedJson = System.Text.Encoding.UTF8.GetString(ms.ToArray());
        await File.WriteAllTextAsync(_filePath, updatedJson);
    }

    /// <summary>
    /// Checks if a property name belongs to DisplayConfig
    /// </summary>
    private static bool IsDisplayConfigProperty(string propertyName)
    {
        // Check if the property exists in DisplayConfig
        var property = typeof(DisplayConfig).GetProperty(propertyName,
            System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.IgnoreCase);

        return property != null;
    }
}