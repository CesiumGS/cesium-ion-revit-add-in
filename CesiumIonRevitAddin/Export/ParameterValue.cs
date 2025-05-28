using Newtonsoft.Json;
using System;
using Autodesk.Revit.DB;

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
#if REVIT2022 || REVIT2023
    public static implicit operator ParameterValue(Autodesk.Revit.DB.ElementId value) => new ParameterValue { LongValue = (System.Int64) value.IntegerValue };
#else
    public static implicit operator ParameterValue(Autodesk.Revit.DB.ElementId value) => new ParameterValue { LongValue = value.Value };
#endif

    public bool Equals(ParameterValue other)
    {
        return Nullable.Equals(IntegerValue, other.IntegerValue)
            && Nullable.Equals(DoubleValue, other.DoubleValue)
            && string.Equals(StringValue, other.StringValue)
            && Nullable.Equals(LongValue, other.LongValue);
    }

    public override bool Equals(object obj)
    {
        return obj is ParameterValue other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 23 + IntegerValue.GetHashCode();
            hash = hash * 23 + DoubleValue.GetHashCode();
            hash = hash * 23 + (StringValue != null ? StringValue.GetHashCode() : 0);
            hash = hash * 23 + LongValue.GetHashCode();
            return hash;
        }
    }

    public static bool operator ==(ParameterValue left, ParameterValue right) => left.Equals(right);
    public static bool operator !=(ParameterValue left, ParameterValue right) => !left.Equals(right);
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
