﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Louman.Models.DTOs.Product
{
    public class ProductInStock
    {
        public int ProductId { get; set; }
         public string ProductName { get; set; }
       public decimal Price { get; set; }
    }
}
