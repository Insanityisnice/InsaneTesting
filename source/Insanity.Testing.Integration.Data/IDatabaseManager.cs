using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Insanity.Testing.Integration.Data
{
    public interface IDatabaseManager
    {
        string ConnectionString { get; }
        IDatabase Database { get; }


    }
}
