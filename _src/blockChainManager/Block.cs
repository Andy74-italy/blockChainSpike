namespace blockChainManager
{
    /// <summary>
    /// The block that contains the 100 elements of the blockchain.
    /// </summary>
    public class Block : BaseBlockInfo
    {
        /// <summary>
        /// The 100 hashcodes of the blockchain elements included in the block.
        /// </summary>
        public string[] DataBlocksReference { get; set; }
        /// <summary>
        /// The closing date of the block.
        /// </summary>
        public DateTime ClosingDate { get; set; }
    }
}
