using Chromia.Postchain.Client.GTX;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace Chromia.Postchain.Ft3
{
    public class Blockchain
    {
        public readonly byte[] Id;
        public readonly BlockchainInfo Info;
        public readonly ConnectionClient Connection;
        private readonly DirectoryService _directoryService;

        public Blockchain(byte[] id, BlockchainInfo info, ConnectionClient connection, DirectoryService directoryService)
        {
            this.Id = id;
            this.Info = info;
            this.Connection = connection;
            this._directoryService = directoryService;
        }

        public async static Task<Blockchain> Initialize(byte[] blockchainRID, DirectoryService directoryService)
        {
            var chainConnectionInfo = directoryService.GetChainConnectionInfo(blockchainRID);

            if(chainConnectionInfo == null)
            {
                throw new Exception("Cannot find details for chain with RID: " + Util.ByteArrayToString(blockchainRID));
            }

            var connection = new ConnectionClient(
                chainConnectionInfo.Url, 
                Util.ByteArrayToString(blockchainRID)
            );

            var info = await BlockchainInfo.GetInfo(connection);
            return new Blockchain(blockchainRID, info, connection, directoryService);
        }

        public BlockchainSession NewSession(User user)
        {
            return new BlockchainSession(user, this);
        }

        public async Task<Account[]> GetAccountsByParticipantId(byte[] id, User user)
        {
            return await Account.GetByParticipantId(id, this.NewSession(user));
        }

        public async Task<Account[]> GetAccountsByAuthDescriptorId(byte[] id, User user)
        {
            return await Account.GetByAuthDescriptorId(id, this.NewSession(user));
        }

        public async Task<Account> RegisterAccount(AuthDescriptor authDescriptor, User user)
        {
            return await Account.Register(authDescriptor, this.NewSession(user));
        }

        public async Task<Asset[]> GetAssetsByName(string name)
        {
            return await Asset.GetByName(name, this);
        }

        public async Task<Asset> GetAssetById(byte[] id)
        {
            return await Asset.GetById(id, this);
        }

        public async Task<Asset[]> GetAllAssets()
        {
            return await Asset.GetAssets(this);
        }

        public async Task LinkChain(byte[] chainId)
        {
            var tx = this.Connection.Gtx.NewTransaction(new byte[][] {});
            tx.AddOperation("ft3.xc.link_chain", Util.ByteArrayToString(chainId));
            await tx.PostAndWaitConfirmation();
        }

        public async Task<bool> IsLinkedWithChain(byte[] chainId)
        {
            var info = await this.Query<int>(
                "ft3.xc.is_linked_with_chain",
                ("chain_rid", Util.ByteArrayToString(chainId))
            );

            if(info.control.Error)
            {
                return false;
            }

            return info.content == 1;
        }

        public async Task<List<byte[]>> GetLinkedChainsIds()
        {
            var linkedChains = await this.Query<string[]>("ft3.xc.get_linked_chains");
            
            if(linkedChains.control.Error)
            {
                Console.WriteLine(linkedChains.control.ErrorMessage);
                return new List<byte[]>();
            }
            
            return linkedChains.content.Select(elem => Util.HexStringToBuffer(elem)).ToList();
        }

        public async Task<Blockchain[]> GetLinkedChains()
        {
            var chainIds = await this.GetLinkedChainsIds();
            return await Task.WhenAll(chainIds.Select(elem => Blockchain.Initialize(elem, this._directoryService)));
        }
        
        public async Task<(T content, PostchainErrorControl control)> Query<T>(string name, params dynamic[] queryObject)
        {
            return await this.Connection.Query<T>(name, queryObject);
        }

        public async Task<PostchainErrorControl> Call(Operation operation, User user)
        {
            var txBuilder = this.CreateTransactionBuilder();
            txBuilder.AddOperation(operation);
            var tx = txBuilder.Build(user.AuthDescriptor.Signers);
            tx.Sign(user.KeyPair);
            return await tx.Post();
        }

        public TransactionBuilder CreateTransactionBuilder()
        {
            return new TransactionBuilder(this);
        }
    }
}