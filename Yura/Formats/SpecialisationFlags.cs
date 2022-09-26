using System;

namespace Yura.Formats
{
    [Flags]
    enum SpecialisationFlags : uint
    {
        English = 1 << 0,
        French = 1 << 1,
        German = 1 << 2,
        Italian = 1 << 3,
        Spanish = 1 << 4,

        GfxNextGen = 0x80000000
    }
}
