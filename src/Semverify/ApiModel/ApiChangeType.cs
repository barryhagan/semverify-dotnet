
using System;

namespace Semverify.ApiModel
{
    [Flags]
    internal enum ApiChangeType
    {
        None = 0,
        Addition = 1,
        Removal = 2,
    }
}
