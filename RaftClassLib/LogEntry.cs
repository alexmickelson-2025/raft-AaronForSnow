using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaftClassLib;

public record LogEntry (int Term,
    Operation Command = Operation.None,
    string UniqueValue = "none" );


