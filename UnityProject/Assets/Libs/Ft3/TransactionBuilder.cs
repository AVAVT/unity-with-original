using System.Collections.Generic;
using System;
using Chromia.Postchain.Client;

namespace Chromia.Postchain.Ft3
{
  public class TransactionBuilder
  {
    private List<Operation> _operations;
    public readonly Blockchain Blockchain;
    public TransactionBuilder(Blockchain blockchain)
    {
      this.Blockchain = blockchain;
      _operations = new List<Operation>();
    }

    public TransactionBuilder AddOperation(Operation operation)
    {
      this._operations.Add(operation);
      return this;
    }

    private List<object> ToGTV(object[] args)
    {
      List<object> gtvList = new List<object>();

      foreach (var arg in args)
      {
        if (arg is AuthDescriptor)
        {
          AuthDescriptor authDescriptor = (AuthDescriptor)arg;
          gtvList.Add(authDescriptor.ToGTV());
        }
        else if (arg is System.Byte[] sbyte_args)
        {
          gtvList.Add(PostchainUtil.ByteArrayToString(sbyte_args));
        }
        else if (arg is byte[] byte_args)
        {
          gtvList.Add(PostchainUtil.ByteArrayToString(byte_args));
        }
        else if (arg is string)
        {
          gtvList.Add(arg);
        }
        else if (arg is int)
        {
          gtvList.Add(arg);
        }
        else if (arg is System.Array)
        {
          gtvList.Add(arg);
        }
        else
        {
          Console.WriteLine("Wrong Datatype.");
        }
      }

      return gtvList;
    }

    public Transaction Build(List<byte[]> signers)
    {
      var tx = this.Blockchain.Connection.Gtx.NewTransaction(signers.ToArray());
      foreach (var operation in this._operations)
      {
        tx.AddOperation(operation.Name, ToGTV(operation.Args).ToArray());
      }
      return new Transaction(tx, this.Blockchain);
    }

    public Transaction BuildAndSign(User user)
    {
      var tx = this.Build(user.AuthDescriptor.Signers);
      tx.Sign(user.KeyPair);
      return tx;
    }
  }
}