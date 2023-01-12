using System;
using System.Buffers;
using System.Buffers.Text;
using System.Drawing;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GScraper.Google;

internal class GoogleColorConverter : JsonConverter<Color?>
{
    private static ReadOnlySpan<byte> Start => "rgb("u8;

    /// <inheritdoc />
    public override Color? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (!reader.ValueSpan.StartsWith(Start)) return default;

        var rgb = reader.ValueSpan[Start.Length..];

        // R
        int index = rgb.IndexOf((byte)',');
        if (index == -1 || !Utf8Parser.TryParse(rgb[..index], out int r, out _)) return default;

        rgb = rgb[(index + 1)..];

        // G
        index = rgb.IndexOf((byte)',');
        if (index == -1 || !Utf8Parser.TryParse(rgb[..index], out int g, out _)) return default;

        rgb = rgb[(index + 1)..];

        // B
        index = rgb.IndexOf((byte)')');
        if (index == -1 || !Utf8Parser.TryParse(rgb[..index], out int b, out _)) return default;

        return Color.FromArgb(r, g, b);
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, Color? value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }

        Span<byte> hex = stackalloc byte[7];
        hex[0] = (byte)'#';

        if (!Utf8Formatter.TryFormat(value.Value.ToArgb() & 0x00FFFFFF, hex[1..], out _, new StandardFormat('X', 6)))
        {
            throw new FormatException("Unable to format the RGB color as an hex string.");
        }

        writer.WriteStringValue(hex);
    }
}