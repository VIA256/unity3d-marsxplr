using System;

public class SemVer
{
    public byte maj;
    public byte min;
    public byte patch;

    public SemVer(byte mj, byte mn, byte pt)
    {
        maj = mj;
        min = mn;
        patch = pt;
    }

    public SemVer(byte mj, byte mn)
    {
        maj = mj;
        min = mn;
        patch = 0;
    }

    public SemVer(byte mj)
    {
        maj = mj;
        min = 0;
        patch = 0;
    }

    public SemVer(String str)
    {
        SemVer sv = SemVer.Parse(str);
        maj = sv.maj;
        min = sv.min;
        patch = sv.patch;
    }

    public static bool operator ==(SemVer a, SemVer b)
    {
		return a.GetSum() == b.GetSum();
    }

    public static bool operator !=(SemVer a, SemVer b)
    {
		return a.GetSum() != b.GetSum();
    }

    public static bool operator >(SemVer a, SemVer b)
    {
		return a.GetSum() > b.GetSum();
    }

    public static bool operator >=(SemVer a, SemVer b)
    {
		return a.GetSum() >= b.GetSum();
    }

    public static bool operator <=(SemVer a, SemVer b)
    {
		return a.GetSum() <= b.GetSum();
    }

    public static bool operator <(SemVer a, SemVer b)
    {
		return a.GetSum() < b.GetSum();
    }

    public override string ToString()
    {
        if (patch == 0) return maj.ToString() + "." + min.ToString();
        return maj.ToString() + "." + min.ToString() + "." + patch.ToString();
    }

    public static SemVer Parse(String str)
    {
        String[] sv = str.Split("."[0]);
        if (sv.Length == 3) return new SemVer(
            byte.Parse(sv[0]),
            byte.Parse(sv[1]),
            byte.Parse(sv[2]));
        if (sv.Length == 2) return new SemVer(
            byte.Parse(sv[0]),
            byte.Parse(sv[1]));
        if (sv.Length == 1) return new SemVer(
            byte.Parse(sv[0]));
        throw new FormatException();
    }

	public UInt32 GetSum()
	{
		return
			((UInt32)maj*256*256) +
			((UInt32)min*256) +
			((UInt32)patch);
	}

    public override int GetHashCode()
    {
        return maj.GetHashCode() ^ (min.GetHashCode() << 2) ^ (patch.GetHashCode() >> 2);
    }

    public override bool Equals(object other)
    {
        if (!(other is SeaData)) return false;
        SemVer osv = (SemVer)other;
        return (
            osv.maj == maj &&
            osv.min == min &&
            osv.patch == patch);
    }
}