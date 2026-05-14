using System;
using System.Collections.Generic;
using System.Text;

namespace AuthForge.Core.Models
{
    public record HashResult(string Hash, string? Salt);
}