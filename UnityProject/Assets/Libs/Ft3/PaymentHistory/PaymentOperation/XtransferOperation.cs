using System.Collections.Generic;
using Chromia.Postchain.Client;

namespace Chromia.Postchain.Ft3
{
  public class XTransferOperation
  {
    public readonly TransferParam Source;
    public readonly XTransferTarget Target;
    public readonly string[] Hops;

    public XTransferOperation(TransferParam source, XTransferTarget target, string[] hops)
    {
      this.Source = source;
      this.Target = target;
      this.Hops = hops;
    }

    public static XTransferOperation From(dynamic rawTransfer)
    {
      var transfer = rawTransfer[1];
      var hops = new List<string>();
      var source = new TransferParam(PostchainUtil.ByteArrayToString(transfer[0][0]), PostchainUtil.ByteArrayToString(transfer[0][1]), (int)transfer[0][3]);
      var target = new XTransferTarget(PostchainUtil.ByteArrayToString(transfer[1][0]));

      foreach (var hop in transfer[2])
      {
        hops.Add(PostchainUtil.ByteArrayToString(hop));
      }

      return new XTransferOperation(source, target, hops.ToArray());
    }
  }
}