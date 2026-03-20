using System;
using System.Collections.Generic;

namespace PersistencePlugin.Models;

public partial class order
{
    public long id { get; set; }

    public DateOnly order_date { get; set; }

    public string status { get; set; } = null!;

    public long customer_id { get; set; }

    public DateTime? created_at { get; set; }

    public DateTime? updated_at { get; set; }

    public virtual computer? computer { get; set; }

    public virtual customer customer { get; set; } = null!;
}
