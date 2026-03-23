using System;
using System.Collections.Generic;

namespace PersistencePlugin.Models;

public partial class wattage_list
{
    public long id { get; set; }

    public int wattage { get; set; }

    public long component_id { get; set; }

    public DateTime? created_at { get; set; }

    public DateTime? updated_at { get; set; }

    public virtual component component { get; set; } = null!;
}
