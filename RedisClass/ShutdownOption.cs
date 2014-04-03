namespace ClientRedisLib.RedisClass
{
  /// <summary>
  /// Redis command SHUTDOWN arguments
  /// </summary>
  public enum ShutdownOption
  {
    /// <summary>
    /// Force save before shutdown
    /// </summary>
    SAVE,

    /// <summary>
    /// Shutdown without saving even if needed
    /// </summary>
    NOSAVE,

    /// <summary>
    /// Default option : no parameter
    /// </summary>
    None
  }
}
