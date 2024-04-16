using blockChainManagerTests.Helpers;
using blockChainManagerTests.TestClass;
using MongoDB.Driver;
using Moq;

namespace blockChainManager.Tests
{
    [TestClass()]
    public class BlockChainManagerTests
    {
        static Mock<BlockChainManager<Voucher>> voucherBlockChain;
        const string connectionString = "mongodb://mongoadmin:secret@localhost:27017/";
        const string databaseName = "testDBVoucherBlockChain";
        const string collectionDataName = "dataBCVoucher";
        const string collectionBlockName = "blockBCVoucher";
        const string postFix = "-first";
        const long elementInBlock = 10;
        const int numberOfElementToInsert = 35;

        [ClassInitialize]
        public static async Task ClassInitialize(TestContext context)
        {
            voucherBlockChain = new Mock<BlockChainManager<Voucher>>(connectionString, databaseName, collectionDataName, collectionBlockName, elementInBlock);
        }

        [TestMethod()]
        public async Task CheckBlockchainCreationTest()
        {
            var localManager = new Mock<BlockChainManager<Voucher>>(connectionString, databaseName, collectionDataName + postFix, collectionBlockName + postFix, elementInBlock);
            var vbcman = localManager.Object;
            localManager.Verify(x => x.test_LoadFromDBLastTwoBlocks(), Times.Once);
            localManager.Verify(x => x.test_CloseLastBlockAndCreateNewOne(), Times.Once);
            Assert.IsTrue(true);
        }

        [TestMethod]
        public async Task LoadFullDB() {
            voucherBlockChain.Object.LoadFromDBFull();
            Assert.IsTrue(true);
        }

        [TestMethod()]
        public async Task SingleTestSimulation()
        {
            var listVoucher = new List<string>();
            foreach (var voucher in FakeGenerator.GenerateRandomVouchers(numberOfElementToInsert).Select((el, indx) => (el, indx))) {
                voucher.el.voucherCode = Guid.NewGuid().ToString();
                if (voucher.indx % elementInBlock == 0)
                    listVoucher.Add(voucher.el.voucherCode);
                await voucherBlockChain.Object.CreateNewDataBlock(voucher.el);
            }
            voucherBlockChain.Verify(x => x.test_CreateNewDataBlock(), Times.Exactly(numberOfElementToInsert));
            voucherBlockChain.Verify(x => x.test_CloseLastBlockAndCreateNewOne(), Times.Exactly((int)Math.Floor((decimal)numberOfElementToInsert / elementInBlock) + 1));

            var rnd = new Random(DateTime.Now.Millisecond);
            var history = rnd.Next(3, 6);
            for (int i = 0; i < history; i++)
            {
                foreach (var item in listVoucher)
                {
                    var element = await voucherBlockChain.Object.GetExistingDataBlock(Builders<DataBlock<Voucher>>.Filter.Eq(x => x.Data.voucherCode, item));
                    element.Data.amount += rnd.Next(-5000, 5000) / 100;
                    await voucherBlockChain.Object.CreateNewDataBlock(element.Data);
                    foreach (var voucher in FakeGenerator.GenerateRandomVouchers(12))
                    {
                        await voucherBlockChain.Object.CreateNewDataBlock(voucher);
                    }
                }
            }

            foreach (var item in listVoucher)
                Assert.AreEqual((await voucherBlockChain.Object.GetHistoryOfExistingDataBlock(Builders<DataBlock<Voucher>>.Filter.Eq(x => x.Data.voucherCode, item))).Count(), history + 1);

            Assert.IsTrue(true);
        }
    }
}