using System;
using System.Collections.Generic;

namespace PersistencePlugin.Models;

public partial class category
{
    public long id { get; set; }

    public string name { get; set; } = null!;

    public DateTime? created_at { get; set; }

    public DateTime? updated_at { get; set; }

    public virtual ICollection<component> components { get; set; } = new List<component>();
}
