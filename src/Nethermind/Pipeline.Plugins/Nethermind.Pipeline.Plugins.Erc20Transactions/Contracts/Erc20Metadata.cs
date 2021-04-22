using Nethermind.Abi;
using Nethermind.Core;
using Nethermind.Facade;

namespace Nethermind.Pipeline.Plugins.Erc20Transactions.Contracts
{
    public class Erc20Metadata : BlockchainBridgeContract
    {
        private IConstantContract Constant { get; }

        public Erc20Metadata(IAbiEncoder abiEncoder, Address contractAddress, IBlockchainBridge blockchainBridge) :
            base(abiEncoder, contractAddress)
        {
            Constant = GetConstant(blockchainBridge);
        }

        public string Name(BlockHeader header) => Constant.Call<string>(header, nameof(Name), Address.Zero);
    }
}
