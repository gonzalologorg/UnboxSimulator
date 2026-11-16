using SQLite4Unity3d;

public class ItemDB
{

    [PrimaryKey, AutoIncrement]
    public int id { get; set; }
    public string name { get; set; }
    public int state { get; set; }
    public int unusual { get; set; }
    public int strange { get; set; }
    public int quality { get; set; }
    public string crate { get; set; }

    public override string ToString()
    {
        return string.Format("[Person: Id={0}, Name={1}", id, name);
    }
}
