using System;
using System.Collections.Generic;

namespace PersistencePlugin.Models;

public partial class customer
{
    public long id { get; set; }

    public string name { get; set; } = null!;

    public string email { get; set; } = null!;

    public string? address { get; set; }

    public DateTime? created_at { get; set; }

    public DateTime? updated_at { get; set; }

    public virtual ICollection<order> orders { get; set; } = new List<order>();
}
