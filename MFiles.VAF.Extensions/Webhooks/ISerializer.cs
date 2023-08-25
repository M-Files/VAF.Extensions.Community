using MFiles.VAF.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions.Webhooks
{

    public interface ISerializer
    {
        byte[] Serialize(object input);
        byte[] Serialize<T>(T input);
        object Deserialize(byte[] input, Type t);
        T Deserialize<T>(byte[] input);
    }
}
