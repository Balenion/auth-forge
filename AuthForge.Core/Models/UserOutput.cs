using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace AuthForge.Core.Models
{
    public record UserOutput(
        string Login,
        string PasswordHash,
        string? Salt,
        string Algorithm
    );
}