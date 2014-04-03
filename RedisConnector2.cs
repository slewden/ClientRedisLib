using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ClientRedisLib.RedisClass;

namespace ClientRedisLib
{
  /// <summary>
  /// A client for Redis Server
  /// Public Methods mapping Server REDIS commands are in RedisConnector2.cs
  /// All principals methods are in the RedisConnector.cs
  /// </summary>
  public partial class RedisConnector
  {
    #region Key Methods
    /// <summary>
    /// Launch the "DEL" command To remove a key from the DB
    /// </summary>
    /// <param name="key">The keys to del</param>
    /// <returns>return the number of keys that were really removed</returns>
    public int Del(params string[] key)
    {
      if (key.Length == 0)
      {
        this.LastErrorText = "DEL need one at least";
        return -1;
      }
      else
      {
        RedisReponse answer = this.SendCommand(RedisConnector.MergeStringBefore("DEL", key));
        if (!answer.Success)
        {
          this.LastErrorText = answer.ErrorMessage;
          return -1;
        }
        else
        {
          return Convert.ToInt32(answer.Number);
        }
      }
    }

    /// <summary>
    /// Launch the "DUMP" command To get a key serialized content 
    /// </summary>
    /// <param name="key">the key to dump</param>
    /// <returns>the serialized content</returns>
    public string Dump(string key)
    {
      if (this.GetServerVersion() >= SERVER26)
      {
        RedisReponse answer = this.SendCommand("DUMP", key);
        if (!answer.Success)
        {
          this.LastErrorText = answer.ErrorMessage;
          return null;
        }
        else
        {
          return answer.Line;
        }
      }
      else
      {
        this.LastCommandText = "DUMP " + key + RedisConnector.NOTIMPLEMENTEDINTHISVERSION;
        this.LastErrorText = "Not implemented on this server version";
        return null;
      }
    }

    /// <summary>
    /// Launch the "EXISTS" command to know if a key exists
    /// </summary>
    /// <param name="key">the key to test</param>
    /// <returns>true if the key exists, false else</returns>
    public bool Exists(string key)
    {
      RedisReponse answer = this.SendCommand("EXISTS", key);
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
        return false;
      }
      else
      {
        // here we clear LastErrorText : to be sur to make the difference between : Exist =no and Exist raise an error
        this.LastErrorText = string.Empty;
        return answer.Number == 1.0;
      }
    }

