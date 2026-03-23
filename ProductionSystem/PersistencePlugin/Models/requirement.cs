using System;
using System.Collections.Generic;

namespace PersistencePlugin.Models;

public partial class requirement
{
    public long id { get; set; }

    public string name { get; set; } = null!;

    public string value { get; set; } = null!;

    public long component_id { get; set; }

    public DateTime? created_at { get; set; }

    public DateTime? updated_at { get; set; }

    public virtual component component { get; set; } = null!;
}
