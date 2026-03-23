using System;
using System.Collections.Generic;

namespace PersistencePlugin.Models;

public partial class computer_component_list
{
    public long id { get; set; }

    public long computer_id { get; set; }

    public long component_id { get; set; }

    public DateTime? created_at { get; set; }

    public DateTime? updated_at { get; set; }

    public virtual component component { get; set; } = null!;

    public virtual computer computer { get; set; } = null!;
}
