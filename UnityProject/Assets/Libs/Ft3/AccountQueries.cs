using System.Collections.Generic;
using Chromia.Postchain.Client;

namespace Chromia.Postchain.Ft3
{
  public static class AccountQueries
  {
    public static object[] AccountAuthDescriptors(byte[] accountId)
    {
      var gtv = new List<object>() {
                "ft3.get_account_auth_descriptors",
                ("id", PostchainUtil.ByteArrayToString(accountId))
            };

      return gtv.ToArray();
    }

    public static object[] AccountById(byte[] id)
    {
      var gtv = new List<object>() {
                "ft3.get_account_by_id",
                ("id", PostchainUtil.ByteArrayToString(id))
            };

      return gtv.ToArray();
    }

    public static object[] AccountsByParticipantId(byte[] id)
    {
      var gtv = new List<object>() {
                "ft3.get_accounts_by_participant_id",
                ("id", PostchainUtil.ByteArrayToString(id))
            };

      return gtv.ToArray();
    }

    public static object[] AccountsByAuthDescriptorId(byte[] id)
    {
      var gtv = new List<object>() {
                "ft3.get_accounts_by_auth_descriptor_id",
                ("descriptor_id", PostchainUtil.ByteArrayToString(id))
            };

      return gtv.ToArray();
    }
  }
}