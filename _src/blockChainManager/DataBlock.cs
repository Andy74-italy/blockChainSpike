namespace blockChainManager
{
    /// <summary>
    /// The blockchain element that defines and contains the specific generalized data type.
    /// </summary>
    /// <typeparam name="T">The type of data that identifies the blockchain element.</typeparam>
    public class DataBlock<T> : BaseBlockInfo
    {
        /// <summary>
        /// The reference to the blockchain block to which the item belongs.
        /// </summary>
        public Guid BlockReference { get; set; }
        /// <summary>
        /// The data to be kept within the blockchain.
        /// </summary>
        public T Data { get; set; }
    }
}
