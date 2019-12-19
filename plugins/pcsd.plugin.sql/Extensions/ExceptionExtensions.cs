using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pcsd.plugin.sql.Extensions
{
    public static class ExceptionExtensions
    {
        public static Exception GetInnermostException(this Exception e)
        {
            if (e == null)
            {
                throw new ArgumentNullException(nameof(e));
            }

            while (e.InnerException != null)
            {
                e = e.InnerException;
            }

            return e;
        }
    }
}
