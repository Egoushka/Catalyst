using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentResults;

namespace Catalyst.Core.Converters;

public class FluentResultConverter : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(Result<>);
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        Type valueType = typeToConvert.GetGenericArguments()[0];
        JsonConverter converter = (JsonConverter)Activator.CreateInstance(
            typeof(FluentResultGenericConverter<>).MakeGenericType(valueType),
            BindingFlags.Instance | BindingFlags.Public,
            binder: null,
            args: null,
            culture: null)!;

        return converter;
    }

    public class FluentResultGenericConverter<TValue> : JsonConverter<Result<TValue>>
    {
        public override Result<TValue>? Read(ref Utf8JsonReader reader, Type typeToConvert,
            JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            bool? isSuccess = null;
            TValue? value = default;
            List<Error>? errors = null; // Use FluentResults.Error

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    break;
                }

                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    string? propertyName = reader.GetString();
                    reader.Read();

                    switch (propertyName)
                    {
                        case "IsSuccess":
                            isSuccess = reader.GetBoolean();
                            break;
                        case "Value":
                            if (isSuccess ?? false)
                            {
                                value = JsonSerializer.Deserialize<TValue>(ref reader, options);
                            }
                            else
                            {
                                reader.Skip();
                            }

                            break;
                        case "Errors":
                            if (!(isSuccess ?? true))
                            {
                                // Deserialize directly into List<FluentResults.Error>
                                errors = JsonSerializer.Deserialize<List<Error>>(ref reader, options);
                            }
                            else
                            {
                                reader.Skip();
                            }

                            break;
                        default:
                            reader.Skip();
                            break;
                    }
                }
            }

            if (isSuccess == null)
            {
                throw new JsonException("The 'IsSuccess' property is required.");
            }


            return isSuccess.Value
                ? Result.Ok(value!)
                : Result.Fail(errors ?? new List<Error>()); // Use FluentResults.Error
        }

        public override void Write(Utf8JsonWriter writer, Result<TValue> value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteBoolean("IsSuccess", value.IsSuccess);

            if (value.IsFailed)
            {
                writer.WritePropertyName("Errors");
                // No need for anonymous type; serialize value.Errors directly.
                JsonSerializer.Serialize(writer, value.Errors, options);
            }
            else
            {
                writer.WritePropertyName("Value");
                JsonSerializer.Serialize(writer, value.Value, options);
            }

            writer.WriteEndObject();
        }
    }
}