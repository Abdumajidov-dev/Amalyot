using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amalyot.Entities
{
    public class ApiResponse<T>
    {
        public bool Status { get; set; }
        public Result<T> Resoult { get; set; }
    }
    public class Result<T>
    {
        public List<T> Data { get; set; }
    }
}
