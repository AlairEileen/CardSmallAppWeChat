using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CardWXSmallApp.ResponseModels
{
    public class BaseResponseModel<T>
    {
        public int StatusCode { get; set; }
        public T JsonData { get; set; }
    }
}
