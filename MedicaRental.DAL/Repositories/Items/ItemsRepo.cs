﻿using MedicaRental.DAL.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedicaRental.DAL.Repositories;

public class ItemsRepo : IItemsRepo
{
    private readonly MedicaRentalDbContext _context;

    public ItemsRepo(MedicaRentalDbContext context)
    {
        _context = context;
    }
}
