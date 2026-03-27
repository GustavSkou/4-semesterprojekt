using System;
using System.Collections.Generic;

namespace PersistencePlugin.Models;

public partial class component
{
    public long id { get; set; }

    public string name { get; set; } = null!;

    public int? tray_id { get; set; }

    public long? category_id { get; set; }

    public decimal price { get; set; }

    public DateTime? created_at { get; set; }

    public DateTime? updated_at { get; set; }

    public virtual category? category { get; set; }

    public virtual ICollection<computer_component_list> computer_component_lists { get; set; } = new List<computer_component_list>();

    public virtual ICollection<requirement> requirements { get; set; } = new List<requirement>();

    public virtual ICollection<specification> specifications { get; set; } = new List<specification>();

    public virtual ICollection<wattage_list> wattage_lists { get; set; } = new List<wattage_list>();
}
