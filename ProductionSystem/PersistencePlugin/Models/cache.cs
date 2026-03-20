using System;
using System.Collections.Generic;

namespace PersistencePlugin.Models;

public partial class cache
{
    public string key { get; set; } = null!;

    public string value { get; set; } = null!;

    public int expiration { get; set; }
}
