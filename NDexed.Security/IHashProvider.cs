using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDexed.Security
{
    public interface IHashProvider
    {
        string GenerateHash(string value, string salt, string key);
        void RegisterKeyPair(string publicKey, string privateKey);
    }
}