    /// <summary>
    /// Launch the "EXPIRE" command to set the expiration timeout of a key
    /// </summary>
    /// <param name="key">the key to set the time out</param>
    /// <param name="timeOut">Time out in seconds</param>
    /// <returns>true if the key timeout is set, false else</returns>
    public bool Expire(string key, long timeOut)
    {
      RedisReponse answer = this.SendCommand("EXPIRE", key, timeOut.ToString());
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
        return false;
      }
      else
      {
        // here we clear LastErrorText : to be sur to make the difference between : Exist =no and Exist raise an error
        this.LastErrorText = string.Empty;
        return answer.Number == 1.0;
      }
    }

    /// <summary>
    /// Launch the "EXPIREAT" command to set the expiration date of a key
    /// </summary>
    /// <param name="key">the key to set the time out</param>
    /// <param name="timeOut">the date in seconds where the key must expire</param>
    /// <returns>true if the key  timeout is set, false else</returns>
    public bool ExpireAt(string key, DateTime timeOut)
    {
      double seconds = RedisConnector.GetUnixTimeFromDateTime(timeOut);
      RedisReponse answer = this.SendCommand("EXPIREAT", key, Convert.ToInt64(seconds).ToString());
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
        return false;
      }
      else
      {
        // here we clear LastErrorText : to be sur to make the difference between : Exist =no and Exist raise an error
        this.LastErrorText = string.Empty;
        return answer.Number == 1.0;
      }
    }

    /// <summary>
    /// Launch the "KEYS" command to get all keys matching pattern
    /// </summary>
    /// <param name="pattern">the pattern to find key accept ? (one char) * (all char) and [] (range of char)</param>
    /// <returns>the list of key matching pattern</returns>
    public List<string> Keys(string pattern)
    {
      RedisReponse answer = this.SendCommand("KEYS", pattern);
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
        return null;
      }
      else
      {
        // here we clear LastErrorText : to be sur to make the difference between : Exist =no and Exist raise an error
        this.LastErrorText = string.Empty;
        return answer.Datas;
      }
    }

    /// <summary>
    /// Launch the "MIGRATE" command To transfer a key from the server to another one
    /// </summary>
    /// <param name="host">The Destination server host</param>
    /// <param name="port">The destination server port</param>
    /// <param name="key">the key to migrate</param>
    /// <param name="databaseId">the id of the destination database</param>
    /// <param name="timeOut">the idle time OUT during migration (in millisecond)</param>
    /// <returns>the serialized content</returns>
    public bool Migrate(string host, int port, string key, int databaseId, long timeOut)
    {
      if (this.GetServerVersion() >= SERVER26)
      {
        RedisReponse answer = this.SendCommand("MIGRATE", host, port.ToString(), key, databaseId.ToString(), timeOut.ToString());
        if (!answer.Success)
        {
          if (answer.ErrorMessage.ToUpper().StartsWith("IOERR"))
          {
            this.LastErrorText = answer.ErrorMessage + " Command not completed but check the key in the destination database it may be created";
          }
          else
          {
            this.LastErrorText = answer.ErrorMessage;
          }

          return false;
        }
        else
        {
          return answer.Success;
        }
      }
      else
      {
        this.LastCommandText = string.Format("MIGRATE {0} {1} {2} {3} {4} {5}", host, port, key, databaseId, timeOut, RedisConnector.NOTIMPLEMENTEDINTHISVERSION);
        this.LastErrorText = "Not implemented on this server version";
        return false;
      }
    }

    /// <summary>
    /// Launch the "MOVE" command To transfer a key to another database on the same server
    /// </summary>
    /// <param name="key">the key to migrate</param>
    /// <param name="databaseId">the id of the destination database</param>
    /// <returns>the serialized content</returns>
    public bool Move(string key, int databaseId)
    {
      RedisReponse answer = this.SendCommand("MOVE", key, databaseId.ToString());
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
        return false;
      }
      else
      {
        // here we clear LastErrorText : to be sur to make the difference between : Exist =no and Exist raise an error
        this.LastErrorText = string.Empty;
        return answer.Number == 1.0;
      }
    }

    /// <summary>
    /// Launch the "OBJECT REFCOUNT" command To get the number of reference for the key
    /// </summary>
    /// <param name="key">The key to count the references</param>
    /// <returns>return the number of references</returns>
    public int ObjectRefcount(string key)
    {
      RedisReponse answer = this.SendCommand("OBJECT", "REFCOUNT", key);
      if (!answer.Success || answer.Number.Equals(double.NaN))
      {
        this.LastErrorText = answer.ErrorMessage;
        return -1;
      }
      else
      {
        return Convert.ToInt32(answer.Number);
      }
    }

    /// <summary>
    /// Launch the "OBJECT ENCODING" command To get the encoding format of a key
    /// </summary>
    /// <param name="key">The key to analyze</param>
    /// <returns>return the encoding format</returns>
    public string ObjectEncoding(string key)
    {
      RedisReponse answer = this.SendCommand("OBJECT", "ENCODING", key);
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
        return string.Empty;
      }
      else
      {
        return answer.Line;
      }
    }

    /// <summary>
    /// Launch the "OBJECT IDLETIME" command To get the time witch a key is not used
    /// </summary>
    /// <param name="key">The key to analyze</param>
    /// <returns>returns the time during which a key was not used</returns>
    public TimeSpan ObjectIdletime(string key)
    {
      RedisReponse answer = this.SendCommand("OBJECT", "IDLETIME", key);
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
        return TimeSpan.MaxValue;
      }
      else if (answer.Number.Equals(double.NaN))
      {
        this.LastErrorText = CSTERRORNAN;
        return TimeSpan.MaxValue;
      }
      else
      {
        return TimeSpan.FromSeconds(Convert.ToInt32(answer.Number));
      }
    }

    /// <summary>
    /// Launch the "PERSIST" command to remove the expiration timeout of a key
    /// </summary>
    /// <param name="key">the key to set the time out</param>
    /// <returns>true if the key timeout is set, false else</returns>
    public bool Persist(string key)
    {
      RedisReponse answer = this.SendCommand("PERSIST", key);
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
        return false;
      }
      else
      {
        // here we clear LastErrorText : to be sur to make the difference between : Exist =no and Exist raise an error
        this.LastErrorText = string.Empty;
        return answer.Number == 1.0;
      }
    }

    /// <summary>
    /// Launch the "PEXPIRE" command to set the expiration timeout of a key
    /// </summary>
    /// <param name="key">the key to set the time out</param>
    /// <param name="timeOutMillisecond">Time out in milliseconds</param>
    /// <returns>true if the key timeout is set, false else</returns>
    public bool PExpire(string key, long timeOutMillisecond)
    {
      if (this.GetServerVersion() >= SERVER26)
      { // Good version
        RedisReponse answer = this.SendCommand("PEXPIRE", key, timeOutMillisecond.ToString());
        if (!answer.Success)
        {
          this.LastErrorText = answer.ErrorMessage;
          return false;
        }
        else
        {
          // here we clear LastErrorText : to be sur to make the difference between : Exist =no and Exist raise an error
          this.LastErrorText = string.Empty;
          return answer.Number == 1.0;
        }
      }
      else
      { // use default procedure
        return this.Expire(key, timeOutMillisecond / 1000);
      }
    }

    /// <summary>
    /// Launch the "PEXPIREAT" command to set the expiration date of a key
    /// </summary>
    /// <param name="key">the key to set the time out</param>
    /// <param name="timeOut">the date where the key must expire</param>
    /// <returns>true if the key  timeout is set, false else</returns>
    public bool PExpireAt(string key, DateTime timeOut)
    {
      if (this.GetServerVersion() >= SERVER26)
      { // Good version
        double seconds = RedisConnector.GetUnixTimeFromDateTime(timeOut) * 1000;
        RedisReponse answer = this.SendCommand("PEXPIREAT", key, Convert.ToInt64(seconds).ToString());
        if (!answer.Success)
        {
          this.LastErrorText = answer.ErrorMessage;
          return false;
        }
        else
        {
          // here we clear LastErrorText : to be sur to make the difference between : Exist =no and Exist raise an error
          this.LastErrorText = string.Empty;
          return answer.Number == 1.0;
        }
      }
      else
      { // use default procedure
        return this.PExpireAt(key, timeOut);
      }
    }

    /// <summary>
    /// Launch the "PTTL" command to get the time to live of a key in milliseconds
    /// </summary>
    /// <param name="key">the key to get the time to live</param>
    /// <returns>duration in milliseconds</returns>
    public TimeSpan PTTL(string key)
    {
      if (this.GetServerVersion() >= SERVER26)
      { // Good version
        RedisReponse answer = this.SendCommand("PTTL", key);
        if (!answer.Success)
        {
          this.LastErrorText = answer.ErrorMessage;
          return TimeSpan.MaxValue;
        }
        else
        {
          // here we clear LastErrorText : to be sur to make the difference between : Exist =no and Exist raise an error
          this.LastErrorText = string.Empty;
          return RedisConnector.GetTimeSpanFromMilliSecDouble(answer.Number);
        }
      }
      else
      { // use default procedure
        return this.TTL(key);
      }
    }

    /// <summary>
    /// Launch the "RANDOMKEY" command To get a key 
    /// </summary>
    /// <returns>the key</returns>
    public string RandomKey()
    {
      RedisReponse answer = this.SendCommand("RANDOMKEY");
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
        return null;
      }
      else
      {
        this.LastErrorText = string.Empty;
        return answer.Line;
      }
    }

    /// <summary>
    /// Launch the "RENAME" command To rename key to NEWKEY
    /// </summary>
    /// <param name="key">The key to rename</param>
    /// <param name="newKey">The new name of key</param>
    /// <returns>true if done</returns>
    public bool Rename(string key, string newKey)
    {
      RedisReponse answer = this.SendCommand("RENAME", key, newKey);
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
        return false;
      }
      else
      {
        return answer.Line == "OK";
      }
    }

    /// <summary>
    /// Launch the "RENAMENX" command To rename key to NEWKEY only if NEWKEY does not exists
    /// </summary>
    /// <param name="key">The key to rename</param>
    /// <param name="newKey">The new name of key</param>
    /// <returns>true if done</returns>
    public bool RenameNX(string key, string newKey)
    {
      RedisReponse answer = this.SendCommand("RENAMENX", key, newKey);
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
        return false;
      }
      else
      {
        return answer.Number == 1.0;
      }
    }

    /// <summary>
    /// Launch the "RESTORE" command To set a serialized key 
    /// </summary>
    /// <param name="key">the key to restore</param>
    /// <param name="ttl">Time to live to set to the key</param>
    /// <param name="serializedValue">Value to restore</param>
    /// <returns>the serialized content</returns>
    public bool Restore(string key, int ttl, string serializedValue)
    {
      if (this.GetServerVersion() >= SERVER26)
      {
        RedisReponse answer = this.SendCommand("RESTORE", key, ttl.ToString(), serializedValue);
        if (!answer.Success)
        {
          this.LastErrorText = answer.ErrorMessage;
          return false;
        }
        else
        {
          return answer.Line == "OK";
        }
      }
      else
      {
        this.LastCommandText = string.Format("RESTORE {0} {1} ... {2}", key, ttl, RedisConnector.NOTIMPLEMENTEDINTHISVERSION);
        this.LastErrorText = "Not implemented on this server version";
        return false;
      }
    }

    /// <summary>
    /// Launch the "SORT" command to returns or stores the elements contained in the list, set or sorted set at key
    /// </summary>
    /// <param name="key">the key to sort</param>
    /// <param name="patternBy">By pattern if filled sort by a external key</param>
    /// <param name="limitArg">Limit argument to limit the response</param>
    /// <param name="order">Indicate the sort order</param>
    /// <param name="alpha">Choose an alphabetical sort instead of numerical sort</param>
    /// <param name="destination">Key to store the result</param>
    /// <param name="getPatterns">List of pattern to returned keys (can be [pattern]*->field for hash</param>
    /// <returns>duration in seconds</returns>
    public List<string> Sort(string key, string patternBy, Limit limitArg, SortOrder order, bool alpha, string destination, List<string> getPatterns)
    {
      int nb = 2 + (string.IsNullOrWhiteSpace(patternBy) ? 0 : 2) + ((limitArg != null && !limitArg.IsEmpty) ? 3 : 0) + (order != SortOrder.None ? 1 : 0) + (alpha ? 1 : 0) + (string.IsNullOrWhiteSpace(destination) ? 0 : 2) + (getPatterns == null ? 0 : getPatterns.Count * 2);
      string[] args = new string[nb];
      int n = 0;

      args[n++] = "SORT";
      args[n++] = key;

      if (!string.IsNullOrWhiteSpace(patternBy))
      {
        args[n++] = "BY";
        args[n++] = patternBy;
      }

      if (limitArg != null && !limitArg.IsEmpty)
      {
        args[n++] = "LIMIT";
        args[n++] = limitArg.Offset.ToString();
        args[n++] = limitArg.Count.ToString();
      }

      if (order != SortOrder.None)
      {
        args[n++] = order.ToString();
      }

      if (alpha)
      {
        args[n++] = "ALPHA";
      }

      if (!string.IsNullOrWhiteSpace(destination))
      {
        args[n++] = "STORE";
        args[n++] = destination;
      }

      if (getPatterns != null)
      {
        foreach (string getPattern in getPatterns)
        {
          args[n++] = "GET";
          args[n++] = getPattern;
        }
      }

      RedisReponse answer = this.SendCommand(args);
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
        return null;
      }
      else
      {
        // here we clear LastErrorText : to be sur to make the difference between : Exist =no and Exist raise an error
        this.LastErrorText = string.Empty;
        return answer.Datas;
      }
    }

    /// <summary>
    /// Launch the "TTL" command to get the time to live of a key in seconds
    /// </summary>
    /// <param name="key">the key to get the time to live</param>
    /// <returns>duration in seconds</returns>
    public TimeSpan TTL(string key)
    {
      RedisReponse answer = this.SendCommand("TTL", key);
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
        return TimeSpan.MaxValue;
      }
      else
      {
        // here we clear LastErrorText : to be sur to make the difference between : Exist =no and Exist raise an error
        this.LastErrorText = string.Empty;
        return RedisConnector.GetTimeSpanFromSecDouble(answer.Number);
      }
    }

    /// <summary>
    /// Launch the "TYPE" command To get the type of a key 
    /// </summary>
    /// <param name="key">the key to analyze</param>
    /// <returns>the type of a key</returns>
    public string Type(string key)
    {
      RedisReponse answer = this.SendCommand("TYPE", key);
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
        return null;
      }
      else
      {
        this.LastErrorText = string.Empty;
        return answer.Line;
      }
    }
    #endregion

    #region strings Methods
    /// <summary>
    /// Launch the "APPEND" command To add extra string to  a key
    /// </summary>
    /// <param name="key">the key to update</param>
    /// <param name="value">the text to add</param>
    /// <returns>the new length of the key</returns>
    public int Append(string key, string value)
    {
      RedisReponse answer = this.SendCommand("APPEND", key, value);
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
        return 0;
      }
      else if (double.IsNaN(answer.Number))
      {
        this.LastErrorText = CSTERRORNAN;
        return 0;
      }
      else
      {
        // here we clear LastErrorText : to be sur to make the difference between : Exist =no and Exist raise an error
        this.LastErrorText = string.Empty;
        return Convert.ToInt32(answer.Number);
      }
    }

    /// <summary>
    /// Launch the "BITCOUNT" command To Count the number of set bits (population counting) in a string.
    /// </summary>
    /// <param name="key">the key to count</param>
    /// <returns>The number of bit</returns>
    public int BitCount(string key)
    {
      return this.BitCount(key, int.MinValue, int.MaxValue);
    }

    /// <summary>
    /// Launch the "BITCOUNT" command To Count the number of set bits (population counting) in a string.
    /// </summary>
    /// <param name="key">the key to count</param>
    /// <param name="start">the index to start</param>
    /// <param name="end">the index to end</param>
    /// <returns>the count</returns>
    public int BitCount(string key, int start, int end)
    {
      if (this.GetServerVersion() >= SERVER26)
      {
        RedisReponse answer;
        if (start != int.MinValue && end != int.MinValue && start != int.MaxValue && end != int.MaxValue)
        {
          answer = this.SendCommand("BITCOUNT", start.ToString(), end.ToString());
        }
        else
        {
          answer = this.SendCommand("BITCOUNT");
        }

        if (!answer.Success)
        {
          this.LastErrorText = answer.ErrorMessage;
          return 0;
        }
        else if (double.IsNaN(answer.Number))
        {
          this.LastErrorText = CSTERRORNAN;
          return 0;
        }
        else
        {
          // here we clear LastErrorText : to be sur to make the difference between : Exist =no and Exist raise an error
          this.LastErrorText = string.Empty;
          return Convert.ToInt32(answer.Number);
        }
      }
      else
      {
        this.LastCommandText = "BITCOUNT" + RedisConnector.NOTIMPLEMENTEDINTHISVERSION;
        this.LastErrorText = "BITCOUNT : Invalid command in this version";
        return 0;
      }
    }

    /// <summary>
    /// Launch the "BITOP" command To Perform a bitwise operation between multiple keys 
    /// (containing string values) and store the result in the destination key.
    /// </summary>
    /// <param name="operation">the combine operation</param>
    /// <param name="destKey">the destination key</param>
    /// <param name="keys">the array of key to combine</param>
    /// <returns>the size of the result string</returns>
    public int BitOp(BitOperation operation, string destKey, params string[] keys)
    {
      if (this.GetServerVersion() >= SERVER26)
      {
        RedisReponse answer = this.SendCommand(RedisConnector.MergeStringBefore(RedisConnector.MergeStringBefore("BITOP", operation.ToString(), destKey), keys));

        if (!answer.Success)
        {
          this.LastErrorText = answer.ErrorMessage;
          return 0;
        }
        else if (double.IsNaN(answer.Number))
        {
          this.LastErrorText = CSTERRORNAN;
          return 0;
        }
        else
        {
          // here we clear LastErrorText : to be sur to make the difference between : Exist =no and Exist raise an error
          this.LastErrorText = string.Empty;
          return Convert.ToInt32(answer.Number);
        }
      }
      else
      {
        this.LastCommandText = "BITOP" + RedisConnector.NOTIMPLEMENTEDINTHISVERSION;
        this.LastErrorText = "BITOP : Invalid command in this version";
        return 0;
      }
    }

    /// <summary>
    /// Launch the "DECR" command To decrements the number stored at key by one.
    /// </summary>
    /// <param name="key">the key to decrement</param>
    /// <returns>the new value</returns>
    public int Decr(string key)
    {
      RedisReponse answer = this.SendCommand("DECR", key);
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
        return 0;
      }
      else if (double.IsNaN(answer.Number))
      {
        this.LastErrorText = CSTERRORNAN;
        return 0;
      }
      else
      {
        // here we clear LastErrorText : to be sur to make the difference between : Exist =no and Exist raise an error
        this.LastErrorText = string.Empty;
        return Convert.ToInt32(answer.Number);
      }
    }

    /// <summary>
    /// Launch the "DECRBY" command To decrements the number stored at key by a decrement.
    /// </summary>
    /// <param name="key">the key to decrement</param>
    /// <param name="decrement">the value to decrement key</param>
    /// <returns>the new value</returns>
    public int DecrBy(string key, int decrement)
    {
      RedisReponse answer = this.SendCommand("DECRBY", key, decrement.ToString());
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
        return 0;
      }
      else if (double.IsNaN(answer.Number))
      {
        this.LastErrorText = CSTERRORNAN;
        return 0;
      }
      else
      {
        // here we clear LastErrorText : to be sur to make the difference between : Exist =no and Exist raise an error
        this.LastErrorText = string.Empty;
        return Convert.ToInt32(answer.Number);
      }
    }

    /// <summary>
    /// Launch the "GET" command To get the value of a key
    /// </summary>
    /// <param name="key">the key to get the value</param>
    /// <returns>the value</returns>
    public string Get(string key)
    {
      RedisReponse answer = this.SendCommand("GET", key);
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
        return string.Empty;
      }
      else
      {
        // here we clear LastErrorText : to be sur to make the difference between : Exist =no and Exist raise an error
        this.LastErrorText = string.Empty;
        return answer.Line;
      }
    }

    /// <summary>
    /// Launch the "GETBIT" command To return the bit value at offset in the string value stored at key.
    /// </summary>
    /// <param name="key">the key to get bit</param>
    /// <param name="offset">the offset</param>
    /// <returns>the value</returns>
    public bool GetBit(string key, int offset)
    {
      RedisReponse answer = this.SendCommand("GETBIT", key, offset.ToString());
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
        return false;
      }
      else if (double.IsNaN(answer.Number))
      {
        this.LastErrorText = CSTERRORNAN;
        return false;
      }
      else
      {
        // here we clear LastErrorText : to be sur to make the difference between : Exist =no and Exist raise an error
        this.LastErrorText = string.Empty;
        return answer.Number == 1;
      }
    }

    /// <summary>
    /// Launch the "GETRANGE" command To get the substring of the string value stored at key
    /// </summary>
    /// <param name="key">the key to get the value</param>
    /// <param name="start">the start index (0 based, or negative to extract from the end of the string)</param>
    /// <param name="end">the ent index (0 based, or negative to extract from the end of the string)</param>
    /// <returns>the value</returns>
    public string GetRange(string key, int start, int end)
    {
      RedisReponse answer = this.SendCommand("GETRANGE", key, start.ToString(), end.ToString());
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
        return string.Empty;
      }
      else
      {
        // here we clear LastErrorText : to be sur to make the difference between : Exist =no and Exist raise an error
        this.LastErrorText = string.Empty;
        return answer.Line;
      }
    }

    /// <summary>
    /// Launch the "GETSET" command To atomically sets key to value and returns the old value stored at key
    /// </summary>
    /// <param name="key">the key to get / set </param>
    /// <param name="value">the value ti set</param>
    /// <returns>the value store in key before setting value</returns>
    public string GetSet(string key, string value)
    {
      RedisReponse answer = this.SendCommand("GETSET", key, value);
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
        return string.Empty;
      }
      else
      {
        // here we clear LastErrorText : to be sur to make the difference between : Exist =no and Exist raise an error
        this.LastErrorText = string.Empty;
        return answer.Line;
      }
    }

    /// <summary>
    /// Launch the "INCR" command To increments the number stored at key by one.
    /// </summary>
    /// <param name="key">the key to increment</param>
    /// <returns>the new value</returns>
    public int Incr(string key)
    {
      RedisReponse answer = this.SendCommand("INCR", key);
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
        return 0;
      }
      else if (double.IsNaN(answer.Number))
      {
        this.LastErrorText = CSTERRORNAN;
        return 0;
      }
      else
      {
        // here we clear LastErrorText : to be sur to make the difference between : Exist =no and Exist raise an error
        this.LastErrorText = string.Empty;
        return Convert.ToInt32(answer.Number);
      }
    }

    /// <summary>
    /// Launch the "INCRBY" command To increments the number stored at key by an increment.
    /// </summary>
    /// <param name="key">the key to increment</param>
    /// <param name="increment">the value to increment key</param>
    /// <returns>the new value</returns>
    public int IncrBy(string key, int increment)
    {
      RedisReponse answer = this.SendCommand("INCRBY", key, increment.ToString());
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
        return 0;
      }
      else if (double.IsNaN(answer.Number))
      {
        this.LastErrorText = CSTERRORNAN;
        return 0;
      }
      else
      {
        // here we clear LastErrorText : to be sur to make the difference between : Exist =no and Exist raise an error
        this.LastErrorText = string.Empty;
        return Convert.ToInt32(answer.Number);
      }
    }

    /// <summary>
    /// Launch the "INCRBYFLOAT" command To increments the number stored at key by an increment.
    /// </summary>
    /// <param name="key">the key to increment</param>
    /// <param name="increment">the value to increment key</param>
    /// <returns>the new value</returns>
    public double IncrByFloat(string key, double increment)
    {
      if (this.GetServerVersion() >= SERVER26)
      {
        RedisReponse answer = this.SendCommand("INCRBYFLOAT", key, increment.ToString());
        if (!answer.Success)
        {
          this.LastErrorText = answer.ErrorMessage;
          return 0;
        }
        else if (double.IsNaN(answer.Number))
        {
          this.LastErrorText = CSTERRORNAN;
          return 0;
        }
        else
        {
          // here we clear LastErrorText : to be sur to make the difference between : Exist =no and Exist raise an error
          this.LastErrorText = string.Empty;
          return Convert.ToDouble(answer.Number);
        }
      }
      else
      {
        this.LastCommandText = "INCRBYFLOAT" + RedisConnector.NOTIMPLEMENTEDINTHISVERSION;
        this.LastErrorText = "INCRBYFLOAT : Invalid command in this version";
        return 0;
      }
    }

    /// <summary>
    /// Launch the "MGET" command To get the value of series of keys
    /// </summary>
    /// <param name="keys">the keys to get the values</param>
    /// <returns>the list of values</returns>
    public List<string> MGet(params string[] keys)
    {
      RedisReponse answer = this.SendCommand(RedisConnector.MergeStringBefore("MGET", keys));
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
        return null;
      }
      else
      {
        return answer.Datas;
      }
    }

    /// <summary>
    /// Launch the "MSET" command To set the values of series of keys
    /// </summary>
    /// <param name="keyValues">list of the key values parameters</param>
    /// <returns>true if ok</returns>
    public bool MSet(List<KeyValuePair<string, string>> keyValues)
    {
      string[] keys = new string[(keyValues.Count * 2) + 1];
      int i = 1;
      keys[0] = "MSET";
      foreach (var val in keyValues)
      {
        keys[i++] = val.Key;
        keys[i++] = val.Value;
      }

      RedisReponse answer = this.SendCommand(keys);
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
        return false;
      }
      else
      {
        return true;
      }
    }

    /// <summary>
    /// Launch the "MSETNX" command To set the values of series of keys
    /// </summary>
    /// <param name="keyValues">list of the key values parameters</param>
    /// <returns>true if ok</returns>
    public bool MSetNX(List<KeyValuePair<string, string>> keyValues)
    {
      string[] keys = new string[(keyValues.Count * 2) + 1];
      int i = 1;
      keys[0] = "MSETNX";
      foreach (var val in keyValues)
      {
        keys[i++] = val.Key;
        keys[i++] = val.Value;
      }

      RedisReponse answer = this.SendCommand(keys);
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
        return false;
      }
      else
      {
        // here we clear LastErrorText : to be sur to make the difference between : Exist =no and Exist raise an error
        this.LastErrorText = string.Empty;
        return answer.Number == 1.0;
      }
    }

    /// <summary>
    /// Launch the "PSETEX" command To set key to hold the string value and set key to timeout after a given number of milliseconds
    /// </summary>
    /// <param name="key">the key to set</param>
    /// <param name="milliSeconds">the time in seconds to hold the key</param>
    /// <param name="value">the value to set</param>
    /// <returns>true if ok</returns>
    public bool PSetEX(string key, int milliSeconds, string value)
    {
      if (this.GetServerVersion() >= SERVER26)
      {
        RedisReponse answer = this.SendCommand("PSETEX", key, milliSeconds.ToString(), value);
        if (!answer.Success)
        {
          this.LastErrorText = answer.ErrorMessage;
          return false;
        }
        else
        {
          return answer.Line.ToLower() == "ok";
        }
      }
      else
      {
        this.LastCommandText = "PSETEX" + RedisConnector.NOTIMPLEMENTEDINTHISVERSION;
        this.LastErrorText = "PSETEX : Invalid command in this version";
        return false;
      }
    }

    /// <summary>
    /// Launch the "SET" command To set the values of a key
    /// </summary>
    /// <param name="key">the key to set</param>
    /// <param name="value">the value to set</param>
    /// <returns>true if ok</returns>
    public bool Set(string key, string value)
    {
      RedisReponse answer = this.SendCommand("SET", key, value);
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
        return false;
      }
      else
      {
        return true;
      }
    }

    /// <summary>
    /// Launch the "SETBIT" command To sets or clears the bit at offset in the string value stored at key
    /// </summary>
    /// <param name="key">the key to set</param>
    /// <param name="offset">the offset of the bit to set</param>
    /// <param name="value">the value to set</param>
    /// <returns>true if ok</returns>
    public bool SetBit(string key, int offset, bool value)
    {
      RedisReponse answer = this.SendCommand("SETBIT", key, offset.ToString(), value ? "1" : "0");
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
        return false;
      }
      else if (double.IsNaN(answer.Number))
      {
        this.LastErrorText = CSTERRORNAN;
        return false;
      }
      else
      {
        return answer.Number == 1.0;
      }
    }

    /// <summary>
    /// Launch the "SETEX" command To set key to hold the string value and set key to timeout after a given number of seconds
    /// </summary>
    /// <param name="key">the key to set</param>
    /// <param name="seconds">the time in seconds to hold the key</param>
    /// <param name="value">the value to set</param>
    /// <returns>true if ok</returns>
    public bool SetEX(string key, int seconds, string value)
    {
      RedisReponse answer = this.SendCommand("SETEX", key, seconds.ToString(), value);
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
        return false;
      }
      else if (string.IsNullOrWhiteSpace(answer.Line))
      {
        this.LastErrorText = string.Empty;
        return false;
      }
      else
      {
        this.LastErrorText = string.Empty;
        return answer.Line.ToLower() == "ok";
      }
    }

    /// <summary>
    /// Launch the "SETNX" command To set the values of a key if this key does not exists
    /// </summary>
    /// <param name="key">the key to set</param>
    /// <param name="value">the value to set</param>
    /// <returns>true if ok</returns>
    public bool SetNX(string key, string value)
    {
      RedisReponse answer = this.SendCommand("SETNX", key, value);
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
        return false;
      }
      else if (double.IsNaN(answer.Number))
      {
        this.LastErrorText = CSTERRORNAN;
        return false;
      }
      else
      {
        this.LastErrorText = string.Empty;
        return answer.Number == 1.0;
      }
    }

    /// <summary>
    /// Launch the "SETRANGE" command To Overwrites part of the string stored at key
    /// starting at the specified offset, for the entire length of value
    /// </summary>
    /// <param name="key">the key to set</param>
    /// <param name="offset">the offset</param>
    /// <param name="value">the value to set</param>
    /// <returns>the total length of the string</returns>
    public int SetRange(string key, int offset, string value)
    {
      RedisReponse answer = this.SendCommand("SETRANGE", key, offset.ToString(), value);
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
        return 0;
      }
      else if (double.IsNaN(answer.Number))
      {
        this.LastErrorText = CSTERRORNAN;
        return 0;
      }
      else
      {
        return Convert.ToInt32(answer.Number);
      }
    }

    /// <summary>
    /// Launch the "STRLEN" command To returns the length of the string value stored at key.
    /// </summary>
    /// <param name="key">the key</param>
    /// <returns>the total length of the key</returns>
    public int StrLen(string key)
    {
      RedisReponse answer = this.SendCommand("STRLEN", key);
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
        return 0;
      }
      else if (double.IsNaN(answer.Number))
      {
        this.LastErrorText = CSTERRORNAN;
        return 0;
      }
      else
      {
        return Convert.ToInt32(answer.Number);
      }
    }
    #endregion

    #region Hashes Methods
    /// <summary>
    /// Launch the "HDEL" command To remove specified field form hash key
    /// </summary>
    /// <param name="key">the key to update</param>
    /// <param name="field">the first field to remove</param>
    /// <param name="fields">the other fields to remove</param>
    /// <returns>the length of deleted fields</returns>
    public int HDel(string key, string field, params string[] fields)
    {
      RedisReponse answer = this.SendCommand(RedisConnector.MergeStringBefore(RedisConnector.MergeStringBefore("HDEL", key, field), fields));
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
        return 0;
      }
      else if (double.IsNaN(answer.Number))
      {
        this.LastErrorText = CSTERRORNAN;
        return 0;
      }
      else
      {
        // here we clear LastErrorText : to be sur to make the difference between : Exist =no and Exist raise an error
        this.LastErrorText = string.Empty;
        return Convert.ToInt32(answer.Number);
      }
    }

    /// <summary>
    /// Launch the "HEXISTS" command To returns if field is an existing field in the hash stored at key
    /// </summary>
    /// <param name="key">the key analyze</param>
    /// <param name="field">the field to analyze</param>
    /// <returns>true if field exists in key</returns>
    public bool HExists(string key, string field)
    {
      RedisReponse answer = this.SendCommand("HEXISTS", key, field);
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
        return false;
      }
      else if (double.IsNaN(answer.Number))
      {
        this.LastErrorText = CSTERRORNAN;
        return false;
      }
      else
      {
        // here we clear LastErrorText : to be sur to make the difference between : Exist =no and Exist raise an error
        this.LastErrorText = string.Empty;
        return Convert.ToInt32(answer.Number) == 1;
      }
    }

    /// <summary>
    /// Launch the "HGET" command To returns the value associated with field in the hash stored at key.
    /// </summary>
    /// <param name="key">the key to get</param>
    /// <param name="field">the field to get</param>
    /// <returns>the value</returns>
    public string HGet(string key, string field)
    {
      RedisReponse answer = this.SendCommand("HGET", key, field);
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
        return null;
      }
      else
      {
        // here we clear LastErrorText : to be sur to make the difference between : Exist =no and Exist raise an error
        this.LastErrorText = string.Empty;
        return answer.Line;
      }
    }

    /// <summary>
    /// Launch the "HGETALL" command To returns all the fields and the values in the hash stored at key.
    /// </summary>
    /// <param name="key">the key to get</param>
    /// <returns>the fields and values list</returns>
    public List<Tuple<string, string>> HGetAll(string key)
    {
      RedisReponse answer = this.SendCommand("HGETALL", key);
      if (!answer.Success || answer.Datas == null)
      {
        this.LastErrorText = answer.ErrorMessage;
        return null;
      }
      else if (double.IsNaN(answer.Number))
      {
        this.LastErrorText = CSTERRORNAN;
        return null;
      }
      else
      {
        // here we clear LastErrorText : to be sur to make the difference between : Exist =no and Exist raise an error
        this.LastErrorText = string.Empty;
        List<Tuple<string, string>> res = new List<Tuple<string, string>>();
        for (int i = 0; i < answer.Datas.Count - 1; i += 2)
        {
          res.Add(new Tuple<string, string>(answer.Datas[i], answer.Datas[i + 1]));
        }

        return res;
      }
    }

    /// <summary>
    /// Launch the "HINCRBY" command To increment the number stored in field in the hash key
    /// </summary>
    /// <param name="key">the key to manipulate</param>
    /// <param name="field">the field to increment</param>
    /// <param name="increment">the increment value (positive or negative)</param>
    /// <returns>the new value after operation</returns>
    public int HIncrBy(string key, string field, int increment)
    {
      RedisReponse answer = this.SendCommand("HINCRBY", key, field, increment.ToString());
      if (!answer.Success || double.IsNaN(answer.Number))
      {
        this.LastErrorText = answer.ErrorMessage;
        return 0;
      }
      else
      {
        // here we clear LastErrorText : to be sur to make the difference between : Exist =no and Exist raise an error
        this.LastErrorText = string.Empty;
        return Convert.ToInt32(answer.Number);
      }
    }

    /// <summary>
    /// Launch the "HINCRBYFLOAT" command To increment the number stored in field in the hash key
    /// </summary>
    /// <param name="key">the key to manipulate</param>
    /// <param name="field">the field to increment</param>
    /// <param name="increment">the increment value (positive or negative)</param>
    /// <returns>the new value after operation</returns>
    public double HIncrBYFloat(string key, string field, double increment)
    {
      if (this.GetServerVersion() >= SERVER26)
      {
        RedisReponse answer = this.SendCommand("HINCRBYFLOAT", key, field, RedisConnector.GetStringFromDouble(increment));
        if (!answer.Success)
        {
          this.LastErrorText = answer.ErrorMessage;
          return 0;
        }
        else
        {
          // here we clear LastErrorText : to be sur to make the difference between : Exist =no and Exist raise an error
          this.LastErrorText = string.Empty;
          return answer.Number;
        }
      }
      else
      {
        this.LastCommandText = string.Format("HINCRBYFLOAT {0} {1} {2} {3}", key, field, RedisConnector.GetStringFromDouble(increment), RedisConnector.NOTIMPLEMENTEDINTHISVERSION);
        this.LastErrorText = "Not implemented on this server version";
        return 0;
      }
    }

    /// <summary>
    /// Launch the "HKEYS" command To returns all field names in the hash stored at key
    /// </summary>
    /// <param name="key">the key to get</param>
    /// <returns>the fields and values list</returns>
    public List<string> HKeys(string key)
    {
      RedisReponse answer = this.SendCommand("HKEYS", key);
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
        return null;
      }
      else
      {
        // here we clear LastErrorText : to be sur to make the difference between : Exist =no and Exist raise an error
        this.LastErrorText = string.Empty;
        return answer.Datas;
      }
    }

    /// <summary>
    /// Launch the "HLEN" command To returns the number of fields contained in the hash stored at key
    /// </summary>
    /// <param name="key">the key to get</param>
    /// <returns>the number of fields in key</returns>
    public int HLen(string key)
    {
      RedisReponse answer = this.SendCommand("HLEN", key);
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
        return 0;
      }
      else if (double.IsNaN(answer.Number))
      {
        this.LastErrorText = CSTERRORNAN;
        return 0;
      }
      else
      {
        // here we clear LastErrorText : to be sur to make the difference between : Exist =no and Exist raise an error
        this.LastErrorText = string.Empty;
        return Convert.ToInt32(answer.Number);
      }
    }

    /// <summary>
    /// Launch the "HMGET" command To returns the values associated with the specified fields in the hash stored at key
    /// </summary>
    /// <param name="key">the key to get</param>
    /// <param name="field">the first field to get the value</param>
    /// <param name="fields">the other fields to get the value</param>
    /// <returns>the values list</returns>
    public List<string> HMGet(string key, string field, params string[] fields)
    {
      RedisReponse answer = this.SendCommand(RedisConnector.MergeStringBefore(RedisConnector.MergeStringBefore("HMGET", key, field), fields));
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
        return null;
      }
      else
      {
        // here we clear LastErrorText : to be sur to make the difference between : Exist =no and Exist raise an error
        this.LastErrorText = string.Empty;
        return answer.Datas;
      }
    }

    /// <summary>
    /// Launch the "HMSET" command To sets the specified fields to their respective values in the hash stored at key.
    /// </summary>
    /// <param name="key">the key to set</param>
    /// <param name="fieldValue">the first field value to set the value</param>
    /// <param name="fieldsValues">the other fields values to set</param>
    /// <returns>True if ok</returns>
    public bool HMSet(string key, Tuple<string, string> fieldValue, params Tuple<string, string>[] fieldsValues)
    {
      RedisReponse answer = this.SendCommand(RedisConnector.MergeStringBefore(RedisConnector.MergeStringBefore("HMSET", key), fieldValue, fieldsValues));
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
        return false;
      }
      else
      {
        // here we clear LastErrorText : to be sur to make the difference between : Exist =no and Exist raise an error
        this.LastErrorText = string.Empty;
        return answer.Success;
      }
    }

    /// <summary>
    /// Launch the "HMSET" command To sets the specified fields to their respective values in the hash stored at key.
    /// </summary>
    /// <param name="key">the key to set</param>
    /// <param name="fieldsValues">All fields values to set</param>
    /// <returns>True if ok</returns>
    public bool HMSet(string key, List<Tuple<string, string>> fieldsValues)
    {
      RedisReponse answer = this.SendCommand(RedisConnector.MergeStringBeforeList("HMSET", key, fieldsValues));
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
        return false;
      }
      else
      {
        // here we clear LastErrorText : to be sur to make the difference between : Exist =no and Exist raise an error
        this.LastErrorText = string.Empty;
        return answer.Success;
      }
    }

    /// <summary>
    /// Launch the "HSET" command To set specified field from hash key
    /// </summary>
    /// <param name="key">the key to update</param>
    /// <param name="field">the field to set</param>
    /// <param name="value">the value</param>
    /// <returns>true if ok</returns>
    public bool HSet(string key, string field, string value)
    {
      RedisReponse answer = this.SendCommand("HSET", key, field, value);
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
        return false;
      }
      else if (double.IsNaN(answer.Number))
      {
        this.LastErrorText = CSTERRORNAN;
        return false;
      }
      else
      {
        // here we clear LastErrorText : to be sur to make the difference between : Exist =no and Exist raise an error
        this.LastErrorText = string.Empty;
        return Convert.ToInt32(answer.Number) == 1;
      }
    }

    /// <summary>
    /// Launch the "HSETNX" command To set specified field from hash key only if field does not exists
    /// </summary>
    /// <param name="key">the key to update</param>
    /// <param name="field">the field to set</param>
    /// <param name="value">the value</param>
    /// <returns>true if ok</returns>
    public bool HSetNX(string key, string field, string value)
    {
      RedisReponse answer = this.SendCommand("HSETNX", key, field, value);
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
        return false;
      }
      else if (double.IsNaN(answer.Number))
      {
        this.LastErrorText = CSTERRORNAN;
        return false;
      }
      else
      {
        // here we clear LastErrorText : to be sur to make the difference between : Exist =no and Exist raise an error
        this.LastErrorText = string.Empty;
        return Convert.ToInt32(answer.Number) == 1;
      }
    }

    /// <summary>
    /// Launch the "HVALS" command To returns all value in the hash stored at key
    /// </summary>
    /// <param name="key">the key to get</param>
    /// <returns>the fields and values list</returns>
    public List<string> HVALS(string key)
    {
      RedisReponse answer = this.SendCommand("HVALS", key);
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
        return null;
      }
      else
      {
        // here we clear LastErrorText : to be sur to make the difference between : Exist =no and Exist raise an error
        this.LastErrorText = string.Empty;
        return answer.Datas;
      }
    }
    #endregion

    #region Lists Methods
    /// <summary>
    /// Launch the "BLPOP" command To removes and returns the first element of the lists stored at keys. If lists are empty BLPOP wait and block the client
    /// </summary>
    /// <param name="timeOut">The time in seconds to wait if all lists are empty</param>
    /// <param name="key">the key of the first list to pop</param>
    /// <param name="keys">the keys of other lists to pop</param>
    /// <returns>null if timeOut : Tuple(key, value) else</returns>
    public Tuple<string, string> BLPop(int timeOut, string key, params string[] keys)
    {
      return this.BPopInternal("BLPOP", timeOut, key, keys);
    }

    /// <summary>
    /// Launch the "BRPOP" command To removes and returns the last element of the lists stored at keys. If lists are empty BRPOP wait and block the client
    /// </summary>
    /// <param name="timeOut">The time in seconds to wait if all lists are empty</param>
    /// <param name="key">the key of the first list to pop</param>
    /// <param name="keys">the keys of other lists to pop</param>
    /// <returns>null if timeOut : Tuple(key, value) else</returns>
    public Tuple<string, string> BRPop(int timeOut, string key, params string[] keys)
    {
      return this.BPopInternal("BRPOP", timeOut, key, keys);
    }

    /// <summary>
    /// Launch the "BRPOPLPUSH" command To atomically returns and removes the last element (tail) of the list stored at source, 
    /// and pushes the element at the first element (head) of the list stored at destination. This operation block the client if there is no element in source
    /// </summary>
    /// <param name="source">the source list</param>
    /// <param name="destination">the destination list</param>
    /// <param name="timeOut">delay in seconds for waiting 0 = infinity</param>
    /// <returns>the element being popped and pushed</returns>
    public string BRPopLPush(string source, string destination, int timeOut)
    {
      RedisReponse answer = this.SendCommand("BRPOPLPUSH", source, destination, timeOut.ToString());
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
        return null;
      }
      else
      {
        // here we clear LastErrorText : to be sur to make the difference between : Exist =no and Exist raise an error
        this.LastErrorText = string.Empty;
        return answer.Line;
      }
    }

    /// <summary>
    /// Launch the "LINDEX" command To returns the element at index index in the list stored at key
    /// </summary>
    /// <param name="key">the key</param>
    /// <param name="index">the index to analyze</param>
    /// <returns>the value at the index</returns>
    public string LIndex(string key, int index)
    {
      RedisReponse answer = this.SendCommand("LINDEX", key, index.ToString());
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
        return null;
      }
      else
      {
        // here we clear LastErrorText : to be sur to make the difference between : Exist =no and Exist raise an error
        this.LastErrorText = string.Empty;
        return answer.Line;
      }
    }

    /// <summary>
    /// Launch the "LINSERT" command To inserts value in the list stored at key either before or after the reference value pivot.
    /// </summary>
    /// <param name="key">the key</param>
    /// <param name="after">the index to analyze</param>
    /// <param name="pivot">the value after or before we must insert value</param>
    /// <param name="value">the inserted value</param>
    /// <returns>the length of the list after the insert operation, or -1 when the value pivot was not found</returns>
    public int LInsert(string key, bool after, string pivot, string value)
    {
      RedisReponse answer = this.SendCommand("LINSERT", key, after ? "AFTER" : "BEFORE", pivot, value);
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
        return 0;
      }
      else if (double.IsNaN(answer.Number))
      {
        this.LastErrorText = CSTERRORNAN;
        return 0;
      }
      else
      {
        // here we clear LastErrorText : to be sur to make the difference between : Exist =no and Exist raise an error
        this.LastErrorText = string.Empty;
        return Convert.ToInt32(answer.Number);
      }
    }

    /// <summary>
    /// Launch the "LLEN" command To returns the length of the list stored at key
    /// </summary>
    /// <param name="key">the key to analyze</param>
    /// <returns>the length of the list</returns>
    public int LLen(string key)
    {
      RedisReponse answer = this.SendCommand("LLEN", key);
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
        return 0;
      }
      else if (double.IsNaN(answer.Number))
      {
        this.LastErrorText = CSTERRORNAN;
        return 0;
      }
      else
      {
        // here we clear LastErrorText : to be sur to make the difference between : Exist =no and Exist raise an error
        this.LastErrorText = string.Empty;
        return Convert.ToInt32(answer.Number);
      }
    }

    /// <summary>
    /// Launch the "LPOP" command To removes and returns the first element of the list stored at key
    /// </summary>
    /// <param name="key">the key to pop</param>
    /// <returns>the first value of the list (the value was removed)</returns>
    public string LPop(string key)
    {
      RedisReponse answer = this.SendCommand("LPOP", key);
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
        return null;
      }
      else
      {
        // here we clear LastErrorText : to be sur to make the difference between : Exist =no and Exist raise an error
        this.LastErrorText = string.Empty;
        return answer.Line;
      }
    }

    /// <summary>
    /// Launch the "LPUSH" command To insert all the specified values at the head of the list stored at key
    /// </summary>
    /// <param name="key">the key to populate</param>
    /// <param name="value">the first value to push into the list</param>
    /// <param name="values">other values to push into the list</param>
    /// <returns>the length of then list after the push operation</returns>
    public int LPush(string key, string value, params string[] values)
    {
      if (values.Length == 0 || this.GetServerVersion() > SERVER24)
      {
        RedisReponse answer = this.SendCommand(RedisConnector.MergeStringBefore(RedisConnector.MergeStringBefore("LPUSH", key, value), values));
        if (!answer.Success)
        {
          this.LastErrorText = answer.ErrorMessage;
          return 0;
        }
        else if (double.IsNaN(answer.Number))
        {
          this.LastErrorText = CSTERRORNAN;
          return 0;
        }
        else
        {
          // here we clear LastErrorText : to be sur to make the difference between : Exist =no and Exist raise an error
          this.LastErrorText = string.Empty;
          return Convert.ToInt32(answer.Number);
        }
      }
      else
      { // in older version : cascade call
        for (int n = values.Length - 1; n >= 0; n--)
        {
          this.LPush(key, values[n]);
        }

        int nb = this.LPush(key, value);
        this.LastCommandText = "LPUSH " + value + " " + values.Aggregate((x, y) => x + " " + y);
        return nb;
      }
    }

    /// <summary>
    /// Launch the "LPUSHX" command To Inserts value at the head of the list stored at key, only if key already exists and holds a list
    /// </summary>
    /// <param name="key">the key to populate</param>
    /// <param name="value">the value to push into the list</param>
    /// <returns>the length of then list after the push operation</returns>
    public int LPushX(string key, string value)
    {
      RedisReponse answer = this.SendCommand("LPUSHX", key, value);
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
        return 0;
      }
      else if (double.IsNaN(answer.Number))
      {
        this.LastErrorText = CSTERRORNAN;
        return 0;
      }
      else
      {
        // here we clear LastErrorText : to be sur to make the difference between : Exist =no and Exist raise an error
        this.LastErrorText = string.Empty;
        return Convert.ToInt32(answer.Number);
      }
    }

    /// <summary>
    /// Launch the "LRANGE" command To returns the specified elements of the list stored at key
    /// </summary>
    /// <param name="key">the key to populate</param>
    /// <param name="start">the start index</param>
    /// <param name="stop">the stop index</param>
    /// <returns>the element in the list between start and stop</returns>
    public List<string> LRange(string key, int start, int stop)
    {
      RedisReponse answer = this.SendCommand("LRANGE", key, start.ToString(), stop.ToString());
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
        return null;
      }
      else
      {
        // here we clear LastErrorText : to be sur to make the difference between : Exist =no and Exist raise an error
        this.LastErrorText = string.Empty;
        return answer.Datas;
      }
    }

    /// <summary>
    /// Launch the "LREM" command To removes the first count occurrences of elements equal to value from the list stored at key
    /// </summary>
    /// <param name="key">the key to populate</param>
    /// <param name="count">the number of removed element &gt;0 start from head to tail, &lt;0 start from tail to head, equal 0 remove all</param>
    /// <param name="value">the value to remove</param>
    /// <returns>the number of removed elements</returns>
    public int LRem(string key, int count, string value)
    {
      RedisReponse answer = this.SendCommand("LREM", key, count.ToString(), value);
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
        return 0;
      }
      else if (double.IsNaN(answer.Number))
      {
        this.LastErrorText = CSTERRORNAN;
        return 0;
      }
      else
      {
        // here we clear LastErrorText : to be sur to make the difference between : Exist =no and Exist raise an error
        this.LastErrorText = string.Empty;
        return Convert.ToInt32(answer.Number);
      }
    }

    /// <summary>
    /// Launch the "LSET" command To sets the list element at index to value
    /// </summary>
    /// <param name="key">the key to update</param>
    /// <param name="index">the index of the value to update</param>
    /// <param name="value">the new value</param>
    /// <returns>true if ok</returns>
    public bool LSet(string key, int index, string value)
    {
      RedisReponse answer = this.SendCommand("LSET", key, index.ToString(), value);
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
        return false;
      }
      else if (string.IsNullOrWhiteSpace(answer.Line))
      {
        this.LastErrorText = string.Empty;
        return false;
      }
      else
      {
        // here we clear LastErrorText : to be sur to make the difference between : Exist =no and Exist raise an error
        this.LastErrorText = string.Empty;
        return answer.Line.ToUpper() == "OK";
      }
    }

    /// <summary>
    /// Launch the "LTRIM" command To trim an existing list so that it will contain only the specified range of elements specified
    /// </summary>
    /// <param name="key">the key to trim</param>
    /// <param name="start">the starting index</param>
    /// <param name="stop">the last index</param>
    /// <returns>true if ok</returns>
    public bool LTrim(string key, int start, int stop)
    {
      RedisReponse answer = this.SendCommand("LTRIM", key, start.ToString(), stop.ToString());
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
        return false;
      }
      else if (string.IsNullOrWhiteSpace(answer.Line))
      {
        this.LastErrorText = string.Empty;
        return false;
      }
      else
      {
        // here we clear LastErrorText : to be sur to make the difference between : Exist =no and Exist raise an error
        this.LastErrorText = string.Empty;
        return answer.Line.ToUpper() == "OK";
      }
    }

    /// <summary>
    /// Launch the "RPOP" command To removes and returns the last element of the list stored at key
    /// </summary>
    /// <param name="key">the key to pop</param>
    /// <returns>the first value of the list (the value was removed)</returns>
    public string RPop(string key)
    {
      RedisReponse answer = this.SendCommand("RPOP", key);
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
        return null;
      }
      else
      {
        // here we clear LastErrorText : to be sur to make the difference between : Exist =no and Exist raise an error
        this.LastErrorText = string.Empty;
        return answer.Line;
      }
    }

    /// <summary>
    /// Launch the "RPOPLPUSH" command To atomically returns and removes the last element (tail) of the list stored at source, 
    /// and pushes the element at the first element (head) of the list stored at destination
    /// </summary>
    /// <param name="source">the source list</param>
    /// <param name="destination">the destination list</param>
    /// <returns>the element being popped and pushed</returns>
    public string RPopLPush(string source, string destination)
    {
      RedisReponse answer = this.SendCommand("RPOPLPUSH", source, destination);
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
        return null;
      }
      else
      {
        // here we clear LastErrorText : to be sur to make the difference between : Exist =no and Exist raise an error
        this.LastErrorText = string.Empty;
        return answer.Line;
      }
    }

    /// <summary>
    /// Launch the "RPUSH" command To insert all the specified values at the tail of the list stored at key
    /// </summary>
    /// <param name="key">the key to populate</param>
    /// <param name="value">the first value to push into the list</param>
    /// <param name="values">other values to push into the list</param>
    /// <returns>the length of then list after the push operation</returns>
    public int RPush(string key, string value, params string[] values)
    {
      if (values.Length == 0 || this.GetServerVersion() > SERVER24)
      {
        RedisReponse answer = this.SendCommand(RedisConnector.MergeStringBefore(RedisConnector.MergeStringBefore("RPUSH", key, value), values));
        if (!answer.Success)
        {
          this.LastErrorText = answer.ErrorMessage;
          return 0;
        }
        else if (double.IsNaN(answer.Number))
        {
          this.LastErrorText = CSTERRORNAN;
          return 0;
        }
        else
        {
          // here we clear LastErrorText : to be sur to make the difference between : Exist =no and Exist raise an error
          this.LastErrorText = string.Empty;
          return Convert.ToInt32(answer.Number);
        }
      }
      else
      { // in older version : cascade call
        for (int n = values.Length - 1; n >= 0; n--)
        {
          this.RPush(key, values[n]);
        }

        int nb = this.RPush(key, value);
        this.LastCommandText = "RPUSH " + value + " " + values.Aggregate((x, y) => x + " " + y);
        return nb;
      }
    }

    /// <summary>
    /// Launch the "RPUSHX" command To Inserts value at the tail of the list stored at key, only if key already exists and holds a list
    /// </summary>
    /// <param name="key">the key to populate</param>
    /// <param name="value">the value to push into the list</param>
    /// <returns>the length of then list after the push operation</returns>
    public int RPushX(string key, string value)
    {
      RedisReponse answer = this.SendCommand("RPUSHX", key, value);
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
        return 0;
      }
      else if (double.IsNaN(answer.Number))
      {
        this.LastErrorText = CSTERRORNAN;
        return 0;
      }
      else
      {
        // here we clear LastErrorText : to be sur to make the difference between : Exist =no and Exist raise an error
        this.LastErrorText = string.Empty;
        return Convert.ToInt32(answer.Number);
      }
    }
    #endregion

    #region Sets Methods
    /// <summary>
    /// Launch the "SADD" command To add a member to a SET
    /// </summary>
    /// <param name="key">the key to update</param>
    /// <param name="member">the first text to add</param>
    /// <param name="members">the other text to add</param>
    /// <returns>Indicate if value is added; When Not added check LastError</returns>
    public int SAdd(string key, string member, params string[] members)
    {
      RedisReponse answer = this.SendCommand(RedisConnector.MergeStringBefore(RedisConnector.MergeStringBefore("SADD", key, member), members));
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
        return 0;
      }
      else if (double.IsNaN(answer.Number))
      {
        this.LastErrorText = CSTERRORNAN;
        return 0;
      }
      else
      {
        // here we clear LastErrorText : to be sur to make the difference between : Exist =no and Exist raise an error
        this.LastErrorText = string.Empty;
        return Convert.ToInt32(answer.Number);
      }
    }

    /// <summary>
    /// Launch the "SCARD" command To get the number of a SET key
    /// </summary>
    /// <param name="key">the key to evaluate</param>
    /// <returns>number of fields</returns>
    public int SCard(string key)
    {
      RedisReponse answer = this.SendCommand("SCARD", key);
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
        return 0;
      }
      else if (double.IsNaN(answer.Number))
      {
        this.LastErrorText = CSTERRORNAN;
        return 0;
      }
      else
      {
        // here we clear LastErrorText : to be sur to make the difference between : Exist =no and Exist raise an error
        this.LastErrorText = string.Empty;
        return Convert.ToInt32(answer.Number);
      }
    }

    /// <summary>
    /// Launch the "SDIFF" command To get the member ok key who are not in the different keys
    /// </summary>
    /// <param name="key">the key source</param>
    /// <param name="keys">the other keys to diff</param>
    /// <returns>the members of key who are not found in keys</returns>
    public List<string> SDiff(string key, params string[] keys)
    {
      RedisReponse answer = this.SendCommand(RedisConnector.MergeStringBefore("SDIFF", RedisConnector.MergeStringBefore(key, keys)));
      if (!answer.Success || answer.Datas == null)
      {
        this.LastErrorText = answer.ErrorMessage;
        return null;
      }
      else
      {
        return answer.Datas;
      }
    }

    /// <summary>
    /// Launch the "SDIFFSTORE" command To get the member ok key who are not in the different keys and store into destination
    /// </summary>
    /// <param name="destination">the destination key</param>
    /// <param name="key">the key source</param>
    /// <param name="keys">the other keys to diff</param>
    /// <returns>number of fields</returns>
    public int SDiffStore(string destination, string key, params string[] keys)
    {
      RedisReponse answer = this.SendCommand(RedisConnector.MergeStringBefore(RedisConnector.MergeStringBefore("SDIFFSTORE", destination, key), keys));
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
        return 0;
      }
      else if (double.IsNaN(answer.Number))
      {
        this.LastErrorText = CSTERRORNAN;
        return 0;
      }
      else
      {
        // here we clear LastErrorText : to be sur to make the difference between : Exist =no and Exist raise an error
        this.LastErrorText = string.Empty;
        return Convert.ToInt32(answer.Number);
      }
    }

    /// <summary>
    /// Launch the "SINTER" command To get the members who are in all sets
    /// </summary>
    /// <param name="key">the key source</param>
    /// <param name="keys">the other keys to diff</param>
    /// <returns>the members of key who are in the intersection of all the given keys</returns>
    public List<string> SInter(string key, params string[] keys)
    {
      RedisReponse answer = this.SendCommand(RedisConnector.MergeStringBefore("SINTER", RedisConnector.MergeStringBefore(key, keys)));
      if (!answer.Success || answer.Datas == null)
      {
        this.LastErrorText = answer.ErrorMessage;
        return null;
      }
      else
      {
        return answer.Datas;
      }
    }

    /// <summary>
    /// Launch the "SINTERSTORE" command To get the members who are in all sets and store into destination
    /// </summary>
    /// <param name="destination">the destination key</param>
    /// <param name="key">the key source</param>
    /// <param name="keys">the other keys to diff</param>
    /// <returns>the number of field intersection</returns>
    public int SInterStore(string destination, string key, params string[] keys)
    {
      RedisReponse answer = this.SendCommand(RedisConnector.MergeStringBefore(RedisConnector.MergeStringBefore("SINTERSTORE", destination, key), keys));
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
        return 0;
      }
      else if (double.IsNaN(answer.Number))
      {
        this.LastErrorText = CSTERRORNAN;
        return 0;
      }
      else
      {
        // here we clear LastErrorText : to be sur to make the difference between : Exist =no and Exist raise an error
        this.LastErrorText = string.Empty;
        return Convert.ToInt32(answer.Number);
      }
    }

    /// <summary>
    /// Launch the "SISMEMBER" command To know if member is in the set key
    /// </summary>
    /// <param name="key">the key source</param>
    /// <param name="member">the member to search</param>
    /// <returns>true if it is</returns>
    public bool SIsMember(string key, string member)
    {
      RedisReponse answer = this.SendCommand("SISMEMBER", key, member);
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
        return false;
      }
      else if (double.IsNaN(answer.Number))
      {
        this.LastErrorText = CSTERRORNAN;
        return false;
      }
      else
      {
        this.LastErrorText = string.Empty;
        return Convert.ToInt32(answer.Number) == 1;
      }
    }

    /// <summary>
    /// Launch the "SMEMBERS" command To get the members of a sets
    /// </summary>
    /// <param name="key">the key to get members</param>
    /// <returns>the members of key</returns>
    public List<string> SMembers(string key)
    {
      RedisReponse answer = this.SendCommand("SMEMBERS", key);
      if (!answer.Success || answer.Datas == null)
      {
        this.LastErrorText = answer.ErrorMessage;
        return null;
      }
      else
      {
        return answer.Datas;
      }
    }

    /// <summary>
    /// Launch the "SMOVE" command To move member from the set source to the set destination
    /// </summary>
    /// <param name="source">the key source</param>
    /// <param name="destination">the key destination</param>
    /// <param name="member">the member to move</param>
    /// <returns>true if ok</returns>
    public bool SMove(string source, string destination, string member)
    {
      RedisReponse answer = this.SendCommand("SMOVE", source, destination, member);
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
        return false;
      }
      else if (double.IsNaN(answer.Number))
      {
        this.LastErrorText = CSTERRORNAN;
        return false;
      }
      else
      {
        this.LastErrorText = string.Empty;
        return Convert.ToInt32(answer.Number) == 1;
      }
    }

    /// <summary>
    /// Launch the "SPOP" command To remove and get a member of a set key
    /// </summary>
    /// <param name="key">the key</param>
    /// <returns>the popped member</returns>
    public string SPop(string key)
    {
      RedisReponse answer = this.SendCommand("SPOP", key);
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
        return null;
      }
      else
      {
        this.LastErrorText = string.Empty;
        return answer.Line;
      }
    }

    /// <summary>
    /// Launch the "SRANDMEMBER" command To get a member of a set key 
    /// </summary>
    /// <param name="key">the key</param>
    /// <param name="count">If Greater then 0 then return count elements, if lower than 0 return the same element count times (2.6) before 2.6 must be 0</param>
    /// <returns>the members</returns>
    public List<string> SRandMember(string key, int count)
    {
      RedisReponse answer;
      if (count != 0 && this.GetServerVersion() >= SERVER26)
      { // 2.6 version with count
        answer = this.SendCommand("SRANDMEMBER", key, count.ToString());
        if (!answer.Success || answer.Datas == null)
        {
          this.LastErrorText = answer.ErrorMessage;
          return null;
        }
        else
        {
          this.LastErrorText = string.Empty;
          return answer.Datas;
        }
      }
      else
      { // older than 2.6 or without count
        int n = 0;
        int fin = count < 0 ? 0 : count;
        List<string> res = new List<string>();
        do
        {
          answer = this.SendCommand("SRANDMEMBER", key);
          if (!answer.Success)
          {
            this.LastErrorText = answer.ErrorMessage;
            return null;
          }
          else
          {
            res.Add(answer.Line);
          }

          n++;
        }
        while (n < fin);

        if (count < -1)
        {
          while (res.Count < Math.Abs(count))
          {
            res.Add(res[0]);
          }
        }

        this.LastErrorText = string.Empty;
        return res;
      }
    }

    /// <summary>
    /// Launch the "SREM" command To remove the members of the set key
    /// </summary>
    /// <param name="key">the key source</param>
    /// <param name="members">the members to remove</param>
    /// <returns>number of removed members</returns>
    public int SRem(string key, params string[] members)
    {
      RedisReponse answer = this.SendCommand(RedisConnector.MergeStringBefore("SREM", RedisConnector.MergeStringBefore(key, members)));
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
        return 0;
      }
      else if (double.IsNaN(answer.Number))
      {
        this.LastErrorText = CSTERRORNAN;
        return 0;
      }
      else
      {
        // here we clear LastErrorText : to be sur to make the difference between : Exist =no and Exist raise an error
        this.LastErrorText = string.Empty;
        return Convert.ToInt32(answer.Number);
      }
    }

    /// <summary>
    /// Launch the "SUNION" command To get the union of all members of sets
    /// </summary>
    /// <param name="key">the key source</param>
    /// <param name="keys">the other keys to diff</param>
    /// <returns>the members of the union</returns>
    public List<string> SUnion(string key, params string[] keys)
    {
      RedisReponse answer = this.SendCommand(RedisConnector.MergeStringBefore("SUNION", RedisConnector.MergeStringBefore(key, keys)));
      if (!answer.Success || answer.Datas == null)
      {
        this.LastErrorText = answer.ErrorMessage;
        return null;
      }
      else
      {
        return answer.Datas;
      }
    }

    /// <summary>
    /// Launch the "SUNIONSTORE" command To get the union of all members of sets and store into destination
    /// </summary>
    /// <param name="destination">the destination key</param>
    /// <param name="key">the key source</param>
    /// <param name="keys">the other keys to diff</param>
    /// <returns>the number of field intersection</returns>
    public int SUnionStore(string destination, string key, params string[] keys)
    {
      RedisReponse answer = this.SendCommand(RedisConnector.MergeStringBefore(RedisConnector.MergeStringBefore("SUNIONSTORE", destination, key), keys));
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
        return 0;
      }
      else if (double.IsNaN(answer.Number))
      {
        this.LastErrorText = CSTERRORNAN;
        return 0;
      }
      else
      {
        // here we clear LastErrorText : to be sur to make the difference between : Exist =no and Exist raise an error
        this.LastErrorText = string.Empty;
        return Convert.ToInt32(answer.Number);
      }
    }

    #endregion

    #region Sorted Sets Methods
    /// <summary>
    /// Launch the "ZADD" command To add a member to a sorted SET
    /// </summary>
    /// <param name="key">the key to update</param>
    /// <param name="member">the first member to add to add</param>
    /// <param name="members">the other members to add to add</param>
    /// <returns>Indicate the number of sorted set added (updated sortedSet are not in the total</returns>
    public int ZAdd(string key, SortedSet member, params SortedSet[] members)
    {
      RedisReponse answer = this.SendCommand(RedisConnector.MergeStringBefore(RedisConnector.MergeStringBefore("ZADD", key), member, members));
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
        return 0;
      }
      else if (double.IsNaN(answer.Number))
      {
        this.LastErrorText = CSTERRORNAN;
        return 0;
      }
      else
      {
        // here we clear LastErrorText : to be sur to make the difference between : Exist =no and Exist raise an error
        this.LastErrorText = string.Empty;
        return Convert.ToInt32(answer.Number);
      }
    }

    /// <summary>
    /// Launch the "ZCARD" command To returns the sorted set cardinality (number of elements) of the sorted set stored at key
    /// </summary>
    /// <param name="key">the key to count</param>
    /// <returns>the number of sorted set</returns>
    public int ZCard(string key)
    {
      RedisReponse answer = this.SendCommand("ZCARD", key);
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
        return 0;
      }
      else if (double.IsNaN(answer.Number))
      {
        this.LastErrorText = CSTERRORNAN;
        return 0;
      }
      else
      {
        // here we clear LastErrorText : to be sur to make the difference between : Exist =no and Exist raise an error
        this.LastErrorText = string.Empty;
        return Convert.ToInt32(answer.Number);
      }
    }

    /// <summary>
    /// Launch the "ZCOUNT" command To returns the number of elements in the sorted set at key with a score between min and max.
    /// </summary>
    /// <param name="key">the key to count</param>
    /// <param name="min">the min score to begin counting</param>
    /// <param name="max">the max score to stop counting</param>
    /// <returns>the number of sorted set</returns>
    public int ZCount(string key, double min, double max)
    {
      RedisReponse answer = this.SendCommand("ZCOUNT", key, RedisConnector.GetStringFromDouble(min), RedisConnector.GetStringFromDouble(max));
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
        return 0;
      }
      else if (double.IsNaN(answer.Number))
      {
        this.LastErrorText = CSTERRORNAN;
        return 0;
      }
      else
      {
        // here we clear LastErrorText : to be sur to make the difference between : Exist =no and Exist raise an error
        this.LastErrorText = string.Empty;
        return Convert.ToInt32(answer.Number);
      }
    }

    /// <summary>
    /// Launch the "ZINCRBY" command To increments the score of member in the sorted set stored at key by increment
    /// </summary>
    /// <param name="key">the key to update</param>
    /// <param name="increment">the value to increment</param>
    /// <param name="member">the first member to add to add</param>
    /// <returns>the new value</returns>
    public double ZIncrBy(string key, double increment, string member)
    {
      RedisReponse answer = this.SendCommand("ZINCRBY", key, RedisConnector.GetStringFromDouble(increment), member);
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
        return double.NaN;
      }
      else
      {
        // here we clear LastErrorText : to be sur to make the difference between : Exist =no and Exist raise an error
        this.LastErrorText = string.Empty;
        if (!string.IsNullOrWhiteSpace(answer.Line))
        {
          double nb = RedisConnector.GetDoubleFromString(answer.Line, double.NaN);
          if (!nb.Equals(double.NaN))
          {
            this.LastErrorText = string.Empty;
            return nb;
          }
        }

        this.LastErrorText = "Invalide response format";
        return double.NaN;
      }
    }

    /// <summary>
    /// Launch the "ZINTERSTORE" command To computes the intersection of NUMKEYS sorted sets given by the specified keys, and stores the result in destination
    /// </summary>
    /// <param name="destination">the key to update or create as the result of the operation</param>
    /// <param name="keys">the list of keys </param>
    /// <param name="aggregate">the aggregate operation</param>
    /// <returns>the number of values in the result set</returns>
    public int ZInterStore(string destination, List<string> keys, ZAggregate aggregate)
    {
      return this.ZOperationStoreInternal("ZINTERSTORE", destination, keys, aggregate);
    }

    /// <summary>
    /// Launch the "ZINTERSTORE" command To computes the intersection of NUMKEYS sorted sets given by the specified keys, and stores the result in destination
    /// </summary>
    /// <param name="destination">the key to update or create as the result of the operation</param>
    /// <param name="keys">the list of key with their weighting</param>
    /// <param name="aggregate">the aggregate operation</param>
    /// <returns>the number of values in the result set</returns>
    public int ZInterStore(string destination, List<Tuple<string, double>> keys, ZAggregate aggregate)
    {
      return this.ZOperationStoreInternal("ZINTERSTORE", destination, keys, aggregate);
    }

    /// <summary>
    /// Launch the "ZRANGE" command To returns the specified range of elements in the sorted (lowest to highest) set stored at key
    /// </summary>
    /// <param name="key">the key to analyze</param>
    /// <param name="start">the starting index to return (zero based)</param>
    /// <param name="stop">the last index to return (zero based)</param>
    /// <param name="withScores">return score or not</param>
    /// <returns>the values</returns>
    public List<SortedSet> ZRange(string key, int start, int stop, bool withScores)
    {
      return this.ZRangeInternal("ZRANGE", key, start, stop, withScores);
    }

    /// <summary>
    /// Launch the "ZRANGEBYSCORE" command To returns all the elements in the sorted set at 
    /// key with a score between min and max (including elements with score equal to min or max).
    /// </summary>
    /// <param name="key">the key to analyze</param>
    /// <param name="min">the min score to return</param>
    /// <param name="max">the max score to return</param>
    /// <param name="withScores">return score or not</param>
    /// <returns>the values</returns>
    public List<SortedSet> ZRangeByScore(string key, double min, double max, bool withScores)
    {
      return this.ZRangeByScore(key, min, max, withScores, true, true, null);
    }

    /// <summary>
    /// Launch the "ZRANGEBYSCORE" command To returns all the elements in the sorted set at 
    /// key with a score between min and max (including elements with score equal to min or max).
    /// </summary>
    /// <param name="key">the key to analyze</param>
    /// <param name="min">the min score to return</param>
    /// <param name="max">the max score to return</param>
    /// <param name="withScores">return score or not</param>
    /// <param name="minIncluded">indicate if the min score is included (by default or excluded)</param>
    /// <param name="maxIncluded">indicate if the max score is included (by default or excluded)</param>
    /// <returns>the values</returns>
    public List<SortedSet> ZRangeByScore(string key, double min, double max, bool withScores, bool minIncluded, bool maxIncluded)
    {
      return this.ZRangeByScore(key, min, max, withScores, minIncluded, maxIncluded, null);
    }

    /// <summary>
    /// Launch the "ZRANGEBYSCORE" command To returns all the elements in the sorted set at 
    /// key with a score between min and max (including elements with score equal to min or max).
    /// </summary>
    /// <param name="key">the key to analyze</param>
    /// <param name="min">the min score to return</param>
    /// <param name="max">the max score to return</param>
    /// <param name="withScores">return score or not</param>
    /// <param name="minIncluded">indicate if the min score is included (by default or excluded)</param>
    /// <param name="maxIncluded">indicate if the max score is included (by default or excluded)</param>
    /// <param name="limitArg">Get a Limit offset and count if needed</param>
    /// <returns>the values</returns>
    public List<SortedSet> ZRangeByScore(string key, double min, double max, bool withScores, bool minIncluded, bool maxIncluded, Limit limitArg)
    {
      return this.ZRangeByScoreInternal("ZRANGEBYSCORE", key, min, max, withScores, minIncluded, maxIncluded, limitArg);
    }

    /// <summary>
    /// Launch the "ZRANK" command To returns the rank of member in the sorted set stored at key, with the scores ordered from low to high
    /// </summary>
    /// <param name="key">the key to analyze</param>
    /// <param name="member">the member to compute</param>
    /// <returns>the index of the member in the z set (-1 if not found or error)</returns>
    public int ZRank(string key, string member)
    {
      return this.ZRankInternal("ZRANK", key, member);
    }

    /// <summary>
    /// Launch the "ZREM" command To removes the specified members from the sorted set stored at key
    /// </summary>
    /// <param name="key">the key to analyze</param>
    /// <param name="member">the first member to remove</param>
    /// <param name="members">Other members to remove</param>
    /// <returns>the number of removed members</returns>
    public int ZRem(string key, string member, params string[] members)
    {
      RedisReponse answer = this.SendCommand(RedisConnector.MergeStringBefore(RedisConnector.MergeStringBefore("ZREM", key, member), members));
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
        return -1;
      }
      else if (double.IsNaN(answer.Number))
      {
        this.LastErrorText = CSTERRORNAN;
        return -1;
      }
      else
      {
        // here we clear LastErrorText : to be sur to make the difference between : Exist =no and Exist raise an error
        this.LastErrorText = string.Empty;
        return Convert.ToInt32(answer.Number);
      }
    }

    /// <summary>
    /// Launch the "ZREMRANGEBYRANK" command To removes all elements in the sorted set stored at key with rank between start and stop
    /// </summary>
    /// <param name="key">the key to remove members</param>
    /// <param name="start">the first member index to remove</param>
    /// <param name="stop">the last member index to remove</param>
    /// <returns>the number of removed members</returns>
    public int ZRemRangeByRank(string key, int start, int stop)
    {
      RedisReponse answer = this.SendCommand("ZREMRANGEBYRANK", key, start.ToString(), stop.ToString());
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
        return -1;
      }
      else if (double.IsNaN(answer.Number))
      {
        this.LastErrorText = CSTERRORNAN;
        return -1;
      }
      else
      {
        // here we clear LastErrorText : to be sur to make the difference between : Exist =no and Exist raise an error
        this.LastErrorText = string.Empty;
        return Convert.ToInt32(answer.Number);
      }
    }

    /// <summary>
    /// Launch the "ZREMRANGEBYSCORE" command To removes all elements in the sorted set stored at key with rank between start and stop
    /// </summary>
    /// <param name="key">the key to remove members</param>
    /// <param name="min">the first member score to remove</param>
    /// <param name="max">the last member score to remove</param>
    /// <returns>the number of removed members</returns>
    public int ZRemRangeByScore(string key, double min, double max)
    {
      return this.ZRemRangeByScore(key, min, max, true, true);
    }

    /// <summary>
    /// Launch the "ZREMRANGEBYSCORE" command To removes all elements in the sorted set stored at key with rank between start and stop
    /// </summary>
    /// <param name="key">the key to remove members</param>
    /// <param name="min">the first member score to remove</param>
    /// <param name="max">the last member score to remove</param>
    /// <param name="minIncluded">indicate if the min score is included (by default or excluded)</param>
    /// <param name="maxIncluded">indicate if the max score is included (by default or excluded)</param>
    /// <returns>the number of removed members</returns>
    public int ZRemRangeByScore(string key, double min, double max, bool minIncluded, bool maxIncluded)
    {
      RedisReponse answer = this.SendCommand(
        "ZREMRANGEBYSCORE",
        key,
        (minIncluded ? string.Empty : "(") + RedisConnector.GetStringFromDouble(min),
        (maxIncluded ? string.Empty : "(") + RedisConnector.GetStringFromDouble(max));
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
        return -1;
      }
      else if (double.IsNaN(answer.Number))
      {
        this.LastErrorText = CSTERRORNAN;
        return -1;
      }
      else
      {
        // here we clear LastErrorText : to be sur to make the difference between : Exist =no and Exist raise an error
        this.LastErrorText = string.Empty;
        return Convert.ToInt32(answer.Number);
      }
    }

    /// <summary>
    /// Launch the "ZREVRANGE" command To returns the specified range of elements in the sorted (highest to lowest) set stored at key
    /// </summary>
    /// <param name="key">the key to analyze</param>
    /// <param name="start">the starting index to return (zero based)</param>
    /// <param name="stop">the last index to return (zero based)</param>
    /// <param name="withScores">return score or not</param>
    /// <returns>the values</returns>
    public List<SortedSet> ZRevRange(string key, int start, int stop, bool withScores)
    {
      return this.ZRangeInternal("ZREVRANGE", key, start, stop, withScores);
    }

    /// <summary>
    /// Launch the "ZREVRANGEBYSCORE" command To returns all the elements in the sorted set at 
    /// key with a score between min and max (including elements with score equal to min or max).
    /// </summary>
    /// <param name="key">the key to analyze</param>
    /// <param name="min">the min score to return</param>
    /// <param name="max">the max score to return</param>
    /// <param name="withScores">return score or not</param>
    /// <returns>the values</returns>
    public List<SortedSet> ZRevRangeByScore(string key, double min, double max, bool withScores)
    {
      return this.ZRevRangeByScore(key, min, max, withScores, true, true, null);
    }

    /// <summary>
    /// Launch the "ZREVRANGEBYSCORE" command To returns all the elements in the sorted set at 
    /// key with a score between min and max (including elements with score equal to min or max).
    /// </summary>
    /// <param name="key">the key to analyze</param>
    /// <param name="min">the min score to return</param>
    /// <param name="max">the max score to return</param>
    /// <param name="withScores">return score or not</param>
    /// <param name="minIncluded">indicate if the min score is included (by default or excluded)</param>
    /// <param name="maxIncluded">indicate if the max score is included (by default or excluded)</param>
    /// <returns>the values</returns>
    public List<SortedSet> ZRevRangeByScore(string key, double min, double max, bool withScores, bool minIncluded, bool maxIncluded)
    {
      return this.ZRevRangeByScore(key, min, max, withScores, minIncluded, maxIncluded, null);
    }

    /// <summary>
    /// Launch the "ZREVRANGEBYSCORE" command To returns all the elements in the sorted set at 
    /// key with a score between min and max (including elements with score equal to min or max).
    /// </summary>
    /// <param name="key">the key to analyze</param>
    /// <param name="min">the min score to return</param>
    /// <param name="max">the max score to return</param>
    /// <param name="withScores">return score or not</param>
    /// <param name="minIncluded">indicate if the min score is included (by default or excluded)</param>
    /// <param name="maxIncluded">indicate if the max score is included (by default or excluded)</param>
    /// <param name="limitArg">Get a Limit offset and count if needed</param>
    /// <returns>the values</returns>
    public List<SortedSet> ZRevRangeByScore(string key, double min, double max, bool withScores, bool minIncluded, bool maxIncluded, Limit limitArg)
    {
      return this.ZRangeByScoreInternal("ZREVRANGEBYSCORE", key, min, max, withScores, minIncluded, maxIncluded, limitArg);
    }

    /// <summary>
    /// Launch the "ZREVRANK" command To returns the rank of member in the sorted set stored at key, with the scores ordered from high to low
    /// </summary>
    /// <param name="key">the key to analyze</param>
    /// <param name="member">the member to compute</param>
    /// <returns>the index of the member in the z set (-1 if not found or error)</returns>
    public int ZRevRank(string key, string member)
    {
      return this.ZRankInternal("ZREVRANK", key, member);
    }

    /// <summary>
    /// Launch the "ZSCORE" command To returns the score of member in the sorted set at key.
    /// </summary>
    /// <param name="key">the key to compute</param>
    /// <param name="member">the member to get the score</param>
    /// <returns>the score of the member</returns>
    public double ZScore(string key, string member)
    {
      RedisReponse answer = this.SendCommand("ZSCORE", key, member);
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
        return double.NaN;
      }
      else
      {
        // here we clear LastErrorText : to be sur to make the difference between : Exist =no and Exist raise an error
        this.LastErrorText = string.Empty;
        if (!string.IsNullOrWhiteSpace(answer.Line))
        {
          double nb = RedisConnector.GetDoubleFromString(answer.Line, double.NaN);
          if (!nb.Equals(double.NaN))
          {
            this.LastErrorText = string.Empty;
            return nb;
          }
        }

        this.LastErrorText = "Invalide response format";
        return double.NaN;
      }
    }

    /// <summary>
    /// Launch the "ZUNIONSTORE" command To computes the union of NUMKEYS sorted sets given by the specified keys, and stores the result in destination
    /// </summary>
    /// <param name="destination">the key to update or create as the result of the operation</param>
    /// <param name="keys">the list of key with their weighting</param>
    /// <param name="aggregate">the aggregate operation</param>
    /// <returns>the number of values in the result set</returns>
    public int ZUnionStore(string destination, List<string> keys, ZAggregate aggregate)
    {
      return this.ZOperationStoreInternal("ZUNIONSTORE", destination, keys, aggregate);
    }

    /// <summary>
    /// Launch the "ZUNIONSTORE" command To computes the union of NUMKEYS sorted sets given by the specified keys, and stores the result in destination
    /// </summary>
    /// <param name="destination">the key to update or create as the result of the operation</param>
    /// <param name="keys">the list of key with their weighting</param>
    /// <param name="aggregate">the aggregate operation</param>
    /// <returns>the number of values in the result set</returns>
    public int ZUnionStore(string destination, List<Tuple<string, double>> keys, ZAggregate aggregate)
    {
      return this.ZOperationStoreInternal("ZUNIONSTORE", destination, keys, aggregate);
    }
    #endregion

    #region Pub/Sub Methods
    /// <summary>
    /// Launch the "PSUBSCRIBE" command : to subscribe to different selected channels by pattern
    /// </summary>
    /// <param name="fct">Call back method to receive data</param>
    /// <param name="patterns">List of pattern to subscribe</param>
    /// <returns>True if PSubscribe ok</returns>
    public bool PSubscribe(EventSubscribeHandeler fct, params string[] patterns)
    {
      if (fct == null && this.processSubscribeCallBack == null)
      {
        this.LastErrorText = "Invalid argument No handler";
        return false;
      }
      else
      {
        if (!this.SendACommand(RedisConnector.MergeStringBefore("PSUBSCRIBE", patterns)))
        {
          // when SendACommand fail, this.LastErrorText is set No need to set here 
          return false;
        }
        else
        {
          if (this.processSubscribeCallBack == null)
          { // first Time
            this.processSubscribeCallBack = new AsyncCallback(this.ProcessPublish);
            PublishAsyncParam param = new PublishAsyncParam(this.bufferStream, fct);
            this.bufferStream.BeginRead(param.Buffer, 0, param.Buffer.Length, this.processSubscribeCallBack, param);
          }

          return true;
        }
      }
    }

    /// <summary>
    /// Launch the "PUBLISH" command To publish a message on a chanel
    /// </summary>
    /// <param name="channel">The channel where publish the message</param>
    /// <param name="message">The published message</param>
    /// <returns>the number of clients that received the message</returns>
    public int Publish(string channel, string message)
    {
      RedisReponse answer = this.SendCommand("PUBLISH", channel, message);
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
        return -1;
      }
      else if (double.IsNaN(answer.Number))
      {
        this.LastErrorText = CSTERRORNAN;
        return -1;
      }
      else
      {
        return Convert.ToInt32(answer.Number);
      }
    }

    /// <summary>
    /// Launch the "PUNSUBSCRIBE" command : to unsubscribe to different selected channels by pattern
    /// </summary>
    /// <param name="patterns">List of pattern to subscribe</param>
    /// <returns>True if UnSubscribe command correct</returns>
    public bool PUnSubscribe(params string[] patterns)
    {
      return this.SendACommand(RedisConnector.MergeStringBefore("PUNSUBSCRIBE", patterns));
    }

    /// <summary>
    /// Launch the "SUBSCRIBE" command : to subscribe to different channels
    /// </summary>
    /// <param name="fct">Call back method to receive data</param>
    /// <param name="chanels">List of channel to subscribe</param>
    /// <returns>True if Subscribe ok</returns>
    public bool Subscribe(EventSubscribeHandeler fct, params string[] chanels)
    {
      if (fct == null && this.processSubscribeCallBack == null)
      {
        this.LastErrorText = "Invalid argument No handler";
        return false;
      }
      else
      {
        if (!this.SendACommand(RedisConnector.MergeStringBefore("SUBSCRIBE", chanels)))
        {
          // when SendACommand fail, this.LastErrorText is set No need to set here 
          return false;
        }
        else
        {
          if (this.processSubscribeCallBack == null)
          { // first Time
            this.processSubscribeCallBack = new AsyncCallback(this.ProcessPublish);
            PublishAsyncParam param = new PublishAsyncParam(this.bufferStream, fct);
            this.bufferStream.BeginRead(param.Buffer, 0, param.Buffer.Length, this.processSubscribeCallBack, param);
          }

          return true;
        }
      }
    }

    /// <summary>
    /// Launch the "UNSUBSCRIBE" command : to unsubscribe to different channels
    /// </summary>
    /// <param name="chanels">List of channels to subscribe</param>
    /// <returns>True if UnSubscribe command correct</returns>
    public bool UnSubscribe(params string[] chanels)
    {
      return this.SendACommand(RedisConnector.MergeStringBefore("UNSUBSCRIBE", chanels));
    }
    #endregion

    #region Transactions Methods
    /// <summary>
    /// Launch the "DISCARD" command To flush all previous MULTI commands
    /// </summary>
    /// <returns>Always true</returns>
    public bool Discard()
    {
      RedisReponse answer = this.SendCommand("DISCARD");
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
        return false;
      }
      else
      {
        return true;
      }
    }

    /// <summary>
    /// Launch the "EXEC" command To execute MULTI commands
    /// </summary>
    /// <returns>Always true</returns>
    public RedisReponse Exec()
    {
      RedisReponse answer = this.SendCommand("EXEC");
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
        return null;
      }
      else
      {
        return answer;
      }
    }

    /// <summary>
    /// Launch the "MULTI" command start a transaction
    /// </summary>
    /// <returns>Always true</returns>
    public bool Multi()
    {
      RedisReponse answer = this.SendCommand("MULTI");
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
        return false;
      }
      else
      {
        return true;
      }
    }

    /// <summary>
    /// Launch the "UNWATCH" command Flush all previously watched keys for a transaction
    /// </summary>
    /// <returns>Always true</returns>
    public bool UnWatch()
    {
      RedisReponse answer = this.SendCommand("UNWATCH");
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
        return false;
      }
      else
      {
        return true;
      }
    }

    /// <summary>
    /// Launch the "WATCH" command to marks the given keys to be watched for conditional execution of a transaction.
    /// </summary>
    /// <param name="keys">the list of watched keys</param>
    /// <returns>Always true</returns>
    public bool Watch(string[] keys)
    {
      RedisReponse answer = this.SendCommand(RedisConnector.MergeStringBefore("WATCH", keys));
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
        return false;
      }
      else
      {
        return true;
      }
    }
    #endregion

    #region Scripting Methods
    /// <summary>
    /// Launch the "EVAL" command To run an LUA script
    /// </summary>
    /// <param name="script">The LUA script</param>
    /// <param name="keys">the array of arguments keys</param>
    /// <param name="args">The array of arguments</param>
    /// <returns>the response depends of the script</returns>
    public RedisReponse Eval(string script, string[] keys, string[] args)
    {
      int n = 0;
      if (keys != null)
      {
        n = keys.Length;
      }

      if (!script.StartsWith("\""))
      {
        script = "\"" + script;
      }

      if (!script.EndsWith("\""))
      {
        script += "\"";
      }

      string[] cmd1 = RedisConnector.MergeStringBefore("EVAL", script, n.ToString());
      string[] cmd2 = RedisConnector.MergeStringBefore(cmd1, keys, args);

      if (this.GetServerVersion() >= SERVER26)
      {
        RedisReponse answer = this.SendCommand(cmd2);
        if (!answer.Success)
        {
          this.LastErrorText = answer.ErrorMessage;
          return null;
        }
        else
        {
          return answer;
        }
      }
      else
      {
        this.LastCommandText = string.Format("{0} {1}", cmd2.Aggregate((x, y) => x + " " + y), RedisConnector.NOTIMPLEMENTEDINTHISVERSION);
        this.LastErrorText = "Not implemented on this server version";
        return null;
      }
    }

    /// <summary>
    /// Launch the "EVALSHA" command To run an LUA script
    /// </summary>
    /// <param name="sha1">The SHA1 key of an LUA script cached</param>
    /// <param name="keys">the array of arguments keys</param>
    /// <param name="args">The array of arguments</param>
    /// <returns>the response depends of the script</returns>
    public RedisReponse EvalSha(string sha1, string[] keys, string[] args)
    {
      int n = 0;
      if (keys != null)
      {
        n = keys.Length;
      }

      string[] cmd1 = RedisConnector.MergeStringBefore("EVALSHA", sha1, n.ToString());
      string[] cmd2 = RedisConnector.MergeStringBefore(cmd1, keys, args);

      if (this.GetServerVersion() >= SERVER26)
      {
        RedisReponse answer = this.SendCommand(cmd2);
        if (!answer.Success)
        {
          this.LastErrorText = answer.ErrorMessage;
          return null;
        }
        else
        {
          return answer;
        }
      }
      else
      {
        this.LastCommandText = string.Format("{0} {1}", cmd2.Aggregate((x, y) => x + " " + y), RedisConnector.NOTIMPLEMENTEDINTHISVERSION);
        this.LastErrorText = "Not implemented on this server version";
        return null;
      }
    }

    /// <summary>
    /// Launch the "SCRIPT EXISTS" command To know if that SHA keys exists
    /// </summary>
    /// <param name="sha1">The SHA1 keys to test</param>
    /// <returns>for each key true if it exists</returns>
    public bool[] ScriptExists(string[] sha1)
    {
      if (this.GetServerVersion() >= SERVER26)
      {
        if (sha1 != null && sha1.Length > 0)
        {
          string[] cmd1 = RedisConnector.MergeStringBefore("SCRIPT", "EXISTS");
          string[] cmd2 = RedisConnector.MergeStringBefore(cmd1, sha1);
          RedisReponse answer = this.SendCommand(cmd2);
          if (!answer.Success)
          {
            this.LastErrorText = answer.ErrorMessage;
            return null;
          }
          else if (answer.Datas == null)
          {
            this.LastErrorText = "Wrong answer from the server";
            return null;
          }
          else
          {
            bool[] reponses = new bool[sha1.Length];
            int i = 0;
            foreach (string data in answer.Datas)
            {
              reponses[i++] = data == "1";
            }

            return reponses;
          }
        }
        else
        {
          this.LastErrorText = "Invalid argument 'sha1' cannot be null or empty";
          return null;
        }
      }
      else
      {
        this.LastCommandText = "SCRIPT EXISTS " + RedisConnector.NOTIMPLEMENTEDINTHISVERSION;
        this.LastErrorText = "Not implemented on this server version";
        return null;
      }
    }

    /// <summary>
    /// Launch the "SCRIPT FLUSH" command To flush all LUA script cache
    /// </summary>
    /// <returns>true if command success</returns>
    public bool ScriptFlush()
    {
      if (this.GetServerVersion() >= SERVER26)
      {
        RedisReponse answer = this.SendCommand("SCRIPT", "FLUSH");
        if (!answer.Success)
        {
          this.LastErrorText = answer.ErrorMessage;
          return false;
        }
        else
        {
          return !string.IsNullOrWhiteSpace(answer.Line) && answer.Line.ToUpper() == "OK";
        }
      }
      else
      {
        this.LastCommandText = "SCRIPT FLUSH " + RedisConnector.NOTIMPLEMENTEDINTHISVERSION;
        this.LastErrorText = "Not implemented on this server version";
        return false;
      }
    }

    /// <summary>
    /// Launch the "SCRIPT KILL" command To flush all LUA script cache
    /// </summary>
    /// <returns>true if command success</returns>
    public bool ScriptKill()
    {
      if (this.GetServerVersion() >= SERVER26)
      {
        RedisReponse answer = this.SendCommand("SCRIPT", "KILL");
        if (!answer.Success)
        {
          this.LastErrorText = answer.ErrorMessage;
          return false;
        }
        else
        {
          return !string.IsNullOrWhiteSpace(answer.Line) && answer.Line.ToUpper() == "OK";
        }
      }
      else
      {
        this.LastCommandText = "SCRIPT KILL " + RedisConnector.NOTIMPLEMENTEDINTHISVERSION;
        this.LastErrorText = "Not implemented on this server version";
        return false;
      }
    }

    /// <summary>
    /// Launch the "SCRIPT LOAD" command To Load a script into the script cache an get the key
    /// </summary>
    /// <param name="script">The LUA script</param>
    /// <returns>the the SHA1 key of this script</returns>
    public string ScriptLoad(string script)
    {
      if (!script.StartsWith("\""))
      {
        script = "\"" + script;
      }

      if (!script.EndsWith("\""))
      {
        script += "\"";
      }

      if (this.GetServerVersion() >= SERVER26)
      {
        RedisReponse answer = this.SendCommand("SCRIPT", "LOAD", script);
        if (!answer.Success)
        {
          this.LastErrorText = answer.ErrorMessage;
          return null;
        }
        else
        {
          return answer.Line;
        }
      }
      else
      {
        this.LastCommandText = string.Format("SCRIPT LOAD {0} {1}", script, RedisConnector.NOTIMPLEMENTEDINTHISVERSION);
        this.LastErrorText = "Not implemented on this server version";
        return null;
      }
    }
    #endregion

    #region Connection Methods
    /// <summary>
    /// Launch the "AUTH" command To connect with a password
    /// </summary>
    /// <param name="password">The password to set</param>
    /// <returns>True if Ok</returns>
    public bool Auth(string password)
    {
      if (string.IsNullOrWhiteSpace(password))
      {
        this.LastErrorText = "AUTH need a not empty password";
        return false;
      }
      else
      {
        RedisReponse answer = this.SendCommand("AUTH", password);
        if (!answer.Success)
        {
          this.LastErrorText = answer.ErrorMessage;
        }
        else
        {
          this.Password = password;
        }

        return answer.Success;
      }
    }

    /// <summary>
    /// Launch the "ECHO" command : Display text !
    /// </summary>
    /// <param name="message">The message to display</param>
    /// <returns>the Message to display</returns>
    public string Echo(string message)
    {
      RedisReponse answer = this.SendCommand("ECHO", message);
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
      }

      return answer.Line;
    }

    /// <summary>
    /// Launch the "PING" command To now if the server is ok
    /// </summary>
    /// <returns>True if Pong, False else</returns>
    public bool Ping()
    {
      RedisReponse answer = this.SendCommand("PING");
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
      }

      return answer.Line != null && answer.Line.ToUpper() == "PONG";
    }

    /// <summary>
    /// Launch the "QUIT" command To close connection
    /// </summary>
    /// <returns>True if Pong, False else</returns>
    public bool Quit()
    {
      RedisReponse answer = this.SendCommand("QUIT");
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
      }
      else
      { // Quit = the server connection is closed so we close the soket too
        this.SafeConnectionClose();
      }

      return answer.Success;
    }

    /// <summary>
    /// Launch the "SELECT" command To change the current database ID
    /// </summary>
    /// <param name="databaseId">The id to set</param>
    /// <returns>True if Ok</returns>
    public bool Select(int databaseId)
    {
      if (databaseId < RedisConnector.MINDBID || databaseId > RedisConnector.MAXDBID)
      {
        this.LastErrorText = string.Format("Db Id must be between {0} and {1}", RedisConnector.MINDBID, RedisConnector.MAXDBID);
        return false;
      }
      else
      {
        RedisReponse answer = this.SendCommand("SELECT", databaseId.ToString());
        if (!answer.Success)
        {
          this.LastErrorText = answer.ErrorMessage;
        }
        else
        {
          this.db = databaseId;
        }

        return answer.Success;
      }
    }

    #endregion

    #region Server Methods
    /// <summary>
    /// Launch the "BGREWRITEAOF" command : start writing AOF file
    /// </summary>
    /// <returns>Redis Server Message</returns>
    public string BgRewriteAOF()
    {
      RedisReponse answer = this.SendCommand("BGREWRITEAOF");
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
      }

      return answer.Line;
    }

    /// <summary>
    /// Launch the "BGSAVE" command : Save the DB
    /// </summary>
    /// <returns>Redis Server Message</returns>
    public string BgSave()
    {
      RedisReponse answer = this.SendCommand("BGSAVE");
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
      }

      return answer.Line;
    }

    /// <summary>
    /// Launch the "CLIENT KILL" command : Kill a client connection and get the result
    /// </summary>
    /// <param name="adresseIp">Client IP to kill</param>
    /// <returns>true if client killed, false if error see LastErrorText for explain</returns>
    public bool ClientKill(string adresseIp)
    {
      if (string.IsNullOrWhiteSpace(adresseIp))
      {
        this.LastErrorText = "Client adresse is empty";
        return false;
      }
      else
      {
        RedisReponse answer = this.SendCommand("CLIENT", "KILL", adresseIp);
        if (!answer.Success)
        {
          this.LastErrorText = answer.ErrorMessage;
        }

        return answer.Success;
      }
    }

    /// <summary>
    /// Launch the "CLIENT LIST" command : Get the list of connected clients
    /// </summary>
    /// <returns>A list of all client properties</returns>
    public List<Dictionary<string, string>> ClientList()
    {
      RedisReponse answer = this.SendCommand("CLIENT", "LIST");
      List<Dictionary<string, string>> result = new List<Dictionary<string, string>>();
      if (answer.Success)
      {
        if (!string.IsNullOrEmpty(answer.Line))
        { // there is some clients
          string[] clients = answer.Line.Replace("\r", string.Empty).Split('\n');
          foreach (string clientInfos in clients)
          { // for each client 
            if (!string.IsNullOrWhiteSpace(clientInfos))
            {
              Dictionary<string, string> dico = this.ExtractFromString(clientInfos, ' ', '=');
              result.Add(dico);
            }
          }
        }
      }
      else
      {
        this.ThrowErrorIfneeded(answer, "Fail to get server CLIENT LIST");
      }

      return result;
    }

    /// <summary>
    /// Launch the "CONFIG GET [pattern]" command Get the list of server properties that matching pattern
    /// </summary>
    /// <param name="pattern">Pattern to apply : type * for all</param>
    /// <returns>Dictionary([config], [value])</returns>
    public Dictionary<string, string> ConfigGet(string pattern)
    {
      if (string.IsNullOrWhiteSpace(pattern))
      {
        pattern = "*"; // All config
      }

      RedisReponse answer = this.SendCommand("CONFIG", "GET", pattern);
      Dictionary<string, string> result = new Dictionary<string, string>();
      if (answer.Success)
      {
        if (answer.Datas != null)
        { // here we've got one line = property, next one = value
          int i;
          for (i = 0; i + 1 < answer.Datas.Count; i += 2)
          {
            result.Add(answer.Datas[i], answer.Datas[i + 1]);
          }

          if (i < answer.Datas.Count)
          { // odd number of datas ??
            result.Add(answer.Datas[i], string.Empty);
          }
        }
        else
        {
          this.ThrowErrorIfneeded(null, "No data found or bad format receive");
        }
      }
      else
      {
        this.ThrowErrorIfneeded(answer, "Fail to get server CONFIG GET");
      }

      return result;
    }

    /// <summary>
    /// Launch the "CONFIG RESETSTAT" command reset server statistics counters
    /// </summary>
    /// <returns>True if non problem</returns>
    public bool ConfigResetStat()
    {
      RedisReponse answer = this.SendCommand("CONFIG", "RESETSTAT");
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
      }

      return answer.Success;
    }

    /// <summary>
    /// Launch the "CONFIG SET Key Value" command  adjust the config property 'key' with 'value'
    /// </summary>
    /// <param name="key">the key to adjust</param>
    /// <param name="value">the value</param>
    /// <returns>true if ok</returns>
    public bool ConfigSet(string key, string value)
    {
      RedisReponse answer = this.SendCommand("CONFIG", "SET", key, value);
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
      }

      return answer.Success;
    }

    /// <summary>
    /// Launch the "DBSIZE" command and get the number of keys in the DB
    /// </summary>
    /// <returns>Number of keys on the DB, -1 if error</returns>
    public long DbSize()
    {
      RedisReponse answer = this.SendCommand("DBSIZE");
      if (answer.Success)
      {
        if (double.IsNaN(answer.Number))
        {
          return 0;
        }
        else
        {
          return Convert.ToInt64(answer.Number);
        }
      }
      else
      {
        this.LastErrorText = answer.ErrorMessage;
        return -1;
      }
    }

    /// <summary>
    /// Launch the "DEBUG OBJECT" command get the debug info on the key
    /// </summary>
    /// <param name="key">the key to debug</param>
    /// <returns>all the debug properties</returns>
    public Dictionary<string, string> DebugObject(string key)
    {
      if (string.IsNullOrWhiteSpace(key))
      {
        this.LastErrorText = "Invalid key for Debug Object";
        return null;
      }

      RedisReponse answer = this.SendCommand("DEBUG", "OBJECT", key);
      Dictionary<string, string> result = new Dictionary<string, string>();
      if (answer.Success)
      {
        if (!string.IsNullOrEmpty(answer.Line))
        { // there is debug info
          result = this.ExtractFromString(answer.Line, ' ', ':');
        }
      }
      else
      {
        this.LastErrorText = answer.ErrorMessage;
        return null;
      }

      return result;
    }

    /// <summary>
    /// Launch the "DEBUG SEGFAULT" command crash the server for debug !!
    /// </summary>
    /// <returns>True if ok</returns>
    public bool DebugSegfault()
    {
      RedisReponse answer = this.SendCommand("DEBUG", "SEGFAULT");
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
      }

      return answer.Success;
    }

    /// <summary>
    /// Launch the "FLUSHALL" command RAZ all data of all DBs
    /// </summary>
    /// <returns>True if ok, false else</returns>
    public bool FlushAll()
    {
      RedisReponse answer = this.SendCommand("FLUSHALL");
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
      }

      return answer.Success;
    }

    /// <summary>
    /// Launch the "FLUSHDB" command RAZ all data of the current DB
    /// </summary>
    /// <returns>True if ok, false else</returns>
    public bool FlushDB()
    {
      RedisReponse answer = this.SendCommand("FLUSHDB");
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
      }

      return answer.Success;
    }

    /// <summary>
    /// Launch the "INFO" command : get all information about the server
    /// </summary>
    /// <returns>Key Value dictionary</returns>
    public Dictionary<string, string> Info()
    {
      RedisReponse answer = this.SendCommand("INFO");
      Dictionary<string, string> result = new Dictionary<string, string>();
      if (answer.Success)
      {
        if (!string.IsNullOrWhiteSpace(answer.Line))
        { // here we've got tis style of pattern : property1:value1\r\nproperty2:value2\r\nproperty3:value3\r\n...
          string source = answer.Line.Replace("\r", string.Empty);
          result = this.ExtractFromString(source, '\n', ':');

          if (result.ContainsKey("redis_version"))
          { // we use the result to store some properties
            this.ComputeVersion(result["redis_version"]);
          }

          if (result.ContainsKey("redis_mode"))
          { // we are in sentinel Mode
            this.ServerInSentinelMode = result["redis_mode"] == "sentinel";
          }
        }
        else
        {
          this.ThrowErrorIfneeded(null, "No data found or bad format receive");
        }
      }
      else
      {
        this.ThrowErrorIfneeded(answer, "Fail to get server INFO");
      }

      return result;
    }

    /// <summary>
    /// Launch the "LASTSAVE" command get the date of the last completed save.
    /// </summary>
    /// <returns>Number of keys on the DB, -1 if error</returns>
    public DateTime LastSave()
    {
      RedisReponse answer = this.SendCommand("LASTSAVE");
      if (answer.Success)
      {
        return RedisConnector.GetDateTimeFromUnixTime(answer.Number);
      }
      else
      {
        this.LastErrorText = answer.ErrorMessage;
        return DateTime.MaxValue;
      }
    }

    /// <summary>
    /// Launch the "MONITOR" command : send data to the call by the parameter function
    /// </summary>
    /// <param name="fct">Call back method to receive data</param>
    /// <returns>True if monitor ok</returns>
    public bool Monitor(EventMonitorHandler fct)
    {
      if (fct == null)
      {
        this.LastErrorText = "Invalid argument No handler";
        return false;
      }
      else
      {
        if (!this.SendACommand("MONITOR"))
        {
          // when SendACommand fail, this.LastErrorText is set No need to set here 
          return false;
        }
        else
        {
          this.processMonitorCallBack = new AsyncCallback(this.ProcessMonitor);
          MonitorAsyncParam param = new MonitorAsyncParam(this.bufferStream, fct);
          this.bufferStream.BeginRead(param.Buffer, 0, param.Buffer.Length, this.processMonitorCallBack, param);
          return true;
        }
      }
    }

    /// <summary>
    /// Launch the "SAVE" command save the server (blocking process)
    /// </summary>
    /// <returns>True if Ok</returns>
    public bool Save()
    {
      RedisReponse answer = this.SendCommand("SAVE");
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
      }

      return answer.Success;
    }

    /// <summary>
    /// Launch the "SHUTDOWN" command to stop the server no response pending
    /// </summary>
    /// <param name="opt">Shutdown option (Only from 2.6 version)</param>
    public void Shutdown(ShutdownOption opt)
    {
      if (this.GetServerVersion() >= SERVER26)
      { // After 2.6 version we have more options
        switch (opt)
        {
          case ShutdownOption.None:
            this.SendCommand("SHUTDOWN");
            break;
          case ShutdownOption.NOSAVE:
            this.SendCommand("SHUTDOWN", "NOSAVE");
            break;
          case ShutdownOption.SAVE:
            this.SendCommand("SHUTDOWN", "SAVE");
            break;
        }
      }
      else
      { // before 2.6 : no options
        this.SendCommand("SHUTDOWN");
      }
    }

    /// <summary>
    /// Launch the "SLAVEOF" command Make this server slave of another one
    /// </summary>
    /// <param name="hostname">Host name of the master</param>
    /// <param name="port">port of the master</param>
    /// <returns>True if Ok</returns>
    public bool SlaveOf(string hostname, int port)
    {
      if (string.IsNullOrWhiteSpace(hostname) || port <= 0)
      {
        this.LastErrorText = "Invalid hostname or port. To clear salve option use the SlaveOfNoOne() method instead";
        return false;
      }

      RedisReponse answer = this.SendCommand("SLAVEOF", hostname, port.ToString());
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
      }

      return answer.Success;
    }

    /// <summary>
    /// Launch the "SLAVEOF" command Make this server slave of no one
    /// </summary>
    /// <returns>True if Ok</returns>
    public bool SlaveOfNoOne()
    {
      RedisReponse answer = this.SendCommand("SLAVEOF", "NO", "ONE");
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
      }

      return answer.Success;
    }

    /// <summary>
    /// Launch the "SLOWLOG GET xx" command that return the "slowest commands" list
    /// </summary>
    /// <param name="size">number of most recent rows (use -1 for all)</param>
    /// <returns>the list of slowest commands</returns>
    public List<SlowLogData> SlowLogGet(int size)
    {
      List<SlowLogData> result = new List<SlowLogData>();
      RedisReponse answer;
      if (size > 0)
      {
        answer = this.SendCommand("SLOWLOG", "GET", size.ToString());
      }
      else
      {
        answer = this.SendCommand("SLOWLOG", "GET");
      }

      if (answer.Success)
      {
        if (answer.Datas != null)
        { // here we've got something
          foreach (string line in answer.Datas)
          {
            result.Add(new SlowLogData(line));
          }
        }
        else
        {
          this.ThrowErrorIfneeded(null, "Bad format receive");
        }
      }
      else
      {
        this.ThrowErrorIfneeded(answer, string.Format("Fail to get server SLOWLOG GET {0}", size));
      }

      return result;
    }

    /// <summary>
    /// Launch the "SLOWLOG LEN" command that return the current length of the "slowest commands" list
    /// </summary>
    /// <returns>the current length of the slowest command list</returns>
    public int SlowLogLen()
    {
      RedisReponse answer = this.SendCommand("SLOWLOG", "LEN");
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
        return -1;
      }
      else if (double.IsNaN(answer.Number))
      {
        this.LastErrorText = CSTERRORNAN;
        return -1;
      }
      else
      {
        return Convert.ToInt32(answer.Number);
      }
    }

    /// <summary>
    /// Launch the "SLOWLOG RESET" command that clear the "slowest commands" list
    /// </summary>
    /// <returns>true is ok</returns>
    public bool SlowLogReset()
    {
      RedisReponse answer = this.SendCommand("SLOWLOG", "RESET");
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
      }

      return answer.Success;
    }

    /// <summary>
    /// Launch the "SYNC" command : send data to the call by the parameter function
    /// </summary>
    /// <param name="fct">Call back method to receive data</param>
    /// <returns>True if monitor ok</returns>
    public bool Sync(EventMonitorHandler fct)
    {
      if (fct == null)
      {
        this.LastErrorText = "Invalid argument No handler";
        return false;
      }
      else
      {
        if (!this.SendACommand("SYNC"))
        {
          // when SendACommand fail, this.LastErrorText is set No need to set here 
          return false;
        }
        else
        {
          this.processMonitorCallBack = new AsyncCallback(this.ProcessMonitor);
          MonitorAsyncParam param = new MonitorAsyncParam(this.bufferStream, fct);
          this.bufferStream.BeginRead(param.Buffer, 0, param.Buffer.Length, this.processMonitorCallBack, param);
          return true;
        }
      }
    }

    /// <summary>
    /// Launch the "SLOWLOG RESET" command and return the server time 
    /// </summary>
    /// <returns>the date time and micro seconds</returns>
    public PrecisionTime Time()
    {
      if (this.GetServerVersion() >= SERVER26)
      {
        RedisReponse answer = this.SendCommand("TIME");
        if (answer.Success)
        {
          if (answer.Datas != null || answer.Datas.Count != 2)
          {
            return new PrecisionTime(answer.Datas[0], answer.Datas[1]);
          }
          else
          {
            this.LastErrorText = "Invalid reponse from the server";
            return PrecisionTime.Empty;
          }
        }
        else
        {
          this.LastErrorText = answer.ErrorMessage;
          return PrecisionTime.Empty;
        }
      }
      else
      {
        this.LastCommandText = "TIME" + RedisConnector.NOTIMPLEMENTEDINTHISVERSION;
        this.LastErrorText = "TIME : Invalid command in this version";
        return PrecisionTime.Now;
      }
    }
    #endregion

    #region Sentienel commands
    /// <summary>
    /// Returns the result of the command Sentinel masters :show a list of monitored masters and their state
    /// </summary>
    /// <returns>A list of dictionaries : masters / information</returns>
    public List<Dictionary<string, string>> SentinelMasters()
    {
      RedisReponse answer = this.SendCommand("SENTINEL", "MASTERS");
      List<Dictionary<string, string>> result;
      if (answer.Success && answer.Datas != null)
      {
        result = new List<Dictionary<string, string>>();
        foreach (string masterinfo in answer.Datas)
        {
          string[] infos = masterinfo.Split(new string[] { "<0>" }, StringSplitOptions.None);
          Dictionary<string, string> dico = new Dictionary<string, string>();
          for (int i = 0; i < infos.Length; i += 2)
          {
            dico.Add(infos[i], infos[i + 1]);
          }

          result.Add(dico);
        }
      }
      else
      {
        this.LastErrorText = "Fail to get Sentinel Masters details : " + answer.ErrorMessage;
        result = null;
      }

      return result;
    }

    /// <summary>
    /// Returns the result of the command Sentinel slaves : show a list of slaves for this master, and their state
    /// </summary>
    /// <param name="masterKey">Get the master to query</param>
    /// <returns>A list of dictionaries : masters / information</returns>
    public List<Dictionary<string, string>> SentinelSlaves(string masterKey)
    {
      RedisReponse answer = this.SendCommand("SENTINEL", "SLAVES", masterKey);
      List<Dictionary<string, string>> result = null;
      if (answer.Success && answer.Datas != null)
      {
        result = new List<Dictionary<string, string>>();
        foreach (string masterinfo in answer.Datas)
        {
          string[] infos = masterinfo.Split(new string[] { "<0>" }, StringSplitOptions.None);
          Dictionary<string, string> dico = new Dictionary<string, string>();
          for (int i = 0; i < infos.Length; i += 2)
          {
            dico.Add(infos[i], infos[i + 1]);
          }

          result.Add(dico);
        }
      }
      else
      {
        this.LastErrorText = "Fail to get Sentinel Slave details : " + answer.ErrorMessage;
        result = null;
      }

      return result;
    }

    /// <summary>
    /// Returns the result of the command Sentinel is master down by address :
    /// return a two elements multi bulk reply where the first is 0 or 1 
    /// (0 if the master with that address is known and is in SDOWN state, 1 otherwise). 
    /// The second element of the reply is the subjective leader for this master, that is, 
    /// the RUNID of the Redis Sentinel instance that should perform the failover accordingly 
    /// to the queried instance
    /// </summary>
    /// <param name="adresse">the address IP of the master to ask</param>
    /// <param name="port">the port of the server</param>
    /// <returns>a pair of information</returns>
    public KeyValuePair<string, string> SentinelIsMasterDownByAddr(string adresse, int port)
    {
      RedisReponse answer = this.SendCommand("SENTINEL", "is-master-down-by-addr", adresse, port.ToString());
      KeyValuePair<string, string> result;
      if (answer.Success && answer.Datas.Count >= 2)
      {
        result = new KeyValuePair<string, string>(answer.Datas[0], answer.Datas[1]);
      }
      else
      {
        this.LastErrorText = "Fail to get Sentinel SentinelIsMasterDownByAddr details : " + answer.ErrorMessage;
        result = new KeyValuePair<string, string>();
      }

      return result;
    }

    /// <summary>
    /// Returns the result of the command Sentinel get master address by name :
    /// return the IP and port number of the master with that name. 
    /// If a failover is in progress or terminated successfully for this master 
    /// it returns the address and port of the promoted slave
    /// </summary>
    /// <param name="masterName">the name of the server to ask</param>
    /// <returns>the address and the port</returns>
    public KeyValuePair<string, string> SentinelGetMasterAddrByName(string masterName)
    {
      RedisReponse answer = this.SendCommand("SENTINEL", "get-master-addr-by-name", masterName);
      KeyValuePair<string, string> result;
      if (answer.Success && answer.Datas.Count >= 2)
      {
        result = new KeyValuePair<string, string>(answer.Datas[0], answer.Datas[1]);
      }
      else
      {
        this.LastErrorText = "Fail to get Sentinel SentinelGetMaseterAddrByName details : " + answer.ErrorMessage;
        result = new KeyValuePair<string, string>();
      }

      return result;
    }

    /// <summary>
    /// Returns the result of the command Sentinel reset :
    /// this command will reset all the masters with matching name. 
    /// The pattern argument is a glob-style pattern. 
    /// The reset process clears any previous state in a master (including a failover in progress), 
    /// and removes every slave and sentinel already discovered and associated with the master
    /// </summary>
    /// <param name="pattern">the pattern to match server</param>
    /// <returns>the address and the port</returns>
    public int SentinelReset(string pattern)
    {
      RedisReponse answer = this.SendCommand("SENTINEL", "reset", pattern);
      if (!answer.Success)
      {
        this.LastErrorText = "Fail to get Sentinel reset details : " + answer.ErrorMessage;
        return -1;
      }
      else if (double.IsNaN(answer.Number))
      {
        this.LastErrorText = CSTERRORNAN;
        return -1;
      }
      else
      {
        return Convert.ToInt32(answer.Number);
      }
    }
    #endregion

    #region Sorted Set internal common methods
    /// <summary>
    /// Launch the "ZRANGE" OR "ZREVRANGE" command To returns the specified range of elements in the sorted set stored at key
    /// </summary>
    /// <param name="keyWord">Must be : "ZRANGE" OR "ZREVRANGE"</param>
    /// <param name="key">the key to analyze</param>
    /// <param name="start">the starting index to return (zero based)</param>
    /// <param name="stop">the last index to return (zero based)</param>
    /// <param name="withScores">return score or not</param>
    /// <returns>the values</returns>
    private List<SortedSet> ZRangeInternal(string keyWord, string key, int start, int stop, bool withScores)
    {
      RedisReponse answer;
      if (withScores)
      {
        answer = this.SendCommand(keyWord, key, start.ToString(), stop.ToString(), "WITHSCORES");
      }
      else
      {
        answer = this.SendCommand(keyWord, key, start.ToString(), stop.ToString());
      }

      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
        return null;
      }
      else
      {
        // here we clear LastErrorText : to be sur to make the difference between : Exist =no and Exist raise an error
        this.LastErrorText = string.Empty;
        List<SortedSet> res = new List<SortedSet>();
        if (withScores)
        {
          for (int i = 0; i < answer.Datas.Count; i += 2)
          {
            res.Add(new SortedSet(RedisConnector.GetDoubleFromString(answer.Datas[i + 1], double.NaN), answer.Datas[i]));
          }
        }
        else
        {
          foreach (string s in answer.Datas)
          {
            res.Add(new SortedSet(double.NaN, s));
          }
        }

        return res;
      }
    }

    /// <summary>
    /// Launch the "ZRANGEBYSCORE" or "ZREVRANGEBYSCORE" command To returns all the elements in the sorted set at 
    /// key with a score between min and max (including elements with score equal to min or max).
    /// </summary>
    /// <param name="keyWord">Must be "ZRANGEBYSCORE" or "ZREVRANGEBYSCORE"</param>
    /// <param name="key">the key to analyze</param>
    /// <param name="min">the min score to return</param>
    /// <param name="max">the max score to return</param>
    /// <param name="withScores">return score or not</param>
    /// <param name="minIncluded">indicate if the min score is included (by default or excluded)</param>
    /// <param name="maxIncluded">indicate if the max score is included (by default or excluded)</param>
    /// <param name="limitArg">Get a Limit offset and count if needed</param>
    /// <returns>the values</returns>
    private List<SortedSet> ZRangeByScoreInternal(string keyWord, string key, double min, double max, bool withScores, bool minIncluded, bool maxIncluded, Limit limitArg)
    {
      bool isLimit = limitArg != null && !limitArg.IsEmpty;
      string[] args = new string[4 + (withScores ? 1 : 0) + (isLimit ? 3 : 0)];
      args[0] = keyWord;
      args[1] = key;
      args[2] = (minIncluded ? string.Empty : "(") + RedisConnector.GetStringFromDouble(min);
      args[3] = (maxIncluded ? string.Empty : "(") + RedisConnector.GetStringFromDouble(max);
      int n = 4;
      if (withScores)
      {
        args[n++] = "WITHSCORES";
      }

      if (isLimit)
      {
        args[n++] = "LIMIT";
        args[n++] = limitArg.Offset.ToString();
        args[n++] = limitArg.Count.ToString();
      }

      RedisReponse answer = this.SendCommand(args);

      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
        return null;
      }
      else if (answer.Datas == null)
      {
        this.LastErrorText = "No Datas";
        return null;
      }
      else
      {
        // here we clear LastErrorText : to be sur to make the difference between : Exist =no and Exist raise an error
        this.LastErrorText = string.Empty;
        List<SortedSet> res = new List<SortedSet>();
        if (withScores)
        {
          for (int i = 0; i < answer.Datas.Count; i += 2)
          {
            res.Add(new SortedSet(RedisConnector.GetDoubleFromString(answer.Datas[i + 1], double.NaN), answer.Datas[i]));
          }
        }
        else
        {
          foreach (string s in answer.Datas)
          {
            res.Add(new SortedSet(double.NaN, s));
          }
        }

        return res;
      }
    }

    /// <summary>
    /// Launch the "ZRANK" or "ZREVRANK" command To returns the rank of member in the sorted set stored at key, with the scores ordered from high to low
    /// </summary>
    /// <param name="keyWord">Must be "ZRANK" or "ZREVRANK"</param>
    /// <param name="key">the key to analyze</param>
    /// <param name="member">the member to compute</param>
    /// <returns>the index of the member in the z set (-1 if not found or error)</returns>
    private int ZRankInternal(string keyWord, string key, string member)
    {
      RedisReponse answer = this.SendCommand(keyWord, key, member);
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
        return -1;
      }
      else if (double.IsNaN(answer.Number))
      {
        this.LastErrorText = CSTERRORNAN;
        return -1;
      }
      else
      {
        // here we clear LastErrorText : to be sur to make the difference between : Exist =no and Exist raise an error
        this.LastErrorText = string.Empty;
        return Convert.ToInt32(answer.Number);
      }
    }

    /// <summary>
    /// Launch the "ZINTERSTORE" or "ZUNIONSTORE" command To computes the union of NUMKEYS sorted sets given by the specified keys, and stores the result in destination
    /// </summary>
    /// <param name="keyWord">must be "ZINTERSTORE" or "ZUNIONSTORE"</param>
    /// <param name="destination">the key to update or create as the result of the operation</param>
    /// <param name="keys">the list of key with their weighting</param>
    /// <param name="aggregate">the aggregate operation</param>
    /// <returns>the number of values in the result set</returns>
    private int ZOperationStoreInternal(string keyWord, string destination, List<string> keys, ZAggregate aggregate)
    {
      int nb = 4 + keys.Count + (aggregate == ZAggregate.Default ? 0 : 2);
      string[] args = new string[nb];
      int n = 0;
      args[n++] = keyWord;
      args[n++] = destination;
      args[n++] = keys.Count.ToString();
      foreach (var key in keys)
      {
        args[n++] = key;
      }

      if (aggregate != ZAggregate.Default)
      {
        args[n++] = "AGGREGATE";
        args[n++] = aggregate.ToString().ToUpper();
      }

      RedisReponse answer = this.SendCommand(args);
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
        return 0;
      }
      else if (double.IsNaN(answer.Number))
      {
        this.LastErrorText = CSTERRORNAN;
        return 0;
      }
      else
      {
        // here we clear LastErrorText : to be sur to make the difference between : Exist =no and Exist raise an error
        this.LastErrorText = string.Empty;
        return Convert.ToInt32(answer.Number);
      }
    }

    /// <summary>
    /// Launch the "ZINTERSTORE" or "ZUNIONSTORE" command To computes the union of NUMKEYS sorted sets given by the specified keys, and stores the result in destination
    /// </summary>
    /// <param name="keyWord">must be "ZINTERSTORE" or "ZUNIONSTORE"</param>
    /// <param name="destination">the key to update or create as the result of the operation</param>
    /// <param name="keys">the list of key with their weighting</param>
    /// <param name="aggregate">the aggregate operation</param>
    /// <returns>the number of values in the result set</returns>
    private int ZOperationStoreInternal(string keyWord, string destination, List<Tuple<string, double>> keys, ZAggregate aggregate)
    {
      int nb = 4 + (keys.Count * 2) + (aggregate == ZAggregate.Default ? 0 : 2);
      string[] args = new string[nb];
      int n = 0;
      args[n++] = keyWord;
      args[n++] = destination;
      args[n++] = keys.Count.ToString();
      foreach (var x in keys)
      {
        args[n++] = x.Item1;
      }

      args[n++] = "WEIGHTS";
      foreach (var x in keys)
      {
        args[n++] = RedisConnector.GetStringFromDouble(x.Item2);
      }

      if (aggregate != ZAggregate.Default)
      {
        args[n++] = "AGGREGATE";
        args[n++] = aggregate.ToString().ToUpper();
      }

      RedisReponse answer = this.SendCommand(args);
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
        return 0;
      }
      else if (double.IsNaN(answer.Number))
      {
        this.LastErrorText = CSTERRORNAN;
        return 0;
      }
      else
      {
        // here we clear LastErrorText : to be sur to make the difference between : Exist =no and Exist raise an error
        this.LastErrorText = string.Empty;
        return Convert.ToInt32(answer.Number);
      }
    }
    #endregion

    #region List internal common methods
    /// <summary>
    /// Launch the "BLPOP" or 'BRPOP" command To removes and returns the first (or last) element of the lists stored at keys. If lists are empty wait and block the client
    /// </summary>
    /// <param name="keyWord">Must be "BLPOP" or 'BRPOP"</param>
    /// <param name="timeOut">The time in seconds to wait if all lists are empty</param>
    /// <param name="key">the key of the first list to pop</param>
    /// <param name="keys">the keys of other lists to pop</param>
    /// <returns>null if timeOut : Tuple(key, value) else</returns>
    private Tuple<string, string> BPopInternal(string keyWord, int timeOut, string key, string[] keys)
    {
      string[] args = new string[3 + keys.Length];
      int n = 0;
      args[n++] = keyWord;
      args[n++] = key;
      foreach (string k in keys)
      {
        args[n++] = k;
      }

      args[n++] = timeOut.ToString();

      RedisReponse answer = this.SendCommand(args);
      if (!answer.Success)
      {
        this.LastErrorText = answer.ErrorMessage;
        return null;
      }
      else
      {
        // here we clear LastErrorText : to be sur to make the difference between : Exist =no and Exist raise an error
        this.LastErrorText = string.Empty;
        if (answer.Datas != null && answer.Datas.Count == 2)
        {
          return new Tuple<string, string>(answer.Datas[0], answer.Datas[1]);
        }
        else if (answer.Datas != null && answer.Datas.Count == 1)
        {
          return new Tuple<string, string>(key, answer.Datas[0]);
        }
        else
        {
          return null;
        }
      }
    }
    #endregion
  }
}
