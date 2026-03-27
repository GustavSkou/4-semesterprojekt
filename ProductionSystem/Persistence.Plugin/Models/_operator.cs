using System;
using System.Collections.Generic;

namespace PersistencePlugin.Models;

public partial class _operator
{
    public long id { get; set; }

    public string name { get; set; } = null!;

    public string email { get; set; } = null!;

    public DateTime? email_verified_at { get; set; }

    public string password { get; set; } = null!;

    public string? remember_token { get; set; }

    public DateTime? created_at { get; set; }

    public DateTime? updated_at { get; set; }
}
