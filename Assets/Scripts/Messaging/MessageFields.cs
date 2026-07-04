using System;
using System.Buffers.Binary;
using System.Text;
using UnityEngine;

public static class MessageFields
{
    // int32 - encoded big endian
    public static int LengthInt32(int value = 0)
    {
        return sizeof(int);
    }
    public static bool WriteInt32(ref Span<byte> data, int value)
    {
        if(data.Length < LengthInt32())
        {
            return false;
        }
        BinaryPrimitives.WriteInt32BigEndian(data, value);
        data = data[LengthInt32()..];
        return true;
    }
    public static int? ReadInt32(ref ReadOnlySpan<byte> data)
    {
        if(data.Length < LengthInt32())
        {
            return null;
        }
        int value = BinaryPrimitives.ReadInt32BigEndian(data);
        data = data[LengthInt32()..];
        return value;
    }

    // uint32 - encoded big endian
    public static int LengthUInt32(uint value = 0)
    {
        return sizeof(uint);
    }
    public static bool WriteUInt32(ref Span<byte> data, uint value)
    {
        if(data.Length < LengthUInt32())
        {
            return false;
        }
        BinaryPrimitives.WriteUInt32BigEndian(data, value);
        data = data[LengthUInt32()..];
        return true;
    }
    public static uint? ReadUInt32(ref ReadOnlySpan<byte> data)
    {
        if(data.Length < LengthUInt32())
        {
            return null;
        }
        uint value = BinaryPrimitives.ReadUInt32BigEndian(data);
        data = data[LengthUInt32()..];
        return value;
    }

    // uint16 - encoded big endian
    public static int LengthUInt16(ushort value = 0)
    {
        return sizeof(ushort);
    }
    public static bool WriteUInt16(ref Span<byte> data, ushort value)
    {
        if(data.Length < LengthUInt16())
        {
            return false;
        }
        BinaryPrimitives.WriteUInt16BigEndian(data, value);
        data = data[LengthUInt16()..];
        return true;
    }
    public static ushort? ReadUInt16(ref ReadOnlySpan<byte> data)
    {
        if(data.Length < LengthUInt16())
        {
            return null;
        }
        ushort value = BinaryPrimitives.ReadUInt16BigEndian(data);
        data = data[LengthUInt16()..];
        return value;
    }

    // float - encoded big endian
    public static int LengthFloat(float value = 0)
    {
        return sizeof(float);
    }
    public static bool WriteFloat(ref Span<byte> data, float value)
    {
        int equivalentInt = BitConverter.SingleToInt32Bits(value);
        return WriteInt32(ref data, equivalentInt);
    }
    public static float? ReadFloat(ref ReadOnlySpan<byte> data)
    {
        int? equivalentInt = ReadInt32(ref data);
        return equivalentInt is not null ? BitConverter.Int32BitsToSingle(equivalentInt.Value) : null;
    }

    // string - encoded with big endian length (int16) and UTF-8 encoding
    public static int LengthString(string value)
    {
        return sizeof(ushort) + Encoding.UTF8.GetByteCount(value);
    }
    public static bool WriteString(ref Span<byte> data, string value)
    {
        int length = Encoding.UTF8.GetByteCount(value);
        if(length > ushort.MaxValue || data.Length < (length + sizeof(ushort)))
        {
            return false;
        }
        BinaryPrimitives.WriteUInt16BigEndian(data, (ushort)length);
        data = data[sizeof(ushort)..];
        Encoding.UTF8.GetBytes(value, data);
        data = data[length..];
        return true;
    }
    public static string? ReadString(ref ReadOnlySpan<byte> data)
    {
        if(data.Length < sizeof(ushort))
        {
            return null;
        }
        ushort length = BinaryPrimitives.ReadUInt16BigEndian(data);
        data = data[sizeof(ushort)..];
        if(data.Length < length)
        {
            return null;
        }
        string value = Encoding.UTF8.GetString(data[..length]);
        data = data[length..];
        return value;
    }

    // bool - encoded by a single byte
    public static int LengthBool(bool value = false)
    {
        return 1;
    }
    public static bool WriteBool(ref Span<byte> data, bool value)
    {
        if(data.Length < 1)
        {
            return false;
        }
        data[0] = value ? (byte)1 : (byte)0;
        data = data[1..];
        return true;
    }
    public static bool? ReadBool(ref ReadOnlySpan<byte> data)
    {
        if(data.Length < 1)
        {
            return null;
        }
        bool value = data[0] == 0 ? false : true;
        data = data[1..];
        return value;
    }

    // guid - encoded as byte array
    public static int LengthGuid()
    {
        return 16;
    }
    public static bool WriteGuid(ref Span<byte> data, Guid value)
    {
        if(data.Length < 16)
        {
            return false;
        }
        value.TryWriteBytes(data);
        data = data[16..];
        return true;
    }
    public static Guid? ReadGuid(ref ReadOnlySpan<byte> data)
    {
        if(data.Length < 16)
        {
            return null;
        }
        Guid value = new Guid(data[..16]);
        data = data[16..];
        return value;
    }

    // byte array - encoded with big endian length (int16)
    public static int LengthByteArray(ReadOnlySpan<byte> array)
    {
        return sizeof(ushort) + array.Length;
    }
    public static bool WriteByteArray(ref Span<byte> data, ReadOnlySpan<byte> value)
    {
        if(value.Length > ushort.MaxValue)
        {
            return false;
        }
        ushort length = (ushort)value.Length;
        if(!WriteUInt16(ref data, length)) return false;
        if(data.Length < value.Length)
        {
            return false;
        }
        value.CopyTo(data);
        data = data[value.Length..];
        return true;
    }
    public static byte[] ReadByteArray(ref ReadOnlySpan<byte> data)
    {
        ushort? length = ReadUInt16(ref data);
        if(length is null) return null;
        if(data.Length < length.Value) return null;
        byte[] value = data[..length.Value].ToArray();
        data = data[length.Value..];
        return value;
    }
}
