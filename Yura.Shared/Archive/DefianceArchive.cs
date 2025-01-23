using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yura.Shared.Archive
{
    public class DefianceArchive : ArchiveFile
    {
        public DefianceArchive(ArchiveOptions options) : base(options)
        {
        }

        public override void Open()
        {
            throw new NotImplementedException();
        }

        public override byte[] Read(ArchiveRecord record)
        {
            throw new NotImplementedException();
        }
    }
}
