using System.Text;

public class PlayerData : BaseNetData
{
    public int atk;
    public int lev;
    public string name;
    public override int GetBytesNum()
    {
        return 4 + 4 + 4 + Encoding.UTF8.GetBytes(name).Length;
    }

    public override int Reading(byte[] bytes, int beginIndex = 0)
    {
        int index = beginIndex;
        atk = ReadInt(bytes, ref index);
        lev = ReadInt(bytes, ref index);
        name = ReadString(bytes, ref index);
        return index - beginIndex;
    }
    //���л��ͷ����л���˳��Ҫһ��
    public override byte[] Writing()
    {
        byte[] bytes = new byte[GetBytesNum()];
        int index = 0;
        WriteInt(bytes, atk, ref index);
        WriteInt(bytes, lev, ref index);
        WriteString(bytes, name, ref index);
        return bytes;
    }
}
