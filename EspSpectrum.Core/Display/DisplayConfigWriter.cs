using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace EspSpectrum.Core.Display;

public class DisplayConfigWriter : IDisplayConfigWriter
{
    private readonly ILogger<DisplayConfigWriter> _logger;
    private readonly string _filePath;

    public DisplayConfigWriter(ILogger<DisplayConfigWriter> logger)
    {
        _logger = logger;
        _filePath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
    }

    /// <summary>
    /// Updates specific properties of DisplayConfig in the JSON file
    /// </summary>
    public async Task UpdateConfig(Action<DisplayConfig> updateAction)
    {
        // Read the entire JSON file
        string jsonText = await File.ReadAllTextAsync(_filePath);

        // Deserialize the entire JSON to get both DisplayConfig properties and other properties
        using JsonDocument document = JsonDocument.Parse(jsonText);
        var rootElement = document.RootElement;

        // Convert the root element to a DisplayConfig object
        // Note: This will only pick up the DisplayConfig properties
        var displayConfig = JsonSerializer.Deserialize<DisplayConfig>(jsonText);

        // Apply the updates to DisplayConfig
        updateAction(displayConfig);

        // Create a new JSON object that merges the updated DisplayConfig with other root properties
        using MemoryStream ms = new MemoryStream();
        using Utf8JsonWriter writer = new Utf8JsonWriter(ms, new JsonWriterOptions { Indented = true });

        writer.WriteStartObject();

        // Serialize DisplayConfig properties
        var displayConfigJson = JsonSerializer.Serialize(displayConfig);
        using JsonDocument displayConfigDoc = JsonDocument.Parse(displayConfigJson);

        // First, write all DisplayConfig properties
        foreach (JsonProperty property in displayConfigDoc.RootElement.EnumerateObject())
        {
            property.WriteTo(writer);
        }

        // Then, write all other properties from the original document that aren't DisplayConfig properties
        foreach (JsonProperty property in rootElement.EnumerateObject())
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
        string updatedJson = System.Text.Encoding.UTF8.GetString(ms.ToArray());
        await File.WriteAllTextAsync(_filePath, updatedJson);

        _logger.LogInformation($"Successfully updated config in {_filePath}");

    }

    /// <summary>
    /// Checks if a property name belongs to DisplayConfig
    /// </summary>
    private bool IsDisplayConfigProperty(string propertyName)
    {
        // Check if the property exists in DisplayConfig
        var property = typeof(DisplayConfig).GetProperty(propertyName,
            System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.IgnoreCase);

        return property != null;
    }
}