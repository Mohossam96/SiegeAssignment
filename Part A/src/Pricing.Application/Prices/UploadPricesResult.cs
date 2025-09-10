using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pricing.Application.Prices
{
    public class UploadPricesResult
    {
        public bool IsSuccess { get; set; }
        public List<string> Errors { get; set; } = new();

        public static UploadPricesResult Success() => new() { IsSuccess = true };
        public static UploadPricesResult Failure(List<string> errors) => new() { IsSuccess = false, Errors = errors };
    }
}
