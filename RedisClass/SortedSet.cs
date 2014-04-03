namespace ClientRedisLib.RedisClass
{
  /// <summary>
  /// Class to manipulate a sorted set IE : a score and a member
  /// </summary>
  public class SortedSet
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="SortedSet" /> class.
    /// Initialize an new sorted set
    /// </summary>
    /// <param name="score">the score</param>
    /// <param name="member">the member</param>
    public SortedSet(double score, string member)
    {
      this.Score = score;
      this.Member = member;
    }

    /// <summary>
    /// The score to sort
    /// </summary>
    public double Score { get; set; }

    /// <summary>
    /// The sorted member
    /// </summary>
    public string Member { get; set; }
  }
}
