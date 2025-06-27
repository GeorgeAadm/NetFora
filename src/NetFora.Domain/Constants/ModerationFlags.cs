using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetFora.Domain.Constants
{
    public static class ModerationFlags
    {
        public const int None = 0;
        public const int Misleading = 1;
        public const int False = 2;

        public static bool HasFlag(int flags, int flag) => (flags & flag) > 0;
        public static int AddFlag(int flags, int flag) => flags | flag;
        public static int RemoveFlag(int flags, int flag) => flags & ~flag;
    }
}
