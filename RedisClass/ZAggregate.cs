using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClientRedisLib.RedisClass
{
  /// <summary>
  /// ZUNIONSTORE or ZINTERSTORE aggregate argument values
  /// </summary>
  public enum ZAggregate
  {
    /// <summary>
    /// Apply the default aggregate function (the sum function)
    /// </summary>
    Default,

    /// <summary>
    /// Sum the scores
    /// </summary>
    Sum,

    /// <summary>
    /// Get the minimum of the scores
    /// </summary>
    Min,

    /// <summary>
    /// Get the maximum of the scores
    /// </summary>
    Max,
  }
}
