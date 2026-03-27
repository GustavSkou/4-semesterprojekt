using System;
using System.Collections.Generic;

namespace PersistencePlugin.Models;

public partial class computer
{
    public long id { get; set; }

    public int? tray_id { get; set; }

    public long order_id { get; set; }

    public DateTime? created_at { get; set; }

    public DateTime? updated_at { get; set; }

    public virtual ICollection<computer_component_list> computer_component_lists { get; set; } = new List<computer_component_list>();

    public virtual order order { get; set; } = null!;
}
