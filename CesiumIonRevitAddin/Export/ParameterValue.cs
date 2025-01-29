using Newtonsoft.Json;
using System;

[JsonConverter(typeof(ParameterValueConverter))]
public struct ParameterValue
{
    public int? IntegerValue { get; set; }
    public double? DoubleValue { get; set; }
    public string StringValue { get; set; }
    public long? LongValue { get; set; }

    public static implicit operator ParameterValue(int value) => new ParameterValue { IntegerValue = value };
    public static implicit operator ParameterValue(double value) => new ParameterValue { DoubleValue = value };
    public static implicit operator ParameterValue(long value) => new ParameterValue { LongValue = value };
    public static implicit operator ParameterValue(string value) => new ParameterValue { StringValue = value };
}

public class ParameterValueConverter : JsonConverter<ParameterValue>
{
    public override void WriteJson(JsonWriter writer, ParameterValue value, JsonSerializer serializer)
    {
        if (value.IntegerValue.HasValue)
        {
            writer.WriteValue(value.IntegerValue.Value);
        }
        else if (value.DoubleValue.HasValue)
        {
            writer.WriteValue(value.DoubleValue.Value);
        }
        else if (value.LongValue.HasValue)
        {
            writer.WriteValue(value.LongValue.Value);
        }
        else if (!string.IsNullOrEmpty(value.StringValue))
        {
            writer.WriteValue(value.StringValue);
        }
        else
        {
            writer.WriteNull();
        }
    }

    public override ParameterValue ReadJson(JsonReader reader, Type objectType, ParameterValue existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        throw new NotImplementedException("Deserialization is not supported for ParameterValue.");
    }
}
