﻿#nullable enable

namespace AzureIoTHub.Portal.Server.Helpers
{
    using System;
    using System.Buffers.Binary;

    readonly partial record struct DevEui : IFormattable
    {
        public const int Size = sizeof(ulong);

        readonly ulong value;

        public DevEui(ulong value) => this.value = value;

        public ulong AsUInt64 => this.value;

        public override string ToString() => ToString(null, null);

        public static DevEui Read(ReadOnlySpan<byte> buffer) =>
            new(BinaryPrimitives.ReadUInt64LittleEndian(buffer));

        public static DevEui Read(ref ReadOnlySpan<byte> buffer)
        {
            var result = Read(buffer);
            buffer = buffer[Size..];
            return result;
        }

        public Span<byte> Write(Span<byte> buffer)
        {
            BinaryPrimitives.WriteUInt64LittleEndian(buffer, this.value);
            return buffer[Size..];
        }

        public static DevEui Parse(ReadOnlySpan<char> input) =>
            TryParse(input, out var result) ? result : throw new FormatException();

        public static bool TryParse(ReadOnlySpan<char> input, out DevEui result)
        {
            if (Eui.TryParse(input, out var raw))
            {
                result = new DevEui(raw);
                return true;
            }
            else
            {
                result = default;
                return false;
            }
        }

        public string ToString(string? format, IFormatProvider? formatProvider) => Eui.Format(this.value, format);

        public string ToHex() => ToHex(null);
        public string ToHex(LetterCase letterCase) => ToHex(null, letterCase);
        public string ToHex(char? separator) => ToHex(separator, LetterCase.Upper);
        public string ToHex(char? separator, LetterCase letterCase) => Eui.ToHex(this.value, separator, letterCase);
    }

    readonly partial record struct JoinEui : IFormattable
    {
        public const int Size = sizeof(ulong);

        readonly ulong value;

        public JoinEui(ulong value) => this.value = value;

        public ulong AsUInt64 => this.value;

        public override string ToString() => ToString(null, null);

        public static JoinEui Read(ReadOnlySpan<byte> buffer) =>
            new(BinaryPrimitives.ReadUInt64LittleEndian(buffer));

        public static JoinEui Read(ref ReadOnlySpan<byte> buffer)
        {
            var result = Read(buffer);
            buffer = buffer[Size..];
            return result;
        }

        public Span<byte> Write(Span<byte> buffer)
        {
            BinaryPrimitives.WriteUInt64LittleEndian(buffer, this.value);
            return buffer[Size..];
        }

        public static JoinEui Parse(ReadOnlySpan<char> input) =>
            TryParse(input, out var result) ? result : throw new FormatException();

        public static bool TryParse(ReadOnlySpan<char> input, out JoinEui result)
        {
            if (Eui.TryParse(input, out var raw))
            {
                result = new JoinEui(raw);
                return true;
            }
            else
            {
                result = default;
                return false;
            }
        }

        public string ToString(string? format, IFormatProvider? formatProvider) => Eui.Format(this.value, format);

        public string ToHex() => ToHex(null);
        public string ToHex(LetterCase letterCase) => ToHex(null, letterCase);
        public string ToHex(char? separator) => ToHex(separator, LetterCase.Upper);
        public string ToHex(char? separator, LetterCase letterCase) => Eui.ToHex(this.value, separator, letterCase);
    }

    readonly partial record struct StationEui : IFormattable
    {
        public const int Size = sizeof(ulong);

        readonly ulong value;

        public StationEui(ulong value) => this.value = value;

        public ulong AsUInt64 => this.value;

        public override string ToString() => ToString(null, null);

        public static StationEui Read(ReadOnlySpan<byte> buffer) =>
            new(BinaryPrimitives.ReadUInt64LittleEndian(buffer));

        public static StationEui Read(ref ReadOnlySpan<byte> buffer)
        {
            var result = Read(buffer);
            buffer = buffer[Size..];
            return result;
        }

        public Span<byte> Write(Span<byte> buffer)
        {
            BinaryPrimitives.WriteUInt64LittleEndian(buffer, this.value);
            return buffer[Size..];
        }

        public static StationEui Parse(ReadOnlySpan<char> input) =>
            TryParse(input, out var result) ? result : throw new FormatException();

        public static bool TryParse(ReadOnlySpan<char> input, out StationEui result)
        {
            if (Eui.TryParse(input, out var raw))
            {
                result = new StationEui(raw);
                return true;
            }
            else
            {
                result = default;
                return false;
            }
        }

        public string ToString(string? format, IFormatProvider? formatProvider) => Eui.Format(this.value, format);

        public string ToHex() => ToHex(null);
        public string ToHex(LetterCase letterCase) => ToHex(null, letterCase);
        public string ToHex(char? separator) => ToHex(separator, LetterCase.Upper);
        public string ToHex(char? separator, LetterCase letterCase) => Eui.ToHex(this.value, separator, letterCase);
    }
}
