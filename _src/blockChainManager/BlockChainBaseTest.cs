using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace blockChainManager
{
    [ExcludeFromCodeCoverage]
    public class BlockChainBaseTest
    {
        /// <summary>
        /// For test purpose only.
        /// </summary>
        public virtual void test_CloseLastBlockAndCreateNewOne() { }

        /// <summary>
        /// For test purpose only.
        /// </summary>
        public virtual void test_LoadFromDBLastTwoBlocks() { }

        /// <summary>
        /// For test purpose only.
        /// </summary>
        public virtual void test_CreateNewDataBlock() { }
    }
}
