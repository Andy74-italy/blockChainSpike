namespace blockChainManager
{
    /// <summary>
    /// The basic structure for both types of blockchain data (item and block).
    /// </summary>
    public class BaseBlockInfo
    {
        /// <summary>
        /// Id of the block or element.
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// Creation date of the item or block.
        /// </summary>
        public DateTime CreationDate { get; set; }
        /// <summary>
        /// The HashCode of the element or block, performed on the entire data structure
        /// </summary>
        public string HashCode { get; set; }
        /// <summary>
        /// The HashCode of the previous blockchain item.
        /// </summary>
        public string PreviousHashCode { get; set; }
    }
}