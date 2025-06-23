using PCL.Neo.Core.Models.Minecraft.Game.Data.GaJvArguments;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PCL.Neo.Core.Models.Minecraft.Game.Data;

/// <summary>
/// Converts a JSON string or array of strings to a List.
/// </summary>
public class StringOrStringListConverter : JsonConverter<List<string>>
{
    /// <inheritdoc/>
    public override List<string> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.String => [reader.GetString()!],
            JsonTokenType.StartArray => JsonSerializer.Deserialize<List<string>>(ref reader, options) ??
                                        new List<string>(),
            _ => throw new JsonException("Expected JSON string or array of strings.")
        };
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, List<string> value, JsonSerializerOptions options)
    {
        // For simplicity, always write as an array.
        // Minecraft often writes single-element lists as a plain string, but reading both is key.
        JsonSerializer.Serialize(writer, value, options);
    }
}

/// <summary>
/// Converts a JSON element to either StringArgument or RuledArgument.
/// </summary>
public class ArgumentElementConverter : JsonConverter<ArgumentElement>
{
    /// <inheritdoc/>
    public override ArgumentElement Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.String => new StringArgument(reader.GetString()!),
            JsonTokenType.StartObject => JsonSerializer.Deserialize<RuledArgument>(ref reader, options) ??
                                         throw new JsonException("Failed to deserialize RuledArgument."),
            _ => throw new JsonException("Argument element must be a JSON string or object.")
        };
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, ArgumentElement value, JsonSerializerOptions options)
    {
        switch (value)
        {
            case StringArgument strArg:
                writer.WriteStringValue(strArg.Value);
                break;
            case RuledArgument ruledArg:
                // Serialize the concrete type.
                JsonSerializer.Serialize(writer, ruledArg, ruledArg.GetType(), options);
                break;
            default:
                throw new JsonException($"Unsupported ArgumentElement type for serialization: {value?.GetType()}");
        }
    }
}

/// <summary>
/// Converts a JSON array to a List, using ArgumentElementConverter for each item.
/// </summary>
public class ListOfArgumentElementConverter : JsonConverter<List<ArgumentElement>>
{
    /// <inheritdoc/>
    public override List<ArgumentElement> Read(ref Utf8JsonReader reader, Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException("Expected start of JSON array for argument list.");
        }

        var list = new List<ArgumentElement>();
        // Get an instance of the ArgumentElementConverter.
        // It's safer to ask options for it in case it's registered with specific settings,
        // or fall back to a new instance.
        var elementConverter = (ArgumentElementConverter?)options.GetConverter(typeof(ArgumentElement))
                               ?? new ArgumentElementConverter();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
            {
                return list;
            }

            list.Add(elementConverter.Read(ref reader, typeof(ArgumentElement), options));
        }

        throw new JsonException("Unexpected end of JSON while reading argument list.");
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, List<ArgumentElement> value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        var elementConverter = (ArgumentElementConverter?)options.GetConverter(typeof(ArgumentElement))
                               ?? new ArgumentElementConverter();
        foreach (var item in value)
        {
            elementConverter.Write(writer, item, options);
        }

        writer.WriteEndArray();
    }
}