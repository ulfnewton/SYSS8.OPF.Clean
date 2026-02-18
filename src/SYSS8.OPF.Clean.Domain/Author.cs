using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SYSS8.OPF.Clean.Domain
{
    public class Author : AuditableEntity
    {
        public string Name { get; set; } = "";
        public List<Book> Books { get; set; } = new();
    }
}
