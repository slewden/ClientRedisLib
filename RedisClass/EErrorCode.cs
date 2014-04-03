using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClientRedisLib.RedisClass
{
  /// <summary>
  /// Unified protocol error
  /// </summary>
  public enum EErrorCode
  {
    /// <summary>
    /// No error
    /// </summary>
    None = 0,

    /// <summary>
    /// Partial answer received
    /// </summary>
    NoAnswerReceived =  -1,

    /// <summary>
    /// Error during the analyse of the answer
    /// </summary>
    CommunicationError = -2,

    /// <summary>
    /// Command DEBUGSEGFAULT send ==> no response can comme yet !
    /// </summary>
    ServerDown = -3,

    /// <summary>
    /// Unknowed response
    /// </summary>
    UnknowError = -4,

    /// <summary>
    /// An error is received from the server
    /// </summary>
    ServerError = -5,
  }
}
