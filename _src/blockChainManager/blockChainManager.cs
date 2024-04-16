using MongoDB.Bson;
using MongoDB.Driver;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace blockChainManager
{
    /// <summary>
    /// The generalized blockchain manager.
    /// </summary>
    /// <typeparam name="T">The type of date that the blockchain managed by the manager will contain.</typeparam>
    public class BlockChainManager<T> :
#if DEBUG
        BlockChainBaseTest,
#endif
        IBlockChainManager<T>
    {
        /// <summary>
        /// Closing the block must be an atomic functionality, it is therefore necessary
        /// to manage a critical section for the closing and creation operations of the new block.
        /// </summary>
        private Semaphore semaphone;
        private BlockChain<T> blockChain { get; set; }

        private MongoClient mongoClient;
        private IMongoCollection<Block> blocksCollection;
        private IMongoCollection<DataBlock<T>> dataCollection;

        /// <summary>
        /// The number of elements contained in a block.
        /// </summary>
        private long blockSize = 100;

        /// <summary>
        /// Constructor for initializing the blockchain manager.
        /// </summary>
        /// <param name="stringConnection">Connection string to MongoDB.</param>
        /// <param name="dbName">Database name Mongo</param>
        /// <param name="collectionDataName">Name of the collection that identifies the blockchain data.</param>
        /// <param name="collectionBlocksName">Name of the collection that identifies the blocks of blockchain data.</param>
        /// <param name="blockSize">The number of elements to insert into a block.</param>
        public BlockChainManager(string stringConnection, string dbName, string collectionDataName, string collectionBlocksName, long? blockSize = null)
        {
            blockChain = new BlockChain<T>();
            mongoClient = new MongoClient(stringConnection);
            var database = mongoClient.GetDatabase(dbName);
            blocksCollection = database.GetCollection<Block>(collectionBlocksName);
            dataCollection = database.GetCollection<DataBlock<T>>(collectionDataName);
            this.blockSize = (blockSize ?? this.blockSize);

            semaphone = new Semaphore(initialCount: 1, maximumCount: 1);
            LoadFromDBLastTwoBlocks().Wait();
            if (blockChain.CurrentBlock == null)
                CloseLastBlockAndCreateNewOne().Wait();
        }

        /// <summary>
        /// Closes the block that is still open and creates a new one. (public)
        /// </summary>
        /// <returns>The new block just created.</returns>
        public async Task<Block> CloseLastBlockAndCreateNewOne()
        {
            /// The method is public and calls an equivalent private method 
            /// specifying that the critical section must be activated.
            return await CloseLastBlockAndCreateNewOne(false);
        }

        /// <summary>
        ///  Closes the block that is still open and creates a new one. (private)
        /// </summary>
        /// <param name="criticalSectionActive">Identify whether an active critical section already exists or whether one needs to be activated.</param>
        /// <returns>The new block just created.</returns>
#if DEBUG
        private async Task<Block> CloseLastBlockAndCreateNewOne(bool criticalSectionActive) {
            test_CloseLastBlockAndCreateNewOne();
#else
        private async Task<Block> CloseLastBlockAndCreateNewOne(bool criticalSectionActive) {
#endif
            try
            {
                if (!criticalSectionActive)
                    semaphone.WaitOne();

                /// The block list may be empty, in which case you just need to create a new block.
                if (blockChain.CurrentBlock != null)
                {
                    /// The HashCodes of the elements assigned to the block are inserted in advance
                    /// to generate the HashCode of the block.
                    blockChain.CurrentBlock.DataBlocksReference = blockChain.dataBlocks.Where(x => x.BlockReference == blockChain.CurrentBlockId).Select(x => x.HashCode).ToArray();
                    blockChain.CurrentBlock.ClosingDate = DateTime.UtcNow;
                    blockChain.CurrentBlock.HashCode = CalculateHash(blockChain.CurrentBlock);

                    var filtro = Builders<Block>.Filter.Eq(x => x.Id, blockChain.CurrentBlock.Id);
                    blocksCollection.ReplaceOne(filtro, blockChain.CurrentBlock);
                }

                /// A new block is created and is immediately assigned the HashCode of the previous block
                /// (in the case of a null previous block, we are in the presence of the first block and 
                /// it will not be evaluated).
                var newblock = new Block()
                {
                    CreationDate = DateTime.UtcNow,
                    Id = Guid.NewGuid(),
                    PreviousHashCode = blockChain.CurrentBlock?.HashCode
                };

                blocksCollection.InsertOne(newblock);
                blockChain.blocks.Add(newblock);

                return newblock;
            }
            finally
            {
                if (!criticalSectionActive)
                    semaphone.Release();
            }
        }

        /// <summary>
        /// Inserts a new element into the element blockchain.
        /// </summary>
        /// <param name="newData">The data to be inserted into the blockchain.</param>
        /// <returns>The new item inserted.</returns>
#if DEBUG
        public async Task<DataBlock<T>> CreateNewDataBlock(T newData)
        {
            test_CreateNewDataBlock();
#else
        public async Task<DataBlock<T>> CreateNewDataBlock(T newData)
        {
#endif
            try
            {
                semaphone.WaitOne();

                /// The new element will have the reference to the open block and the HashCode of the previous element.
                var newdblock = new DataBlock<T>() { 
                    BlockReference = blockChain.CurrentBlockId,
                    CreationDate = DateTime.UtcNow,
                    Data = newData,
                    Id = Guid.NewGuid(),
                    PreviousHashCode = blockChain.LastDataBlockHash
                };
                newdblock.HashCode = CalculateHash(newdblock);
                blockChain.dataBlocks.Add(newdblock);
                dataCollection.InsertOne(newdblock);

                /// If the number of elements necessary to close the open block has been reached,
                /// a new block is closed and created indicating that the process is already in a critical section.
                if (blockChain.dataBlocks.Count(el => el.BlockReference == blockChain.CurrentBlockId) >= blockSize) {
                    CloseLastBlockAndCreateNewOne(true);
                }

                return newdblock;
            }
            finally
            {
                semaphone.Release();
            }
        }

        /// <summary>
        /// Load the entire block and data DB.
        /// </summary>
        /// <returns>Nothing</returns>
        public async Task LoadFromDBFull()
        {
            blockChain.blocks = blocksCollection.Find(new BsonDocument()).ToList();
            blockChain.dataBlocks = dataCollection.Find(new BsonDocument()).ToList();
        }

        /// <summary>
        /// Load the DB starting from the last two blocks and the related data contained in the two selected blocks.
        /// </summary>
        /// <returns>Nothing</returns>
#if DEBUG
        public async Task LoadFromDBLastTwoBlocks()
        {
            test_LoadFromDBLastTwoBlocks();
#else
        public async Task LoadFromDBLastTwoBlocks()
        {
#endif
            var filter = Builders<Block>.Filter.Empty;
            var sort = Builders<Block>.Sort.Descending(x => x.CreationDate);
            var options = new FindOptions<Block, Block> { Sort = sort, Limit = 2 };
            blockChain.blocks = blocksCollection.FindSync(filter, options).ToList();
            var filterData = Builders<DataBlock<T>>.Filter.In(x => x.BlockReference, blockChain.blocks.Select(el => el.Id));
            blockChain.dataBlocks = dataCollection.Find(filterData).ToList();
        }

        /// <summary>
        /// Calculate the HashCode of the block or element.
        /// </summary>
        /// <param name="data">The block or data for which the HashCode is calculated.</param>
        /// <returns>The HashCode.</returns>
        public static string CalculateHash(object data) {
            SHA256 sha256 = SHA256.Create();
            return Convert.ToBase64String(sha256.ComputeHash(Encoding.ASCII.GetBytes(JsonSerializer.Serialize(data))));
        }

        /// <summary>
        /// Return the last element that satisfies the condition the condition
        /// </summary>
        /// <param name="filter">The condition to apply to the blockchain</param>
        /// <returns>The Element of the blockChain</returns>
        public async Task<DataBlock<T>> GetExistingDataBlock(FilterDefinition<DataBlock<T>> filter)
        {
            var sort = Builders<DataBlock<T>>.Sort.Descending(x => x.CreationDate);
            var options = new FindOptions<DataBlock<T>, DataBlock<T>> { Sort = sort, Limit = 1 };
            return dataCollection.FindSync(filter, options).Single();
        }

        /// <summary>
        /// Return all the elements that satisfies the condition the condition in order they are inserted
        /// </summary>
        /// <param name="filter">The condition to apply to the blockchain</param>
        /// <returns>The Elements of the blockChain</returns>
        public async Task<List<DataBlock<T>>> GetHistoryOfExistingDataBlock(FilterDefinition<DataBlock<T>> filter)
        {
            var sort = Builders<DataBlock<T>>.Sort.Ascending(x => x.CreationDate);
            var options = new FindOptions<DataBlock<T>, DataBlock<T>> { Sort = sort };
            return dataCollection.FindSync(filter, options).ToList();
        }
    }

    /// <summary>
    /// Identifies the blockchain as distinct lists of blocks and elements.
    /// </summary>
    /// <typeparam name="T">The type of data that the blockchain manages.</typeparam>
    public class BlockChain<T> {
        public BlockChain()
        {
            blocks = new List<Block>();
            dataBlocks = new List<DataBlock<T>>();
        }
        public List<Block> blocks;
        public List<DataBlock<T>> dataBlocks;
        /// <summary>
        /// Returns the ID of the last block still open.
        /// </summary>
        public Guid CurrentBlockId => blocks.Last().Id;
        /// <summary>
        /// Returns the HashCode of the last item.
        /// </summary>
        public string LastDataBlockHash => dataBlocks.Any() ? dataBlocks.Last().HashCode : null;
        /// <summary>
        /// Returns The last block.
        /// </summary>
        public Block CurrentBlock => blocks.LastOrDefault((Block)null);
    }

    /// <summary>
    /// The interface that defines the behavior of the blockchain's typified manager.
    /// </summary>
    /// <typeparam name="T">The type of data that the blockchain manages.</typeparam>
    public interface IBlockChainManager<T> {
        /// <summary>
        /// It must load the entire blockchain.
        /// </summary>
        /// <returns>There is no need to return anything.</returns>
        public Task LoadFromDBFull();
        /// <summary>
        /// It must load the elements contained in the last two blocks of the blockchain.
        /// </summary>
        /// <returns>There is no need to return anything.</returns>
        public Task LoadFromDBLastTwoBlocks();
        /// <summary>
        /// It must close the last open block and create a new one.
        /// </summary>
        /// <returns>It must return the new block just created.</returns>
        public Task<Block> CloseLastBlockAndCreateNewOne();
        /// <summary>
        /// It must create a new item in the blockchain.
        /// </summary>
        /// <param name="newData">The new data to be inserted into the blockchain.</param>
        /// <returns>It must return the new item just created.</returns>
        public Task<DataBlock<T>> CreateNewDataBlock(T newData);
        /// <summary>
        /// It must return the last element of the chain that satisfies the condition
        /// </summary>
        /// <returns>The Element of the blockChain</returns>
        public Task<DataBlock<T>> GetExistingDataBlock(FilterDefinition<DataBlock<T>> filter);
        /// <summary>
        /// It must return the entire list of elements that satisfies the condition
        /// </summary>
        /// <returns>The Elements of the blockChain</returns>
        public Task<List<DataBlock<T>>> GetHistoryOfExistingDataBlock(FilterDefinition<DataBlock<T>> filter);
    }
}
