using System;
using System.Collections.Generic;

namespace PersistencePlugin.Models;

public partial class level
{
    public long id { get; set; }

    public DateTime? created_at { get; set; }

    public DateTime? updated_at { get; set; }

    public string name { get; set; } = null!;

    public virtual ICollection<log> logs { get; set; } = new List<log>();
}
