using SQLite4Unity3d;
using UnityEngine;
using System;
#if !UNITY_EDITOR
using System.Collections;
using System.IO;
#endif
using System.Collections.Generic;

public class DataService
{

    private SQLiteConnection _connection;

    public DataService(string DatabaseName)
    {
        bool wrote = false;
#if UNITY_EDITOR
        var dbPath = string.Format(@"Assets/StreamingAssets/{0}", DatabaseName);
        
#elif UNITY_STANDALONE_WIN
        var dbPath = string.Format(@Application.dataPath+"/StreamingAssets/{0}", DatabaseName);
#else
        // check if file exists in Application.persistentDataPath
        var filepath = string.Format("{0}/{1}", Application.persistentDataPath, DatabaseName);

        if (!File.Exists(filepath))
        {
            Debug.Log("Database not in Persistent path");
            // if it doesn't ->
            // open StreamingAssets directory and load the db ->

#if UNITY_ANDROID
            var loadDb = new WWW("jar:file://" + Application.dataPath + "!/assets/" + DatabaseName);  // this is the path to your StreamingAssets in android
            while (!loadDb.isDone) { }  // CAREFUL here, for safety reasons you shouldn't let this while loop unattended, place a timer and error check
            // then save to Application.persistentDataPath
            File.WriteAllBytes(filepath, loadDb.bytes);
#elif UNITY_IOS
                 var loadDb = Application.dataPath + "/Raw/" + DatabaseName;  // this is the path to your StreamingAssets in iOS
                // then save to Application.persistentDataPath
                File.Copy(loadDb, filepath);
#elif UNITY_WP8
                var loadDb = Application.dataPath + "/StreamingAssets/" + DatabaseName;  // this is the path to your StreamingAssets in iOS
                // then save to Application.persistentDataPath
                File.Copy(loadDb, filepath);

#elif UNITY_WINRT
			var loadDb = Application.dataPath + "/StreamingAssets/" + DatabaseName;  // this is the path to your StreamingAssets in iOS
			// then save to Application.persistentDataPath
			File.Copy(loadDb, filepath);
#endif

            Debug.Log("Database written");
            wrote = true;
        }

        var dbPath = filepath;
#endif
        _connection = new SQLiteConnection(dbPath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create);
        if (wrote)
        {
            _connection.Execute("CREATE TABLE IF NOT EXISTS ItemDB ( `id` INTEGER, `name` TEXT, `state` INTEGER, `unusual` INTEGER, `strange` INTEGER DEFAULT 0, `quality` INTEGER, `crate` TEXT, PRIMARY KEY(`id`) );");
            Debug.Log("Database struct created");
        }
    }

    public void erase()
    {
        _connection.DeleteAll<ItemDB>();
    }

    public void sort(string kind)
    {
        PlayerPrefs.SetString("SortMode", kind);
        PlayerPrefs.SetInt("ReverseMode", PlayerPrefs.GetInt("ReverseMode", 0) == 0 ? 1 : 0);
    }

    public List<ItemDB> GetItems()
    {
        string kind = PlayerPrefs.GetString("SortMode", "id");
        try
        {
            return _connection.Query<ItemDB>("SELECT * FROM ItemDB ORDER BY " + kind + " " + (PlayerPrefs.GetInt("ReverseMode", 0) == 0 ? "ASC" : "DESC") + ";");
        }
        catch (Exception)
        {
            _connection.Execute("CREATE TABLE 'ItemDB' ( `id` INTEGER, `name` TEXT, `state` INTEGER, `unusual` INTEGER, `strange` INTEGER DEFAULT 0, `quality` INTEGER, `crate` TEXT, PRIMARY KEY(`id`) );");
            Debug.Log("Database struct created");
            return _connection.Query<ItemDB>("SELECT * FROM ItemDB ORDER BY " + kind + " " + (PlayerPrefs.GetInt("ReverseMode", 0) == 0 ? "ASC" : "DESC") + ";");
            throw;
        }
        
    }

    public ItemDB addItem(string id, int st=0, int un=0, int str=0,int ql=-1,string cr="")
    {
        var p = new ItemDB
        {
            name = id,
            state = st,
            unusual = un,
            strange = str,
            quality = ql,
            crate = cr
        };
        _connection.Insert(p);
        return p;
    }

    public void update(ItemDB item)
    {
        _connection.Update(item);
    }

    public void removeItem(itemBase id)
    {
        foreach (ItemDB item in GetItems())
        {
            if(item.name == id.item_name && item.strange == id.strange && (item.unusual>0) == id.unusual && item.state == id.state && id.page == PlayerPrefs.GetInt("page"))
            {
                _connection.Query<ItemDB>("DELETE FROM ItemDB WHERE id in( SELECT id FROM ItemDB WHERE name='" + item.name + "' AND strange= '"+item.strange+"' AND state='"+item.state+"' AND unusual='"+item.unusual+"' LIMIT 1);");
                return;
            }
        }
    }

    public void removeItem(ItemDB id)
    {
        foreach (ItemDB item in GetItems())
        {
            if (item.name == id.name && item.strange == id.strange && item.unusual == id.unusual && item.state == id.state)
            {
                _connection.Query<ItemDB>("DELETE FROM ItemDB WHERE id in( SELECT id FROM ItemDB WHERE name='" + item.name + "' AND strange= '" + item.strange + "' AND state='" + item.state + "' AND unusual='" + item.unusual + "' LIMIT 1);");
                return;
            }
        }
    }

    public void removeItem(string id)
    {
        foreach (ItemDB item in GetItems())
        {
            if (item.name == id)
            {
                _connection.Query<ItemDB>("DELETE FROM ItemDB WHERE id in( SELECT id FROM ItemDB WHERE name='" + item.name + "' AND strange= '" + item.strange + "' AND state='" + item.state + "' AND unusual='" + item.unusual + "' LIMIT 1);");
                //_connection.Delete(item);
                return;
            }
        }
    }
}