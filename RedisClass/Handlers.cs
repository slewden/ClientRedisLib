using System;

namespace ClientRedisLib.RedisClass
{
  /// <summary>
  /// Delagate to Monitor method call back
  /// </summary>
  /// <param name="e">All information needed to analyse MONITOR informations</param>
  public delegate void EventMonitorHandler(EventMonitorArgs e);

  /// <summary>
  /// Delegate to Subscribe methods call back
  /// </summary>
  /// <param name="e">All informations needed to analyse a published information</param>
  public delegate void EventSubscribeHandeler(EventSubscribeArgs e);
}
