using System.Linq;
using System.Collections.Generic;

namespace Chromia.Postchain.Ft3
{
    // PaymentOperation class is used to represent transfers and cross-chain transfers using one type. Original
    // idea vas to implement PaymentOperation as adapter for for TransferOperation and XTransferOperation, to abstract
    // different types of transfers, but that implementation would be more complex, and at the moment it looks like there
    // is no value add from adding more complexity.
    // When parsing raw transactions, inputs and outputs from transfers and cross-chain transfer will be copied to the
    // PaymentOperation objects.
    public class PaymentOperation
    {
        public readonly PaymentParam[] Inputs;
        public readonly PaymentParam[] Outputs;
    
        public PaymentOperation(PaymentParam[] inputs, PaymentParam[] outputs)
        {
            this.Inputs = inputs;
            this.Outputs = outputs;
        }

        public bool HasInputOrOutputWithChainAndAccount(string chainId, string accountId)
        {
            return this.Inputs.Any(input => input.IsChainId(chainId) && input.IsAccountId(accountId) ||
                    this.Outputs.Any(output => output.IsChainId(chainId) && output.IsAccountId(accountId))
            );
        }

        public PaymentParam[] InputsWithChainAndAccount(string chainId, string accountId)
        {
            return this.Inputs.Where(input => input.IsChainId(chainId) && input.IsAccountId(accountId)).ToArray();
        }

        public PaymentParam[] OutputsWithChainAndAccount(string chainId, string accountId)
        {
            return this.Outputs.Where(output => output.IsChainId(chainId) && output.IsAccountId(accountId)).ToArray();
        }

        public PaymentParam[] InputsWithAsset(string assetId)
        {
            return this.Inputs.Where(input => input.IsAssetId(assetId)).ToArray(); 
        }

        public PaymentParam[] OutputsWithAsset(string assetId)
        {
            return this.Outputs.Where(output => output.IsAssetId(assetId)).ToArray(); 
        }

        public static PaymentOperation FromTransfer(TransferOperation transfer, string chainId)
        {
            var inputs = transfer.Inputs.Select(input => PaymentParam.FromTransferParam(input, chainId)).ToArray();
            var outputs = transfer.Outputs.Select(output => PaymentParam.FromTransferParam(output, chainId)).ToArray();

            return new PaymentOperation(inputs, outputs);
        }

        public static PaymentOperation FromXTransfer(XTransferOperation transfer, string sourceChainId)
        {
            var input = PaymentParam.FromTransferParam(transfer.Source, sourceChainId);

            var output = new PaymentParam(
                transfer.Hops[transfer.Hops.Count() - 1],
                transfer.Target.AccountId,
                transfer.Source.AssetId,
                transfer.Source.Amount
            );

            return new PaymentOperation(new List<PaymentParam>(){input}.ToArray(), new List<PaymentParam>(){output}.ToArray());
        }
    }
}
