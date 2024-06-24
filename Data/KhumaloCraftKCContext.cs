using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using KhumaloCraftKC.Models;

namespace KhumaloCraftKC.Data
{
    public class KhumaloCraftKCContext : DbContext
    {
        public KhumaloCraftKCContext(DbContextOptions<KhumaloCraftKCContext> options)
            : base(options)
        {
        }
    }

}
