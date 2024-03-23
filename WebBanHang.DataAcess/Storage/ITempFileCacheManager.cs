using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebBanHang.DataAcess.Storage
{
    public interface ITempFileCacheManager
    {
        void SetFile(string token, byte[] content);

        byte[] GetFile(string token);
    }
}
