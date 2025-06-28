using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetFora.Application.DTOs
{
    public class ModerationRequest
    {
        public int Flags { get; set; }
        public string? Reason { get; set; }
    }
}
