using System;
using System.Collections.Generic;
using ClientRedisLib.RedisClass;

namespace ClientRedisLib
{
  /// <summary>
  /// Redis pipeline object
  /// </summary>
  public class RedisPipeline
  {
    /// <summary>
    /// List of queued commands
    /// </summary>
    private List<Func<RedisReponse>> myList = new List<Func<RedisReponse>>();

    /// <summary>
    /// Enqueue the reponses to find
    /// </summary>
    /// <param name="returnMethod">Method to call</param>
    public void CompleteBytesQueuedCommand(Func<RedisReponse> returnMethod)
    {
      this.myList.Add(returnMethod);
    }
  }
}
