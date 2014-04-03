using System;

namespace ClientRedisLib.RedisClass
{
  /// <summary>
  /// Delegate to monitor method call back
  /// </summary>
  /// <param name="e">All information needed to analyze MONITOR information</param>
  public delegate void EventMonitorHandler(EventMonitorArgs e);

  /// <summary>
  /// Delegate to Subscribe methods call back
  /// </summary>
  /// <param name="e">All information needed to analyze a published information</param>
  public delegate void EventSubscribeHandeler(EventSubscribeArgs e);
}
