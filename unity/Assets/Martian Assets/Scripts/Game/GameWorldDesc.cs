using System;

[Serializable]
public class GameWorldDesc
{
	public string name;
	public string url;
	public bool featured;

	private bool initialized;

	public GameWorldDesc()
    {
		if (!initialized)
		{
			name = "";
			url = "";
			initialized = true;
		}
	}

	public GameWorldDesc(string n, string u, bool feat)
    {
		if (!initialized)
		{
			name = "";
			url = "";
			initialized = true;
		}
		name = n;
		url = u;
		featured = feat;
	}
}
