using System;
using System.Buffers;
using System.Buffers.Text;
using System.Drawing;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GScraper.Brave;

internal class BraveColorConverter : JsonConverter<Color>
{
    /// <inheritdoc />
    public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (!Utf8Parser.TryParse(reader.ValueSpan.Slice(1), out int rgb, out _, 'X'))
        {
            throw new FormatException("Unable to parse hex value.");
        }
        
        return Color.FromArgb(rgb);
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options)
    {
        Span<byte> hex = stackalloc byte[7];
        hex[0] = (byte)'#';

        if (!Utf8Formatter.TryFormat(value.ToArgb() & 0x00FFFFFF, hex.Slice(1), out _, new StandardFormat('X', 6)))
        {
            throw new FormatException("Unable to format the RGB color as an hex string.");
        }

        writer.WriteStringValue(hex);
    }
}