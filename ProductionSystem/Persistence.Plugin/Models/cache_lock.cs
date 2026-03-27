using System;
using System.Collections.Generic;

namespace PersistencePlugin.Models;

public partial class cache_lock
{
    public string key { get; set; } = null!;

    public string owner { get; set; } = null!;

    public int expiration { get; set; }
}
