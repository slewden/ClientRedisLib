namespace ClientRedisLib.RedisClass
{
  /// <summary>
  /// Argumet of SORT command in REDIS to limit returns
  /// </summary>
  public class Limit
  {
    /// <summary>
    /// Default class constructor
    /// </summary>
    /// <param name="offset">the offset to set</param>
    /// <param name="count">the count elements</param>
    public Limit(int offset, int count)
    {
      this.Offset = offset;
      this.Count = count;
    }

    /// <summary>
    /// Get or Set the offset witch is the number of elements to skip
    /// </summary>
    public int Offset { get; set; }

    /// <summary>
    /// Get or Set the count witch is the number of elements to return
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// Get true if the object is empty or not correctly filled
    /// </summary>
    public bool IsEmpty
    {
      get
      {
        return this.Offset == int.MinValue || this.Offset == int.MaxValue || this.Count == int.MinValue || this.Count == int.MaxValue;
      }
    }
  }
}