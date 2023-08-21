using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonopolyTest.Utils
{
    public static class SqlDataReaderExtensions
    {
        public static Guid SafeGetGuid(this SqlDataReader reader, int columnIndex)
        {
            if (!reader.IsDBNull(columnIndex))
            {
                return reader.GetGuid(columnIndex);
            }
            return Guid.Empty;
        }
    }
}
