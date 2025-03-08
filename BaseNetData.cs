using System.Text;

public abstract class BaseNetData
{
    /// <summary>
    /// 用于子类重写的 获取字节数组容器大小的方法
    /// </summary>
    /// <returns></returns>
    public abstract int GetBytesNum();

    /// <summary>
    /// 把成员变量 序列化为 对应的字节数组
    /// </summary>
    /// <returns></returns>
    public abstract byte[] Writing();

    /// <summary>
    /// 把2进制字节数组 反序列化到 成员变量当中 返回读取了多少个字节
    /// </summary>
    /// <param name="bytes">反序列化使用的字节数组</param>
    /// <param name="beginIndex">从该字节数组的第几个位置开始解析 默认是0</param>
    public abstract int Reading(byte[] bytes, int beginIndex = 0);

    #region 存储指定类型到字节数组中 序列化
    protected void WriteInt(byte[] bytes, int value, ref int index)
    {
        BitConverter.GetBytes(value).CopyTo(bytes, index);
        index += sizeof(int);
    }
    protected void WriteShort(byte[] bytes, short value, ref int index)
    {
        BitConverter.GetBytes(value).CopyTo(bytes, index);
        index += sizeof(short);
    }
    protected void WriteLong(byte[] bytes, long value, ref int index)
    {
        BitConverter.GetBytes(value).CopyTo(bytes, index);
        index += sizeof(long);
    }
    protected void WriteFloat(byte[] bytes, float value, ref int index)
    {
        BitConverter.GetBytes(value).CopyTo(bytes, index);
        index += sizeof(float);
    }
    protected void WriteByte(byte[] bytes, byte value, ref int index)
    {
        bytes[index] = value;
        index += sizeof(byte);
    }
    protected void WriteBool(byte[] bytes, bool value, ref int index)
    {
        BitConverter.GetBytes(value).CopyTo(bytes, index);
        index += sizeof(bool);
    }
    protected void WriteString(byte[] bytes, string value, ref int index)
    {
        byte[] strBytes = Encoding.UTF8.GetBytes(value);
        //先存储string字节数组的长度
        WriteInt(bytes, strBytes.Length, ref index);
        //再存 string字节数组
        strBytes.CopyTo(bytes, index);
        index += strBytes.Length;
    }
    protected void WriteData(byte[] bytes, BaseNetData data, ref int index)
    {
        data.Writing().CopyTo(bytes, index);
        index += data.GetBytesNum();
    }
    #endregion

    #region 根据类型 将字节数组转换为数据类型 反序列化
    protected int ReadInt(byte[] bytes, ref int index)
    {
        int value = BitConverter.ToInt32(bytes, index);
        index += sizeof(int);
        return value;
    }
    protected short ReadShort(byte[] bytes, ref int index)
    {
        short value = BitConverter.ToInt16(bytes, index);
        index += sizeof(short);
        return value;
    }
    protected long ReadLong(byte[] bytes, ref int index)
    {
        long value = BitConverter.ToInt64(bytes, index);
        index += sizeof(long);
        return value;
    }
    protected float ReadFloat(byte[] bytes, ref int index)
    {
        float value = BitConverter.ToSingle(bytes, index);
        index += sizeof(float);
        return value;
    }
    protected byte ReadByte(byte[] bytes, ref int index)
    {
        byte value = bytes[index];
        index += sizeof(byte);
        return value;
    }
    protected bool ReadBool(byte[] bytes, ref int index)
    {
        bool value = BitConverter.ToBoolean(bytes, index);
        index += sizeof(bool);
        return value;
    }
    protected string ReadString(byte[] bytes, ref int index)
    {
        //首先读取长度
        int length = ReadInt(bytes, ref index);
        //再读取string
        string value = Encoding.UTF8.GetString(bytes, index, length);
        index += length;
        return value;
    }
    protected T ReadData<T>(byte[] bytes, ref int index) where T : BaseNetData, new()
    {
        T value = new T();
        index += value.Reading(bytes, index);
        return value;
    }
    #endregion
}
