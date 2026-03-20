using System;
using System.Collections.Generic;

namespace PersistencePlugin.Models;

public partial class migration
{
    public int id { get; set; }

    public string migration1 { get; set; } = null!;

    public int batch { get; set; }
}
