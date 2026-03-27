using System;
using System.Collections.Generic;

namespace PersistencePlugin.Models;

public partial class failed_job
{
    public long id { get; set; }

    public string uuid { get; set; } = null!;

    public string connection { get; set; } = null!;

    public string queue { get; set; } = null!;

    public string payload { get; set; } = null!;

    public string exception { get; set; } = null!;

    public DateTime failed_at { get; set; }
}
