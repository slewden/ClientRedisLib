namespace ClientRedisLib.RedisClass
{
  /// <summary>
  /// Redis Bitop operations
  /// </summary>
  public enum BitOperation
  {
    /// <summary>
    /// Combine bit with an AND
    /// </summary>
    AND,

    /// <summary>
    /// Combine bit with an OR
    /// </summary>
    OR,

    /// <summary>
    /// Combine bit with an XOR
    /// </summary>
    XOR,

    /// <summary>
    /// Combine bit with an NOT
    /// </summary>
    NOT
  }
}
