using System;
using System.Collections.Generic;

namespace PersistencePlugin.Models;

public partial class log
{
    public long id { get; set; }

    public DateTime timestamp { get; set; }

    public string? description { get; set; }

    public long level_id { get; set; }

    public long source_id { get; set; }

    public long type_id { get; set; }

    public DateTime? created_at { get; set; }

    public DateTime? updated_at { get; set; }

    public virtual level level { get; set; } = null!;

    public virtual source source { get; set; } = null!;

    public virtual type type { get; set; } = null!;
}
